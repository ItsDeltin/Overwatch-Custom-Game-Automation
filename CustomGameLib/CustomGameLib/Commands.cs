using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Diagnostics;

namespace Deltin.CustomGameAutomation
{
    partial class CustomGame
    {
        /// <summary>
        /// Chat commands for Overwatch.
        /// </summary>
        public Commands Commands;
    }
    /// <summary>
    /// Chat commands for Overwatch.
    /// </summary>
    /// <remarks>
    /// The Commands class is accessed in a CustomGame object on the <see cref="CustomGame.Commands"/> field.
    /// </remarks>
    /// <include file="docs.xml" path="doc/commands/example"></include>
    /// <seealso cref="CustomGame.Commands"/>
    /// <seealso cref="CustomGameAutomation.ListenTo"/>
    /// <seealso cref="CommandData"/>
    /// <seealso cref="PlayerIdentity"/>
    public class Commands : CustomGameBase
    {
        internal Commands(CustomGame cg) : base(cg)
        {
            ScanCommandsTask = new Task(() =>
            {
                ScanCommands();
            });
            ScanCommandsTask.Start();
        }

        /// <summary>
        /// Commands to listen to.
        /// </summary>
        public List<ListenTo> ListenTo = new List<ListenTo>();

        /// <summary>
        /// Set to true to start listening to commands. Set to false to stop.
        /// </summary>
        public bool Listen = false;

