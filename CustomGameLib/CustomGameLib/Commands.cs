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
        public Commands Command;
        public class Commands
        {
            private CustomGame cg;
            internal Commands(CustomGame cg)
            {
                this.cg = cg;
                ScanCommandsTask = new Task(() =>
                {
                    ScanCommands();
                });
                ScanCommandsTask.Start();
            }

            /// <summary>
            /// <para>Indexer: Command to listen to.</para>
            /// <para>Value: If true, listen to this command.</para>
            /// </summary>
            public Dictionary<string, bool> ListenTo = new Dictionary<string, bool>();

            /// <summary>
            /// List of executed commands.
            /// </summary>
            public ReadOnlyCollection<CommandData> ExecutedCommands
            {
                get
                {
                    return _executedCommands.AsReadOnly();
                }
            }

            List<CommandData> _executedCommands = new List<CommandData>();

            /// <summary>
            /// Removes an executed command from the executed commands list.
            /// </summary>
            /// <param name="index">Command to remove by index.</param>
            public void RemoveExecutedCommand(int index)
            {
                _executedCommands[index].executor.Dispose();
                _executedCommands.RemoveAt(index);
            }

            /// <summary>
            /// Removes all executed commands.
            /// </summary>
            public void RemoveAllExecutedCommands()
            {
                for (int i = 0; i < _executedCommands.Count; i++)
                    _executedCommands[i].executor.Dispose();
                _executedCommands = new List<CommandData>();
            }

            /// <summary>
            /// Set to true to start listening to commands. Set to false to stop.
            /// </summary>
            public bool Listen = false;

            /// <summary>
            /// Listens to all commands rather than just the commands in the ListenTo dictionary.
            /// </summary>
            public bool ListenToAllCommands = false;
            
            /// <summary>
            /// Allows one executor to execute more than one command.
            /// </summary>
            public bool AllowMultipleCommandExecutions = false;

            public bool SameExecutorCommandUpdate = false;

            public class CommandData
            {
                /// <summary>
                /// Command player executed.
                /// </summary>
                public string command;
                /// <summary>
                /// Noise data of executor.
                /// </summary>
                public Bitmap executor;
                /// <summary>
                /// Channel the command was executed on.
                /// </summary>
                public Channel channel;
                public CommandData(string command, Bitmap executor, Channel channel)
                {
                    this.command = command;
                    this.executor = executor;
                    this.channel = channel;
                }
            }

            #region Letters
            /*  Sorry to anyone who maintains this monstrocity in the future :)
                Each coordinate in the first argument represents the location of a pixel making the letters. For example, the letter C:
                
               -4□■■■□
               -3■□□□■
               -2■□□□□
               -1■□□□■
                0□■■■□
                -10123
                
                ScanCommands() scans for letters at the 0 Y coordinate. When it hits a color of the chat, for example orange/tan for match chat, it will
                check for each letter in the "letters" array below. The most likely letter is chosen.
            */
            static Letter[] letters = new Letter[]
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

            #region Scan

            // Area of text
            static Rectangle shotarea = new Rectangle(50, 461, 169, 26); // Location on screen for chat

            // Scale of debug images
            static int scale = 5;

            internal void StopScanning()
            {
                KeepScanning = false;
            }
            private bool KeepScanning = true;

            void ScanCommands()
            {
                var bmp = new Bitmap(shotarea.Width, shotarea.Height, PixelFormat.Format32bppArgb);

                Stopwatch toggle = new Stopwatch();
                if (cg.debugmode)
                    toggle.Start();

                while (KeepScanning)
                {
                    // Wait for listen to equal true
                    System.Threading.Thread.Sleep(10);
                    while (Listen == false) System.Threading.Thread.Sleep(1000);

                    updatescreen(ref bmp);

                    // Word result
                    string word = "";

                    // Scan the second line in the chat.
                    var seed = GetSeed(bmp, 13);
                    var seedfade = GetSeedFade(bmp, 13);
                    LineScanResult linescan = ScanLine(bmp, 13, seed, seedfade);
                    if (/* bmp.CompareColor(0, 13, seed, seedfade) == false && */ linescan.Word.Contains("]"))
                    {
                        // If the second line contains ], scan the first line.
                        LineScanResult secondlinescan = ScanLine(bmp, 23, seed, seedfade);
                        AddExecutedCommand(bmp, 13, linescan.NameLength, seed, seedfade, linescan.Word + " " + secondlinescan.Word);
                        word = linescan.Word + " " + secondlinescan.Word;
                    }
                    else
                    {
                        // Scan the first line in chat.
                        seed = GetSeed(bmp, 24);
                        seedfade = GetSeedFade(bmp, 24);
                        linescan = ScanLine(bmp, 24, seed, seedfade);
                        if (linescan.Word.Contains("]"))
                        {
                            AddExecutedCommand(bmp, 24, linescan.NameLength, seed, seedfade, linescan.Word);
                            word = linescan.Word;
                        }
                    }
                    ShowScan(ref bmp, 23, seed, seedfade);

                    // Write the word into the window if debug more is on.
                    if (cg.debugmode)
                    {
                        cg.debug.Invalidate(new Rectangle(0, bmp.Height * scale, cg.debug.Width, cg.debug.Height - (bmp.Height * scale)));
                        cg.g.DrawString(word, new Font("Arial", 16), Brushes.Black, new PointF(0, bmp.Height * scale + 5));
                        cg.g.FillRectangle(new SolidBrush(Color.FromArgb(seed[0], seed[1], seed[2])), new Rectangle(0, 0, 30, 30));
                        cg.g.DrawRectangle(new Pen(Color.Blue), new Rectangle(0, 0, 30, 30));

                        int ecy = bmp.Height * scale + 35;
                        for (int i = 0; i < _executedCommands.Count; i++)
                        {
                            try
                            {
                                cg.g.DrawImage(_executedCommands[i].executor, new Rectangle(0, ecy, _executedCommands[i].executor.Width * 5, _executedCommands[i].executor.Height * 5));
                                cg.g.DrawString(_executedCommands[i].command, new Font("Arial", 12), Brushes.Black, _executedCommands[i].executor.Width * scale, ecy);
                                ecy += _executedCommands[i].executor.Height * 5;
                            }
                            catch (ArgumentOutOfRangeException)
                            { }
                        }
                    }
                } // while

                bmp.Dispose();
            }

            #endregion

            #region Extras

            void updatescreen(ref Bitmap bmp)
            {
                cg.updateScreen();
                if (bmp != null)
                    bmp.Dispose();
                bmp = cg.bmp.Clone(new Rectangle(shotarea.X, shotarea.Y, shotarea.Width, shotarea.Height), cg.bmp.PixelFormat);

                //var gfx = Graphics.FromImage(bmp);
                //gfx.CopyFromScreen(shotarea.X, shotarea.Y, 0, 0, shotarea.Size, CopyPixelOperation.SourceCopy);
                //gfx.Dispose();
            }

            bool IsEqualToAny(char op, params char[] equal)
            {
                for (int i = 0; i < equal.Length; i++)
                    if (op == equal[i])
                        return true;
                return false;
            }

            void ShowScan(ref Bitmap bmp, int y, int[] seed, int seedfade)
            {
                // Show valid seed pixels in debug mode
                if (cg.debugmode)
                {
                    for (int xi = 0; xi < bmp.Width; xi++)
                        for (int yi = y - 7; yi < y + 2; yi++)
                            if (bmp.CompareColor(xi, yi, seed, seedfade))
                                bmp.SetPixel(xi, yi, Color.Purple);
                    //bmp.SetPixel(0, y, Color.Orange);
                    cg.g.DrawImage(bmp, new Rectangle(0, 0, bmp.Width * scale, bmp.Height * scale));
                    updatescreen(ref bmp);
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
                // Get seedfade by getting the average numbers of the RGB of the first pixel and the second pixel divided by 2.5. Default is 50 
                return (int)(((((seedpix.R + seedpix.G + seedpix.B) / 3) + ((antipix.R + antipix.G + antipix.B) / 3)) / 2) / 2.5);
            }

            // Checks if an executed command should be added to the list of commands, then adds it.
            void AddExecutedCommand(Bitmap bmp, int y, int namelength, int[] seed, int seedfade, string word)
            {
                // Clean up the word. makes something like "] $APPLE " into "$APPLE"
                var wordtemp = word.Split(new char[] { ']' }, 2);
                if (wordtemp.Length > 1)
                    word = wordtemp[1];
                word = word
                    .TrimStart(' ')
                    .TrimEnd(' ');

                if (word.Length > 0)
                {
                    try
                    {
                        // See if command is being listened to. If it is, continue.
                        if (ListenToAllCommands || ListenTo[word.Split(' ')[0]])
                        {
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

                            bool add = true;
                            // Get percent of executed commands to executing command
                            for (int ei = 0; ei < _executedCommands.Count; ei++)
                            {
                                double percent = 0;
                                int identiclecount = 0; // number of pixels that are identicle.
                                int count = 0;
                                for (int xi = 0; xi < executor.Width && xi < _executedCommands[ei].executor.Width; xi++)
                                    for (int yi = 0; yi < executor.Height; yi++)
                                    {
                                        count++;
                                        if (executor.GetPixelAt(xi, yi) == _executedCommands[ei].executor.GetPixelAt(xi, yi))
                                            identiclecount++;
                                    }
                                percent = (Convert.ToDouble(identiclecount) / Convert.ToDouble(count)) * 100;
                                // If the same person has executed a command...
                                if (percent >= 90)
                                {
                                    // If its the same command or AllowMultipleCommandExecutions equals false, do not add the command to the list.
                                    if (_executedCommands[ei].command == word || !AllowMultipleCommandExecutions) // check if command is the same
                                    {
                                        add = false;
                                        // If SameExecutorCommandUpdate equals true, then update the already-executed command.
                                        if (SameExecutorCommandUpdate)
                                        {
                                            _executedCommands[ei].command = word;
                                            _executedCommands[ei].channel = GetChannelFromSeed(seed);
                                        }
                                        break;
                                    }
                                }
                            }

                            if (add)
                            {
                                // If executor is new, add command to executedcommands
                                _executedCommands.Add(new CommandData(word, executor, GetChannelFromSeed(seed)));
                                System.Threading.Thread.Sleep(100);
                            }
                            else
                            {
                                executor.Dispose();
                            }
                        } // checks if command is being listened to
                    } // try
                    catch (KeyNotFoundException) { }
                }
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
                        var bestletter = checkLetter(bmp, i, y, seed, seedfade); // Scan for letter.

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
            LetterResult checkLetter(Bitmap bmp, int x, int y, int[] seed, int seedfade)
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
                            if (x + letters[li].pixel[pi, 0] >= 0 && y + letters[li].pixel[pi, 1] >= 0 && x + letters[li].pixel[pi, 0] < shotarea.Width && y + letters[li].pixel[pi, 1] < shotarea.Height)
                            {
                                if (bmp.CompareColor(x + letters[li].pixel[pi, 0], y + letters[li].pixel[pi, 1], seed, seedfade))
                                {
                                    successcount++;
                                }
                            }
                        }
                        // Check optional pixels
                        else if (x + letters[li].pixel[pi, 0] >= 0 && y + letters[li].pixel[pi, 1] >= 0 && x + letters[li].pixel[pi, 0] < shotarea.Width && y + letters[li].pixel[pi, 1] < shotarea.Height)
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
                            if (x + letters[li].ignore[pi, 0] > 0 && y + letters[li].ignore[pi, 1] > 0 && x + letters[li].ignore[pi, 0] < shotarea.Width && y + letters[li].ignore[pi, 1] < shotarea.Height)
                                if (bmp.CompareColor(x + letters[li].ignore[pi, 0], y + letters[li].ignore[pi, 1], seed, seedfade))
                                    totalpixels++;

                    // Get percent.
                    double percent = Convert.ToDouble(successcount) / Convert.ToDouble(totalpixels) * 100;

                    if (percent == 100)
                    {
                        LetterResult connected = null;
                        // The letter L can connect to other letters. LM can be confused for U1. This checks for the letter that the L could be connected to.
                        if (letters[li].letter == 'L')
                            connected = checkLetter(bmp, x + letters[li].length + 1, y, seed, seedfade);
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
                            if ((bestletter.letter == 'L' && letterresult[bi].letter == 'U' && IsEqualToAny(bestletter.connected.letter, 'M', 'N', 'X', 'L', 'U')) == false)
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

            private Channel GetChannelFromSeed(int[] seed)
            {
                for (int i = 0; i < CALData.ChatColors.Length; i++)
                    if (Math.Abs(CALData.ChatColors[i][0] - seed[0]) < CALData.ChatFade &&
                        Math.Abs(CALData.ChatColors[i][1] - seed[1]) < CALData.ChatFade &&
                        Math.Abs(CALData.ChatColors[i][2] - seed[2]) < CALData.ChatFade)
                    {
                        return (Channel)i;
                    }
                return Channel.General;
            }

            class Letter
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

            class LetterResult
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

            class LineScanResult
            {
                public string Word;
                public int NameLength;
                public LineScanResult(string word, int namelength)
                {
                    Word = word;
                    NameLength = namelength;
                }
            }
            #endregion
        }
    }
}
