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

        private bool IsSlotFilled(int slot, int yoffset, bool noUpdate)
        {
            using (LockHandler.Passive)
            {
                if (!IsSlotValid(slot))
                    throw new InvalidSlotException(slot);

                if (!IsSlotInQueue(slot))
                {
                    if (!noUpdate)
                        UpdateScreen();

                    int x = Points.SLOT_LOCATIONS[slot].X,
                        y = Points.SLOT_LOCATIONS[slot].Y;

                    if (IsDeathmatch(true))
                    {
                        if (IsSlotBlue(slot))
                        {
                            x += Distances.LOBBY_SLOT_DM_BLUE_X_OFFSET;
                            y += Distances.LOBBY_SLOT_DM_Y_OFFSET;
                        }
                        else if (IsSlotRed(slot))
                        {
                            x += Distances.LOBBY_SLOT_DM_RED_X_OFFSET;
                            y += Distances.LOBBY_SLOT_DM_Y_OFFSET;
                        }
                    }

                    int compareToX = Points.SLOT_LOCATIONS[slot].X;

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

                    return !Capture.CompareColor(x, y, compareToX, y, Fades.SLOT_FADE);
                }
                else
                {
                    return GetQueueCount(false, noUpdate) + QueueID > slot;
                }
            }
        }

        private List<int> CheckRange(int[] slotsToCheck, int yoffset, bool noUpdate)
        {
            if (!noUpdate)
                UpdateScreen();
            List<int> slots = new List<int>();
            foreach (int slot in slotsToCheck)
                if (IsSlotFilled(slot, yoffset, true))
                    slots.Add(slot);
            return slots;
        }

        private int GetQueueCount(bool includeHidden, bool noUpdate)
        {
            using (LockHandler.Passive)
            {
                if (!noUpdate)
                    UpdateScreen();

                int inq = 0;

                // <image url="$(ProjectDir)\ImageComments\GetInfo.cs\Offset.png" scale="1.3" />
                // The SPECTATORS text moves down for every player in the queue. Check for all possible locations for the SPECTATORS text.
                for (int i = 0; i < 6; i++)
                    if (Capture.CompareColor(727, 266 + (i * 13), new int[] { 132, 147, 151 }, Fades.SLOT_FADE))
                        inq = i + 1;
                // If there are more than 6 players in the queue, a scrollbar appears to show the rest of the players in the queue.
                // Check for the length of the scrollbar to get the number of players in the queue
                if (inq == 6 && includeHidden)
                {
                    // There can only be 10 players in the queue, which means with a full queue 4 can be hidden.
                    for (int i = 0; i < 4; i++)
                    {
                        int y = 304 - (i * (10 - i));
                        if (Capture.CompareColor(894, y, new int[] { 153, 153, 152 }, Fades.SLOT_FADE)
                            || Capture.CompareColor(894, y, new int[] { 132, 126, 123 }, Fades.SLOT_FADE))
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
                UpdateScreen();

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
            return slot >= 0 && slot < SlotCount;
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
            return slot >= 12 && slot < QueueID;
        }
        /// <summary>
        /// Returns true if the slot is in queue.
        /// </summary>
        /// <param name="slot">Slot to check</param>
        /// <returns></returns>
        public static bool IsSlotInQueue(int slot)
        {
            return slot >= QueueID && slot < QueueID + 6;
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
            using (LockHandler.Passive)
            {
                List<int> slots = new List<int>();

                if (!noUpdate)
                    UpdateScreen();

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

                // Filter players/AI slots
                if (flags.HasFlag(SlotFlags.PlayersOnly) || flags.HasFlag(SlotFlags.AIOnly))
                {
                    List<int> aiSlots = AI.GetAISlots(flags.HasFlag(SlotFlags.AccurateGetAI));

                    // Make the list AI only.
                    if (flags.HasFlag(SlotFlags.AIOnly))
                        slots = slots.Where(slot => aiSlots.Contains(slot)).ToList();
                    // Make the list players only.
                    if (flags.HasFlag(SlotFlags.PlayersOnly))
                        slots = slots.Where(slot => !aiSlots.Contains(slot)).ToList();
                }

                // Filter alive/dead slots
                if (flags.HasFlag(SlotFlags.DeadOnly) || flags.HasFlag(SlotFlags.AliveOnly))
                {
                    List<int> deadPlayers = PlayerInfo.GetDeadSlots(true);

                    // Make the list dead slots only.
                    if (flags.HasFlag(SlotFlags.DeadOnly))
                        slots = slots.Where(slot => deadPlayers.Contains(slot)).ToList();
                    // Make the list alive slots only.
                    if (flags.HasFlag(SlotFlags.AliveOnly))
                        slots = slots.Where(slot => !deadPlayers.Contains(slot)).ToList();
                }

                return slots;
            }
        }

        internal bool IsDeathmatch(bool noUpdate = false)
        {
            if (!noUpdate)
                UpdateScreen();
            return !Capture.CompareColor(327, 302, new int[] { 172, 173, 175 }, 15);
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
        public List<int> GetDeadSlots(bool noUpdate = false)
        {
            using (cg.LockHandler.Passive)
            {
                // Returns which players that are dead by checking the killed marker locations for a red 'X'.
                // <image url="$(ProjectDir)\ImageComments\GetInfo.cs\DeadPlayers.png" scale="1.3" />

                if (!noUpdate)
                    cg.UpdateScreen();
                List<int> playersDead = new List<int>();
                for (int i = 0; i < 12; i++)
                    if (Capture.CompareColor(Points.KILLED_PLAYER_MARKERS[i], Points.KILLED_PLAYER_MARKER_Y, Colors.DEAD_PLAYER, Fades.DEAD_PLAYER)
                        && !HasHealthBar(i, true))
                        playersDead.Add(i);
                return playersDead;
            }
        }

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
                cg.UpdateScreen();

            for (int x = healthBarLocations[slot]; x < healthBarLocations[slot] + xLength; x++)
                if (Capture.CompareColor(x, y, new int[] { 110, 110, 110 }, 10))
                    return true;
            return false;
        }

        /// <summary>
        /// Get max player count for both teams.
        /// </summary>
        /// <returns>Returns an int[] where [0] is blue max player count and [1] is red max player count.</returns>
        public int[] MaxPlayerCount()
        {
            using (cg.LockHandler.Passive)
            {
                // <image url="$(ProjectDir)\ImageComments\GetInfo.cs\MaxPlayers.png" scale="1" />
                cg.UpdateScreen();
                int[] result = new int[2];
                // Blue: X = 613
                // Red: X = 752
                int[] searchArea = new int[] { 250, 275, 310, 340, 370, 400 };
                int[] blueColor = new int[] { 148, 202, 224 };
                int[] redColor = new int[] { 167, 76, 86 };
                for (int i = 0; i < searchArea.Length; i++)
                    if (Capture.CompareColor(302, searchArea[i], blueColor, 50))
                        result[0]++;
                for (int i = 0; i < searchArea.Length; i++)
                    if (Capture.CompareColor(370, searchArea[i], redColor, 50))
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
            using (cg.LockHandler.Passive)
            {
                if (!(CustomGame.IsSlotBlue(slot) || CustomGame.IsSlotRed(slot)))
                    throw new InvalidSlotException(slot);
                if (cg.PlayerSlots.Contains(slot) == false)
                    return false;

                cg.UpdateScreen();

                if (GetDeadSlots(true).Contains(slot))
                    return true;

                return _HeroChosen(slot);
            }
        }

        bool _HeroChosen(int slot)
        {
            if (CustomGame.IsSlotBlue(slot))
            {
                return !Capture.CompareColor(Points.HERO_LOCATIONS[slot] + 6, Points.HERO_Y + 3, Colors.HERO_CHOSEN_BLUE, Fades.HEROES_CHOSEN);
            }
            else
            {
                //return !cg.CompareColor(CALData.HeroChosenLocations[slot], CALData.HeroChosenY, CALData.HeroChosenRed, CALData.HeroChosenFade);
                return !Capture.CompareColor(Points.HERO_LOCATIONS[slot] + 6, Points.HERO_Y + 3, Colors.HERO_CHOSEN_RED, Fades.HEROES_CHOSEN);
            }
        }
        
        /// <summary>
        /// Gets the slot the moderator is on.
        /// </summary>
        /// <returns>Slot the moderator is on.</returns>
        public int ModeratorSlot()
        {
            using (cg.LockHandler.Passive)
            {
                // <image url="$(ProjectDir)\ImageComments\GetInfo.cs\ModeratorSlot.png" scale="0.7" />
                // Find the moderator icon.

                cg.UpdateScreen();
                int fade = 40;

                // Red and blue
                for (int i = 0; i < 12; i++)
                    if (Capture.CompareColor(Points.MODERATOR_ICON_LOCATIONS[i], Colors.MODERATOR_ICON, fade))
                        return i;

                // Spectators
                int offset = cg.FindSpectatorOffset();
                for (int i = 12; i < CustomGame.QueueID; i++)
                    if (Capture.CompareColor(Points.MODERATOR_ICON_LOCATIONS[i].X, Points.MODERATOR_ICON_LOCATIONS[i].Y + offset, Colors.SPECTATOR_MODERATOR_ICON, fade))
                        return i;

                // Queue
                int queuecount = cg.QueueCount;
                for (int i = 12; i < 12 + queuecount; i++)
                    if (Capture.CompareColor(Points.MODERATOR_ICON_LOCATIONS[i].X, Points.MODERATOR_ICON_LOCATIONS[i].Y - Distances.LOBBY_QUEUE_OFFSET, Colors.SPECTATOR_MODERATOR_ICON, fade))
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
            using (cg.LockHandler.Passive)
            {
                if (!CustomGame.IsSlotInQueue(slot))
                    throw new InvalidSlotException(slot);
                Point check = cg.Interact.FindSlotLocation(slot);
                if (!noUpdate)
                    cg.UpdateScreen();
                Color color = Capture.GetPixel(check.X, check.Y);

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
            using (cg.LockHandler.Passive)
            {
                if (!(CustomGame.IsSlotBlue(slot) || CustomGame.IsSlotRed(slot)))
                    throw new InvalidSlotException(slot);
                if (!noUpdate)
                    cg.UpdateScreen();
                return Capture.CompareColor(Points.ULTIMATE_LOCATIONS[slot], new int[] { 134, 134, 134 }, 5);
            }
        }

        /// <summary>
        /// Obtains the markup of a hero icon.
        /// </summary>
        /// <param name="slot">Slot to get hero icon from.</param>
        /// <include file='docs.xml' path='doc/exceptions/invalidslot/exception'/>
        public Bitmap GetHeroMarkup(int slot)
        {
            using (cg.LockHandler.Passive)
            {
                if (!CustomGame.IsSlotBlueOrRed(slot))
                    throw new InvalidSlotException(slot);

                cg.UpdateScreen();

                return cg.Capture.CloneAsBitmap(Points.HERO_LOCATIONS[slot], Points.HERO_Y, 20, 9);
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
            using (cg.LockHandler.Passive)
            {
                if (!CustomGame.IsSlotBlueOrRed(slot))
                    throw new InvalidSlotException(string.Format("Slot {0} is out of range. Slot must be a player on blue or red team.", slot));

                if (!cg.PlayerSlots.Contains(slot))
                {
                    resultInfo = HeroResultInfo.SlotEmpty;
                    return null;
                }

                if (GetDeadSlots(true).Contains(slot))
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
                                int bmpX = Points.HERO_LOCATIONS[slot] + x;
                                int bmpY = Points.HERO_Y + y;

                                int[] markupColor = Markups.HERO_MARKUPS[m].GetPixel(x, y).ToInt();

                                if (markupColor[0] != 0 && markupColor[1] != 0 && markupColor[2] != 0)
                                {
                                    total++;
                                    if (Capture.CompareColor(bmpX, bmpY, markupColor, 20))
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

        /// <summary>
        /// Checks if a slot is your friend.
        /// </summary>
        /// <param name="slot"></param>
        /// <returns></returns>
        public bool IsFriend(int slot)
        {
            return cg.Interact.PeakOption(slot, Markups.REMOVE_FRIEND);
        }
    }
}
