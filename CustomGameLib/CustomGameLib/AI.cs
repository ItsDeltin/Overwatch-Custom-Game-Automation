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
        public CG_AI AI;
        /// <summary>
        /// AI settings for Overwatch.
        /// </summary>
        public class CG_AI
        {
            private CustomGame cg;
            internal CG_AI(CustomGame cg)
            {
                this.cg = cg;
            }

            /// <summary>
            /// Add AI to the game.
            /// </summary>
            /// <param name="hero">Hero type to add.</param>
            /// <param name="difficulty">Difficulty of hero.</param>
            /// <param name="team">Team that AI joins.</param>
            /// <param name="count">Amount of AI that is added. Set to -1 for max. Default is -1</param>
            /// <returns></returns>
            public bool AddAI(AIHero hero, Difficulty difficulty, BotTeam team, int count = -1)
            {
                cg.updateScreen();

                // Find the maximum amount of bots that can be placed on a team, and store it in the maxBots variable

                if (cg.DoesAddButtonExist())
                /*
                 * If the blue shade of the "Move" button is there, that means that the Add AI button is there. 
                 * If the Add AI button is missing, we can't add AI, so return false. If it is there, add the bots.
                 * The AI button will be missing if the server is full
                 */
                {
                    // Open AddAI menu.
                    cg.Cursor = new Point(835, 182);
                    cg.WaitForUpdate(835, 182, 20, 2000);
                    cg.LeftClick(835, 182, 500);

                    List<Keys> press = new List<Keys>();

                    if (hero != AIHero.Recommended)
                    {
                        press.Add(Keys.Space);
                        int heroid = (int)hero;
                        for (int i = 0; i < heroid; i++)
                            press.Add(Keys.Down);
                        press.Add(Keys.Space);
                        press.Add(Keys.Down);
                    }

                    press.Add(Keys.Down);

                    if (difficulty != Difficulty.Easy)
                    {
                        press.Add(Keys.Space);
                        int difficultyID = (int)difficulty;
                        for (int i = 0; i < difficultyID; i++)
                            press.Add(Keys.Down);
                        press.Add(Keys.Space);
                        press.Add(Keys.Down);
                        press.Add(Keys.Down);
                    }

                    press.Add(Keys.Down);
                    press.Add(Keys.Down);

                    if (team != BotTeam.Both)
                    {
                        press.Add(Keys.Space);
                        int teamID = (int)team;
                        for (int i = 0; i < teamID; i++)
                            press.Add(Keys.Down);
                        press.Add(Keys.Space);
                        press.Add(Keys.Down);
                        press.Add(Keys.Down);
                        press.Add(Keys.Down);
                        press.Add(Keys.Down);
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

                    cg.ResetMouse();

                    return true;
                }
                else
                    return false;
            }

            /// <summary>
            /// Obtains the markup of an AI's difficulty.
            /// </summary>
            /// <param name="scalar">Garanteed index of difficulty. (0 = easy, 1 = medium, 2 = hard)</param>
            /// <param name="saveAt">Location to save markup at.</param>
            public void GetAIDifficultyMarkup(int scalar, string saveAt)
            {
                cg.updateScreen();
                int[] scales = new int[] { 33, 49, 34 };
                Bitmap tmp = cg.BmpClone(402, 244, scales[scalar], 16);
                for (int x = 0; x < tmp.Width; x++)
                    for (int y = 0; y < tmp.Height; y++)
                    {
                        if (tmp.CompareColor(x, y, CALData.WhiteColor, 25))
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

            /// <summary>
            /// Gets the difficulty of the AI in the input slot.
            /// <para>If the input slot is not an AI, returns null.
            /// If checking an AI's difficulty in the queue, it will always return easy, or null if it is a player.</para>
            /// </summary>
            /// <param name="slot">Slot to check</param>
            /// <param name="noUpdate"></param>
            /// <returns>Returns a value in the Difficulty enum if the difficulty is found. Returns null if the input slot is not an AI.</returns>
            /// <exception cref="InvalidSlotException">Thrown when slot is out of range.</exception>
            public Difficulty? GetAIDifficulty(int slot, bool noUpdate = false)
            {
                if (!cg.IsSlotValid(slot))
                    throw new InvalidSlotException(string.Format("Slot {0} is out of range of possible slots to check for AI.", slot));

                if (slot == 5 && cg.OpenChatIsDefault)
                    cg.Chat.CloseChat();

                if (!noUpdate)
                    cg.updateScreen();

                if (cg.IsSlotBlue(slot) || cg.IsSlotRed(slot))
                {
                    bool draw = cg.debugmode; // For debug mode

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
                        if (cg.CompareColor(DifficultyLocations[slot, 0] + xi, DifficultyLocations[slot, 1], CALData.WhiteColor, 110)
                            && (slot > 5 || cc.B - cc.R < 20))
                        {
                            foundWhite = true;

                            // For each difficulty markup
                            for (int b = 0; b < DifficultyMarkups.Length; b++)
                            {
                                Bitmap tmp = null;
                                if (draw)
                                    tmp = cg.BmpClone(0, 0, cg.bmp.Width, cg.bmp.Height);

                                // Check if bitmap matches checking area
                                double success = 0;
                                double total = 0;
                                for (int x = 0; x < DifficultyMarkups[b].Width; x++)
                                    for (int y = DifficultyMarkups[b].Height - 1; y >= 0; y--)
                                    {
                                        // If the color pixel of the markup is not white, check if valid.
                                        Color pc = DifficultyMarkups[b].GetPixel(x, y);
                                        if (pc != Color.FromArgb(255, 255, 255, 255))
                                        {
                                            // tc is true if the pixel is black, false if it is red.
                                            bool tc = pc == Color.FromArgb(255, 0, 0, 0);

                                            total++; // Indent the total
                                                     // If the checking color in the bmp bitmap is equal to the pc color, add to success.
                                            if (cg.CompareColor(DifficultyLocations[slot, 0] + xi + x, DifficultyLocations[slot, 1] - Extensions.InvertNumber(y, DifficultyMarkups[b].Height - 1), CALData.WhiteColor, 50) == tc)
                                            {
                                                success++;

                                                if (draw)
                                                {
                                                    if (tc)
                                                        tmp.SetPixel(DifficultyLocations[slot, 0] + xi + x, DifficultyLocations[slot, 1] - Extensions.InvertNumber(y, DifficultyMarkups[b].Height - 1), Color.Blue);
                                                    else
                                                        tmp.SetPixel(DifficultyLocations[slot, 0] + xi + x, DifficultyLocations[slot, 1] - Extensions.InvertNumber(y, DifficultyMarkups[b].Height - 1), Color.Lime);
                                                }
                                            }
                                            else if (draw)
                                            {
                                                if (tc)
                                                    tmp.SetPixel(DifficultyLocations[slot, 0] + xi + x, DifficultyLocations[slot, 1] - Extensions.InvertNumber(y, DifficultyMarkups[b].Height - 1), Color.Red);
                                                else
                                                    tmp.SetPixel(DifficultyLocations[slot, 0] + xi + x, DifficultyLocations[slot, 1] - Extensions.InvertNumber(y, DifficultyMarkups[b].Height - 1), Color.Orange);
                                            }

                                            if (draw)
                                            {
                                                tmp.SetPixel(DifficultyLocations[slot, 0] + xi + x, DifficultyLocations[slot, 1] - DifficultyMarkups[b].Height * 2 + y, DifficultyMarkups[b].GetPixel(x, y));
                                                cg.g.DrawImage(tmp, new Rectangle(0, -750, tmp.Width * 3, tmp.Height * 3));
                                                Thread.Sleep(1);
                                            }
                                        }
                                    }
                                // Get the result
                                double result = (success / total) * 100;

                                rl.Add((int)result);
                                dl.Add((Difficulty)b);

                                if (draw)
                                {
                                    tmp.SetPixel(DifficultyLocations[slot, 0] + xi, DifficultyLocations[slot, 1], Color.MediumPurple);
                                    cg.g.DrawImage(tmp, new Rectangle(0, -750, tmp.Width * 3, tmp.Height * 3));
                                    Console.WriteLine((Difficulty)b + " " + result);
                                    Thread.Sleep(1000);
                                }
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
                    int y = DifficultyLocationsQueue[slot - Queueid];
                    for (int x = DifficultyLocationQueueX; x < 150 + DifficultyLocationQueueX; x++)
                        if (cg.CompareColor(x, y, new int[] { 180, 186, 191 }, 10))
                            return null;
                    return Difficulty.Easy;
                }

                else
                    return null;
            }

            static internal Bitmap[] DifficultyMarkups = new Bitmap[]
            {
                new Bitmap(Properties.Resources.easy_difficulty),
                new Bitmap(Properties.Resources.medium_difficulty),
                new Bitmap(Properties.Resources.hard_difficulty)
            };

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
            public bool RemoveAllBotsAuto()
            {
                cg.updateScreen();

                for (int i = 0; i < 12; i++)
                    if (IsAI(i, true))
                        if (cg.Interact.RemoveAllBots(i))
                            return true;

                for (int i = Queueid; i < Queueid + 6; i++)
                    if (IsAI(i, true))
                        if (cg.Interact.RemoveAllBots(Queueid + i))
                            return true;
                return false;
            }

            /// <summary>
            /// Checks if the input slot is an AI.
            /// </summary>
            /// <param name="slot">Slot to check.</param>
            /// <param name="noUpdate">Determines if the captured screen should be updated before scanning.</param>
            /// <returns>Returns true if slot is AI.</returns>
            public bool IsAI(int slot, bool noUpdate = false)
            {
                // Look for the commendation icon for the slot chosen.

                // If the slot is not valid, throw an exception.
                if (!cg.IsSlotValid(slot))
                    throw new InvalidSlotException(string.Format("Slot {0} is out of range of possible slots to check for AI.", slot));

                // Since AI cannot join spectator, return false if the slot is a spectator slot.
                if (cg.IsSlotSpectator(slot))
                    return false;

                if (!noUpdate)
                    cg.updateScreen();

                int[] checkY = new int[0]; // The potential Y locations of the commendation icon
                int checkX = 0; // Where to start scanning on the X axis for the commendation icon
                int checkXLength = 0; // How many pixels to scan on the X axis for the commendation icon

                if (cg.IsSlotBlue(slot) || cg.IsSlotRed(slot))
                {
                    int checkslot = slot;
                    if (cg.IsSlotRed(checkslot))
                        checkslot -= 6;

                    // Find the potential Y locations of the commendation icon.
                    // 248 is the Y location of the first commendation icon of the player in the first slot of red and blue. 28 is how many pixels it is to the next commendation icon on the next slot.
                    int y1 = 248 + (checkslot * 28),
                        y2 = y1 + 9; // The second potential Y location is 9 pixels under the first potential spot.
                    checkY = new int[] { y1, y2 };

                    if (cg.IsSlotBlue(slot))
                        checkX = 74; // The start of the blue slots on the X axis
                    else if (cg.IsSlotRed(slot))
                        checkX = 399; // The start of the red slots on the X axis

                    checkXLength = 195; // The length of the slots.
                }
                else if (cg.IsSlotInQueue(slot))
                {
                    int checkslot = slot - Queueid;

                    // 245 is the Y location of the first commendation icon of the player in the first slot in queue. 14 is how many pixels it is to the next commendation icon on the next slot.
                    int y = 245 + (checkslot * 14);
                    checkY = new int[] { y };

                    checkX = 707; // The start of the queue slots on this X axis
                    checkXLength = 163; // The length of the queue slots.
                }

                for (int x = checkX; x < checkX + checkXLength; x++)
                    for (int yi = 0; yi < checkY.Length; yi++)
                    {
                        int y = checkY[yi];
                        // Check for the commendation icon. 91, 162, 166 is the RGB values of the color of the commendation icon.
                        if (cg.CompareColor(x, y, new int[] { 91, 162, 166 }, 30))
                            return false; // If the commendation icon is found, the slot is a player so return false.
                    }

                // No commendation icon was found, so return true.
                return true;
            }

            /// <summary>
            /// Edits the hero an AI is playing and the difficulty of the AI.
            /// </summary>
            /// <param name="slot">Slot to edit.</param>
            /// <param name="setToHero">Hero to change to.</param>
            /// <param name="setToDifficulty">Difficulty to change to.</param>
            /// <returns>Returns true on success.</returns>
            public bool EditAI(int slot, AIHero setToHero, Difficulty setToDifficulty)
            {
                return EditAI(slot, setToHero, setToDifficulty, true);
            }
            /// <summary>
            /// Edits the hero an AI is playing.
            /// </summary>
            /// <param name="slot">Slot to edit.</param>
            /// <param name="setToHero">Hero to change to.</param>
            /// <returns>Returns true on success.</returns>
            public bool EditAI(int slot, AIHero setToHero)
            {
                return EditAI(slot, setToHero, null, true);
            }
            /// <summary>
            /// Edits the difficulty of an AI.
            /// </summary>
            /// <param name="slot">Slot to edit.</param>
            /// <param name="setToDifficulty">Difficulty to change to.</param>
            /// <returns>Returns true on success.</returns>
            public bool EditAI(int slot, Difficulty setToDifficulty)
            {
                return EditAI(slot, null, setToDifficulty, true);
            }

            bool EditAI(int slot, object setToHero, object setToDifficulty, bool x)
            {
                // Make sure there is a player or AI in selected slot, or if they are a valid slot to select in queue.
                if (cg.PlayerSlots.Contains(slot) || (slot >= Queueid && slot - (Queueid) < cg.QueueCount))
                {
                    // Click the slot of the selected slot.
                    var slotlocation = cg.Interact.FindSlotLocation(slot);
                    cg.LeftClick(slotlocation.X, slotlocation.Y);
                    // Check if Edit AI window has opened by checking if the confirm button exists.
                    cg.updateScreen();
                    if (cg.CompareColor(447, 354, CALData.ConfirmColor, 20))
                    {
                        var sim = new List<Keys>();
                        // Set hero if setToHero does not equal null.
                        if (setToHero != null)
                        {
                            AIHero selectHero = (AIHero)setToHero;
                            // Open hero menu
                            sim.Add(Keys.Space);
                            // <image url="$(ProjectDir)\ImageComments\AI.cs\EditAIHero.png" scale="0.5" />
                            // Select the topmost hero option
                            for (int i = 0; i < Enum.GetNames(typeof(AIHero)).Length; i++)
                                sim.Add(Keys.Up);
                            // Select the hero in selectHero.
                            for (int i = 0; i < (int)selectHero; i++)
                                sim.Add(Keys.Down);
                            sim.Add(Keys.Space);
                            sim.Add(Keys.Down);
                        }
                        sim.Add(Keys.Down); // Select difficulty option
                                                      // Set difficulty if setToDifficulty does not equal null.
                        if (setToDifficulty != null)
                        {
                            Difficulty selectDifficulty = (Difficulty)setToDifficulty;
                            // Open difficulty menu
                            sim.Add(Keys.Space);
                            // <image url="$(ProjectDir)\ImageComments\AI.cs\EditAIDifficulty.png" scale="0.6" />
                            // Select the topmost difficulty
                            for (int i = 0; i < Enum.GetNames(typeof(Difficulty)).Length; i++)
                                sim.Add(Keys.Up);
                            // Select the difficulty in selectDifficulty.
                            for (int i = 0; i < (int)selectDifficulty; i++)
                                sim.Add(Keys.Down);
                            sim.Add(Keys.Space);
                        }
                        // Confirm the changes
                        sim.Add(Keys.Return);

                        // Send the keypresses.
                        cg.KeyPress(sim.ToArray());

                        cg.ResetMouse();
                        return true;
                    }
                    else
                    {
                        cg.ResetMouse();
                        return false;
                    }
                }
                else
                {
                    return false;
                }
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