        #region Letters
        /*  Sorry to anyone who maintains this monstrocity in the future :)
            Each coordinate in the first argument represents the location of a pixel making the letters. For example, the letter C:
                
            -4□■■■□ | -4 ■■■ 
            -3■□□□■ | -3■   ■ 
            -2■□□□□ | -2■    
            -1■□□□■ | -1■   ■
             0□■■■□ |  0 ■■■  0,0 being the first black pixel on the first row (y=0).
            -10123     -10123
                
            ScanCommands() scans for letters at the 0 Y coordinate. When it hits a color of the chat, for example orange/tan for match chat, it will
            check for each letter in the "letters" array below. The most likely letter is chosen.
        */
        static internal Letter[] letters = new Letter[]
        {
            new Letter(new int[,] {{0,0},{1,-1},{2,-1},{3,-1},{4,0},{1,-2},{3,-3},{3,-2},{2,-4}}, 'A', 4), // uppercase A
            new Letter(new int[,] {{0,0},{1,0},{2,0},{3,0},{4,0},{0,-1},{4,-1},{0,-2},{1,-2},{2,-2},{3,-2},/*{4,-2},*/{0,-3},{3,-3},{0,-4},{1,-4},{2,-4},{3,-4}}, 'B', 4), // uppercase B
            new Letter(new int[,] {{0,0},{1,0},{2,0},{3,0},{3,-1},{3,-3},{3,-4},{2,-4},{1,-4},{-1,-1},{-1,-2},{-1,-3}}, 'C', 4, -1), // uppercase C
            new Letter(new int[,] {{0,0},{1,0},{2,0},{3,-1},{3,-3},{2,-4},{1,-4},{0,-4},{-1,-1},{-1,-2},{-1,-3}}, 'C', 4, -1), // uppercase C for open chat
            new Letter(new int[,] {{0,0},{1,0},{2,0},{3,0},{4,-1},{4,-2},{4,-3},{3,-4},{2,-4},{1,-4},{0,-4},{0,-3},{0,-2},{0,-1}}, 'D', 4), // uppercase D
            new Letter(new int[,] {{0,0},{1,0},{2,0},{3,0},{0,-1},{0,-2},{1,-2},{2,-2},{3,-2},{0,-3},{0,-4},{1,-4},{2,-4},{3,-4}}, 'E', 4), // uppercase E
            new Letter(new int[,] {{0,0},{0,-1},{0,-2},{1,-2},{2,-2},{3,-2},{0,-3},{0,-4},{1,-4},{2,-4},{3,-4}}, 'F', 3), // uppercase F
            new Letter(new int[,] {{0,0},{1,0},{2,0},{3,0},{4,0},{4,-1},{2,-2},{3,-2},{4,-2},{-1,-1},{-1,-2},{-1,-3},{0,-4},{1,-4},{2,-4},{3,-4}}, 'G', 4, -1), // uppercase G
            new Letter(new int[,] {{0,0},{0,-1},{0,-2},{0,-3},{0,-4},{1,-2},{2,-2},{3,-2},{4,-2},{4,0},{4,-1},{4,-3},{4,-4}}, 'H', 4), // uppercase H
            new Letter(new int[,] {{0,0},{0,-1},{0,-2},{0,-3},{0,-4}}, 'I', 1), // uppercase I
            new Letter(new int[,] {{0,0},{0,-1},{1,0},{2,0},{3,0},{3,-1},{3,-2},{3,-3},{3,-4}}, 'J', 4, 0, null, new int[,] {{0,-2},{0,-3}}), // uppercase J
            new Letter(new int[,] {{0,0},{0,-1},{0,-2},{0,-3},{0,-4},{1,-2},{2,-2},{2,-3},{2,-1},{3,-1},{3,0},{4,0},{3,-4}}, 'K', 4), // uppercase K
            new Letter(new int[,] {{0,0},{0,-1},{0,-2},{0,-3},{0,-4},{1,0},{2,0},{3,0}}, 'L', 3, 0, null, new int[,] {{3,-1},{3,-2},{3,-3},{3,-4}}), // uppercase L
            new Letter(new int[,] {{0,0},{0,-1},{0,-2},{0,-3},{0,-4},{1,-2},{1,-3},{1,-4},{2,0},{2,-1},{3,0},{3,-1},{4,-2},{4,-3},{4,-4},{5,0},{5,-1},{5,-2},{5,-3},{5,-4}}, 'M', 5), // uppercase M
            new Letter(new int[,] {{0,0},{0,-1},{0,-2},{0,-3},{0,-4},{1,-4},{1,-3},{2,-2},{3,-1},{3,0},{4,0},{4,-1},{4,-2},{4,-3},{4,-4}}, 'N', 4, 0, new int[] { 5, 9 }), // uppercase N
            new Letter(new int[,] {{0,0},{1,0},{2,0},{3,0},{-1,-1},{-1,-2},{-1,-3},{4,-1},{4,-2},{4,-3},{0,-4},{1,-4},{2,-4},{3,-4}}, 'O', 4, -1), // uppercase O
            new Letter(new int[,] {{0,0},{0,-1},{0,-2},{0,-3},{0,-4},{1,-2},{2,-2},{3,-2},{3,-3},{3,-4},{2,-4},{1,-4}}, 'P', 3, 0, new int[] {7}), // uppercase P
            new Letter(new int[,] {{0,0},{1,0},{2,0},{3,0},{4,0},{-1,-1},{-1,-2},{-1,-3},{4,-1},{4,-2},{4,-3},{0,-4},{1,-4},{2,-4},{3,-4}}, 'Q', 4, -1), // uppercase Q
            new Letter(new int[,] {{0,0},{0,-1},{0,-2},{0,-3},{0,-4},{1,-2},{2,-2},{3,-2},{4,-2},{4,0},{4,-1},{4,-3},{4,-4},{3,-4},{2,-4},{1,-4}}, 'R', 4), // uppercase R
            new Letter(new int[,] {{0,0},{1,0},{2,0},{3,0},{3,-1},{2,-2},{1,-2},{0,-2},{0,-3},{0,-4},{1,-4},{2,-4},{3,-3}}, 'S', 3), // uppercase S
            new Letter(new int[,] {{0,0},{1,0},{2,0},{3,-1},{0,-2},{1,-2},{2,-2},{3,-3},{0,-4},{1,-4},{2,-4}}, 'S', 3), // uppercase S for open chat
            new Letter(new int[,] {{0,0},{0,-1},{0,-2},{0,-3},{0,-4},{-1,-4},{-2,-4},{1,-4},{2,-4}}, 'T', 2, -2), // uppercase T
            new Letter(new int[,] {{0,0},{0,-1},{0,-2},{0,-3},{0,-4},{1,0},{2,0},{3,0},{4,0},{4,-1},{4,-2},{4,-3},{4,-4}}, 'U', 5, 0, new int[] {0, 8}, new int[,] {{1,-4},{2,-4},{3,-4}}), // uppercase U
            new Letter(new int[,] {{0,0},{1,0},{2,0},{3,0},{-1,-1},{-1,-2},{-1,-3},{-1,-4},{3,-1},{3,-2},{3,-3},{3,-4}}, 'U', 4, -1, new int[] {3}, new int[,] {{1,-4},{2,-4},{3,-4}}), // uppercase U
            new Letter(new int[,] {{0,0},{1,0},{2,0},{-1,-1},{-1,-2},{-1,-3},{-1,-4},{3,-1},{3,-2},{3,-3},{3,-4}}, 'U', 4, -1), // uppercase U for open chat
            new Letter(new int[,] {{0,0},{0,-1},{-1,-2},{-1,-3},{-2,-4},{1,0},{1,-1},{1,-2},{2,-3},{2,-4}}, 'V', 2, -2), // uppercase V
            new Letter(new int[,] {{0,0},{0,-1},{0,-2},{0,-3},{-1,-3},{-1,-4},{1,0},{1,-1},{1,-2},{2,-3},{2,-4},{3,-4},{3,-3},{3,-2},{3,-1},{4,-1},{4,0},{5,-1},{5,-2},{5,-3},{5,-4}}, 'W', 5, 0), // uppercase W
            new Letter(new int[,] {{0,0},{1,-1},{2,-2},{3,-3},{4,-4},{3,-1},{4,0},{1,-3}}, 'X', 4), // uppercase X
            new Letter(new int[,] {{0,0},{0,-1},{0,-2},{1,-3},{2,-4},{-1,-3},{-2,-4}}, 'Y', 2, -2), // uppercase Y
            new Letter(new int[,] {{0,0},{1,0},{2,0},{3,0},{1,-1},{2,-2},{3,-3},{4,-4},{3,-4},{2,-4},{1,-4},{0,-4}}, 'Z', 4), // uppercase Z

            // Numbers
            new Letter(new int[,] {{0,0},{1,0},{2,0},{3,0},{3,-1},{3,-2},{3,-3},{3,-4},{2,-4},{1,-4},{0,-4},{0,-3},{0,-2},{0,-1}}, '0', 3, 0, new int[] {3, 7, 10}, new int[,] {{2,-2}}), // Number 0
            new Letter(new int[,] {{0,0},{1,0},{2,0},{2,-1},{2,-2},{2,-3},{2,-4},{1,-4},{0,-4},{-1,-4},{-1,-3},{-1,-2},{-1,-1}}, '0', 3, -1, new int[] {2,6,9}), // Number 0
            new Letter(new int[,] {{0,0},{0,-1},{0,-2},{0,-3},{0,-4},{-1,-3}}, '1', 2), // Number 1
            new Letter(new int[,] {{0,0},{1,0},{2,0},{3,0},{1,-1},{2,-2},{3,-3},{3,-4},{2,-4},{1,-4},{0,-4},{0,-3}}, '2', 3), // Number 2
            new Letter(new int[,] {{0,-1},{0,0},{1,0},{2,0},{3,0},{3,-1},{3,-2},{2,-2},{3,-3},{3,-4},{2,-4},{1,-4},{0,-4},{0,-3}}, '3', 5), // Number 3
            new Letter(new int[,] {{0,0},{0,-1},{-1,-1},{-2,-1},{-2,-2},{-1,-3},{0,-2},{0,-3},{0,-4},{1,-1}}, '4', 1, -2), // Number 4
            new Letter(new int[,] {{0,-1},{0,0},{1,0},{2,0},{3,-1},{3,-2},{2,-3},{1,-3},{0,-3},{0,-4},{1,-4},{2,-4},{3,-4}}, '5', 4), // Number 5
            new Letter(new int[,] {{0,0},{1,0},{-1,-1},{-1,-2},{2,-1},{2,-2},{0,-3},{1,-3},{-1,-3},{0,-4},{1,-4}}, '6', 3), // Number 6
            new Letter(new int[,] {{0,0},{0,-1},{1,-2},{2,-3},{2,-4},{1,-4},{0,-4},{-1,-4}}, '7', 3), // Number 7
            new Letter(new int[,] {{0,0},{1,0},{2,0},{3,0},{0,-1},{3,-1},{0,-2},{1,-2},{2,-2},{3,-2},{0,-3},{1,-3},{3,-3},{0,-4},{1,-4},{2,-4},{3,-4}}, '8', 4), // Number 8
            new Letter(new int[,] {{0,0},{1,0},{2,0},{0,-1},{3,-1},{0,-2},{1,-2},{2,-2},{3,-2},{0,-3},{3,-3},{0,-4},{1,-4},{2,-4},{3,-4}}, '9', 3), // Number 9

            // Other
            new Letter(new int[,] {{0,1},{0,0},{0,-1},{0,-2},{0,-3},{0,-4},{-1,-4},{2,0},{2,-3}}, ']', 1, 0, null, new int[,] {{0,-5},{1,0},{1,-1},{1,-2},{1,-3},{1,-4}}), // End square bracket ]
            new Letter(new int[,] {{0,1},{0,0},{0,-1},{0,-2},{0,-3},{0,-4},{2,0}}, ']', 1, 0, null, new int[,] {{0,-5},{1,0},{1,-1},{1,-2},{1,-3},{1,-4}}), // End square bracket ] for open chat
            new Letter(new int[,] {{0,0},{1,0},{2,0},{3,0},{3,-1},{2,-2},{1,-2},{0,-3},{0,-4},{1,-4},{2,-4},{3,-4},{1,-3},{1,-1},{1,1}}, '$', 3, 0, new int[] { 0,-1 }), // $ symbol
        };
        #endregion

