using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Diagnostics;
using System.Threading;
using System.Linq;
using System.Windows.Forms;

namespace Deltin.CustomGameAutomation
{
    partial class CustomGame
    {
        static int fade = 20;

        /*
            * Checking the slots works by seeing if the color of a pixel on the slot and the color of a pixel slightly off the slot.
            * If they are mostly different, the slot is occupied with a player.
            * If not, the slot is empty.
        */

        #region Players in red and blue
        /// <summary>
        /// Get player count of red team and blue team.
        /// </summary>
        public int PlayerCount
        {
            get
            {
                updateScreen();
                int playersConnected = 0;
                for (int i = 0; i < 12; i++)
                    if (CompareColor(playerLoc[i, 0], playerLoc[i, 1], slotLoc[i, 0], slotLoc[i, 1], fade) == false)
                        playersConnected++;
                return playersConnected;
            }
        }
        /// <summary>
        /// Gets the slots filled in red team and blue team.
        /// </summary>
        public List<int> PlayerSlots
        {
            get
            {
                updateScreen();
                List<int> slot = new List<int>();
                for (int i = 0; i < 12; i++)
                    if (CompareColor(playerLoc[i, 0], playerLoc[i, 1], slotLoc[i, 0], slotLoc[i, 1], fade) == false)
                        slot.Add(i);
                return slot;
            }
        }
        #endregion

        #region Players in blue
        /// <summary>
        /// Gets the player count of blue team.
        /// </summary>
        public int BlueCount
        {
            get
            {
                updateScreen();
                int playersConnected = 0;
                for (int i = 0; i < 6; i++)
                    if (CompareColor(playerLoc[i, 0], playerLoc[i, 1], slotLoc[i, 0], slotLoc[i, 1], fade) == false)
                        playersConnected++;
                return playersConnected;
            }
        }
        /// <summary>
        /// Gets the slots filled in blue team.
        /// </summary>
        public List<int> BlueSlots
        {
            get
            {
                updateScreen();
                List<int> slot = new List<int>();
                for (int i = 0; i < 6; i++)
                    if (CompareColor(playerLoc[i, 0], playerLoc[i, 1], slotLoc[i, 0], slotLoc[i, 1], fade) == false)
                        slot.Add(i);
                return slot;
            }
        }
        #endregion

        #region Players in red
        /// <summary>
        /// Gets the player count of red team.
        /// </summary>
        public int RedCount
        {
            get
            {
                updateScreen();
                int playersConnected = 0;
                for (int i = 6; i < 12; i++)
                    if (CompareColor(playerLoc[i, 0], playerLoc[i, 1], slotLoc[i, 0], slotLoc[i, 1], fade) == false)
                        playersConnected++;
                return playersConnected;
            }
        }
        /// <summary>
        /// Gets the slots filled in red team.
        /// </summary>
        public List<int> RedSlots
        {
            get
            {
                updateScreen();
                List<int> slot = new List<int>();
                for (int i = 6; i < 12; i++)
                    if (CompareColor(playerLoc[i, 0], playerLoc[i, 1], slotLoc[i, 0], slotLoc[i, 1], fade) == false)
                        slot.Add(i);
                return slot;
            }
        }
        #endregion

