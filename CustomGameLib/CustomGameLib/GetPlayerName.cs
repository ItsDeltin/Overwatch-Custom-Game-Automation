using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Text;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Deltin.CustomGameAutomation
{
    partial class CustomGame
    {
        #region Generate Letters
#pragma warning disable
        /// <summary>
        /// Generates the alphabet to be used with <see cref="GetPlayerName(int, PlayerNameAlphabet)"/>
        /// </summary>
        /// <param name="letters">Characters to generate.</param>
        /// <returns>An alphabet to be used with <see cref="GetPlayerName(int, PlayerNameAlphabet)"/></returns>
#pragma warning restore
        public static PlayerNameAlphabet GenerateAlphabet(string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890")
        {
            FontFamily fontFamily = new FontFamily("BigNoodleTooOblique");
            Font font = new Font(fontFamily, 25);

            Bitmap[] generated = new Bitmap[letters.Length];
            int[] letterLengths = new int[letters.Length];

            for (int l = 0; l < letters.Length; l++)
            {
                string letter = letters[l].ToString();

                Size size = TextRenderer.MeasureText(letter, font);

                Bitmap bmp = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);

                Graphics g = Graphics.FromImage(bmp);
                g.FillRectangle(Brushes.White, new Rectangle(0, 0, bmp.Width, bmp.Height));
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.TextRenderingHint = TextRenderingHint.AntiAlias;
                g.DrawString(letter, font, Brushes.Black, Point.Empty);
                g.Dispose();

                for (int x = 0; x < bmp.Width; x++)
                    for (int y = 0; y < bmp.Height; y++)
                        if (bmp.GetPixel(x,y).CompareColor(new int[] { 0, 0, 0 }, 35))
                            bmp.SetPixel(x, y, Color.Black);
                        else
                            bmp.SetPixel(x, y, Color.White);

                Bitmap final = cropImage(bmp, GetBounds(bmp));
                bmp.Dispose();

                List<Point> outside = GetOutside(final);

                for (int x = 0; x < final.Width; x++)
                    for (int y = 0; y < final.Height; y++)
                    {
                        Point[] checkZones = new Point[]
                        {
                            new Point(-1, 1),
                            new Point(0, 1),
                            new Point(1, 1),
                            new Point(1, 0),
                            new Point(1, -1),
                            new Point(0, -1),
                            new Point(-1, -1),
                            new Point(-1, 0),

                            new Point(0, 0),
                        };
                        bool close = false;
                        for (int i = 0; i < checkZones.Length; i++)
                        {
                            int checkX = x + checkZones[i].X;
                            int checkY = y + checkZones[i].Y;
                            if (checkX >= 0 && checkY >= 0 && checkX < final.Width && checkY < final.Height && ContainsPixel(final, checkX, checkY))
                            {
                                close = true;
                                break;
                            }
                        }

                        if (!close && !outside.Contains(new Point(x, y)))
                            final.SetPixel(x, y, Color.DarkRed);
                    }

                // Get the vital pixels
                for (int x = 0; x < final.Width; x++)
                    for (int y = 0; y < final.Height; y++)
                        if (ContainsPixel(final, x, y)
                            && ((x > 0 && x < final.Width - 1 && ContainsPixel(final, x + 1, y) && ContainsPixel(final, x - 1, y)) 
                            || (y > 0 && y < final.Height - 1 && ContainsPixel(final, x, y + 1) && ContainsPixel(final, x, y - 1))))
                            final.SetPixel(x, y, Color.Blue);

                //if (letter == "D")
                  //  final.SetPixel(0, final.Height - 1, Color.Blue);

                // Get the length of the letter
                bool lastPixelState = false;
                int letterLength = 0;
                for (int x = 0; x < final.Width; x++)
                {
                    bool currentPixelState = ContainsPixel(final, x, final.Height - 1);
                    if (lastPixelState && !currentPixelState)
                        letterLength = x + 1;
                    lastPixelState = currentPixelState;
                }
                if (letterLength == 0)
                    letterLength = final.Width - 1;
                letterLengths[l] = letterLength;

                // Do stuff with final
                generated[l] = final;
            }

            return new PlayerNameAlphabet(generated, letters, letterLengths);
        }

        private static bool ContainsPixel(Bitmap bmp, int x, int y)
        {
            return bmp.GetPixel(x, y) == Color.FromArgb(255, 0, 0, 0) || bmp.GetPixel(x, y) == Color.FromArgb(255, 0, 0, 255);
        }

        private static Rectangle GetBounds(Bitmap bmp)
        {
            bool xFound = false;
            int xs;
            for (xs = 0; xs < bmp.Width && !xFound; xs++)
                for (int y = 0; y < bmp.Height && !xFound; y++)
                    xFound = !bmp.GetPixel(xs, y).CompareColor(new int[] { 255, 255, 255 }, 100);

            bool yFound = false;
            int ys;
            for (ys = 0; ys < bmp.Height && !yFound; ys++)
                for (int x = 0; x < bmp.Width && !yFound; x++)
                    yFound = !bmp.GetPixel(x, ys).CompareColor(new int[] { 255, 255, 255 }, 100);

            bool widthFound = false;
            int width;
            for (width = bmp.Width - 1; width >= 0 && !widthFound; width--)
                for (int y = 0; y < bmp.Height && !widthFound; y++)
                    widthFound = !bmp.GetPixel(width, y).CompareColor(new int[] { 255, 255, 255 }, 100);

            bool heightFound = false;
            int height;
            for (height = bmp.Height - 1; height >= 0 && !heightFound; height--)
                for (int x = 0; x < bmp.Width && !heightFound; x++)
                    heightFound = !bmp.GetPixel(x, height).CompareColor(new int[] { 255, 255, 255 }, 100);

            return new Rectangle(xs - 1, ys - 1, width + 3 - xs, height + 3 - ys);
        }

        private static Bitmap cropImage(Image img, Rectangle cropArea)
        {
            Bitmap bmpImage = new Bitmap(img);
            return bmpImage.Clone(cropArea, bmpImage.PixelFormat);
        }

        private static List<Point> GetOutside(Bitmap bmp)
        {
            // Add seeds to the outer ring of the bitmap.
            List<Point> outside = new List<Point>();
            // Left
            for (int y = 0; y < bmp.Height; y++)
                if (!ContainsPixel(bmp, 0, y))
                    outside.Add(new Point(0, y));
            // Right
            for (int y = 0; y < bmp.Height; y++)
                if (!ContainsPixel(bmp, bmp.Width - 1, y))
                    outside.Add(new Point(bmp.Width - 1, y));

            // The outside variable is now a list of points around the bitmap, excluding the pixels that are black in the bitmap.
            // Spread the outside inside.

            Point[] spreadTo = new Point[]
            {
                new Point(0, 1),
                new Point(1, 0),
                new Point(0, -1),
                new Point(-1, 0)
            };
            for (int i = 0; i < outside.Count; i++)
                for (int st = 0; st < spreadTo.Length; st++)
                {
                    Point checkPoint = new Point(
                        outside[i].X + spreadTo[st].X,
                        outside[i].Y + spreadTo[st].Y);
                    if (!outside.Contains(checkPoint)
                        && checkPoint.X >= 0 && checkPoint.X < bmp.Width
                        && checkPoint.Y >= 0 && checkPoint.Y < bmp.Height
                        && !ContainsPixel(bmp, checkPoint.X, checkPoint.Y))
                    {
                        outside.Add(checkPoint); // Added seeds will be iterated upon automatically.
                    }
                }

            return outside;
        }
        #endregion

        #region Read
        /// <summary>
        /// Gets the name of a player.
        /// </summary>
        /// <param name="slot">The slot of the player.</param>
        /// <param name="alphabet">The alphabet to use.</param>
        /// <returns>The name of the player.</returns>
        public string GetPlayerName(int slot, PlayerNameAlphabet alphabet
#if DEBUG
#pragma warning disable
            , string debugLetters = ""
#pragma warning restore
#endif
            )
        {
            if (!IsSlotValid(slot))
                throw new InvalidSlotException(slot);

            if (alphabet == null)
                throw new ArgumentNullException(nameof(alphabet));

            int lineScanFade = 80;
            int letterCheckFade = 80;
            float cutoff = .92f;
            LetterPrioritize[] prioritize = new LetterPrioritize[]
            {
                new LetterPrioritize('D', 'O'),
            };
            int cy = 131;
            int xStart = 131;
            int xEnd = 270;
            int maxSinceLastLetter = 10;

            using (LockHandler.Interactive)
            {
                // Open the career profile.
                bool careerProfileOpenSuccess = Interact.ClickOption(slot, Markups.VIEW_CAREER_PROFILE);
                if (!careerProfileOpenSuccess)
                    return null;
                Commands.WaitForCareerProfileToLoad();

                string word = string.Empty;

                // Scan a line under the name in the career profile for text.
                for (int cx = xStart, sinceLastLetter = 0; cx < xEnd && sinceLastLetter < maxSinceLastLetter; cx++)
                {
                    if (Capture.CompareColor(cx, cy, Colors.WHITE, lineScanFade))
                    {
                        sinceLastLetter = 0;
                        PlayerNameLetterResult[] results = new PlayerNameLetterResult[alphabet.Length];
                        for (int i = 0; i < alphabet.Length; i++)
                        {
                            List<Point> filledPixels = new List<Point>();
                            float total = 0;
                            float match = 0;
                            bool failed = false;
                            for (int lx = 0; lx < alphabet.Markups[i].Width && !failed; lx++)
                                for (int ly = 0; ly < alphabet.Markups[i].Height && !failed; ly++)
                                {
                                    PixelType pixelType = GetPixelType(alphabet.Markups[i], lx, ly);

                                    if (pixelType != PixelType.Any)
                                    {
                                        int px = cx + lx;
                                        int py = cy + ly + 2 - alphabet.Markups[i].Height;
                                        bool pixelFilled = Capture.CompareColor(px, py, Colors.WHITE, letterCheckFade);

                                        failed = pixelType == PixelType.Required && !pixelFilled;
                                        if (failed)
                                            break;

                                        total++;
                                        if ((pixelFilled && (pixelType == PixelType.Filled || pixelType == PixelType.Required)) || (!pixelFilled && pixelType == PixelType.Empty))
                                        {
                                            match++;
                                            if (pixelType == PixelType.Filled || pixelType == PixelType.Required)
                                            {
                                                filledPixels.Add(new Point(lx, ly));
                                                #region DEBUG
#if DEBUG
                                                if (DebugMenu != null)
                                                    DebugMenu.SetDebugImage(px, py, Color.Green);
#endif
                                                #endregion
                                            }
                                            #region DEBUG
#if DEBUG
                                            else if (DebugMenu != null)
                                                DebugMenu.SetDebugImage(px, py, Color.DarkBlue);
#endif
                                            #endregion
                                        }
                                        #region DEBUG
#if DEBUG
                                        else if (DebugMenu != null)
                                        {
                                            if (pixelType == PixelType.Filled)
                                                DebugMenu.SetDebugImage(px, py, Color.DarkRed);
                                            else
                                                DebugMenu.SetDebugImage(px, py, Color.Yellow);
                                        }
#endif
                                        #endregion
                                    }
                                }

                            if (!failed)
                            {
                                bool isConnected = PointsAreConnected(filledPixels);
                                float result = !isConnected ? 0 : match / total;

                                results[i] = new PlayerNameLetterResult(alphabet.Letters[i], result, (int)total, (int)match, alphabet.Markups[i], alphabet.LetterLengths[i]);
                                #region DEBUG
#if DEBUG
                                Console.Write($"{alphabet.Letters[i]} - R: ");
                                if (result >= cutoff)
                                {
                                    Console.BackgroundColor = ConsoleColor.DarkGreen;
                                    Console.Write($"{result}");
                                    Console.BackgroundColor = ConsoleColor.Black;
                                }
                                else
                                    Console.Write($"{result}");
                                Console.WriteLine($" T: {total} - M: {match} - C: {isConnected}");
#endif
                                #endregion
                            }
                            #region DEBUG
#if DEBUG
                            else
                                Console.WriteLine($"{alphabet.Letters[i]} vital pixel missing.");

                            if (DebugMenu != null)
                                DebugMenu.InvalidateDebugImage();

                            if (debugLetters.Contains(alphabet.Letters[i]))
                                Console.ReadLine();

                            if (DebugMenu != null)
                                DebugMenu.ResetDebugImage();
#endif
                            #endregion
                        }

                        PlayerNameLetterResult mostLikely = results.Where(result => result != null && result.Result >= cutoff).OrderByDescending(result => result.Match).ThenByDescending(result => result.Result).FirstOrDefault();

                        if (mostLikely != null)
                        {
                            for (int p = 0; p < prioritize.Length; p++)
                                if (mostLikely.Letter == prioritize[p].Letter)
                                    for (int t = 0; t < prioritize[p].To.Length; t++)
                                    {
                                        var to = results.FirstOrDefault(result => result != null && result.Result >= cutoff && result.Letter == prioritize[p].To[t]);
                                        if (to != null)
                                        {
                                            mostLikely = to;
                                            p = prioritize.Length;
                                            break;
                                        }
                                    }
                            #region DEBUG
#if DEBUG
                            Console.BackgroundColor = ConsoleColor.DarkBlue;
                            Console.WriteLine($"Letter detected: {mostLikely.Letter}");
                            Console.BackgroundColor = ConsoleColor.Black;
#endif
                            #endregion
                            word += mostLikely.Letter;
                            cx += mostLikely.LetterLength;
                        }
                    } // Color check
                    else
                        sinceLastLetter++;
                } // Line scan

                GoBack(1);

                return word;
            }
        }

        private static PixelType GetPixelType(Bitmap bmp, int x, int y)
        {
            Color pixelColor = bmp.GetPixel(x, y);

            if (pixelColor == Color.FromArgb(255, 255, 255, 255)) // Black
                return PixelType.Any;

            if (pixelColor == Color.FromArgb(255, 0, 0, 0)) // White
                return PixelType.Filled;

            if (pixelColor == Color.FromArgb(255, 139, 0, 0)) // Dark Red
                return PixelType.Empty;

            if (pixelColor == Color.FromArgb(255, 0, 0, 255)) // Blue
                return PixelType.Required;

            throw new Exception($"Could not get the pixel type for color {pixelColor}.");
        }

        private static bool PointsAreConnected(List<Point> points)
        {
            int height = points.Max(p => p.Y);
            int width = points.Max(p => p.X);

            List<Point> detected = new List<Point>();

            Point[] checkZones = new Point[]
            {
                new Point(-1, 1),
                new Point(0, 1),
                new Point(1, 1),
                new Point(1, 0),
                new Point(1, -1),
                new Point(0, -1),
                new Point(-1, -1),
                new Point(-1, 0),
            };

            for (int bx = 0; bx < width; bx++)
            {
                var checkPoint = new Point(bx, height);
                if (points.Contains(checkPoint))
                {
                    detected.Add(checkPoint);
                    break;
                }
            }

            if (detected.Count == 0)
                throw new Exception("Could not find floor!");

            for (int d = 0; d < detected.Count; d++)
                for (int i = 0; i < checkZones.Length; i++)
                {
                    Point checkPoint = new Point(detected[d].X + checkZones[i].X,
                        detected[d].Y + checkZones[i].Y);
                    if (points.Contains(checkPoint) && !detected.Contains(checkPoint))
                        detected.Add(checkPoint);
                }

            return points.All(detected.Contains) && points.Count == detected.Count;
        }
        #endregion
    }

#pragma warning disable
    /// <summary>
    /// Data for letter scanning that <see cref="CustomGame.GetPlayerName(int, PlayerNameAlphabet)" /> uses. Generated from <see cref="CustomGame.GenerateAlphabet(string)"/>.
    /// </summary>
#pragma warning restore
    public class PlayerNameAlphabet : IDisposable
    {
        internal Bitmap[] Markups { get; private set; }
        internal string Letters { get; private set; }
        internal int[] LetterLengths { get; private set; }
        internal int Length { get; private set; }

        internal PlayerNameAlphabet(Bitmap[] markups, string letters, int[] letterLengths)
        {
            Markups = markups;
            Letters = letters;
            Length = letters.Length;
            LetterLengths = letterLengths;
        }

        /// <summary>
        /// Disposes data used by the alphabet.
        /// </summary>
        public void Dispose()
        {
            for (int i = 0; i < Markups.Length; i++)
                Markups[i].Dispose();
        }

#if DEBUG
#pragma warning disable CS1591
        public void Save(string directory)
        {
            directory = System.IO.Path.GetDirectoryName(directory) + System.IO.Path.DirectorySeparatorChar;

            for (int i = 0; i < Markups.Length; i++)
                Markups[i].Save($"{directory}{Letters[i]}.png");
        }
#pragma warning restore CS1591
#endif
    }

    internal class LetterPrioritize
    {
        public LetterPrioritize(char letter, params char[] to)
        {
            Letter = letter;
            To = to;
        }
        public char Letter { get; private set; }
        public char[] To { get; private set; }
    }

    internal class PlayerNameLetterResult
    {
        public PlayerNameLetterResult(char letter, float result, int total, int match, Bitmap letterBmp, int letterLength)
        {
            Letter = letter;
            Result = result;
            Match = match;
            Total = total;
            LetterBmp = letterBmp;
            LetterLength = letterLength;
        }

        public char Letter { get; private set; }
        public float Result { get; private set; }
        public int Total { get; private set; }
        public int Match { get; private set; }
        public Bitmap LetterBmp { get; private set; }
        public int LetterLength { get; private set; }
    }
}
