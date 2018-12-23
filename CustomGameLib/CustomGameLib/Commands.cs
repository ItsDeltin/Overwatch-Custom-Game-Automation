using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        public Commands Commands { get; private set; }
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
    /// <seealso cref="CommandExecuted"/>
    /// <seealso cref="PlayerIdentity"/>
    /// <seealso cref="ChatIdentity"/>
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

        #region Constants
        #region Letters
        /*  Each coordinate in the first argument represents the location of a pixel making the letters. For example, the letter C:
                        V This row is -1 because the first pixel on y0 is a pixel ahead
            -4□■■■□ | -4 ■■■ 
            -3■□□□■ | -3■   ■ 
            -2■□□□□ | -2■    
            -1■□□□■ | -1■   ■
             0□■■■□ |  0 ■■■  0,0 being the first pixel on the first row (y=0).
            -10123     -10123
                
            ScanCommands() scans for letters at the y0 coordinate. 
            When it gets a color of the chat it will check for each letter in the "letters" array below. 
            The most likely letter is chosen.
        */
        static internal readonly Letter[] letters = new Letter[]
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
            new Letter(new int[,] {{0,0},{0,-1},{-1,-2},{-1,-3},{-2,-4},{1,0},{1,-1},{1,-2},{2,-3},{2,-4}}, 'V', 2, -1), // uppercase V
            new Letter(new int[,] {{0,0},{0,-1},{0,-2},{0,-3},{-1,-3},{-1,-4},{1,0},{1,-1},{1,-2},{2,-3},{2,-4},{3,-4},{3,-3},{3,-2},{3,-1},{4,-1},{4,0},{5,-1},{5,-2},{5,-3},{5,-4}}, 'W', 5, 0), // uppercase W
            new Letter(new int[,] {{0,0},{1,-1},{2,-2},{3,-3},{4,-4},{3,-1},{4,0},{1,-3}}, 'X', 4), // uppercase X
            new Letter(new int[,] {{0,0},{0,-1},{0,-2},{1,-3},{2,-4},{-1,-3},{-2,-4}}, 'Y', 2, -2), // uppercase Y
            new Letter(new int[,] {{0,0},{1,0},{2,0},{3,0},{1,-1},{2,-2},{3,-3},{4,-4},{3,-4},{2,-4},{1,-4},{0,-4}}, 'Z', 4), // uppercase Z

            // Numbers
            new Letter(new int[,] {{0,0},{1,0},{2,0},{3,0},{3,-1},{3,-2},{3,-3},{3,-4},{2,-4},{1,-4},{0,-4},{0,-3},{0,-2},{0,-1}}, '0', 3, 0, null, new int[,] {{1,-3},{2,-3}}), // Number 0
            new Letter(new int[,] {{0,0},{1,0},{2,0},{2,-1},{2,-2},{2,-3},{2,-4},{1,-4},{0,-4},{-1,-4},{-1,-3},{-1,-2},{-1,-1}}, '0', 3, -1, new int[] {2,6,9}, new int[,] {{0,-3},{1,-3}}), // Number 0
            new Letter(new int[,] {{0,0},{0,-1},{0,-2},{0,-3},{0,-4},{-1,-3}}, '1', 2), // Number 1
            new Letter(new int[,] {{0,0},{1,0},{2,0},{3,0},{1,-1},{2,-2},{3,-3},{3,-4},{2,-4},{1,-4},{0,-4},{0,-3}}, '2', 3), // Number 2
            new Letter(new int[,] {{0,-1},{0,0},{1,0},{2,0},{3,0},{3,-1},{3,-2},{2,-2},{3,-3},{3,-4},{2,-4},{1,-4},{0,-4},{0,-3}}, '3', 4), // Number 3
            new Letter(new int[,] {{0,0},{0,-1},{-1,-1},{-2,-1},{-2,-2},{-1,-3},{0,-2},{0,-3},{0,-4},{1,-1}}, '4', 1, -2), // Number 4
            new Letter(new int[,] {{0,-1},{0,0},{1,0},{2,0},{3,-1},{3,-2},{2,-3},{1,-3},{0,-3},{0,-4},{1,-4},{2,-4},{3,-4}}, '5', 3), // Number 5
            new Letter(new int[,] {{0,0},{1,0},{-1,-1},{-1,-2},{2,-1},{2,-2},{0,-3},{1,-3},{-1,-3},{0,-4},{1,-4}}, '6', 3), // Number 6
            new Letter(new int[,] {{-1,-3},{-1,-2},{-1,-1},{0,-4},{0,-3},{0,0},{1,-4},{1,-3},{1,0},{2,-4},{2,-3},{2,-2},{2,-1}}, '6', 2, -1),
            new Letter(new int[,] {{0,0},{0,-1},{1,-2},{2,-3},{2,-4},{1,-4},{0,-4},{-1,-4}}, '7', 2, -1), // Number 7
            new Letter(new int[,] {{0,0},{1,0},{2,0},{3,0},{0,-1},{3,-1},{0,-2},{1,-2},{2,-2},{3,-2},{0,-3},{1,-3},{3,-3},{0,-4},{1,-4},{2,-4},{3,-4}}, '8', 3), // Number 8
            new Letter(new int[,] {{0,-3},{0,-1},{0,0},{1,-4},{1,-3},{1,-2},{1,0},{2,-4},{2,-2},{2,0},{3,-3},{3,-1},{3,0}}, '8', 3, 0),
            new Letter(new int[,] {{0,-3},{0,-1},{0,0},{1,-4},{1,-3},{1,-2},{1,0},{2,-4},{2,-2},{2,0},{3,-3},{3,-2},{3,-1},{3,0}}, '8', 3, 0),
            new Letter(new int[,] {{0,0},{1,0},{2,0},{0,-1},{3,-1},{0,-2},{1,-2},{2,-2},{3,-2},{0,-3},{3,-3},{0,-4},{1,-4},{2,-4},{3,-4}}, '9', 3), // Number 9
            new Letter(new int[,] {{-1,-4},{-1,-3},{-1,-1},{0,-4},{0,-2},{0,0},{1,-4},{1,-2},{1,0},{2,-3},{2,-2},{2,-1}}, '9', 3, -1),

            // Other
            new Letter(new int[,] {{0,1},{0,0},{0,-1},{0,-2},{0,-3},{0,-4},{-1,-4}/*,{2,0},{2,-3}*/}, ']', 1, 0, null, new int[,] {{0,-5},{1,0},{1,-1},{1,-2},{1,-3},{1,-4}}), // End square bracket ]
            new Letter(new int[,] {{0,1},{0,0},{0,-1},{0,-2},{0,-3},{0,-4},/*{2,0}*/}, ']', 1, 0, null, new int[,] {{0,-5},{1,0},{1,-1},{1,-2},{1,-3},{1,-4}}), // End square bracket ] for open chat
            new Letter(new int[,] {{0,0},{1,0},{2,0},{3,0},{3,-1},{2,-2},{1,-2},{0,-3},{0,-4},{1,-4},{2,-4},{3,-4},{1,-3},{1,-1},{1,1}}, '$', 3, 0, new int[] { 0,-1 }), // $ symbol
        };
        #endregion

        private static readonly char[] TouchingCharacters = new char[] { 'M', 'N', 'X', 'L', 'U' };

        private const int MarkerX = 50; // The X location of the chat marker.
        private const int TextStart = 54; // The X location that the chat text starts.
        private const int ChatLength = 200; // The amount of pixels to scan after TextStart (preferably the width of the chatbox)

        // As of Winter Wonderland 2018, chat messages are preceeded by dots.
        // Marker determines the Y location of the dot, which gets moved lower if it is a multiline chat message.
        // The preceding values determine each Y coordinate of the lines in the chat message.
        private static readonly LineInfo[] lineInfo = new LineInfo[]
        {
            // One line command
            new LineInfo(marker:483,
                485),
            // Two line command
            new LineInfo(marker:472,
                474, 484),
            // Three line command
            new LineInfo(marker:463,
                465, 475, 485),
            // Four line command
            new LineInfo(marker:453,
                455, 465, 475, 485)
        };
        #endregion

        /// <summary>
        /// Commands to listen to.
        /// </summary>
        public List<ListenTo> ListenTo
        {
            get
            {
                lock (ListenToAccessLock)
                    return _listenTo;
            }
            set
            {
                lock (ListenToAccessLock)
                    _listenTo = value;
            }
        }
        private List<ListenTo> _listenTo = new List<ListenTo>();
        private readonly object ListenToAccessLock = new object();
        /// <summary>
        /// Set to true to start listening to commands. Set to false to stop.
        /// </summary>
        public bool Listen { get; set; }

        private Task ScanCommandsTask;
        private bool KeepScanning = true;
        private DirectBitmap PreviousChatMarkup = null;

        internal void StopScanning()
        {
            KeepScanning = false;
        }

        private void ScanCommands()
        {
            while (KeepScanning)
            {
                // Wait for listen to equal true
                Thread.Sleep(5);
                if (!Listen || ListenTo.Count == 0)
                    continue;

                try
                {
                    using (cg.LockHandler.SemiInteractive)
                    {
                        cg.UpdateScreen();

                        // Check if the chat updated
                        #region Check For Chat Update
                        DirectBitmap chatMarkup = Capture.Clone(Rectangles.LOBBY_CHATBOX);

                        if (PreviousChatMarkup == null)
                            PreviousChatMarkup = chatMarkup;
                        else
                        {
                            if (PreviousChatMarkup.CompareTo(chatMarkup, 5, 98, DBCompareFlags.Multithread))
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
                        #endregion

                        foreach (LineInfo line in lineInfo)
                            foreach (int[] color in Chat.ChatColors)
                                if (Capture.CompareColor(MarkerX, line.Marker, color, Chat.ChatFade))
                                {
                                    string command = "";
                                    int nameLength = -1;
#if DEBUG
                                    List<LetterResult> letterInfos = new List<LetterResult>();
#endif

                                    for (int i = 0; i < line.Lines.Length; i++)
                                    {
                                        LineScanResult linescan = ScanLine(TextStart, ChatLength, line.Lines[i], color);

                                        command += linescan.Word;

#if DEBUG
                                        letterInfos.AddRange(linescan.LetterInfos);
#endif

                                        if (i == 0)
                                            nameLength = linescan.NameLength;
                                    }
                                    
                                    AddExecutedCommand(line.Lines[0], nameLength, color, command
#if DEBUG
                                        , letterInfos
#endif
                                        );

                                    break;
                                }
                    }
                }
                catch (OverwatchClosedException) { }
            } // while

            // Dispose of resources used by this class.
            if (PreviousChatMarkup != null)
                PreviousChatMarkup.Dispose();
        }

        // Scans a chat line.
        private LineScanResult ScanLine(int xStart, int length, int y, int[] color)
        {
            int namelength = -1; // Length of the name of the player that sent a chat message.
            int space = 0; // Space in pixels between letters.
            string word = ""; // Text of chat message
#if DEBUG
            List<LetterResult> letterInfos = new List<LetterResult>();
#endif
            // For each pixel for the width of the chat message box.
            for (int x = xStart; x < xStart + length; x++)
            {
                // Test if pixel color is near the chat color. if it is, scan for all the letters.
                if (Capture.CompareColor(x, y, color, Chat.ChatFade))
                {
                    var bestletter = CheckLetter(x, y, color); // Scan for letter.

                    // bestletter will equal null if no letters is possible.
                    if (bestletter != null)
                    {
                        // If space is 2 or higher, add a space to the word.
                        if (x + bestletter.Letter.Least - space >= 2)
                        {
                            word += " ";
#if DEBUG
                            letterInfos.Add(null);
#endif
                        }
                        // Add the letter to the word
                        word += bestletter.Letter.Char;
#if DEBUG
                        letterInfos.Add(bestletter);
#endif
                        // Increment i to letter length to prevent pixels being checked that don't need to be checked because a confirmed letter is there.
                        x += bestletter.Letter.Length;
                        space = x + 1;
                        // If the bestletter is ] for the first time, then the name length in pixels has been found. 
                        if (bestletter.Letter.Char == ']' && namelength == -1)
                            namelength = x;
                    }
                }
            }
            return new LineScanResult(word, namelength)
#if DEBUG
            {
                LetterInfos = letterInfos
            }
#endif
                ;
        }

        // Checks for a chat letter at the input X and Y value.
        private LetterResult CheckLetter(int x, int y, int[] color)
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
                for (int pi = 0; pi < letters[li].Pixel.GetLength(0); pi++)
                {
                    int checkX = x + letters[li].Pixel[pi, 0],
                        checkY = y + letters[li].Pixel[pi, 1];

                    if (letters[li].Optional == null || letters[li].Optional.Contains(pi) == false)
                    {
                        totalpixels++;
                        if (Capture.CompareColor(checkX, checkY, color, Chat.ChatFade))
                            successcount++;
                    }
                    // Check optional pixels
                    else if (Capture.CompareColor(checkX, checkY, color, Chat.ChatFade))
                        optional++;
                }

                // Check for ignore. These are pixels that shouldn't equal the seed.
                if (letters[li].Ignore != null)
                    for (int pi = 0; pi < letters[li].Ignore.GetLength(0); pi++)
                    {
                        int checkX = x + letters[li].Ignore[pi, 0],
                            checkY = y + letters[li].Ignore[pi, 1];
                        if (Capture.CompareColor(checkX, checkY, color, Chat.ChatFade))
                        {
                            totalpixels++;
                        }
                    }

                float result = (float)successcount / totalpixels;

                if (result == 1)
                {
                    LetterResult connected = null;
                    // The letter L can connect to other letters. LM can be confused for U1. This checks for the letter that the L could be connected to.
                    if (letters[li].Char == 'L')
                        connected = CheckLetter(x + letters[li].Length + 1, y, color);
                    letterresult.Add(new LetterResult(letters[li], letters[li].Pixel.GetLength(0) + optional, connected)
#if DEBUG
                    {
                        Location = new Point(x, y)
                    }
#endif
                        ); // Add letter to possible letters.
                }
            }

            LetterResult bestletter = null; // The most likely letter that the letter found is.
            for (int bi = 0; bi < letterresult.Count; bi++)
            {
                if (bestletter == null)
                    bestletter = letterresult[bi];

                // If letterresult[bi]'s pixel count is higher than bestletter's pixel count, make bestletter letterresult[bi] 
                // Also, make sure LM/LN/LX/LL/LU isn't confused for something else.
                if (letterresult[bi].TotalPixels > bestletter.TotalPixels)
                {
                    if (bestletter.Connected != null)
                    {
                        // M, N, X, L, U are the letters L can connect to.
                        if ((bestletter.Letter.Char == 'L' && letterresult[bi].Letter.Char == 'U' && TouchingCharacters.Contains(bestletter.Connected.Letter.Char)) == false)
                            bestletter = letterresult[bi];
                    }
                    else
                        bestletter = letterresult[bi];
                }
            }

            return bestletter;
        }

        // Checks if an executed command should be added to the list of commands, then adds it.
        private string AddExecutedCommand(int y, int namelength, int[] seed, string command
#if DEBUG
            , List<LetterResult> letterInfos
#endif
            )
        {
            #region Command Cleanup
            ListenTo ltd = null;

            int nameSeperator = command.IndexOf(']') + 2;

            if (nameSeperator == 1 || nameSeperator > command.Length)
                return command;

            command = command.Substring(nameSeperator)
                .Trim();
            string firstWord = command.Split(' ')[0];

#if DEBUG
            letterInfos.RemoveRange(0, nameSeperator);
            cg.DebugMenu.ShowScan(letterInfos);
#endif

            lock (ListenToAccessLock)
                foreach (ListenTo listenData in ListenTo)
                    if (listenData.Command == firstWord)
                    {
                        ltd = listenData;
                        break;
                    }

            if (ltd == null || !ltd.Listen)
                return command;
            #endregion

            #region Chat Identity
            // Store executor noise data in a bitmap.
            var executorscan = new Rectangle(0, y - 4, namelength, 6);
            DirectBitmap executor = Capture.Clone(executorscan);
            // Set name pixels to black and everything else to white
            for (int xi = 0; xi < executor.Width; xi++)
                for (int yi = 0; yi < executor.Height; yi++)
                {
                    if (executor.CompareColor(xi, yi, seed, Chat.ChatFade))
                        executor.SetPixel(xi, yi, Color.Black);
                    else
                        executor.SetPixel(xi, yi, Color.White);
                }

            ChatIdentity ci = new ChatIdentity(executor);
            #endregion

            #region Profile and Friend Check
            PlayerIdentity pi = null;
            bool isFriend = false;

            // If it was not found, pi is still null. Register the profile if _registerPlayerProfiles is true.
            if (ltd.RegisterProfile || ltd.CheckIfFriend)
            {
                Point openMenuAt = new Point(56, y);

                // Open the chat
                cg.Chat.OpenChat();

                // Open the career profile
                cg.RightClick(openMenuAt, Timing.OPTION_MENU);

                // If the Send Friend Request option exists, they are not a friend.
                isFriend = !(bool)cg.Interact.MenuOptionScan(openMenuAt, OptionScanFlags.ReturnFound, null, Markups.SEND_FRIEND_REQUEST);

                if (ltd.RegisterProfile)
                {
                    using (cg.LockHandler.Interactive)
                    {
                        // By default, the career profile option is selected and we can just press enter to open it.
                        cg.KeyPress(Keys.Enter);

                        // Wait for the career profile to load.
                        WaitForCareerProfileToLoad();

                        // Take a screenshot of the career profile.
                        cg.UpdateScreen();
                        DirectBitmap careerProfileSnapshot = Capture.Clone(Rectangles.LOBBY_CAREER_PROFILE);

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
                }
                else
                    cg.Interact.CloseOptionMenu();
            }
            #endregion

            CommandData commandData = new CommandData(command, GetChannelFromSeed(seed), pi, ci, isFriend);
            if (ltd.Callback != null)
                ltd.Callback.Invoke(commandData);
            else
            {
                commandData.ChatIdentity.Dispose();
                commandData.PlayerIdentity?.Dispose();
            }

            return command;
        }

        private Channel GetChannelFromSeed(int[] seed)
        {
            for (int i = 0; i < Chat.ChatColors.Length; i++)
                if (Chat.ChatColors[i] == seed)
                    return (Channel)i;
            return Channel.Match;
        }

        internal class Letter
        {
            public Letter(int[,] pixel, char letter, int length, int least = 0, int[] optional = null, int[,] ignore = null)
            {
                Pixel = pixel;
                Char = letter;
                Length = length;
                Least = least;
                Optional = optional;
                Ignore = ignore;
            }
            public int[,] Pixel { get; private set; }
            public char Char { get; private set; }
            public int Length { get; private set; }
            public int Least { get; private set; }
            public int[] Optional { get; private set; }
            public int[,] Ignore { get; private set; }
        }

        internal class LetterResult
        {
            public LetterResult(Letter letter, int totalPixels, LetterResult connected)
            {
                Letter = letter;
                TotalPixels = totalPixels;
                Connected = connected;
            }
            public Letter Letter { get; private set; }
            public int TotalPixels { get; private set; }
            public LetterResult Connected { get; private set; }
#if DEBUG
            public Point Location { get; set; }
#endif
        }

        private class LineScanResult
        {
            public LineScanResult(string word, int namelength)
            {
                Word = word;
                NameLength = namelength;
            }
            public string Word { get; private set; }
            public int NameLength { get; private set; }
#if DEBUG
            public List<LetterResult> LetterInfos { get; set; }
#endif
        }

        private class LineInfo
        {
            // As of Winter Wonderland 2018, chat messages are preceded by a dot. Marker determines the Y location of the dot.
            public LineInfo(int marker, params int[] lines)
            {
                if (lines.Length == 0)
                    throw new Exception("There must be at least one line.");

                Marker = marker;
                Lines = lines;
            }

            public int Marker { get; private set; }
            public int[] Lines { get; private set; }
        }

        /// <summary>
        /// Gets the player identity of a slot.
        /// </summary>
        /// <param name="slot">Slot to check.</param>
        /// <returns>The player identity of the slot.</returns>
        public PlayerIdentity GetPlayerIdentity(int slot)
        {
            using (cg.LockHandler.Interactive)
            {
                bool careerProfileOpenSuccess = cg.Interact.ClickOption(slot, Markups.VIEW_CAREER_PROFILE);
                if (!careerProfileOpenSuccess)
                    return null;

                WaitForCareerProfileToLoad();

                cg.UpdateScreen();

                DirectBitmap careerProfile = Capture.Clone(Rectangles.LOBBY_CAREER_PROFILE);

                cg.GoBack(1);

                Thread.Sleep(500);

                return new PlayerIdentity(careerProfile);
            }
        }

        internal void WaitForCareerProfileToLoad()
        {
            cg.WaitForColor(345, 164, new int[] { 85, 91, 108 }, 5, 10000);
            System.Threading.Thread.Sleep(250);
        }
    }

    /// <summary>
    /// Data for commands to listen to on the Commands class.
    /// </summary>
    /// <seealso cref="CommandData"/>
    /// <seealso cref="Commands"/>
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
                    if (command[c] == Commands.letters[l].Char)
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
        /// Data for commands to listen to on the Commands class.
        /// </summary>
        /// <param name="command">Command to listen to.</param>
        /// <param name="callback">Method to be executed when the command is executed.</param>
        public ListenTo(string command, CommandExecuted callback) : this(command, true, false, false, callback) { }

        /// <summary>
        /// Command to listen to.
        /// </summary>
        public string Command { get; private set; }
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