        #region Players in queue
        /// <summary>
        /// Gets the number of players in the queue.
        /// </summary>
        public int QueueCount
        {
            get
            {
                updateScreen();
                int inq = 0;

                // <image url="$(ProjectDir)\ImageComments\GetInfo.cs\Offset.png" scale="1.3" />
                // The SPECTATORS text moves down for every player in the queue. Check for all possible locations for the SPECTATORS text.
                for (int i = 0; i < 6; i++)
                    if (CompareColor(727, 266 + (i * 13), new int[] { 132, 147, 151 }, 20))
                        inq = i + 1;
                // If there are more than 6 players in the queue, a scrollbar appears to show the rest of the players in the queue.
                // Check for the length of the scrollbar to get the number of players in the queue
                if (inq == 6)
                {
                    // There can only be 10 players in the queue, which means with a full queue 4 can be hidden.
                    for (int i = 0; i < 4; i++)
                    {
                        int y = 304 - (i * (10 - i));
                        if (CompareColor(894, y, new int[] { 153, 153, 152 }, 20)
                            || CompareColor(894, y, new int[] { 132, 126, 123 }, 20))
                        {
                            inq = inq + i + 1;
                            break;
                        }
                    }
                }
                return inq;
            }
        }
        /// <summary>
        /// Gets the slots filled in the queue.
        /// </summary>
        public List<int> QueueSlots
        {
            get
            {
                // No need for updateScreen() because QueueCount updates it.
                List<int> slots = new List<int>();
                int inq = QueueCount;
                for (int i = 0; i < inq; i++)
                    slots.Add(Queueid + i);
                return slots;
            }
        }
        #endregion

        #region Players in spectator
        /// <summary>
        /// Gets the number of players in spectator.
        /// </summary>
        public int SpectatorCount
        {
            get
            {
                updateScreen();

                int offset = FindOffset(); // The spectator list moves down when players join the queue, this finds the offset in pixels how far the list of slots moves down.

                int specConnected = 0;
                for (int i = 12; i < Queueid; i++)
                    if (CompareColor(playerLoc[i, 0], playerLoc[i, 1] + offset, slotLoc[i, 0], slotLoc[i, 1] + offset, fade) == false)
                        specConnected++;
                return specConnected;
            }
        }
        /// <summary>
        /// Gets the slots filled in spectator excluding the first slot.
        /// </summary>
        public List<int> SpectatorSlots
        {
            get
            {
                updateScreen();

                int offset = FindOffset(); // The spectator list moves down when players join the queue, this finds the offset in pixels how far the list of slots moves down.

                // List of slots filled
                List<int> ss = new List<int>();
                for (int i = 12; i < Queueid; i++)
                {
                    if (CompareColor(playerLoc[i, 0], playerLoc[i, 1] + offset, slotLoc[i, 0], slotLoc[i, 1] + offset, fade) == false)
                        ss.Add(i);
                }
                return ss;
            }
        }
        #endregion

        internal static int[,] playerLoc = new int[,] {
            // First set is slot loc, second set is area next to slot
            // blue
            { 51, 255 }, // slot 1
            { 51, 283 }, // slot 2
            { 51, 311 }, // slot 3
            { 51, 341 }, // slot 4
            { 51, 369 }, // slot 5
            { 51, 384 }, // slot 6 // Y = 397
            // red
            { 620, 255 }, // slot 7
            { 620, 283 }, // slot 8
            { 620, 311 }, // slot 9
            { 620, 341 }, // slot 10
            { 620, 369 }, // slot 11
            { 620, 397 }, // slot 12
            // spectators
            { 893, 248 }, // slot 1
            { 893, 264 }, // slot 2
            { 893, 277 }, // slot 3
            { 893, 290 }, // slot 4
            { 893, 304 }, // slot 5
            { 893, 317 }, // slot 6
        }; // playerLoc
        internal static int[,] slotLoc = new int[,]
        {
            // blue
            { 48, 255 }, // slot 1
            { 48, 283 }, // slot 2
            { 48, 311 }, // slot 3
            { 48, 341 }, // slot 4
            { 48, 369 }, // slot 5
            { 48, 384 }, // slot 6 // Y = 397
            // red
            { 624, 255 }, // slot 7
            { 624, 283 }, // slot 8
            { 624, 311 }, // slot 9
            { 624, 341 }, // slot 10
            { 624, 369 }, // slot 11
            { 624, 397 }, // slot 12
            // spectators
            { 896, 248 }, // slot 1
            { 896, 264 }, // slot 2
            { 896, 277 }, // slot 3
            { 896, 290 }, // slot 4
            { 896, 304 }, // slot 5
            { 896, 317 }, // slot 6
        }; // slotloc

