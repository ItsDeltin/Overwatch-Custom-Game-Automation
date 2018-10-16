using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Deltin.CustomGameAutomation
{
    partial class CustomGame
    {
        /// <summary>
        /// AI settings for Overwatch.
        /// </summary>
        public AI AI { get; private set; }
    }

    /// <summary>
    /// AI settings for Overwatch.
    /// </summary>
    /// <remarks>
    /// The AI class is accessed in a CustomGame object on the <see cref="CustomGame.AI"/> field.
    /// </remarks>
    public class AI : CustomGameBase
    {
        internal AI(CustomGame cg) : base(cg) { }

        /// <summary>
        /// Add AI to the game.
        /// </summary>
        /// <param name="hero">Hero type to add.</param>
        /// <param name="difficulty">Difficulty of hero.</param>
        /// <param name="team">Team that AI joins. Can be red, blue, or both.</param>
        /// <param name="count">Amount of AI that is added. Set to -1 for max. Default is -1</param>
        /// <returns>Returns false if no AI can be added.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="count"/> is less than -1 or <paramref name="team"/> is Spectator or Queue.</exception>
        /// <include file='docs.xml' path='doc/AddAI/example'></include>
        public bool AddAI(AIHero hero, Difficulty difficulty, Team team, int count = -1)
        {
            lock (cg.CustomGameLock)
            {
                if (team.HasFlag(Team.Queue) || team.HasFlag(Team.Spectator))
                    throw new ArgumentOutOfRangeException("team", team, "Team cannot be Spectator or Queue.");

                if (count < -1)
                    throw new ArgumentOutOfRangeException("count", count, "AI count must be at least -1.");

                cg.updateScreen();

                if (cg.DoesAddButtonExist())
                /*
                 * If the blue shade of the "Move" button is there, that means that the Add AI button is there. 
                 * If the Add AI button is missing, we can't add AI, so return false. If it is there, add the bots.
                 * The AI button will be missing if the server is full
                 */
                {
                    if (cg.OpenChatIsDefault)
                        cg.Chat.CloseChat();

                    // Open AddAI menu.
                    cg.MoveMouseTo(Points.LOBBY_ADD_AI);
                    cg.WaitForUpdate(Points.LOBBY_ADD_AI, 20, 2000);
                    cg.LeftClick(Points.LOBBY_ADD_AI, 500);

                    List<Keys> press = new List<Keys>();

                    if (hero != AIHero.Recommended)
                    {
                        press.Add(Keys.Space);
                        int heroid = (int)hero;
                        for (int i = 0; i < heroid; i++)
                            press.Add(Keys.Down);
                        press.Add(Keys.Space);
                    }

                    press.Add(Keys.Down);

                    if (difficulty != Difficulty.Easy)
                    {
                        press.Add(Keys.Space);
                        int difficultyID = (int)difficulty;
                        for (int i = 0; i < difficultyID; i++)
                            press.Add(Keys.Down);
                        press.Add(Keys.Space);
                    }

                    press.Add(Keys.Down);
                    press.Add(Keys.Down);

                    if (team != Team.BlueAndRed)
                    {
                        press.Add(Keys.Space);
                        int teamID = (int)team;
                        for (int i = 0; i < teamID; i++)
                            press.Add(Keys.Down);
                        press.Add(Keys.Space);
                    }

                    if (count > 0)
                    {
                        press.Add(Keys.Up);
                        for (int i = 0; i < 12; i++)
                            press.Add(Keys.Left);
                        for (int i = 0; i < count; i++)
                            press.Add(Keys.Right);
                        press.Add(Keys.Down);
                    }

                    press.Add(Keys.Down);
                    press.Add(Keys.Space);

                    cg.KeyPress(press.ToArray());

                    //cg.//ResetMouse();

                    if (cg.OpenChatIsDefault)
                        cg.Chat.OpenChat();

                    return true;
                }
                else
                    return false;
            }
        }

        /// <summary>
        /// Obtains the markup of an AI's difficulty.
        /// </summary>
        /// <param name="scalar">Garanteed index of difficulty. (0 = easy, 1 = medium, 2 = hard)</param>
        /// <param name="saveAt">Location to save markup at.</param>
        public void GetAIDifficultyMarkup(int scalar, string saveAt)
        {
            lock (cg.CustomGameLock)
            {
                cg.updateScreen();
                int[] scales = new int[] { 33, 49, 34 };
                Bitmap tmp = cg.BmpClone(402, 244, scales[scalar], 16);
                for (int x = 0; x < tmp.Width; x++)
                    for (int y = 0; y < tmp.Height; y++)
                    {
                        if (tmp.CompareColor(x, y, Colors.WHITE, 25))
                            tmp.SetPixel(x, y, Color.Black);
                        else
                            tmp.SetPixel(x, y, Color.White);
                    }
                if (cg.debugmode)
                {
                    int scale = 5;
                    cg.g.DrawImage(tmp, new Rectangle(750, 0, tmp.Width * scale, tmp.Height * scale));
                }
                tmp.Save(saveAt);
                tmp.Dispose();
            }
        }

        /// <summary>
        /// Gets the difficulty of the AI in the input slot.
        /// <para>If the input slot is not an AI, returns null.
        /// If checking an AI's difficulty in the queue, it will always return easy, or null if it is a player.</para>
        /// </summary>
        /// <param name="slot">Slot to check</param>
        /// <param name="noUpdate"></param>
        /// <returns>Returns the if the difficulty is found. Returns null if the input slot is not an AI.</returns>
        /// <include file='docs.xml' path='doc/exceptions/invalidslot/exception'/>
        public Difficulty? GetAIDifficulty(int slot, bool noUpdate = false)
        {
            lock (cg.CustomGameLock)
            {
                if (!CustomGame.IsSlotValid(slot))
                    throw new InvalidSlotException(slot);

                if (slot == 5 && cg.OpenChatIsDefault)
                    cg.Chat.CloseChat();

                if (!noUpdate)
                    cg.updateScreen();

                if (CustomGame.IsSlotBlue(slot) || CustomGame.IsSlotRed(slot))
                {
                    List<int> rl = new List<int>(); // Likelyhood in percent for difficulties.
                    List<Difficulty> dl = new List<Difficulty>(); // Difficulty

                    bool foundWhite = false;
                    int foundWhiteIndex = 0;
                    int maxWhite = 3;
                    // For each check length in IsAILocations
                    for (int xi = 0; xi < DifficultyLocations[slot, 2] && foundWhiteIndex < maxWhite; xi++)
                    {
                        if (foundWhite)
                            foundWhiteIndex++;

                        Color cc = cg.GetPixelAt(DifficultyLocations[slot, 0] + xi, DifficultyLocations[slot, 1]);
                        // Check for white color of text
                        if (cg.CompareColor(DifficultyLocations[slot, 0] + xi, DifficultyLocations[slot, 1], Colors.WHITE, 110)
                            && (slot > 5 || cc.B - cc.R < 20))
                        {
                            foundWhite = true;

                            // For each difficulty markup
                            for (int b = 0; b < Markups.DIFFICULTY_MARKUPS.Length; b++)
                            {

                                // Check if bitmap matches checking area
                                double success = 0;
                                double total = 0;
                                for (int x = 0; x < Markups.DIFFICULTY_MARKUPS[b].Width; x++)
                                    for (int y = Markups.DIFFICULTY_MARKUPS[b].Height - 1; y >= 0; y--)
                                    {
                                        // If the color pixel of the markup is not white, check if valid.
                                        Color pc = Markups.DIFFICULTY_MARKUPS[b].GetPixel(x, y);
                                        if (pc != Color.FromArgb(255, 255, 255, 255))
                                        {
                                            // tc is true if the pixel is black, false if it is red.
                                            bool tc = pc == Color.FromArgb(255, 0, 0, 0);

                                            total++; // Indent the total
                                                     // If the checking color in the bmp bitmap is equal to the pc color, add to success.
                                            if (cg.CompareColor(DifficultyLocations[slot, 0] + xi + x, DifficultyLocations[slot, 1] - Extensions.InvertNumber(y, Markups.DIFFICULTY_MARKUPS[b].Height - 1), Colors.WHITE, 50) == tc)
                                                success++;
                                        }
                                    }
                                // Get the result
                                double result = (success / total) * 100;

                                rl.Add((int)result);
                                dl.Add((Difficulty)b);
                            }
                        }
                    }

                    if (slot == 5 && cg.OpenChatIsDefault)
                        cg.Chat.OpenChat();

                    // Return the difficulty that is most possible.
                    if (rl.Count > 0)
                    {
                        int max = rl.Max();
                        if (max >= 75)
                            return dl[rl.IndexOf(max)];
                        else
                            return null;
                    }
                    else
                        return null;
                }

                else if (cg.QueueCount > 0)
                {
                    int y = DifficultyLocationsQueue[slot - CustomGame.Queueid];
                    for (int x = DifficultyLocationQueueX; x < 150 + DifficultyLocationQueueX; x++)
                        if (cg.CompareColor(x, y, new int[] { 180, 186, 191 }, 10))
                            return null;
                    return Difficulty.Easy;
                }

                else
                    return null;
            }
        }

        static int[,] DifficultyLocations = new int[,]
        {
            // X    Y  Length
            // Blue
            { 145, 259, 100 },
            { 145, 288, 100 },
            { 145, 316, 100 },
            { 145, 345, 100 },
            { 145, 373, 100 },
            { 145, 402, 100 },
            // Red
            { 401, 259, 25 },
            { 401, 288, 25 },
            { 401, 316, 25 },
            { 401, 345, 25 },
            { 401, 373, 25 },
            { 401, 402, 25 }
        };

        static int DifficultyLocationQueueX = 686;
        static int[] DifficultyLocationsQueue = new int[]
        {
            244,
            257,
            270,
            283,
            297,
            310
        };

        /// <summary>
        /// Removes all AI from the game.
        /// </summary>
        /// <returns>Returns true if successful.</returns>
        /// <seealso cref="Interact.RemoveAllBots(int)"/>
        /// <seealso cref="RemoveFromGameIfAI(int)"/>
        public bool RemoveAllBotsAuto()
        {
            lock (cg.CustomGameLock)
            {
                cg.updateScreen();

                var allSlots = cg.AllSlots;

                for (int i = 0; i < allSlots.Count; i++)
                    if (IsAI(allSlots[i], true))
                        if (cg.Interact.RemoveAllBots(allSlots[i]))
                            return true;

                return false;
            }
        }

        /// <summary>
        /// Checks if the input slot is an AI.
        /// </summary>
        /// <param name="slot">Slot to check.</param>
        /// <param name="noUpdate">Determines if the captured screen should be updated before scanning.</param>
        /// <returns>Returns true if slot is AI.</returns>
        /// <include file='docs.xml' path='doc/exceptions/invalidslot/exception'/>
        public bool IsAI(int slot, bool noUpdate = false)
        {
            lock (cg.CustomGameLock)
            {
                // Look for the commendation icon for the slot chosen.

                // If the slot is not valid, throw an exception.
                if (!CustomGame.IsSlotValid(slot))
                    throw new InvalidSlotException(slot);

                if (CustomGame.IsSlotSpectator(slot) // Since AI cannot join spectator, return false if the slot is a spectator slot.
                    || !cg.AllSlots.Contains(slot)) // Return false if the slot is empty.
                    return false;

                // The chat covers blue slot 5. Close the chat so the scanning will work accurately.
                if (slot == 5)
                    cg.Chat.CloseChat();

                if (!noUpdate)
                    cg.updateScreen();

                int checkY = 0; // The potential Y locations of the commendation icon
                int checkX = 0; // Where to start scanning on the X axis for the commendation icon
                int checkXLength = 0; // How many pixels to scan on the X axis for the commendation icon

                if (CustomGame.IsSlotBlue(slot) || CustomGame.IsSlotRed(slot))
                {
                    int checkslot = slot;
                    if (CustomGame.IsSlotRed(checkslot))
                        checkslot -= 6;

                    // Find the potential Y locations of the commendation icon.
                    // 248 is the Y location of the first commendation icon of the player in the first slot of red and blue. 28 is how many pixels it is to the next commendation icon on the next slot.
                    checkY = 257 + (checkslot * Distances.LOBBY_SLOT_DISTANCE);

                    if (CustomGame.IsSlotBlue(slot))
                        checkX = 74; // The start of the blue slots on the X axis
                    else if (CustomGame.IsSlotRed(slot))
                        checkX = 399; // The start of the red slots on the X axis

                    checkXLength = 195; // The length of the slots.
                }
                else if (CustomGame.IsSlotInQueue(slot))
                {
                    int checkslot = slot - CustomGame.Queueid;

                    // 245 is the Y location of the first commendation icon of the player in the first slot in queue. 14 is how many pixels it is to the next commendation icon on the next slot.
                    checkY = 245 + (checkslot * 14) - Distances.LOBBY_QUEUE_OFFSET;

                    checkX = 707; // The start of the queue slots on this X axis
                    checkXLength = 163; // The length of the queue slots.
                }

                bool isAi = true;

                for (int x = checkX; x < checkX + checkXLength; x += 2)
                    // Check for the commendation icon.
                    if (cg.CompareColor(x, checkY, new int[] { 85, 140, 140 }, new int[] { 115, 175, 175 }))
                    {
                        isAi = false;
                        break;
                    }

                if (slot == 5 && cg.OpenChatIsDefault)
                    cg.Chat.OpenChat();

                return isAi;
            }
        }

        /// <summary>
        /// Checks if the input slot is an AI.
        /// </summary>
        /// <remarks>
        /// IsAI() is faster but requires calling CalibrateAIChecking() beforehand. However this is more accurate and does not require calling CalibrateAIChecking().
        /// </remarks>
        /// <param name="slot">Slot to check.</param>
        /// <returns>Returns true if slot is AI.</returns>
        /// <include file='docs.xml' path='doc/exceptions/invalidslot/exception'/>
        /// <seealso cref="IsAI(int, bool)"/>
        public bool AccurateIsAI(int slot)
        {
            return cg.Interact.PeakOption(slot, Markups.REMOVE_ALL_BOTS);
        }

        /// <summary>
        /// Gets all slots that are AI.
        /// </summary>
        /// <returns>All AI slots.</returns>
        public List<int> GetAISlots(bool accurate = false)
        {
            lock (cg.CustomGameLock)
            {
                List<int> AISlots = new List<int>();

                List<int> allPlayers = cg.AllSlots;

                for (int i = 0; i < allPlayers.Count; i++)
                {
                    if (!accurate)
                    {
                        if (IsAI(allPlayers[i], true))
                            AISlots.Add(allPlayers[i]);
                    }
                    else
                    {
                        if (AccurateIsAI(allPlayers[i]))
                            AISlots.Add(allPlayers[i]);
                    }
                }

                return AISlots;
            }
        }

        /// <summary>
        /// Gets all slots that are not AI.
        /// </summary>
        /// <returns>All player slots.</returns>
        public List<int> GetPlayerSlots(bool accurate = false)
        {
            lock (cg.CustomGameLock)
            {
                List<int> AISlots = new List<int>();

                List<int> allPlayers = cg.AllSlots;

                for (int i = 0; i < allPlayers.Count; i++)
                {
                    if (!accurate)
                    {
                        if (!IsAI(allPlayers[i], true))
                            AISlots.Add(allPlayers[i]);
                    }
                    else
                    {
                        if (!AccurateIsAI(allPlayers[i]))
                            AISlots.Add(allPlayers[i]);
                    }
                }

                return AISlots;
            }
        }

        /// <summary>
        /// AI checking is determined by looking for the commendation icon of players. Sometimes, this icon is missing. This fixes it.
        /// </summary>
        public void CalibrateAIChecking()
        {
            lock (cg.CustomGameLock)
            {
                cg.RightClick(Points.LOBBY_MY_PLAYER_ICON, 250);
                cg.KeyPress(Keys.Enter);
                Thread.Sleep(250);
                cg.GoBack(1);
                //cg.//ResetMouse();
            }
        }

        /// <summary>
        /// Edits the hero an AI is playing and the difficulty of the AI.
        /// </summary>
        /// <param name="slot">Slot to edit.</param>
        /// <param name="setToHero">Hero to change to.</param>
        /// <param name="setToDifficulty">Difficulty to change to.</param>
        /// <returns>Returns true on success.</returns>
        /// <include file='docs.xml' path='doc/exceptions/invalidslot/exception'/>
        public bool EditAI(int slot, AIHero setToHero, Difficulty setToDifficulty)
        {
            return EditAI(slot, (AIHero?)setToHero, (Difficulty?)setToDifficulty);
        }
        /// <summary>
        /// Edits the hero an AI is playing.
        /// </summary>
        /// <param name="slot">Slot to edit.</param>
        /// <param name="setToHero">Hero to change to.</param>
        /// <returns>Returns true on success.</returns>
        /// <include file='docs.xml' path='doc/exceptions/invalidslot/exception'/>
        public bool EditAI(int slot, AIHero setToHero)
        {
            return EditAI(slot, setToHero, null);
        }
        /// <summary>
        /// Edits the difficulty of an AI.
        /// </summary>
        /// <param name="slot">Slot to edit.</param>
        /// <param name="setToDifficulty">Difficulty to change to.</param>
        /// <returns>Returns true on success.</returns>
        /// <include file='docs.xml' path='doc/exceptions/invalidslot/exception'/>
        public bool EditAI(int slot, Difficulty setToDifficulty)
        {
            return EditAI(slot, null, setToDifficulty);
        }

        bool EditAI(int slot, AIHero? setToHero, Difficulty? setToDifficulty)
        {
            lock (cg.CustomGameLock)
            {
                if (!CustomGame.IsSlotValid(slot))
                    throw new InvalidSlotException(slot);

                // Make sure there is a player or AI in selected slot, or if they are a valid slot to select in queue.
                if (cg.PlayerSlots.Contains(slot))
                {
                    if (cg.OpenChatIsDefault)
                        cg.Chat.CloseChat();

                    // Click the slot of the selected slot.
                    var slotlocation = cg.Interact.FindSlotLocation(slot);
                    cg.LeftClick(slotlocation.X, slotlocation.Y);
                    // Check if Edit AI window has opened by checking if the confirm button exists.
                    cg.updateScreen();
                    if (cg.CompareColor(Points.EDIT_AI_CONFIRM, Colors.CONFIRM, 20))
                    {
                        var sim = new List<Keys>();
                        // Set hero if setToHero does not equal null.
                        if (setToHero != null)
                        {
                            // Open hero menu
                            sim.Add(Keys.Space);
                            // <image url="$(ProjectDir)\ImageComments\AI.cs\EditAIHero.png" scale="0.5" />
                            // Select the topmost hero option
                            for (int i = 0; i < Enum.GetNames(typeof(AIHero)).Length; i++)
                                sim.Add(Keys.Up);
                            // Select the hero in selectHero.
                            for (int i = 0; i < (int)setToHero; i++)
                                sim.Add(Keys.Down);
                            sim.Add(Keys.Space);
                        }
                        sim.Add(Keys.Down); // Select difficulty option
                                            // Set difficulty if setToDifficulty does not equal null.
                        if (setToDifficulty != null)
                        {
                            // Open difficulty menu
                            sim.Add(Keys.Space);
                            // <image url="$(ProjectDir)\ImageComments\AI.cs\EditAIDifficulty.png" scale="0.6" />
                            // Select the topmost difficulty
                            for (int i = 0; i < Enum.GetNames(typeof(Difficulty)).Length; i++)
                                sim.Add(Keys.Up);
                            // Select the difficulty in selectDifficulty.
                            for (int i = 0; i < (int)setToDifficulty; i++)
                                sim.Add(Keys.Down);
                            sim.Add(Keys.Space);
                        }
                        // Confirm the changes
                        sim.Add(Keys.Return);

                        // Send the keypresses.
                        cg.KeyPress(sim.ToArray());

                        //cg.//ResetMouse();

                        if (cg.OpenChatIsDefault)
                            cg.Chat.OpenChat();

                        return true;
                    }
                    else
                    {
                        //cg.//ResetMouse();

                        if (cg.OpenChatIsDefault)
                            cg.Chat.OpenChat();

                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Safely removes a slot from the game if they are an AI.
        /// </summary>
        /// <param name="slot">Slot to remove from game.</param>
        /// <returns>Returns true if the slot is an AI and removing them from the game was successful.</returns>
        /// <include file='docs.xml' path='doc/exceptions/invalidslot/exception'/>
        /// <seealso cref="RemoveAllBotsAuto"/>
        /// <seealso cref="Interact.RemoveFromGame(int)"/>
        public bool RemoveFromGameIfAI(int slot)
        {
            lock (cg.CustomGameLock)
            {
                if (!CustomGame.IsSlotValid(slot))
                    throw new InvalidSlotException(slot);

                bool optionFound = (bool)cg.Interact.MenuOptionScan(slot, OptionScanFlags.OpenMenu | OptionScanFlags.CloseIfNotFound | OptionScanFlags.ReturnFound, null, Markups.REMOVE_ALL_BOTS);

                if (!optionFound)
                    return false;

                return (bool)cg.Interact.MenuOptionScan(slot, OptionScanFlags.Click | OptionScanFlags.CloseIfNotFound | OptionScanFlags.ReturnFound, null, Markups.REMOVE_FROM_GAME);
            }
        }
    }

    /// <summary>
    /// AI heroes.
    /// </summary>
    public enum AIHero
    {
        /// <summary>
        /// Overwatch's reccommended AI hero.
        /// </summary>
        Recommended,
        /// <summary>
        /// Ana AI hero.
        /// </summary>
        Ana,
        /// <summary>
        /// Bastion AI hero.
        /// </summary>
        Bastion,
        /// <summary>
        /// Lucio AI hero.
        /// </summary>
        Lucio,
        /// <summary>
        /// McCree AI hero.
        /// </summary>
        McCree,
        /// <summary>
        /// Mei AI hero.
        /// </summary>
        Mei,
        /// <summary>
        /// Reaper AI hero.
        /// </summary>
        Reaper,
        /// <summary>
        /// Roadhog AI hero.
        /// </summary>
        Roadhog,
        /// <summary>
        /// Soldier 76 AI hero.
        /// </summary>
        Soldier76,
        /// <summary>
        /// Sombra AI hero.
        /// </summary>
        Sombra,
        /// <summary>
        /// Torbjorn AI hero.
        /// </summary>
        Torbjorn,
        /// <summary>
        /// Zarya AI hero.
        /// </summary>
        Zarya,
        /// <summary>
        /// Zenyatta AI hero.
        /// </summary>
        Zenyatta
    }
    /// <summary>
    /// AI difficulties.
    /// </summary>
    public enum Difficulty
    {
        /// <summary>
        /// Easy AI difficulty.
        /// </summary>
        Easy,
        /// <summary>
        /// Medium AI difficulty.
        /// </summary>
        Medium,
        /// <summary>
        /// Hard AI difficulty.
        /// </summary>
        Hard
    }
}