        Task ScanCommandsTask;

        static internal Rectangle CareerProfileShotArea = new Rectangle(46, 101, 265, 82);

        // Scale of debug images
        static int scale = 5;

        internal void StopScanning()
        {
            KeepScanning = false;
            ScanCommandsTask.Wait();
        }
        private bool KeepScanning = true;

        Bitmap PreviousChatMarkup = null;

        void ScanCommands()
        {
            var bmp = new Bitmap(Rectangles.LOBBY_CHATBOX.Width, Rectangles.LOBBY_CHATBOX.Height, PixelFormat.Format32bppArgb);

            Stopwatch toggle = new Stopwatch();
            if (cg.debugmode)
                toggle.Start();

            while (KeepScanning)
            {
                // Wait for listen to equal true
                System.Threading.Thread.Sleep(5);
                while (Listen == false) System.Threading.Thread.Sleep(5);

                lock (cg.CustomGameLock)
                {
                    updatescreen(ref bmp);

                    int[][] chatColors = Chat.ChatColors;
                    int chatFade = Chat.ChatFade;
                    Bitmap chatMarkup = bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), bmp.PixelFormat);
                    for (int x = 0; x < chatMarkup.Width; x++)
                        for (int y = 0; y < chatMarkup.Height; y++)
                        {
                            bool colorFound = false;
                            for (int i = 0; i < chatColors.Length; i++)
                                if (chatMarkup.CompareColor(x, y, chatColors[i], chatFade))
                                {
                                    colorFound = true;
                                    break;
                                }
                            if (colorFound)
                                chatMarkup.SetPixel(x, y, Color.Black);
                            else
                                chatMarkup.SetPixel(x, y, Color.White);
                        }
                    if (PreviousChatMarkup == null)
                        PreviousChatMarkup = chatMarkup;
                    else
                    {
                        if (CompareExecutors(PreviousChatMarkup, chatMarkup))
                        {
                            chatMarkup.Dispose();
                            continue;
                        }
                        else
                        {
                            PreviousChatMarkup.Dispose();
                            PreviousChatMarkup = chatMarkup;
                        }
                    }

                    // Scan the second line in the chat.
                    

                    string word = null;

                    var seed = GetSeed(bmp, 13);
                    var seedfade = GetSeedFade(bmp, 13);
                    LineScanResult linescan = ScanLine(bmp, 13, seed, seedfade);
                    if (linescan.Word.Contains("]"))
                    {
                        // If the first line contains ], scan the second line.
                        LineScanResult secondlinescan = ScanLine(bmp, 23, seed, seedfade);
                        word = linescan.Word + " " + secondlinescan.Word;
                        AddExecutedCommand(bmp, 13, linescan.NameLength, seed, seedfade, word);
                    }
                    else
                    {
                        // Scan the first line in chat.
                        seed = GetSeed(bmp, 24);
                        seedfade = GetSeedFade(bmp, 24);
                        linescan = ScanLine(bmp, 24, seed, seedfade);
                        if (linescan.Word.Contains("]"))
                        {
                            word = linescan.Word;
                            AddExecutedCommand(bmp, 24, linescan.NameLength, seed, seedfade, word);
                        }
                    }

                    ShowScan(bmp, seed, seedfade, word);
                }
            } // while