        #region Players Invited
        /// <summary>
        /// Gets the number of players who were invited but are not ingame.
        /// </summary>
        /// <param name="playersConnected">List of slots that are occupied.</param>
        /// <returns>Number of players who are not ingame.</returns>
        /// <exception cref="InvalidSlotException">Thrown if there is an invalid slot in the playersConnected list.</exception>
        public int GetInvitedCount(List<int> playersConnected)
        {
            return GetInvitedSlots(playersConnected).Count;
        }
        /// <summary>
        /// Gets a list of players who were invited but are not ingame.
        /// </summary>
        /// <param name="playersConnected">List of slots that are occupied.</param>
        /// <returns>Integer list of players who are not ingame.</returns>
        /// <exception cref="InvalidSlotException">Thrown if there is an invalid slot in the playersConnected list.</exception>
        public List<int> GetInvitedSlots(List<int> playersConnected)
        {
            // <image url="$(ProjectDir)\ImageComments\GetInfo.cs\InviteInfo.png" scale="1" />

            if (playersConnected.Count > 0)
            {
                if (playersConnected.Min() < 0)
                    throw new InvalidSlotException(string.Format("Slot argument '{0}' is out of range.", playersConnected.Min()));
                else if (playersConnected.Max() >= InvitedMarkerLocations.Length)
                    throw new InvalidSlotException(string.Format("Slot argument '{0}' is out of range.", playersConnected.Max()));
            }
            updateScreen();
            List<int> invited = new List<int>();
            List<int> pd = PlayerInfo.PlayersDead(true);

            int[] bluecolor = new int[] { 82, 106, 117 };
            int[] redcolor = new int[] { 110, 72, 76 };

            int[] deadbluecolor = new int[] { 81, 82, 82 };
            int[] deadredcolor = new int[] { 87, 83, 85 };

            int[] color = bluecolor;
            int[] deadcolor = deadbluecolor;

            for (int i = 0; i < InvitedMarkerLocations.Length; i++)
            {
                if (i == 6)
                {
                    color = redcolor;
                    deadcolor = deadredcolor;
                }
                if (playersConnected.Contains(i) // If the player slot index is filled,
                    && pd.Contains(i) == false // The player is not dead,
                    && PlayerInfo.IsUltimateReady(i, true) == false // And the player's ultimate is not ready...
                    // <image url="$(ProjectDir)\ImageComments\GetInfo.cs\InviteInfo1.png" scale="1.3" />
                    // And if the color isnt there...
                    // <image url="$(ProjectDir)\ImageComments\GetInfo.cs\InviteInfo2.png" scale="1.3" />
                    && !(CompareColor(InvitedMarkerLocations[i], 81, color, 20) || (CompareColor(InvitedMarkerLocations[i], 83, deadcolor, 20) && !CompareColor(InvitedMarkerLocations[i], 81, deadcolor, 20))))
                    // If everything above is false, the player is invited but not ingame.
                    invited.Add(i);
            }
            return invited;
        }
        private int[] InvitedMarkerLocations = new int[]
        {
        56, // slot 0 61
        106, // slot 1 109
        155, // slot 2 159
        205, // slot 3 208
        256, // slot 4 256
        302, // slot 5 307

        614, // slot 6 614
        663, // slot 7 664
        713, // slot 8 713
        761, // slot 9 762
        810, // slot 10 811
        858 // slot 11 861
        };
        #endregion

        #region All Players
        /// <summary>
        /// Gets the total amount of players in the custom game server.
        /// </summary>
        public int TotalPlayerCount
        {
            get
            {
                updateScreen();

                return PlayerCount + SpectatorCount + QueueCount;
            }
            private set { }
        }
        /// <summary>
        /// Gets all the slots of every player in the custom game server. Does not include players in queue.
        /// </summary>
        public List<int> TotalPlayerSlots
        {
            get
            {
                updateScreen();

                List<int> slots = new List<int>();
                slots.AddRange(PlayerSlots);
                slots.AddRange(SpectatorSlots);
                
                int queuecount = QueueCount;
                for (int i = 0; i < queuecount; i++)
                    slots.Add(Queueid + i);

                return slots;
            }
            private set { }
        }
        #endregion