#pragma warning disable CS1591
    public abstract class Identity : IDisposable
    {
        internal Identity(DirectBitmap identityMarkup)
        {
            IdentityMarkup = identityMarkup;
        }

        internal DirectBitmap IdentityMarkup;

        protected internal static bool CompareIdentities(Identity i1, Identity i2, int percentMatches = 90, int fade = 50)
        {
            if (i1.IdentityMarkup.Width != i2.IdentityMarkup.Width || i1.IdentityMarkup.Height != i2.IdentityMarkup.Height)
                return false;

            return i1.IdentityMarkup.CompareTo(i2.IdentityMarkup, fade, percentMatches, DBCompareFlags.Multithread);
        }

        /// <summary>
        /// Disposes data used by the Identity object.
        /// </summary>
        public void Dispose()
        {
            Disposed = true;
            if (!Disposed && IdentityMarkup != null)
                IdentityMarkup.Dispose();
        }
        private bool Disposed = false;
    }
#pragma warning restore CS1591


    /// <summary>
    /// Contains data for identifying players who executed a command.
    /// </summary>
    public class PlayerIdentity : Identity
    {
        internal PlayerIdentity(DirectBitmap careerProfileMarkup) : base(careerProfileMarkup) { }

        /// <summary>
        /// Compares player identities.
        /// </summary>
        /// <param name="other">The other PlayerIdentity to compare to.</param>
        /// <returns>Returns true if the player identities are equal.</returns>
        public bool CompareIdentities(PlayerIdentity other)
        {
            return CompareIdentities(this, other);
        }
    }

    /// <summary>
    /// Contains data for identifying players who executed a command.
    /// </summary>
    public class ChatIdentity : Identity
    {
        internal ChatIdentity(DirectBitmap chatMarkup) : base(chatMarkup) { }

        /// <summary>
        /// Compares chat identity.
        /// </summary>
        /// <param name="other">The other ChatIdentity to compare to.</param>
        /// <returns>Returns true if the chat identities are equal.</returns>
        public bool CompareIdentities(ChatIdentity other)
        {
            return CompareIdentities(this, other);
        }
    }

    /// <summary>
    /// Data of an executed chat command.
    /// </summary>
    /// <seealso cref="ListenTo"/>
    /// /// <seealso cref="Commands"/>
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
        public string Command { get; private set; }

        /// <summary>
        /// Channel the command was executed on.
        /// </summary>
        public Channel Channel { get; private set; }

        /// <summary>
        /// The identity of the player that executed the command.
        /// </summary>
        public PlayerIdentity PlayerIdentity { get; private set; }

        /// <summary>
        /// The chat identity of the player that executed the command.
        /// </summary>
        public ChatIdentity ChatIdentity { get; private set; }

        /// <summary>
        /// Is the player that executed the command a friend?
        /// </summary>
        public bool IsFriend { get; private set; }
    }

    /// <summary>
    /// Method to be executed when a command is executed.
    /// </summary>
    /// <param name="data">Data of the command executed.</param>
    /// <seealso cref="CommandData"/>
    /// <seealso cref="Commands"/>
    public delegate void CommandExecuted(CommandData data);
}
