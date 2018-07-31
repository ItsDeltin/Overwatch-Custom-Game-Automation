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
        public Commands Command;
        /// <summary>
        /// Chat commands for Overwatch.
        /// </summary>
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
            public List<ListenTo> ListenTo = new List<ListenTo>();

            List<PlayerIdentity> _playerIdentities = new List<PlayerIdentity>();

            /// <summary>
            /// List containing all player identities.
            /// </summary>
            public ReadOnlyCollection<PlayerIdentity> RegisteredPlayerIdentities
            {
                get { return _playerIdentities.AsReadOnly(); }
            }

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

            object CommandLock = new object();

            List<CommandData> _executedCommands = new List<CommandData>();

            /// <summary>
            /// Removes an executed command from the executed commands list.
            /// </summary>
            /// <param name="index">Command to remove by index.</param>
            public void DisposeExecutedCommand(int index)
            {
                if (_executedCommands[index].playerIdentity != null)
                    _executedCommands[index].executor.Dispose();
                _executedCommands.RemoveAt(index);
            }

            /// <summary>
            /// Removes all executed commands.
            /// </summary>
            public void DisposeAllExecutedCommands()
            {
                for (int i = 0; i < _executedCommands.Count; i++)
                    if (_executedCommands[i].playerIdentity != null)
                        _executedCommands[i].executor.Dispose();
                _executedCommands = new List<CommandData>();
            }

            /// <summary>
            /// Disposes of a player identity.
            /// </summary>
            /// <param name="identity">Identity to dispose.</param>
            public void DisposePlayerIdentity(PlayerIdentity identity)
            {
                lock (CommandLock)
                {
                    identity.CareerProfileMarkup.Dispose();
                    identity.ChatMarkup.Dispose();
                    _playerIdentities.Remove(identity);
                }
            }

            /// <summary>
            /// Disposes of a player identity.
            /// </summary>
            /// <param name="index">Index of the identity to dispose in the RegisteredPlayerIdentities list.</param>
            public void DisposePlayerIdentity(int index)
            {
                lock (CommandLock)
                {
                    _playerIdentities[index].CareerProfileMarkup.Dispose();
                    _playerIdentities[index].ChatMarkup.Dispose();
                    _playerIdentities.RemoveAt(index);
                }
            }

            /// <summary>
            /// Disposes all player identities.
            /// </summary>
            public void DisposeAllPlayerIdentities()
            {
                lock (CommandLock)
                {
                    for (int i = 0; i < _playerIdentities.Count; i++)
                    {
                        _playerIdentities[i].CareerProfileMarkup.Dispose();
                        _playerIdentities[i].ChatMarkup.Dispose();
                        _playerIdentities.RemoveAt(i);
                    }
                }
            }

            /// <summary>
            /// Set to true to start listening to commands. Set to false to stop.
            /// </summary>
            public bool Listen = false;

            /// <summary>
            /// Set to true to save player identities. Please note that there is a 250-1000 millisecond delay when a player executes a command for the first time.
            /// </summary>
            public bool RegisterPlayerProfiles
            {
                get { return _registerPlayerProfiles; }
                set
                {
                    lock (CommandLock)
                    {
                        _registerPlayerProfiles = value;
                    }
                }
            }
            bool _registerPlayerProfiles = false;

            /// <summary>
            /// Listens to all commands rather than just the commands in the ListenTo dictionary.
            /// </summary>
            public bool ListenToAllCommands = false;
            
            /// <summary>
            /// Allows one executor to execute more than one command.
            /// </summary>
            public bool AllowMultipleCommandExecutions = false;

            /// <summary>
            /// If a player who already has a command registered in ExecutedCommands executes another command, their old command gets updated to the new one.
            /// </summary>
            public bool SameExecutorCommandUpdate = false;

            #region Letters
            /*  Sorry to anyone who maintains this monstrocity in the future :)
                Each coordinate in the first argument represents the location of a pixel making the letters. For example, the letter C:
                
               -4□■■■□
               -3■□□□■
               -2■□□□□
               -1■□□□■
                0□■■■□ 0,0 being the first black pixel on the first row (y=0).
                -10123
                
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

            // Scale of debug images
            static int scale = 5;

            internal void StopScanning()
            {
                KeepScanning = false;
            }
            private bool KeepScanning = true;

            void ScanCommands()
            {
                var bmp = new Bitmap(Rectangles.LOBBY_CHATBOX.Width, Rectangles.LOBBY_CHATBOX.Height, PixelFormat.Format32bppArgb);

                Stopwatch toggle = new Stopwatch();
                if (cg.debugmode)
                    toggle.Start();

                while (KeepScanning)
                {
                    // Wait for listen to equal true
                    System.Threading.Thread.Sleep(10);
                    while (Listen == false) System.Threading.Thread.Sleep(10);

                    updatescreen(ref bmp);

                    // Word result
                    string word = "";

                    // Scan the second line in the chat.
                    var seed = GetSeed(bmp, 13);
                    var seedfade = GetSeedFade(bmp, 13);

                    LineScanResult linescan = ScanLine(bmp, 13, seed, seedfade);
                    if (/* CompareColor(0, 13, seed, seedfade) == false && */ linescan.Word.Contains("]"))
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

                    // Write the word into the window if debug more is on.
                    if (cg.debugmode)
                    {
                        ShowScan(ref bmp, 23, seed, seedfade);

                        cg.debug.Invalidate(new Rectangle(0, bmp.Height * scale, cg.debug.Width, cg.debug.Height - (bmp.Height * scale)));
                        cg.g.DrawString(word, new Font("Arial", 16), Brushes.Black, new PointF(0, bmp.Height * scale + 5));
                        cg.g.FillRectangle(new SolidBrush(Color.FromArgb(seed[0], seed[1], seed[2])), new Rectangle(0, 0, 30, 30));
                        cg.g.DrawRectangle(new Pen(Color.Blue), new Rectangle(0, 0, 30, 30));

                        int ecy = bmp.Height * scale + 35;
                        lock (CommandLock)
                        {
                            for (int i = 0; i < _executedCommands.Count; i++)
                            {
                                try
                                {
                                    cg.g.DrawImage(_executedCommands[i].executor, new Rectangle(0, ecy, _executedCommands[i].executor.Width * 5, _executedCommands[i].executor.Height * 5));
                                    cg.g.DrawString(_executedCommands[i].Command, new Font("Arial", 12), Brushes.Black, _executedCommands[i].executor.Width * scale, ecy);
                                    ecy += _executedCommands[i].executor.Height * 5;
                                }
                                catch (ArgumentOutOfRangeException)
                                { }
                            }
                        }
                    }
                } // while

                bmp.Dispose();
            }

            void updatescreen(ref Bitmap bmp)
            {
                cg.updateScreen();
                if (bmp != null)
                    bmp.Dispose();
                bmp = cg.BmpClone(shotarea.X, shotarea.Y, shotarea.Width, shotarea.Height);
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
                            if (cg.CompareColor(xi, yi, seed, seedfade))
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
                word = word.Trim();

                string commandFirstWord = word.Split(' ')[0];
                ListenTo ltd = ListenTo.FirstOrDefault(v => v.Command == commandFirstWord);

                // See if command is being listened to. If it is, continue.
                if (word.Length > 0 && ((ListenToAllCommands && word[0] == '$') || (ltd != null && ltd.Listen)))
                {
                    lock (CommandLock)
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
                            // If the same person has executed a command...
                            if (CompareExecutors(executor, _executedCommands[ei].executor))
                            {
                                // If its the same command or AllowMultipleCommandExecutions equals false, do not add the command to the list.
                                if (_executedCommands[ei].Command == word || !AllowMultipleCommandExecutions) // check if command is the same
                                {
                                    add = false;
                                    // If SameExecutorCommandUpdate equals true, then update the already-executed command.
                                    if (SameExecutorCommandUpdate)
                                    {
                                        _executedCommands[ei].Command = word;
                                        _executedCommands[ei].Channel = GetChannelFromSeed(seed);
                                    }
                                    break;
                                }
                            }
                        }

                        // Get the player identity
                        PlayerIdentity pi = null;

                        if (add)
                        {
                            // Find the player identity by comparing the executor of the command that was just executed and the executor of the players in the player identities.
                            for (int i = 0; i < _playerIdentities.Count; i++)
                                if (CompareExecutors(executor, _playerIdentities[i].ChatMarkup))
                                {
                                    pi = _playerIdentities[i];
                                    break;
                                }

                            // If it was not found, pi is still null. Register the profile if _registerPlayerProfiles is true.
                            if (pi == null && _registerPlayerProfiles && (ltd == null || (ltd != null && ltd.RegisterProfile)))
                            {
                                Point openMenuAt = new Point(54, shotarea.Y + y);

                                // Open the career profile
                                cg.RightClick(openMenuAt, 500);

                                // By default, the career profile option is selected and we can just press enter to open it.
                                cg.KeyPress(Keys.Enter);

                                // Wait for the career profile to load.
                                WaitForCareerProfileToLoad();

                                // Take a screenshot of the career profile.
                                cg.updateScreen();
                                //Bitmap careerProfileSnapshot = cg.BmpClone(Rectangles.LOBBY_CAREER_PROFILE.X, Rectangles.LOBBY_CAREER_PROFILE.Y, Rectangles.LOBBY_CAREER_PROFILE.Width, Rectangles.LOBBY_CAREER_PROFILE.Height);
                                Bitmap careerProfileSnapshot = cg.BmpClone(Rectangles.LOBBY_CAREER_PROFILE);

                                // Register the player identity.
                                pi = new PlayerIdentity(executor, careerProfileSnapshot, PlayerIdentityIndex);
                                _playerIdentities.Add(pi);
                                PlayerIdentityIndex++;

                                // Go back to the lobby.
                                cg.GoBack(1);
                                cg.ResetMouse();

                                // If opening the career profile failed, the state of the chat could be incorrect, 
                                // like being wrongly opened or wrongly closed because of when enter was pressed earlier.
                                // This will fix it.
                                cg.Chat.OpenChat();
                                if (!cg.OpenChatIsDefault)
                                    cg.KeyPress(Keys.Enter);
                            }

                            _executedCommands.Add(new CommandData(word, GetChannelFromSeed(seed), executor, pi));
                            System.Threading.Thread.Sleep(50);
                        }
                        else
                        {
                            executor.Dispose();
                        }
                    }
                } // checks if command is being listened to
            }

            int PlayerIdentityIndex = 0;

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

            private bool CompareExecutors(Bitmap e1, Bitmap e2)
            {
                if (e1.Width != e2.Width || e1.Height != e2.Height)
                    return false;

                int identiclecount = 0; // number of pixels that are identicle.
                int count = 0;
                for (int xi = 0; xi < e1.Width; xi++)
                    for (int yi = 0; yi < e1.Height; yi++)
                    {
                        count++;
                        if (e1.GetPixelAt(xi, yi) == e2.GetPixelAt(xi, yi))
                            identiclecount++;
                    }

                return ((Convert.ToDouble(identiclecount) / Convert.ToDouble(count)) * 100) >= 90;
            }
            private double CompareCareerProfiles(Bitmap cp1, Bitmap cp2)
            {
                double total = 0;
                double success = 0;

                for (int x = 0; x < cp1.Width; x++)
                    for (int y = 0; y < cp1.Height; y++)
                    {
                        total++;
                        if (cp1.CompareColor(x, y, cp2.GetPixelAt(x, y).ToInt(), 50))
                            success++;
                    }

                return (success / total) * 100;
            }

            private Channel GetChannelFromSeed(int[] seed)
            {
                for (int i = 0; i < CG_Chat.ChatColors.Length; i++)
                    if (Math.Abs(CG_Chat.ChatColors[i][0] - seed[0]) < CG_Chat.ChatFade &&
                        Math.Abs(CG_Chat.ChatColors[i][1] - seed[1]) < CG_Chat.ChatFade &&
                        Math.Abs(CG_Chat.ChatColors[i][2] - seed[2]) < CG_Chat.ChatFade)
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

            /// <summary>
            /// Gets the player identity of a slot.
            /// </summary>
            /// <param name="slot">Slot to check.</param>
            /// <returns>The player identity of the slot.</returns>
            public PlayerIdentity GetSlotIdentity(int slot)
            {
                bool careerProfileOpenSuccess = cg.Interact.MenuOptionScan(slot, view_career_profile_markup, 80, 100, 1);
                if (!careerProfileOpenSuccess)
                    return null;

                WaitForCareerProfileToLoad();

                List<double> percentages = new List<double>();

                lock (CommandLock)
                {
                    cg.updateScreen();

                    Bitmap compareTo = cg.BmpClone(Rectangles.LOBBY_CAREER_PROFILE);

                    for (int i = 0; i < _playerIdentities.Count; i++)
                    {
                        percentages.Add(CompareCareerProfiles(compareTo, _playerIdentities[i].CareerProfileMarkup));
                    }

                    compareTo.Dispose();
                }

                cg.GoBack(1);

                cg.ResetMouse();

                int highestIndex = percentages.IndexOf(percentages.Max());

                if (highestIndex == -1)
                    return null;

                if (percentages[highestIndex] >= 90)
                    return _playerIdentities[highestIndex];
                else
                    return null;
            }
            static Bitmap view_career_profile_markup = Properties.Resources.view_career_profile;

            internal void WaitForCareerProfileToLoad()
            {
                cg.WaitForColor(423, 164, new int[] { 85, 90, 107 }, 10, 3000);
                System.Threading.Thread.Sleep(250);
            }

            /// <summary>
            /// Gets the commands a player identity has executed.
            /// </summary>
            /// <param name="identity">Identity to check.</param>
            /// <returns>Array of commands executed.</returns>
            public CommandData[] GetIdentityExecutedCommands(PlayerIdentity identity)
            {
                List<CommandData> executedCommands = new List<CommandData>();

                lock (CommandLock)
                {
                    for (int i = 0; i < ExecutedCommands.Count; i++)
                        if (Object.ReferenceEquals(ExecutedCommands[i].playerIdentity.CareerProfileMarkup, identity.CareerProfileMarkup))
                            executedCommands.Add(ExecutedCommands[i]);
                }

                return executedCommands.ToArray();
            }
        }
    }

    /// <summary>
    /// Data for commands to listen to on the Commands class.
    /// </summary>
    public class ListenTo
    {
        /// <summary>
        /// Data for commands to listen to on the Commands class.
        /// </summary>
        public ListenTo(string command, bool listen, bool registerProfile)
        {
            for (int c = 0; c < command.Length; c++)
            {
                bool characterFound = false;

                for (int l = 0; l < CustomGame.Commands.letters.Length; l++)
                    if (command[c] == CustomGame.Commands.letters[l].letter)
                        characterFound = true;

                if (!characterFound)
                    throw new ArgumentException(string.Format("Letter '{0}' is not a valid letter to have in a command. The only valid letters is the uppercase english alphabet, numbers, and $.",
                        command[c].ToString()));
            }

            _command = command;
            Listen = listen;
            RegisterProfile = registerProfile;
        }

        string _command;
        /// <summary>
        /// Command to listen to.
        /// </summary>
        public string Command { get { return _command; } }
        /// <summary>
        /// Should this command be listened to?
        /// </summary>
        public bool Listen;
        /// <summary>
        /// Should the player who executes this command have their profile registered?
        /// </summary>
        public bool RegisterProfile;
    }

    /// <summary>
    /// Contains data for identifying players who executed a command.
    /// </summary>
    public class PlayerIdentity : IEquatable<PlayerIdentity>
    {
        internal PlayerIdentity(Bitmap chatMarkup, Bitmap careerProfileMarkup, int id)
        {
            ChatMarkup = chatMarkup;
            CareerProfileMarkup = careerProfileMarkup;
            ID = id;
        }

        public bool Equals(PlayerIdentity other)
        {
            return ID == other.ID;
        }

        internal Bitmap ChatMarkup;
        internal Bitmap CareerProfileMarkup;
        internal int ID;
    }

    /// <summary>
    /// Data of Overwatch executed chat commands.
    /// </summary>
    public class CommandData
    {
        internal CommandData(string command, Channel channel, Bitmap executor, PlayerIdentity playerIdentity)
        {
            this.command = command;
            this.executor = executor;
            this.channel = channel;
            this.playerIdentity = playerIdentity;
        }

        /// <summary>
        /// Command player executed.
        /// </summary>
        public string Command { get { return command; } internal set { command = value; } }
        string command;

        /// <summary>
        /// Channel the command was executed on.
        /// </summary>
        public Channel Channel { get { return channel; } internal set { channel = value; } }
        Channel channel;

        /// <summary>
        /// Noise data of executor.
        /// </summary>
        internal Bitmap executor;
        /// <summary>
        /// The identity of the player that executed the command.
        /// </summary>
        internal PlayerIdentity playerIdentity;
    }
}