        // Finds the offset in pixels of the queue to spectator displacement
        internal int FindOffset()
        {
            updateScreen();
            int inq = QueueCount;
            // After 6 players, the spectators text doesn't move down anymore and instead forms a scrollbar.
            if (inq > 6)
                inq = 6;
            // The spectator slots moves down 13 pixels for each player in the queue plus 23.
            int offset = inq * 13;
            if (inq > 0)
                offset += 23;
            return offset;
        }

        #region Is slot X?
        /// <summary>
        /// Returns true if the slot is blue, red, spectator, or in queue.
        /// </summary>
        /// <param name="slot">Slot to check</param>
        /// <returns></returns>
        public bool IsSlotValid(int slot)
        {
            return slot >= 0 && slot < Queueid + 6;
        }
        /// <summary>
        /// Returns true if the slot is in blue.
        /// </summary>
        /// <param name="slot">Slot to check</param>
        /// <returns></returns>
        public bool IsSlotBlue(int slot)
        {
            return slot >= 0 && slot <= 5;
        }
        /// <summary>
        /// Returns true if the slot is in red.
        /// </summary>
        /// <param name="slot">Slot to check</param>
        /// <returns></returns>
        public bool IsSlotRed(int slot)
        {
            return slot >= 6 && slot <= 11;
        }
        /// <summary>
        /// Returns true if the slot is in Spectator.
        /// </summary>
        /// <param name="slot">Slot to check</param>
        /// <returns></returns>
        public bool IsSlotSpectator(int slot)
        {
            return slot >= 12 && slot < Queueid;
        }
        /// <summary>
        /// Returns true if the slot is in queue.
        /// </summary>
        /// <param name="slot">Slot to check</param>
        /// <returns></returns>
        public bool IsSlotInQueue(int slot)
        {
            return slot >= Queueid && slot < Queueid + 6;
        }
        #endregion

        /// <summary>
        /// Waits for the slots in overwatch to change.
        /// </summary>
        /// <param name="maxtime">Time to wait. Set to -1 to wait forever.</param>
        /// <returns>Returns true if Overwatch's slots changed. Returns false if the time ran out.</returns>
        public bool WaitForSlotUpdate(int maxtime = 1000)
        {
            Stopwatch time = new Stopwatch();
            List<int> preslots = TotalPlayerSlots;
            time.Start();
            while (time.ElapsedMilliseconds < maxtime || maxtime == -1)
            {
                List<int> newslots = TotalPlayerSlots;
                if (preslots.Count != newslots.Count)
                    return true;
                else
                    for (int i = 0; i < preslots.Count; i++)
                        if (preslots[i] != newslots[i])
                            return true;
                Thread.Sleep(100);
            }
            return false;
        }

        /// <summary>
        /// Info about players in an Overwatch custom game.
        /// </summary>
        public CG_PlayerInfo PlayerInfo;
        /// <summary>
        /// Info about players in an Overwatch custom game.
        /// </summary>
        public class CG_PlayerInfo
        {
            private CustomGame cg;
            internal CG_PlayerInfo(CustomGame cg)
            { this.cg = cg; }

