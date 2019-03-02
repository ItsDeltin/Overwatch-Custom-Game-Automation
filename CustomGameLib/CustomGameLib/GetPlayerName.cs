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
using System.Reflection;
using System.Runtime.InteropServices;

namespace Deltin.CustomGameAutomation
{
    partial class CustomGame
    {
        private static PlayerNameAlphabet EnglishAlphabet;

        #region Generate Letters
#pragma warning disable
        /// <summary>
        /// Generates the alphabet to be used with <see cref="GetPlayerName(int, PlayerNameAlphabet)"/>
        /// </summary>
        /// <param name="letters">Characters to generate.</param>
        /// <returns>An alphabet to be used with <see cref="GetPlayerName(int, PlayerNameAlphabet)"/></returns>
#pragma warning restore
        internal static PlayerNameAlphabet GenerateAlphabet(string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890")
        {
            // Load the BigNoodleTooOblique font from the resources.
            // https://github.com/Resike/Overwatch/blob/master/Fonts/BigNoodleTooOblique.ttf
            var fontBytes = GetFontResourceBytes(typeof(CustomGame).Assembly, "Deltin.CustomGameAutomation.Resources.BigNoodleTooOblique.ttf");
            var fontData = Marshal.AllocCoTaskMem(fontBytes.Length);
            Marshal.Copy(fontBytes, 0, fontData, fontBytes.Length);
            // Assign the BigNoodleTooOblique font's bytes to a private font collection.
            var pfc = new PrivateFontCollection();
            pfc.AddMemoryFont(fontData, fontBytes.Length);
            // Create the font with an EM size of 25.
            Font font = new Font(pfc.Families[0], 25);

            Bitmap[] generated = new Bitmap[letters.Length]; // Stores the generated bitmaps.
            int[] letterLengths = new int[letters.Length]; // Stores the last pixel on the X axis at the bottom of the letters.

            // Generate a bitmap of a picture of every letter using the font just loaded.
            // Store the bitmaps in the generated array.
            for (int l = 0; l < letters.Length; l++)
            {
                string letter = letters[l].ToString();

                // Create a bitmap that can contain the letter.
                Size size = TextRenderer.MeasureText(letter, font);
                Bitmap bmp = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);

                Graphics g = Graphics.FromImage(bmp);
                // Make the bitmap completely white
                g.FillRectangle(Brushes.White, new Rectangle(0, 0, bmp.Width, bmp.Height));
                // Set smoothing stuff
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.TextRenderingHint = TextRenderingHint.AntiAlias;
                // Draw the letter
                g.DrawString(letter, font, Brushes.Black, Point.Empty);
                g.Dispose();

                // The bitmap is now an anti-aliased letter, mixed with black/gray/white. 
                // Convert black and gray pixels to black and everything else white so it can be used as a markup.
                for (int x = 0; x < bmp.Width; x++)
                    for (int y = 0; y < bmp.Height; y++)
                        if (bmp.GetPixel(x,y).CompareColor(new int[] { 0, 0, 0 }, 35))
                            bmp.SetPixel(x, y, Color.Black);
                        else
                            bmp.SetPixel(x, y, Color.White);

                // The bitmap has a lot of extra area around it, crop the image around the letter.
                Bitmap final = CropImage(bmp, GetBounds(bmp));
                bmp.Dispose();
                // Don't use the bmp variable now!

                // Get the pixels that are outside the letter.
                // 'M' outside will get the pixels to the left and right of it,
                // 'P' will get every pixel except the ones inside the circle.
                List<Point> outside = GetOutside(final);
                // Now we mark the empty pixels inside the letter with dark red, signifying that there should not be a pixel there when scanning.
                // 'P' will have red pixels inside the circle. If a pixel is detected inside there when scanning the player's name, 
                // P will have less of a chance of being considered as the letter.
                for (int x = 0; x < final.Width; x++)
                    for (int y = 0; y < final.Height; y++)
                    {
                        // Don't put a red pixel next to black pixels so the scanning has some leeway.
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
                            final.SetPixel(x, y, Color.DarkRed); // Color the pixel red.
                    }

                // Get the vital pixels. These are pixels that must be found when scanning, 
                // if they are not detected then the letter being scanned has a 0% chance of being the letter it's checking.
                // It is determined by a blue pixel.
                for (int x = 1; x < final.Width - 1; x++)
                    for (int y = 0; y < final.Height; y++)
                        if (ContainsPixel(final, x, y) && ContainsPixel(final, x + 1, y) && ContainsPixel(final, x - 1, y))
                            final.SetPixel(x, y, Color.Blue);

                // Do manual adjustments to whatever letters need it.
                // This is required due to how similiar D and O look in the BigNoodleTooOblique font.
                if (letters[l] == 'D')
                    final.SetPixel(0, final.Height - 1, Color.Blue);

                // Get the length of the letter.
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

                generated[l] = final;
            }

            // Dispose of the font.
            pfc.Dispose();
            Marshal.FreeCoTaskMem(fontData);

            return new PlayerNameAlphabet(generated, letters, letterLengths);
        }