            bmp.Dispose();
        }

        void updatescreen(ref Bitmap bmp)
        {
            cg.updateScreen();
            if (bmp != null)
                bmp.Dispose();
            bmp = cg.BmpClone(Rectangles.LOBBY_CHATBOX);
        }

        void ShowScan(Bitmap bmp, int[] seed, int seedfade, string word)
        {
            // Show valid seed pixels in debug mode
            if (cg.debugmode)
            {
                Bitmap nb = bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), bmp.PixelFormat);

                for (int x = 0; x < nb.Width; x++)
                    for (int y = 0; y < nb.Height; y++)
                        if (nb.CompareColor(x, y, seed, seedfade))
                            nb.SetPixel(x, y, Color.Purple);

                cg.g.Clear(Color.White);
                cg.g.DrawImage(nb, new Rectangle(0, 0, nb.Width * scale, nb.Height * scale));
                cg.g.DrawString(word, new Font("Arial", 16), Brushes.Black, new PointF(0, (float)(nb.Height * scale * 1.1)));
                nb.Dispose();
            }
        }

        // Gets chat color
        int[] GetSeed(Bitmap bmp, int y)
        {
            var seedpix = bmp.GetPixelAt(0, y);
            return new int[] { seedpix.R, seedpix.G, seedpix.B };
        }

        // Gets chat color seed fade.
        int GetSeedFade(Bitmap bmp, int y)
        {
            var seedpix = bmp.GetPixelAt(0, y);
            var antipix = bmp.GetPixelAt(1, y);
            // Get seedfade by getting the average numbers of the RGB of the first pixel and the second pixel divided by 2.2 (2.5?). Default is 50 
            return (int)((((seedpix.R + seedpix.G + seedpix.B) / 3) + ((antipix.R + antipix.G + antipix.B) / 3)) / 2 / 2.2);
        }

        // Checks if an executed command should be added to the list of commands, then adds it.
        void AddExecutedCommand(Bitmap bmp, int y, int namelength, int[] seed, int seedfade, string word)
        {
            // Clean up the word. makes something like "] $APPLE " into "$APPLE"
            var wordtemp = word.Split(new char[] { ']' }, 2);
            if (wordtemp.Length > 1)
                word = wordtemp[1];
            word = word.Trim();

            string commandFirstWord = word.Split(' ')[0];
            ListenTo ltd = ListenTo.FirstOrDefault(v => v.Command == commandFirstWord);

            // See if command is being listened to. If it is, continue.
            if (word.Length > 0 && ltd != null && ltd.Listen)
            {
                PlayerIdentity pi = null;
                bool isFriend = false;

                // If it was not found, pi is still null. Register the profile if _registerPlayerProfiles is true.
                if (ltd.RegisterProfile || ltd.CheckIfFriend)
                {
                    Point openMenuAt = new Point(54, Rectangles.LOBBY_CHATBOX.Y + y);

                    // Open the chat
                    cg.Chat.OpenChat();

                    // Open the career profile
                    cg.RightClick(openMenuAt, 500);

                    // If the Send Friend Request option exists, they are not a friend.
                    isFriend = !(bool)cg.Interact.MenuOptionScan(openMenuAt, OptionScanFlags.ReturnFound, null, Markups.SEND_FRIEND_REQUEST);

                    if (ltd.RegisterProfile)
                    {
                        // By default, the career profile option is selected and we can just press enter to open it.
                        cg.KeyPress(Keys.Enter);

                        // Wait for the career profile to load.
                        WaitForCareerProfileToLoad();

                        // Take a screenshot of the career profile.
                        cg.updateScreen();
                        Bitmap careerProfileSnapshot = cg.BmpClone(Rectangles.LOBBY_CAREER_PROFILE);

                        // Register the player identity.
                        pi = new PlayerIdentity(careerProfileSnapshot);

                        // Go back to the lobby.
                        cg.GoBack(1);
                        //cg.//ResetMouse();

                        // If opening the career profile failed, the state of the chat could be incorrect, 
                        // like being wrongly opened or wrongly closed because of when enter was pressed earlier.
                        // This will fix it.
                        cg.Chat.OpenChat();
                        if (!cg.OpenChatIsDefault)
                            cg.KeyPress(Keys.Enter);
                    }
                    else
                        cg.CloseOptionMenu();
                }

                // Store executor noise data in a bitmap.
                var executorscan = new Rectangle(0, y - 4, namelength, 6);
                Bitmap executor = bmp.Clone(executorscan, bmp.PixelFormat);
                // Set name pixels to black and everything else to white
                for (int xi = 0; xi < executor.Width; xi++)
                    for (int yi = 0; yi < executor.Height; yi++)
                    {
                        if (executor.CompareColor(xi, yi, seed, seedfade))
                            executor.SetPixel(xi, yi, Color.Black);
                        else
                            executor.SetPixel(xi, yi, Color.White);
                    }

                ChatIdentity ci = new ChatIdentity(executor);

                CommandData commandData = new CommandData(word, GetChannelFromSeed(seed), pi, ci, isFriend);
                if (ltd?.Callback != null)
                    ltd.Callback.Invoke(commandData);

                System.Threading.Thread.Sleep(50);
            } // if command is being listened to
        }

        // Scans a chat line.
        LineScanResult ScanLine(Bitmap bmp, int y, int[] seed, int seedfade)
        {
            int namelength = 0; // Length of the name of the player that sent a chat message.
            bool namefound = false; // Determines if the name of the player that sent the chat message has been found.
            int space = 0; // Space in pixels between letters.
            string word = ""; // Text of chat message
            // For each pixel for the width of the chat message box.
            for (int i = 0; i < bmp.Width; i++)
            {
                // Test if pixel color is near seed color. if it is, scan for all the letters.
                if (bmp.CompareColor(i, y, seed, seedfade))
                {
                    var bestletter = CheckLetter(bmp, i, y, seed, seedfade); // Scan for letter.

                    // bestletter will equal null if no letters is possible.
                    if (bestletter != null)
                    {
                        // If space is 2 or higher, add a space to the word.
                        if (i + bestletter.least - space >= 2)
                            word += " ";
                        // Add the letter to the word
                        word += bestletter.letter;
                        // Increment i to letter length to prevent pixels being checked that don't need to be checked because a confirmed letter is there.
                        i += bestletter.length;
                        space = i + 1;
                        // If the bestletter is ] for the first time, then the name length in pixels has been found. 
                        if (bestletter.letter == ']' && namefound == false)
                        {
                            namefound = true;
                            namelength = i;
                        }
                    }
                }
            }
            return new LineScanResult(word, namelength);
        }

        // Checks for a chat letter at the input X and Y value.
        LetterResult CheckLetter(Bitmap bmp, int x, int y, int[] seed, int seedfade)
        {
            // Possible letters
            List<LetterResult> letterresult = new List<LetterResult>();
            // For each letter
            for (int li = 0; li < letters.Length; li++)
            {
                int totalpixels = 0;
                int successcount = 0;
                int optional = 0;
                // For each pixel in letter
                for (int pi = 0; pi < letters[li].pixel.GetLength(0); pi++)
                {
                    if (letters[li].optional == null || letters[li].optional.Contains(pi) == false)
                    {
                        totalpixels++;
                        // check if not out of bounds of BMP
                        if (x + letters[li].pixel[pi, 0] >= 0 && y + letters[li].pixel[pi, 1] >= 0 && x + letters[li].pixel[pi, 0] < Rectangles.LOBBY_CHATBOX.Width && y + letters[li].pixel[pi, 1] < Rectangles.LOBBY_CHATBOX.Height)
                        {
                            if (bmp.CompareColor(x + letters[li].pixel[pi, 0], y + letters[li].pixel[pi, 1], seed, seedfade))
                            {
                                successcount++;
                            }
                        }
                    }
                    // Check optional pixels
                    else if (x + letters[li].pixel[pi, 0] >= 0 && y + letters[li].pixel[pi, 1] >= 0 && x + letters[li].pixel[pi, 0] < Rectangles.LOBBY_CHATBOX.Width && y + letters[li].pixel[pi, 1] < Rectangles.LOBBY_CHATBOX.Height)
                    {
                        if (bmp.CompareColor(x + letters[li].pixel[pi, 0], y + letters[li].pixel[pi, 1], seed, seedfade))
                        {
                            optional++;
                        }
                    }
                }

                // Check for ignore. These are pixels that shouldn't equal the seed.
                if (letters[li].ignore != null)
                    for (int pi = 0; pi < letters[li].ignore.GetLength(0); pi++)
                        if (x + letters[li].ignore[pi, 0] > 0 && y + letters[li].ignore[pi, 1] > 0 && x + letters[li].ignore[pi, 0] < Rectangles.LOBBY_CHATBOX.Width && y + letters[li].ignore[pi, 1] < Rectangles.LOBBY_CHATBOX.Height)
                            if (bmp.CompareColor(x + letters[li].ignore[pi, 0], y + letters[li].ignore[pi, 1], seed, seedfade))
                                totalpixels++;

                // Get percent.
                double percent = Convert.ToDouble(successcount) / Convert.ToDouble(totalpixels) * 100;

                if (percent == 100)
                {
                    LetterResult connected = null;
                    // The letter L can connect to other letters. LM can be confused for U1. This checks for the letter that the L could be connected to.
                    if (letters[li].letter == 'L')
                        connected = CheckLetter(bmp, x + letters[li].length + 1, y, seed, seedfade);
                    letterresult.Add(new LetterResult(letters[li].letter, (int)percent, letters[li].pixel.GetLength(0), letters[li].length, letters[li].least, connected, optional)); // Add letter to possible letters.
                }
            }

            LetterResult bestletter = null; // The most likely letter that the letter found is.
            for (int bi = 0; bi < letterresult.Count; bi++)
            {
                if (bestletter == null)
                    bestletter = letterresult[bi];

                /*
                    * If letterresult[bi]'s pixel count is higher than bestletter's pixel count, make bestletter letterresult[bi]
                    * 
                    * Also, make sure LM/LN/LX/LL/LU isn't confused for something else.
                    */
                if (letterresult[bi].totalPixels + letterresult[bi].optional > bestletter.totalPixels + bestletter.optional)
                {
                    if (bestletter.connected != null)
                    {
                        // The letters L can connect to.
                        if ((bestletter.letter == 'L' && letterresult[bi].letter == 'U' && new char[] { 'M', 'N', 'X', 'L', 'U' }.Contains(bestletter.connected.letter)) == false)
                            bestletter = letterresult[bi];
                    }
                    else
                        bestletter = letterresult[bi];
                }
            }

            // Return the best possible letter.
            if (bestletter != null)
            {
                return bestletter;
            }
            return null;
        }

        private bool CompareExecutors(Bitmap e1, Bitmap e2)
        {
            if (e1.Width != e2.Width || e1.Height != e2.Height)
                return false;

            double identiclecount = 0; // number of pixels that are identicle.
            double count = 0;
            for (int xi = 0; xi < e1.Width; xi++)
                for (int yi = 0; yi < e1.Height; yi++)
                {
                    count++;
                    if (e1.GetPixelAt(xi, yi) == e2.GetPixelAt(xi, yi))
                        identiclecount++;
                }

            return (identiclecount / count) * 100 >= 95;
        }

        private Channel GetChannelFromSeed(int[] seed)
        {
            for (int i = 0; i < Chat.ChatColors.Length; i++)
                if (Math.Abs(Chat.ChatColors[i][0] - seed[0]) < Chat.ChatFade &&
                    Math.Abs(Chat.ChatColors[i][1] - seed[1]) < Chat.ChatFade &&
                    Math.Abs(Chat.ChatColors[i][2] - seed[2]) < Chat.ChatFade)
                {
                    return (Channel)i;
                }
            return Channel.General;
        }

        internal class Letter
        {
            public int[,] pixel;
            public char letter;
            public int length;
            public int least;
            public int[] optional;
            public int[,] ignore;

            public Letter(int[,] pixel, char letter, int length, int least = 0, int[] optional = null, int[,] ignore = null)
            {
                this.pixel = pixel;
                this.letter = letter;
                this.length = length;
                this.least = least;
                this.optional = optional;
                this.ignore = ignore;
            }
        }

        private class LetterResult
        {
            public char letter;
            public int percent;
            public int totalPixels;
            public int length;
            public int least;
            public LetterResult connected;
            public int optional;
            public LetterResult(char letter, int percent, int totalPixels, int length, int least, LetterResult connected, int optional)
            {
                this.letter = letter;
                this.percent = percent;
                this.totalPixels = totalPixels;
                this.length = length;
                this.least = least;
                this.connected = connected;
                this.optional = optional;
            }
        }

        private class LineScanResult
        {
            public string Word;
            public int NameLength;
            public LineScanResult(string word, int namelength)
            {
                Word = word;
                NameLength = namelength;
            }
        }

        /// <summary>
        /// Gets the player identity of a slot.
        /// </summary>
        /// <param name="slot">Slot to check.</param>
        /// <returns>The player identity of the slot.</returns>
        public PlayerIdentity GetSlotIdentity(int slot)
        {
            bool careerProfileOpenSuccess = cg.Interact.ClickOption(slot, Markups.VIEW_CAREER_PROFILE);
            if (!careerProfileOpenSuccess)
                return null;

            WaitForCareerProfileToLoad();

            cg.updateScreen();

            Bitmap careerProfile = cg.BmpClone(Rectangles.LOBBY_CAREER_PROFILE);

            cg.GoBack(1);

            //cg.//ResetMouse();

            return new PlayerIdentity(careerProfile);
        }

        internal void WaitForCareerProfileToLoad()
        {
            cg.WaitForColor(423, 164, new int[] { 85, 90, 107 }, 10, 5000);
            System.Threading.Thread.Sleep(250);
        }
    }

    /// <summary>
    /// Data for commands to listen to on the Commands class.
    /// </summary>
    /// <seealso cref="CommandData"/>
    public class ListenTo
    {
        /// <summary>
        /// Data for commands to listen to on the Commands class.
        /// </summary>
        /// <param name="command">Command to listen to.</param>
        /// <param name="listen">Should this command be listened to?</param>
        /// <param name="registerProfile">Should the player who executes this command have their player profile registered?</param>
        /// <param name="checkIfFriend">Should the player who executes this command be checked to see if they are a friend?</param>
        /// <param name="callback">Method to be executed when the command is executed.</param>
        public ListenTo(string command, bool listen, bool registerProfile, bool checkIfFriend, CommandExecuted callback)
        {
            for (int c = 0; c < command.Length; c++)
            {
                bool characterFound = false;

                for (int l = 0; l < Commands.letters.Length; l++)
                    if (command[c] == Commands.letters[l].letter)
                        characterFound = true;

                if (!characterFound)
                    throw new ArgumentException(string.Format("Letter '{0}' is not a valid letter to have in a command. The only valid letters is the uppercase english alphabet, numbers, and $.",
                        command[c].ToString()));
            }

            Command = command;
            Listen = listen;
            RegisterProfile = registerProfile;
            CheckIfFriend = checkIfFriend;
            Callback = callback;
        }

        /// <summary>
        /// Command to listen to.
        /// </summary>
        public string Command { get; internal set; }
        /// <summary>
        /// Should this command be listened to?
        /// </summary>
        public bool Listen;
        /// <summary>
        /// Should the player who executes this command have their profile registered?
        /// </summary>
        public bool RegisterProfile;
        /// <summary>
        /// Should the player who executes this command be checked to see if they are a friend?
        /// </summary>
        public bool CheckIfFriend;
        /// <summary>
        /// Method to be executed when the command is executed.
        /// </summary>
        public CommandExecuted Callback;
    }

    public class Identity : IDisposable
    {
        protected Identity(Bitmap identityMarkup)
        {
            IdentityMarkup = identityMarkup;
        }

        internal Bitmap IdentityMarkup;

        protected static bool CompareIdentities(Identity i1, Identity i2)
        {
            double total = 0;
            double success = 0;

            for (int x = 0; x < i1.IdentityMarkup.Width; x++)
                for (int y = 0; y < i1.IdentityMarkup.Height; y++)
                {
                    total++;
                    if (i1.IdentityMarkup.CompareColor(x, y, i2.IdentityMarkup.GetPixelAt(x, y).ToInt(), 50))
                        success++;
                }

            return (success / total) * 100 >= 90;
        }

        /// <summary>
        /// Disposes data used by the Identity object.
        /// </summary>
        public void Dispose()
        {
            if (!Disposed && IdentityMarkup != null)
                IdentityMarkup.Dispose();
        }
        private bool Disposed = false;
    }

    /// <summary>
    /// Contains data for identifying players who executed a command.
    /// </summary>
    public class PlayerIdentity : Identity
    {
        internal PlayerIdentity(Bitmap careerProfileMarkup) : base(careerProfileMarkup) { }

        /// <summary>
        /// Returns true if the 2 player identities are identicle.
        /// </summary>
        /// <param name="pi1">First player identity</param>
        /// <param name="pi2">Second player identity</param>
        /// <returns>Returns true if the player identities are identicle.</returns>
        public static bool ComparePlayerIdentities(PlayerIdentity pi1, PlayerIdentity pi2)
        {
            return CompareIdentities(pi1, pi2);
        }
        /// <summary>
        /// Returns true if the 2 player identities are identicle.
        /// </summary>
        /// <param name="other">The other PlayerIdentity to compare to.</param>
        /// <returns>Returns true if the player identities are identicle.</returns>
        public bool ComparePlayerIdentities(PlayerIdentity other)
        {
            return ComparePlayerIdentities(this, other);
        }
    }

    /// <summary>
    /// Contains data for identifying players who executed a command.
    /// </summary>
    public class ChatIdentity : Identity
    {
        internal ChatIdentity(Bitmap chatMarkup) : base(chatMarkup) { }

        /// <summary>
        /// Returns true if the 2 player identities are identicle.
        /// </summary>
        /// <param name="ci1">First player identity</param>
        /// <param name="ci2">Second player identity</param>
        /// <returns>Returns true if the player identities are identicle.</returns>
        public static bool CompareChatIdentities(ChatIdentity ci1, ChatIdentity ci2)
        {
            return CompareIdentities(ci1, ci2);
        }
        /// <summary>
        /// Returns true if the 2 player identities are identicle.
        /// </summary>
        /// <param name="other">The other PlayerIdentity to compare to.</param>
        /// <returns>Returns true if the player identities are identicle.</returns>
        public bool CompareChatIdentities(ChatIdentity other)
        {
            return CompareChatIdentities(this, other);
        }
    }

    /// <summary>
    /// Data of Overwatch executed chat commands.
    /// </summary>
    /// <seealso cref="ListenTo"/>
    public class CommandData
    {
        internal CommandData(string command, Channel channel, PlayerIdentity playerIdentity, ChatIdentity chatIdentity, bool isFriend)
        {
            Command = command;
            Channel = channel;
            PlayerIdentity = playerIdentity;
            ChatIdentity = chatIdentity;
            IsFriend = isFriend;
        }

        /// <summary>
        /// Command player executed.
        /// </summary>
        public string Command { get; internal set; }

        /// <summary>
        /// Channel the command was executed on.
        /// </summary>
        public Channel Channel { get; internal set; }

        /// <summary>
        /// The identity of the player that executed the command.
        /// </summary>
        public PlayerIdentity PlayerIdentity;

        /// <summary>
        /// The chat identity of the player that executed the command.
        /// </summary>
        public ChatIdentity ChatIdentity;

        /// <summary>
        /// True if the player that executed the command is a friend.
        /// </summary>
        public bool IsFriend { get; internal set; }
    }

    /// <summary>
    /// Method to be executed when the command is executed.
    /// </summary>
    /// <param name="data">Data of the command executed.</param>
    public delegate void CommandExecuted(CommandData data);
}