            /// <summary>
            /// Gets a list of players who died.
            /// </summary>
            /// <param name="noUpdate"></param>
            /// <returns>List of players who are dead.</returns>
            public List<int> PlayersDead(bool noUpdate = false)
            {
                // Returns which players that are dead by checking the killed marker locations for a red 'X'.
                // <image url="$(ProjectDir)\ImageComments\GetInfo.cs\DeadPlayers.png" scale="1.3" />

                if (!noUpdate)
                    cg.updateScreen();
                List<int> playersDead = new List<int>();
                for (int i = 0; i < 12; i++)
                    if (cg.CompareColor(KilledPlayerMarkerLocations[i], 98, CALData.DeadPlayerColor, CALData.DeadPlayerFade)
                        && !HasHealthBar(i, true))
                        playersDead.Add(i);
                return playersDead;
            }
            private static int[] KilledPlayerMarkerLocations = new int[]
            {
                66, // slot 0
                115, // slot 1
                164, // slot 2
                214, // slot 3
                263, // slot 4
                312, // slot 5

                633, // slot 6
                682, // slot 7
                731, // slot 8
                780, // slot 9
                830, // slot 10
                879, // slot 11
            };

            bool HasHealthBar(int slot, bool noUpdate = false)
            {
                int[] healthBarLocations = new int[]
                {
                    // Blue
                    45,
                    94,
                    143,
                    192,
                    242,
                    291,
                    // Red
                    610,
                    660,
                    709,
                    758,
                    808,
                    856,
                };
                int xLength = 43;
                int y = 96;

                if (!noUpdate)
                    cg.updateScreen();

                for (int x = healthBarLocations[slot]; x < healthBarLocations[slot] + xLength; x++)
                    if (cg.CompareColor(x, y, new int[] { 110, 110, 110 }, 10))
                        return true;
                return false;
            }

            /// <summary>
            /// Get max player count for both teams.
            /// </summary>
            /// <returns>Returns an int[] where [0] is blue max player count and [1] is red max player count.</returns>
            public int[] MaxPlayerCount()
            {
                // <image url="$(ProjectDir)\ImageComments\GetInfo.cs\MaxPlayers.png" scale="1" />
                cg.updateScreen();
                int[] result = new int[2];
                // Blue: X = 613
                // Red: X = 752
                int[] searchArea = new int[] { 250, 275, 310, 340, 370, 400 };
                int[] blueColor = new int[] { 148, 202, 224 };
                int[] redColor = new int[] { 167, 76, 86 };
                for (int i = 0; i < searchArea.Length; i++)
                    if (cg.CompareColor(302, searchArea[i], blueColor, 50))
                        result[0]++;
                for (int i = 0; i < searchArea.Length; i++)
                    if (cg.CompareColor(370, searchArea[i], redColor, 50))
                        result[1]++;
                return result;
            }

            /// <summary>
            /// Test if input slot has chosen a hero.
            /// </summary>
            /// <param name="slot">Slot to check</param>
            /// <returns>True if hero is chosen</returns>
            /// <exception cref="InvalidSlotException">Thrown if slot argument is out of range.</exception>
            public bool IsHeroChosen(int slot)
            {
                if (!(cg.IsSlotBlue(slot) || cg.IsSlotRed(slot)))
                    throw new InvalidSlotException(string.Format("Slot argument '{0}' is out of range.", slot));
                if (cg.PlayerSlots.Contains(slot) == false)
                    return false;

                cg.updateScreen();

                if (PlayersDead(true).Contains(slot))
                    return true;

                return _HeroChosen(slot);
            }

            // Private method to check if a hero is chosen.
            // The screen is not updated, run updateScreen() beforehand.
            // There is no argument checking.
            bool _HeroChosen(int slot)
            {
                if (slot < 6)
                {
                    //return !cg.CompareColor(CALData.HeroChosenLocations[slot], CALData.HeroChosenY, CALData.HeroChosenBlue, CALData.HeroChosenFade);
                    return !cg.CompareColor(HeroCheckLocations[slot] + 6, HeroCheckY + 3, CALData.HeroChosenBlue, CALData.HeroChosenFade);
                }
                else
                {
                    //return !cg.CompareColor(CALData.HeroChosenLocations[slot], CALData.HeroChosenY, CALData.HeroChosenRed, CALData.HeroChosenFade);
                    return !cg.CompareColor(HeroCheckLocations[slot] + 6, HeroCheckY + 3, CALData.HeroChosenRed, CALData.HeroChosenFade);
                }
            }