        private static byte[] GetFontResourceBytes(Assembly assembly, string fontResourceName)
        {
            var resourceStream = assembly.GetManifestResourceStream(fontResourceName);
            if (resourceStream == null)
                throw new Exception(string.Format("Unable to find font '{0}' in embedded resources.", fontResourceName));
            var bytes = new byte[resourceStream.Length];
            resourceStream.Read(bytes, 0, (int)resourceStream.Length);
            resourceStream.Close();
            return bytes;
        }

        private static bool ContainsPixel(Bitmap bmp, int x, int y)
        {
            return bmp.GetPixel(x, y) == Color.FromArgb(255, 0, 0, 0) || bmp.GetPixel(x, y) == Color.FromArgb(255, 0, 0, 255);
        }

        private static Rectangle GetBounds(Bitmap bmp)
        {
            // Get the bounds of the letter.

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

        private static Bitmap CropImage(Image img, Rectangle cropArea)
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

            // spread the seeds inside the bitmap.
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
                        outside.Add(checkPoint); // Added seeds will be iterated upon automatically in the for loop.
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
        /// <returns>The name of the player.</returns>
        public string GetPlayerName(int slot
#if DEBUG
#pragma warning disable 1573
            , string debugLetters = ""
#pragma warning restore 1573
#endif
            )
        {
            if (!IsSlotValid(slot))
                throw new InvalidSlotException(slot);

            using (LockHandler.Interactive)
            {
                // Open the career profile.
                bool careerProfileOpenSuccess = Interact.ClickOption(slot, Markups.VIEW_CAREER_PROFILE);
                if (!careerProfileOpenSuccess)
                    return null;
                Commands.WaitForCareerProfileToLoad();

                string name = GetPlayerName(
#if DEBUG
                    debugLetters
#endif
                    );

                GoBack(1);

                return name;
            }
        }

        /// <summary>
        /// Gets the name of the player when already in the career profile.
        /// </summary>
        /// <returns></returns>
        internal string GetPlayerName(
#if DEBUG
#pragma warning disable 1573
            string debugLetters = ""
#pragma warning restore 1573
#endif
            )
        {
#if DEBUG
            if (debugLetters == null)
                debugLetters = "";
#endif
            // Generate and cache the alphabet.
            if (EnglishAlphabet == null)
                EnglishAlphabet = GenerateAlphabet();

            int lineScanFade = 80; // The fade to detect a letter.
            int letterCheckFade = 80; // The fade to scan each pixel in the letter with.
            float cutoff = .92f; // The percent of pixels scanned a letter needs to be confirmed in order to be considered as the correct letter.
            int cy = 131; // The Y coordinate to scan at.
            int xStart = 131; // The X coordinate to start scanning at.
            int xEnd = 270; // The X coordinate to stop scanning at.
            int maxSinceLastLetter = 10; // The max number of pixels that need to past since the last detected letter to stop scanning.
            Tuple<char, char>[] prioritizeLetter = new Tuple<char, char>[] // Some characters look very similiar to eachother, this improves those conflicts.
            {
                new Tuple<char, char>('O', 'D'),
                new Tuple<char, char>('R', 'A'),
            };

            using (LockHandler.Interactive)
            {
                UpdateScreen();

                string playerName = string.Empty; // The name of the player.

                // Scan a line under the name in the career profile for text.
                for (int cx = xStart, sinceLastLetter = 0; cx < xEnd && sinceLastLetter < maxSinceLastLetter; cx++)
                {
                    if (Capture.CompareColor(cx, cy, Colors.WHITE, lineScanFade))
                    {
                        // A letter was detected.
                        sinceLastLetter = 0;
                        List<PlayerNameLetterResult> results = new List<PlayerNameLetterResult>(); // Stores the results of every letter scanned. The most likely letter will be used.
                        for (int ax = cx; ax <= cx + 1; ax++) // Scan this pixel and the next pixel.
                            for (int i = 0; i < EnglishAlphabet.Length; i++)
                            {
                                List<Point> filledPixels = new List<Point>(); // All detected pixels.
                                float total = 0; // The total number of pixels.
                                float match = 0; // The pixels that match the Capture and the letter markup.
                                bool failed = false;
                                for (int lx = 0; lx < EnglishAlphabet.Markups[i].Width && !failed; lx++)
                                    for (int ly = 0; ly < EnglishAlphabet.Markups[i].Height && !failed; ly++)
                                    {
                                        PixelType pixelType = GetPixelType(EnglishAlphabet.Markups[i], lx, ly);

                                        if (pixelType != PixelType.Any)
                                        {
                                            // px and py is the Capture's relative letter position.
                                            int px = ax + lx;
                                            int py = cy + ly + 2 - EnglishAlphabet.Markups[i].Height;
                                            bool pixelFilled = Capture.CompareColor(px, py, Colors.WHITE, letterCheckFade);

                                            // If a required pixel is missing, fail the letter.
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
                                    /*  Check if every pixel that was detected is connected. For example, || will return false, but H will return true.
                                        This makes letter scanning more certain for the BigNoodleTooOblique font because every pixel in the letters in that font is connected.
                                        If GetPlayerName() and GenerateAlphabet() is extended to other languages this may need to be removed because other languages use
                                        a different font whos characters are seperate; like 'i'. */
                                    bool isConnected = PointsAreConnected(filledPixels);
                                    float result = !isConnected ? 0 : match / total;

                                    // Add the result to the list.
                                    results.Add(new PlayerNameLetterResult(EnglishAlphabet.Letters[i], result, (int)total, (int)match, EnglishAlphabet.Markups[i], EnglishAlphabet.LetterLengths[i]));
                                    #region DEBUG
#if DEBUG
                                    if (DebugMenu != null)
                                    {
                                        Console.Write($"{EnglishAlphabet.Letters[i]} - R: ");
                                        if (result >= cutoff)
                                        {
                                            Console.BackgroundColor = ConsoleColor.DarkGreen;
                                            Console.Write($"{result}");
                                            Console.BackgroundColor = ConsoleColor.Black;
                                        }
                                        else
                                            Console.Write($"{result}");
                                        Console.WriteLine($" T: {total} - M: {match} - C: {isConnected}");
                                    }
#endif
                                    #endregion
                                }
                                #region DEBUG
#if DEBUG
                                else if (DebugMenu != null)
                                    Console.WriteLine($"{EnglishAlphabet.Letters[i]} vital pixel missing.");

                                if (DebugMenu != null)
                                {
                                    DebugMenu.InvalidateDebugImage();

                                    if (debugLetters.Contains(EnglishAlphabet.Letters[i]))
                                        Console.ReadLine();

                                    DebugMenu.ResetDebugImage();
                                }
#endif
                                #endregion
                            }

                        // Remove the results whos result is less than the cutoff or is null.
                        results = results.Where(result => result != null && result.Result >= cutoff).ToList();
                        /*  Get the most likely letter by the most matching pixels. OrderByDescending and ThenByDescending will order the list like so:
                                          | Matching first,        | Then percent.
                            A - Matching: 50, Total: 55 (50 / 55 = 90%)
                            B - Matching: 50, Total: 60 (60 / 50 = 83%)
                            C - Matching: 45, Total: 45 (45 / 45 = 100%) 
                            
                            Getting the highest match is important because if the letter in the player's name is E, then L and F will have 100%
                            result also because L and F have the same lines as E. If the letter in the player's name is F, E won't be considered 
                            because the lack of the bottom line brings it below the cutoff value. */
                        PlayerNameLetterResult mostLikely = results.OrderByDescending(result => result.Match).ThenByDescending(result => result.Result).FirstOrDefault();

                        // mostLikely will be null if no letters made it past the cutoff.
                        if (mostLikely != null)
                        {
                            // Some characters look very similiar to eachother, this improves those conflicts.
                            // The conflicts are manually set in the prioritizeLetter variable from earlier.
                            foreach (var prioritize in prioritizeLetter)
                                if (mostLikely.Letter == prioritize.Item1) // If the mostLikely letter should be prioritized to something else.
                                {
                                    int toCount = results.Count(r => r.Letter == prioritize.Item2); // The number of letters found in the results that the mostLikely's letter should be prioritized to.
                                    var bestTo = results.OrderByDescending(result => result.Match).FirstOrDefault(result => result.Letter == prioritize.Item2); // Get the letter to prioritize to.
                                    // If there are 2 occurences of the letter that mostLikely should be prioritized to,
                                    // or the prioritized letter has a better result percent than the mostLikely letter,
                                    // change the mostlikely letter to the prioritized letter.
                                    if (toCount == 2 || (bestTo != null && bestTo.Result > mostLikely.Result))
                                        mostLikely = bestTo;
                                }
                            #region DEBUG
#if DEBUG
                            if (DebugMenu != null)
                            {
                                Console.BackgroundColor = ConsoleColor.DarkBlue;
                                Console.WriteLine($"Letter detected: {mostLikely.Letter}");
                                Console.BackgroundColor = ConsoleColor.Black;
                            }
#endif
                            #endregion
                            // Add the letter scanned to the player name.
                            playerName += mostLikely.Letter;
                            /*  Increase the spot scanned on the X axis by the width of the letter scanned.
                                This is required so when scanning letters like H, it will start scanning again
                                on the right side of H. If it started scanning again where 'H' was detected,
                                it could incorrectly identify 'I' from the line on the right side of H. */
                            cx += mostLikely.LetterLength;
                        }
                    } // Color check
                    else
                        sinceLastLetter++;
                } // Line scan

                return playerName;
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
    internal class PlayerNameAlphabet : IDisposable
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
