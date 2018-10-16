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
        const int slotFade = 20;

        static readonly int[] TotalRange     = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 };
        static readonly int[] PlayerRange    = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11                                                 };
        static readonly int[] BlueRange      = new int[] { 0, 1, 2, 3, 4, 5                                                                     };
        static readonly int[] RedRange       = new int[] {                   6, 7, 8, 9, 10, 11                                                 };
        static readonly int[] SpectatorRange = new int[] {                                       12, 13, 14, 15, 16, 17                         };
        static readonly int[] QueueRange     = new int[] {                                                               18, 19, 20, 21, 22, 23 };

        #region Players in red and blue
        /// <summary>
        /// Gets the slots filled in red and blue.
        /// </summary>
        public List<int> PlayerSlots
        {
            get
            {
                return CheckRange(PlayerRange, 0, false);
            }
        }
        /// <summary>
        /// Gets the number of players in red and blue.
        /// </summary>
        public int PlayerCount
        {
            get
            {
                return PlayerSlots.Count;
            }
        }
        #endregion

        #region Players in blue
        /// <summary>
        /// Gets the slots filled in blue.
        /// </summary>
        public List<int> BlueSlots
        {
            get
            {
                return CheckRange(BlueRange, 0, false);
            }
        }
        /// <summary>
        /// Gets the number of players in blue.
        /// </summary>
        public int BlueCount
        {
            get
            {
                return BlueSlots.Count;
            }
        }
        #endregion

        #region Players in red
        /// <summary>
        /// Gets the slots filled in red.
        /// </summary>
        public List<int> RedSlots
        {
            get
            {
                return CheckRange(RedRange, 0, false);
            }
        }
        /// <summary>
        /// Gets the number of players in red.
        /// </summary>
        public int RedCount
        {
            get
            {
                return RedSlots.Count;
            }
        }
        #endregion

        #region Players in spectator
        /// <summary>
        /// Gets the slots filled in spectator.
        /// </summary>
        public List<int> SpectatorSlots
        {
            get
            {
                return CheckRange(SpectatorRange, FindSpectatorOffset(), false);
            }
        }
        /// <summary>
        /// Gets the number of players in spectator.
        /// </summary>
        public int SpectatorCount
        {
            get
            {
                return SpectatorSlots.Count;
            }
        }
        #endregion

        #region Players in queue
        /// <summary>
        /// Gets the slots filled in the queue.
        /// </summary>
        public List<int> QueueSlots
        {
            get
            {
                return CheckRange(QueueRange, 0, false);
            }
        }
        /// <summary>
        /// Gets the number of players in the queue.
        /// </summary>
        public int QueueCount
        {
            get
            {
                return QueueSlots.Count;
            }
        }
        #endregion

        #region All Players
        /// <summary>
        /// Gets all slots filled in the custom game.
        /// </summary>
        public List<int> AllSlots
        {
            get
            {
                return CheckRange(TotalRange, FindSpectatorOffset(), false);
            }
        }
        /// <summary>
        /// Gets the number of players in the custom game.
        /// </summary>
        public int AllCount
        {
            get
            {
                return AllSlots.Count;
            }
        }
        #endregion

        internal static Point[] SlotLocations = new Point[]
        {
            // Blue
            new Point(51, 255), // Slot 0
            new Point(51, 283), // Slot 1
            new Point(51, 311), // Slot 2
            new Point(51, 341), // Slot 3
            new Point(51, 369), // Slot 4
            new Point(51, 384), // Slot 5
            // Red
            new Point(621, 255), // Slot 6
            new Point(621, 283), // Slot 7
            new Point(621, 311), // Slot 8
            new Point(621, 341), // Slot 9
            new Point(621, 369), // Slot 10
            new Point(621, 397), // Slot 11
            // Spectator
            new Point(896, 248), // slot 12
            new Point(896, 264), // slot 13
            new Point(896, 277), // slot 14
            new Point(896, 290), // slot 15
            new Point(896, 304), // slot 16
            new Point(896, 317), // slot 17
        };

        private bool IsSlotFilled(int slot, int yoffset, bool noUpdate)
        {
            lock (CustomGameLock)
            {
                if (!IsSlotValid(slot))
                    throw new InvalidSlotException(slot);

                if (!IsSlotInQueue(slot))
                {
                    if (!noUpdate)
                        updateScreen();

                    int x = SlotLocations[slot].X,
                        y = SlotLocations[slot].Y,
                        compareToX = SlotLocations[slot].X;

                    if (slot == 0)
                        compareToX -= 8;
                    else if (IsSlotInQueue(slot) || IsSlotSpectator(slot))
                        compareToX -= 3;
                    else if (IsSlotBlue(slot))
                        compareToX -= 4;
                    else if (IsSlotRed(slot))
                        compareToX += 3;

                    if (IsSlotSpectator(slot))
                        y += yoffset;

                    return !CompareColor(x, y, compareToX, y, slotFade);
                }
                else
                {
                    return GetQueueCount(false, noUpdate) + Queueid > slot;
                }
            }
        }

        private List<int> CheckRange(int[] slotsToCheck, int yoffset, bool noUpdate)
        {
            if (!noUpdate)
                updateScreen();
            List<int> slots = new List<int>();
            foreach (int slot in slotsToCheck)
                if (IsSlotFilled(slot, yoffset, true))
                    slots.Add(slot);
            return slots;
        }

        private int GetQueueCount(bool includeHidden, bool noUpdate)
        {
            lock (CustomGameLock)
            {
                if (!noUpdate)
                    updateScreen();

                int inq = 0;

                // <image url="$(ProjectDir)\ImageComments\GetInfo.cs\Offset.png" scale="1.3" />
                // The SPECTATORS text moves down for every player in the queue. Check for all possible locations for the SPECTATORS text.
                for (int i = 0; i < 6; i++)
                    if (CompareColor(727, 266 + (i * 13), new int[] { 132, 147, 151 }, slotFade))
                        inq = i + 1;
                // If there are more than 6 players in the queue, a scrollbar appears to show the rest of the players in the queue.
                // Check for the length of the scrollbar to get the number of players in the queue
                if (inq == 6 && includeHidden)
                {
                    // There can only be 10 players in the queue, which means with a full queue 4 can be hidden.
                    for (int i = 0; i < 4; i++)
                    {
                        int y = 304 - (i * (10 - i));
                        if (CompareColor(894, y, new int[] { 153, 153, 152 }, slotFade)
                            || CompareColor(894, y, new int[] { 132, 126, 123 }, slotFade))
                        {
                            inq = inq + i + 1;
                            break;
                        }
                    }
                }
                return inq;
            }
        }

        // Finds the offset in pixels of the queue to spectator displacement
        internal int FindSpectatorOffset(bool noUpdate = false)
        {
            if (!noUpdate)
                updateScreen();

            int inq = GetQueueCount(false, false);

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
        public static bool IsSlotValid(int slot)
        {
            return slot >= 0 && slot < Queueid + 6;
        }
        /// <summary>
        /// Returns true if the slot is in blue.
        /// </summary>
        /// <param name="slot">Slot to check</param>
        /// <returns></returns>
        public static bool IsSlotBlue(int slot)
        {
            return slot >= 0 && slot <= 5;
        }
        /// <summary>
        /// Returns true if the slot is in red.
        /// </summary>
        /// <param name="slot">Slot to check</param>
        /// <returns></returns>
        public static bool IsSlotRed(int slot)
        {
            return slot >= 6 && slot <= 11;
        }
        /// <summary>
        /// Returns true if the slot is in blue or red.
        /// </summary>
        /// <param name="slot">Slot to check</param>
        /// <returns></returns>
        public static bool IsSlotBlueOrRed(int slot)
        {
            return IsSlotBlue(slot) || IsSlotRed(slot);
        }
        /// <summary>
        /// Returns true if the slot is in Spectator.
        /// </summary>
        /// <param name="slot">Slot to check</param>
        /// <returns></returns>
        public static bool IsSlotSpectator(int slot)
        {
            return slot >= 12 && slot < Queueid;
        }
        /// <summary>
        /// Returns true if the slot is in queue.
        /// </summary>
        /// <param name="slot">Slot to check</param>
        /// <returns></returns>
        public static bool IsSlotInQueue(int slot)
        {
            return slot >= Queueid && slot < Queueid + 6;
        }
        /// <summary>
        /// Returns true if the slot is in spectator or queue.
        /// </summary>
        /// <param name="slot">Slot to check</param>
        /// <returns></returns>
        public static bool IsSlotSpectatorOrQueue(int slot)
        {
            return IsSlotSpectator(slot) || IsSlotInQueue(slot);
        }
        #endregion

        /// <summary>
        /// Gets slots in the game.
        /// </summary>
        /// <param name="flags">Flags for obtaining slots.</param>
        /// <param name="noUpdate"></param>
        /// <returns>Returns a list of slots following <paramref name="flags"/>.</returns>
        /// <example>
        /// The code below will write all blue players that are not AI to the console.
        /// <code>
        /// CustomGame cg = new CustomGame();
        /// 
        /// List&lt;int&gt; bluePlayerSlots = cg.GetSlots(SlotFlags.Blue | SlotFlags.NoAI);
        /// 
        /// Console.WriteLine(string.Join(", ", bluePlayerSlots));
        /// </code>
        /// The code below will write all AI queueing for red to the console.
        /// <code>
        /// CustomGame cg = new CustomGame();
        /// 
        /// List&lt;int&gt; redQueueAISlots = cg.GetSlots(SlotFlags.NoPlayers | SlotFlags.RedQueue);
        /// 
        /// Console.WriteLine(string.Join(", ", redQueueAISlots));
        /// </code>
        /// </example>
        /// <seealso cref="SlotFlags"/>
        public List<int> GetSlots(SlotFlags flags, bool noUpdate = false)
        {
            lock (CustomGameLock)
            {
                List<int> slots = new List<int>();

                if (!noUpdate)
                    updateScreen();

                // Add the blue slots
                if (flags.HasFlag(SlotFlags.BlueTeam))
                    slots.AddRange(CheckRange(BlueRange, 0, true));

                // Add the red slots
                if (flags.HasFlag(SlotFlags.RedTeam))
                    slots.AddRange(CheckRange(RedRange, 0, true));

                // Add the spectator slots
                if (flags.HasFlag(SlotFlags.Spectators))
                    slots.AddRange(CheckRange(SpectatorRange, FindSpectatorOffset(true), true));

                // Add the queue slots
                if (flags.HasFlag(SlotFlags.NeutralQueue) || flags.HasFlag(SlotFlags.RedQueue) || flags.HasFlag(SlotFlags.BlueQueue))
                {
                    slots.AddRange(CheckRange(QueueRange, 0, true).Where((slot) =>
                    {
                        QueueTeam team = PlayerInfo.GetQueueTeam(slot, true);

                        return team == QueueTeam.Neutral && flags.HasFlag(SlotFlags.NeutralQueue)
                        || team == QueueTeam.Blue && flags.HasFlag(SlotFlags.BlueQueue)
                        || team == QueueTeam.Red && flags.HasFlag(SlotFlags.RedQueue);
                    }));
                }

                if (flags.HasFlag(SlotFlags.NoPlayers) || flags.HasFlag(SlotFlags.NoAI))
                {
                    List<int> aiSlots = AI.GetAISlots(flags.HasFlag(SlotFlags.AccurateGetAI));

                    if (flags.HasFlag(SlotFlags.NoAI))
                        slots = slots.Where(slot => aiSlots.Contains(slot) == false).ToList();

                    if (flags.HasFlag(SlotFlags.NoPlayers))
                        slots = slots.Where(slot => aiSlots.Contains(slot)).ToList();
                }

                return slots;
            }
        }

        /// <summary>
        /// Waits for the slots in overwatch to change.
        /// </summary>
        /// <param name="maxtime">Time to wait. Set to -1 to wait forever.</param>
        /// <returns>Returns true if Overwatch's slots changed. Returns false if the time ran out.</returns>
        public bool WaitForSlotUpdate(int maxtime = 1000)
        {
            lock (CustomGameLock)
            {
                Stopwatch time = new Stopwatch();
                List<int> preslots = AllSlots;
                time.Start();
                while (time.ElapsedMilliseconds < maxtime || maxtime == -1)
                {
                    List<int> newslots = AllSlots;
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
        }

        /// <summary>
        /// Info about players in an Overwatch custom game.
        /// </summary>
        public PlayerInfo PlayerInfo { get; private set; }
    }

    /// <summary>
    /// Info about players in an Overwatch custom game.
    /// </summary>
    /// <remarks>
    /// The PlayerInfo class is accessed in a CustomGame object on the <see cref="CustomGame.PlayerInfo"/> field.
    /// </remarks>
    public class PlayerInfo : CustomGameBase
    {
        internal PlayerInfo(CustomGame cg) : base(cg) { }

        /// <summary>
        /// Gets a list of players who died.
        /// </summary>
        /// <param name="noUpdate"></param>
        /// <returns>List of players who are dead.</returns>
        /// <include file='docs.xml' path='doc/AddAI/example'></include>
        public List<int> PlayersDead(bool noUpdate = false)
        {
            lock (cg.CustomGameLock)
            {
                // Returns which players that are dead by checking the killed marker locations for a red 'X'.
                // <image url="$(ProjectDir)\ImageComments\GetInfo.cs\DeadPlayers.png" scale="1.3" />

                if (!noUpdate)
                    cg.updateScreen();
                List<int> playersDead = new List<int>();
                for (int i = 0; i < 12; i++)
                    if (cg.CompareColor(KilledPlayerMarkerLocations[i], 98, Colors.DEAD_PLAYER, Fades.DEAD_PLAYER)
                        && !HasHealthBar(i, true))
                        playersDead.Add(i);
                return playersDead;
            }
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
            lock (cg.CustomGameLock)
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
        }

        /// <summary>
        /// Test if input slot has chosen a hero.
        /// </summary>
        /// <param name="slot">Slot to check</param>
        /// <returns>Returns true if a hero is chosen.</returns>
        /// <include file='docs.xml' path='doc/exceptions/invalidslot/exception'/>
        public bool IsHeroChosen(int slot)
        {
            lock (cg.CustomGameLock)
            {
                if (!(CustomGame.IsSlotBlue(slot) || CustomGame.IsSlotRed(slot)))
                    throw new InvalidSlotException(slot);
                if (cg.PlayerSlots.Contains(slot) == false)
                    return false;

                cg.updateScreen();

                if (PlayersDead(true).Contains(slot))
                    return true;

                return _HeroChosen(slot);
            }
        }

        // Private method to check if a hero is chosen.
        // The screen is not updated, run updateScreen() beforehand.
        // There is no argument checking.
        bool _HeroChosen(int slot)
        {
            if (CustomGame.IsSlotBlue(slot))
            {
                return !cg.CompareColor(HeroCheckLocations[slot] + 6, HeroCheckY + 3, Colors.HERO_CHOSEN_BLUE, Fades.HEROES_CHOSEN);
            }
            else
            {
                //return !cg.CompareColor(CALData.HeroChosenLocations[slot], CALData.HeroChosenY, CALData.HeroChosenRed, CALData.HeroChosenFade);
                return !cg.CompareColor(HeroCheckLocations[slot] + 6, HeroCheckY + 3, Colors.HERO_CHOSEN_RED, Fades.HEROES_CHOSEN);
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
            lock (cg.CustomGameLock)
            {
                // <image url="$(ProjectDir)\ImageComments\GetInfo.cs\ModeratorSlot.png" scale="0.7" />
                // Find the moderator icon.

                cg.updateScreen();
                int fade = 40;

                // Red and blue
                for (int i = 0; i < 12; i++)
                    if (cg.CompareColor(ModeratorLocations[i, 0], ModeratorLocations[i, 1], Colors.MODERATOR_ICON, fade))
                        return i;

                // Spectators
                int offset = cg.FindSpectatorOffset();
                for (int i = 12; i < CustomGame.Queueid; i++)
                    if (cg.CompareColor(ModeratorLocations[i, 0], ModeratorLocations[i, 1] + offset, Colors.SPECTATOR_MODERATOR_ICON, fade))
                        return i;

                // Queue
                int queuecount = cg.QueueCount;
                for (int i = 12; i < 12 + queuecount; i++)
                    if (cg.CompareColor(ModeratorLocations[i, 0], ModeratorLocations[i, 1] - 5, Colors.SPECTATOR_MODERATOR_ICON, fade))
                        return i + 6;

                return -1;
            }
        }

        /// <summary>
        /// Gets the team a player in the queue is queueing for.
        /// </summary>
        /// <param name="slot">Slot to check.</param>
        /// <param name="noUpdate"></param>
        /// <returns>Team that the player is queueing for.</returns>
        /// <include file='docs.xml' path='doc/exceptions/invalidslot/exception'/>
        public QueueTeam GetQueueTeam(int slot, bool noUpdate = false)
        {
            lock (cg.CustomGameLock)
            {
                if (!CustomGame.IsSlotInQueue(slot))
                    throw new InvalidSlotException(slot);
                Point check = cg.Interact.FindSlotLocation(slot);
                check.X -= 25;
                if (!noUpdate)
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
        }

        /// <summary>
        /// Checks if the input slot has their ultimate.
        /// </summary>
        /// <param name="slot">Slot to check.</param>
        /// <param name="noUpdate"></param>
        /// <returns>Returns true if player as ultimate.</returns>
        /// <include file='docs.xml' path='doc/exceptions/invalidslot/exception'/>
        public bool IsUltimateReady(int slot, bool noUpdate = false)
        {
            lock (cg.CustomGameLock)
            {
                if (!(CustomGame.IsSlotBlue(slot) || CustomGame.IsSlotRed(slot)))
                    throw new InvalidSlotException(slot);
                if (!noUpdate)
                    cg.updateScreen();
                return cg.CompareColor(UltimateCheckLocations[slot].X, UltimateCheckLocations[slot].Y, new int[] { 134, 134, 134 }, 5);
            }
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
        /// <include file='docs.xml' path='doc/exceptions/invalidslot/exception'/>
        public void GetHeroMarkup(int slot, string saveTo)
        {
            lock (cg.CustomGameLock)
            {
                if (!(CustomGame.IsSlotBlue(slot) || CustomGame.IsSlotRed(slot)))
                    throw new InvalidSlotException(slot);

                cg.updateScreen();

                Bitmap save = cg.BmpClone(HeroCheckLocations[slot], HeroCheckY, 20, 9);

                save.Save(saveTo);
            }
        }

        /// <summary>
        /// Gets the hero a player is playing.
        /// </summary>
        /// <param name="slot">Slot to check.</param>
        /// <returns>Returns the hero the slot is playing.</returns>
        /// <include file='docs.xml' path='doc/exceptions/invalidslot/exception'/>
        public Hero? GetHero(int slot)
        {
            HeroResultInfo _;
            return GetHero(slot, out _);
        }

        /// <summary>
        /// Gets the hero a player is playing.
        /// </summary>
        /// <param name="slot">Slot to check.</param>
        /// <param name="resultInfo">Info about the returned value.</param>
        /// <returns>Returns the hero the slot is playing.</returns>
        /// <include file='docs.xml' path='doc/exceptions/invalidslot/exception'/>
        public Hero? GetHero(int slot, out HeroResultInfo resultInfo)
        {
            lock (cg.CustomGameLock)
            {
                if (!(CustomGame.IsSlotBlue(slot) || CustomGame.IsSlotRed(slot)))
                    throw new InvalidSlotException(string.Format("Slot {0} is out of range. Slot must be a player on blue or red team.", slot));

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

                for (int m = 0; m < Markups.HERO_MARKUPS.Length; m++)
                {
                    if (Markups.HERO_MARKUPS[m] != null)
                    {
                        double total = 0;
                        double success = 0;

                        for (int x = 0; x < Markups.HERO_MARKUPS[m].Width; x++)
                            for (int y = 0; y < Markups.HERO_MARKUPS[m].Height; y++)
                            {
                                int bmpX = HeroCheckLocations[slot] + x;
                                int bmpY = HeroCheckY + y;

                                int[] markupColor = Markups.HERO_MARKUPS[m].GetPixelAt(x, y).ToInt();

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
        }
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
        /// Checks if a slot is your friend.
        /// </summary>
        /// <param name="slot"></param>
        /// <returns></returns>
        public bool IsFriend(int slot)
        {
            return cg.Interact.PeakOption(slot, Markups.REMOVE_FRIEND);
        }

        /// <summary>
        /// Checks if a player account exists via battletag. Is case sensitive.
        /// </summary>
        /// <param name="battletag">Battletag of player to check. Is case sensitive.</param>
        /// <returns>Returns true if player exists, else returns false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="battletag"/> is null.</exception>
        public static bool PlayerExists(string battletag)
        {
            if (battletag == null)
                throw new ArgumentNullException("battletag", "Battletag was null.");

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

    /// <summary>
    /// Result info of GetHero().
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

    /// <summary>
    /// Flags for obtaining slots.
    /// </summary>
    /// <seealso cref="CustomGame.GetSlots(SlotFlags, bool)"/>
    [Flags]
    public enum SlotFlags
    {
        /// <summary>
        /// Get blue slots.
        /// </summary>
        BlueTeam = 1 << 0,
        /// <summary>
        /// Get red slots.
        /// </summary>
        RedTeam = 1 << 1,
        /// <summary>
        /// Get spectator slots.
        /// </summary>
        Spectators = 1 << 2,
        /// <summary>
        /// Get neutral queue slots.
        /// </summary>
        NeutralQueue = 1 << 3,
        /// <summary>
        /// Get red queue slots
        /// </summary>
        RedQueue = 1 << 4,
        /// <summary>
        /// Get blue queue slots
        /// </summary>
        BlueQueue = 1 << 5,
        /// <summary>
        /// Get queue slots
        /// </summary>
        Queue = NeutralQueue | RedQueue | BlueQueue,
        /// <summary>
        /// No players, only AI.
        /// </summary>
        NoPlayers = 1 << 6,
        /// <summary>
        /// No AI, only players.
        /// </summary>
        NoAI = 1 << 7,
        /// <summary>
        /// Reliably gets the (non)AI, however is a lot slower.
        /// </summary>
        AccurateGetAI = 1 << 8,
    }
}