            // These are the locations the moderator icon is (green crown)
            int[,] ModeratorLocations = new int[,]
            {
                // blue
                { 65, 257 },
                { 65, 286 },
                { 65, 315 },
                { 65, 343 },
                { 65, 372 },
                { 65, 400 },
                // red
                { 607, 257 },
                { 607, 286 },
                { 607, 315 },
                { 607, 343 },
                { 607, 372 },
                { 607, 400 },
                // spectator
                { 885, 252 },
                { 885, 265 },
                { 885, 279 },
                { 885, 292 },
                { 885, 305 },
                { 885, 318 }
            };
            /// <summary>
            /// Gets the slot the moderator is on.
            /// </summary>
            /// <returns>Slot the moderator is on.</returns>
            public int ModeratorSlot()
            {
                // <image url="$(ProjectDir)\ImageComments\GetInfo.cs\ModeratorSlot.png" scale="0.7" />
                // Find the moderator icon.

                cg.updateScreen();
                int fade = 40;

                // Red and blue
                for (int i = 0; i < 12; i++)
                    if (cg.CompareColor(ModeratorLocations[i, 0], ModeratorLocations[i, 1], CALData.ModeratorIconColor, fade))
                        return i;

                // Spectators
                int offset = cg.FindOffset();
                for (int i = 12; i < Queueid; i++)
                    if (cg.CompareColor(ModeratorLocations[i, 0], ModeratorLocations[i, 1] + offset, CALData.SpectatorModeratorIconColor, fade))
                        return i;

                // Queue
                int queuecount = cg.QueueCount;
                for (int i = 12; i < 12 + queuecount; i++)
                    if (cg.CompareColor(ModeratorLocations[i, 0], ModeratorLocations[i, 1] - 5, CALData.SpectatorModeratorIconColor, fade))
                        return i + 6;

                return -1;
            }

            /// <summary>
            /// Gets the team a player in the queue is queueing for.
            /// </summary>
            /// <param name="slot">Slot to check.</param>
            /// <returns>Team that the player is queueing for.</returns>
            /// <exception cref="InvalidSlotException">Thrown if slot argument is out of range.</exception>
            public QueueTeam GetQueueTeam(int slot)
            {
                if (slot < Queueid || slot > Queueid + 5)
                    throw new InvalidSlotException(string.Format("Slot argument '{0}' is out of range.", slot));
                Point check = cg.Interact.FindSlotLocation(slot);
                check.X -= 25;
                cg.updateScreen();
                Color color = cg.GetPixelAt(check.X, check.Y);

                // If the blue color is greater than the red color, the queue slot is on blue team.
                if (color.B > color.R)
                    return QueueTeam.Blue;
                // If the green color is less than 90, the queue slot is on the red team.
                else if (color.G < 90)
                    return QueueTeam.Red;
                // Else, the queue slot is on neutral team.
                else
                    return QueueTeam.Neutral;
            }

            /// <summary>
            /// Checks if the input slot has their ultimate.
            /// </summary>
            /// <param name="slot">Slot to check.</param>
            /// <param name="noUpdate"></param>
            /// <returns>Returns true if player as ultimate.</returns>
            public bool IsUltimateReady(int slot, bool noUpdate = false)
            {
                if (slot >= UltimateCheckLocations.Length || slot < 0)
                    throw new InvalidSlotException(string.Format("Slot argument '{0}' is out of range.", slot));
                if (!noUpdate)
                    cg.updateScreen();
                return /* GetLoadSlots(new int[] { slot }.ToList()).Contains(slot) == false && */ cg.CompareColor(UltimateCheckLocations[slot].X, UltimateCheckLocations[slot].Y, new int[] { 134, 134, 134 }, 5);
            }
            Point[] UltimateCheckLocations = new Point[]
            {
                new Point(59, 72),
                new Point(108, 72),
                new Point(157, 72),
                new Point(207, 72),
                new Point(255, 72),
                new Point(305, 72),

                new Point(612, 72),
                new Point(661, 72),
                new Point(710, 75),
                new Point(759, 75),
                new Point(808, 75),
                new Point(857, 75)
            };

            /// <summary>
            /// Obtains the markup of a hero icon.
            /// </summary>
            /// <param name="slot">Slot to get hero icon from.</param>
            /// <param name="saveTo">Location on the file system to save it to.</param>
            public void GetHeroMarkup(int slot, string saveTo)
            {
                cg.updateScreen();

                Bitmap save = cg.BmpClone(HeroCheckLocations[slot], HeroCheckY, 20, 9);

                save.Save(saveTo);
            }

            /// <summary>
            /// Gets the hero a player is playing.
            /// </summary>
            /// <param name="slot">Slot to check.</param>
            /// <returns>Returns the hero the slot is playing.</returns>
            public Hero? GetHero(int slot)
            {
                HeroResultInfo _ = new HeroResultInfo();
                return GetHero(slot, out _);
            }

            /// <summary>
            /// Gets the hero a player is playing.
            /// </summary>
            /// <param name="slot">Slot to check.</param>
            /// <param name="resultInfo">Why the method returned what it did.</param>
            /// <returns>Returns the hero the slot is playing.</returns>
            public Hero? GetHero(int slot, out HeroResultInfo resultInfo)
            {
                if (!(cg.IsSlotBlue(slot) || cg.IsSlotRed(slot)))
                    throw new InvalidSlotException("Slot is out of range. Slot must be a player on blue or red team.");

                if (!cg.PlayerSlots.Contains(slot))
                {
                    resultInfo = HeroResultInfo.SlotEmpty;
                    return null;
                }

                if (PlayersDead(true).Contains(slot))
                {
                    resultInfo = HeroResultInfo.PlayerWasDead;
                    return null;
                }

                if (!_HeroChosen(slot))
                {
                    resultInfo = HeroResultInfo.NoHeroChosen;
                    return null;
                }

                List<Tuple<Hero, double>> results = new List<Tuple<Hero, double>>();

                for (int m = 0; m < HeroMarkups.Length; m++)
                {
                    if (HeroMarkups[m] != null)
                    {
                        double total = 0;
                        double success = 0;

                        for (int x = 0; x < HeroMarkups[m].Width; x++)
                            for (int y = 0; y < HeroMarkups[m].Height; y++)
                            {
                                int bmpX = HeroCheckLocations[slot] + x;
                                int bmpY = HeroCheckY + y;

                                int[] markupColor = HeroMarkups[m].GetPixelAt(x, y).ToInt();

                                if (markupColor[0] != 0 && markupColor[1] != 0 && markupColor[2] != 0)
                                {
                                    total++;
                                    if (cg.CompareColor(bmpX, bmpY, markupColor, 20))
                                        success++;
                                }
                            }

                        double probability = (success / total) * 100;

                        if (probability >= 80)
                            results.Add(new Tuple<Hero, double>((Hero)m, probability));
                    }
                }

                if (results.Count == 0)
                {
                    resultInfo = HeroResultInfo.NoCompatibleHeroFound;
                    return null;
                }
                else
                {
                    int highestIndex = -1;
                    double highest = 0;

                    for (int i = 0; i < results.Count; i++)
                        if (results[i].Item2 > highest)
                        {
                            highestIndex = i;
                            highest = results[i].Item2;
                        }

                    resultInfo = HeroResultInfo.Success;
                    return results[highestIndex].Item1;
                }
            }
            static Bitmap[] HeroMarkups = new Bitmap[]
            {
                Properties.Resources.ana_markup, // Ana
                Properties.Resources.bastion_markup, // Bastion
                Properties.Resources.brigitte_markup, // Brigitte
                Properties.Resources.dva_markup, // Dva
                Properties.Resources.doomfist_markup, // Doomfist
                Properties.Resources.gengi_markup, // Genji
                Properties.Resources.hanzo_markup, // Hanzo
                Properties.Resources.junkrat_markup, // Junkrat
                Properties.Resources.lucio_markup, // Lucio
                Properties.Resources.mccree_markup, // McCree
                Properties.Resources.mei_markup, // Mei
                Properties.Resources.mercy_markup, // Mercy
                Properties.Resources.moira_markup, // Moira
                Properties.Resources.orisa_markup, // Orisa
                Properties.Resources.pharah_markup, // Pharah
                Properties.Resources.reaper_markup, // Reaper
                Properties.Resources.reinhardt_markup, // Reinhardt
                Properties.Resources.roadhog_markup, // Roadhog
                Properties.Resources.soldier_markup, // Soldier: 76
                Properties.Resources.sombra_markup, // Sombra
                Properties.Resources.symmetra_markup, // Symmetra
                Properties.Resources.torbjorn_markup, // Torbjorn
                Properties.Resources.tracer_markup, // Tracer
                Properties.Resources.widowmaker_markup, // Widowmaker
                Properties.Resources.winston_markup, // Winston
                Properties.Resources.zarya_markup, // Zarya
                Properties.Resources.zenyatta_markup // Zenyatta
            };
            int[] HeroCheckLocations = new int[]
            {
                76,
                125,
                175,
                224,
                273,
                322,

                629,
                678,
                727,
                777,
                826,
                875
            };
            int HeroCheckY = 73;

            /// <summary>
            /// Checks if player exists via battletag. Is case and region sensitive.
            /// </summary>
            /// <param name="battletag">Battletag of player to check. Is case sensitive.</param>
            /// <returns>Returns true if player exists, else returns false.</returns>
            public static bool PlayerExists(string battletag)
            {
                // If the website "https://playoverwatch.com/en-us/career/pc/(BATTLETAGNAME)-(BATTLETAGID)" exists, then the player exists.
                try
                {
                    string playerprofile = "https://playoverwatch.com/en-us/career/pc/" + battletag.Replace('#', '-');

                    using (WebClient wc = new WebClient())
                    {
                        string pageinfo = wc.DownloadString(playerprofile);
                        wc.Dispose();

                        // Check if the career profile page exists by checking if the title of the page starts with C in Career profile.
                        // If it doesn't, it will be a "page doesn't exist" page with the title starting with O in Overwatch.
                        if (pageinfo[pageinfo.IndexOf("<title>") + 7] == 'C')
                            return true;
                    }
                }
                catch (WebException) { }

                return false;
            }
        }
    }
    
    /// <summary>
    /// Teams on the queue.
    /// </summary>
    public enum QueueTeam
    {
        /// <summary>
        /// Queueing for both blue and red.
        /// </summary>
        Neutral,
        /// <summary>
        /// Queueing for blue.
        /// </summary>
        Blue,
        /// <summary>
        /// Queueing for red.
        /// </summary>
        Red
    }

    /// <summary>
    /// Result info of CG_PlayerInfo.GetHero()
    /// </summary>
    public enum HeroResultInfo
    {
        /// <summary>
        /// The hero the player was playing was successfully found.
        /// </summary>
        Success,
        /// <summary>
        /// Can't get the hero the player was playing because the player is dead. Try rescanning when the player is alive again.
        /// </summary>
        PlayerWasDead,
        /// <summary>
        /// The player did not choose a hero.
        /// </summary>
        NoHeroChosen,
        /// <summary>
        /// The slot was empty.
        /// </summary>
        SlotEmpty,
        /// <summary>
        /// Could not tell what hero the player is playing. Chances are if you get this it is a bug with GetHero().
        /// </summary>
        NoCompatibleHeroFound
    }
}
