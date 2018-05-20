using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace Deltin.CustomGameAutomation
{
    partial class CustomGame
    {
        public static int Queueid = 18;

        public CG_Interact Interact;
        public class CG_Interact
        {
            private CustomGame cg;
            internal CG_Interact(CustomGame cg)
            { this.cg = cg; }

            internal Point FindSlotLocation(int slot)
            {
                cg.updateScreen();
                int offset = 0;
                int xoffset = 0;
                int queuecount = cg.QueueCount;
                if (slot > 11 && slot <= Queueid) offset = cg.findOffset(); // If there is players in the queue, the spectator slots move down. Find the offset in pixels to spectator.
                if (slot - Queueid >= queuecount) return new Point(); // If there is no one in the queue and the slot selected is a queue value, return invalid
                if (slot > 11) xoffset = -100; // If there is more than 6 players in the queue, a scrollbar appears offsetting the slot's x location by a few pixels.
                if (slot > Queueid) slot = slot - 6; // selecting a person in the queue where spectator slots are normally at. Not 17 because that is set when returning 891, 24

                if (slot != Queueid) return new Point(playerLoc[slot, 0] + xoffset, playerLoc[slot, 1] + offset); // Blue, Red, Spectators, and all of queue except for the first slot.
                else return new Point(playerLoc[12, 0] + xoffset, 248); // Queue 1 location
            }

            private bool OpenSlotMenu(int slot)
            {
                Point slotlocation = FindSlotLocation(slot); // Get location of slot
                if (slotlocation.IsEmpty)
                    return false;
                if (slot > 11)
                    slotlocation.X += -100; // If the slot selected is a spectator or in queue, this prevents the selected slot from sliding to the left.
                                            // Open slot menu by right clicking on slot.
                cg.Cursor = slotlocation;
                Thread.Sleep(100);
                cg.RightClick(cg.Cursor.X, cg.Cursor.Y);
                cg.updateScreen();
                return true;
            }

            private bool OpenSlotMenu(Point point)
            {
                // Open slot menu by right clicking on slot.
                cg.Cursor = point;
                Thread.Sleep(100);
                cg.RightClick(cg.Cursor.X, cg.Cursor.Y);
                cg.updateScreen();
                return true;
            }

            // Selects an option in the slot menu.
            private bool SelectMenuOption(int x, int y)
            {
                Point point = new Point(x, y);
                cg.Cursor = point; // Select the option
                Thread.Sleep(100);
                // <image url="$(ProjectDir)\ImageComments\Interact.cs\OptionSelect.png" scale="0.7" />
                cg.updateScreen();
                if (cg.bmp.CompareColor(point.X, point.Y, new int[] { 83, 133, 155 }, 20)) // Detects if the blue color of the selected option is there, clicks then returns true
                {
                    cg.LeftClick(point.X, point.Y, 0);
                    return true;
                }
                return false;
            }

            private enum Direction
            {
                RightDown,
                RightUp,
            }
            private Direction Getmenudirection(Point point)
            {
                cg.updateScreen();

                // Tests for the blue outline for the first option selection.
                if (cg.bmp.CompareColor(point.X + 12, point.Y + 9, new int[] { 75, 106, 120 }, 5))
                    return Direction.RightDown;
                // Tests for the border of the option menu for right/left-up
                else if (cg.bmp.CompareColor(point.X + 5, point.Y - 5, new int[] { 166, 165, 166 }, 50))
                    return Direction.RightUp;
                else
                    return Direction.RightDown;
            }
            private int[] OffsetViaDirection(Direction direction)
            {
                if (direction == Direction.RightDown)
                    return new int[] { 1, 1 };
                else if (direction == Direction.RightUp)
                    return new int[] { 1, -1 };
                else return null;
            }

            public void MenuOptionMarkup(int slot, int max, int savestart = 0, string savelocation = null)
            {
                if (slot < 0 || slot > Queueid + 5)
                    throw new InvalidSlotException(string.Format("Slot argument '{0}' is out of range.", slot));

                Point slotlocation = FindSlotLocation(slot);
                if (slotlocation.IsEmpty)
                    return;
                OpenSlotMenu(slotlocation);

                if (savelocation == null)
                    savelocation = @"C:\Users\" + Environment.UserName + @"\Documents\Abyxa\Library\";

                // Get direction opened menu is going.
                Direction dir = Getmenudirection(slotlocation);
                int[] offset = OffsetViaDirection(dir);

                int xstart = slotlocation.X + 14,  // X position to start scanning
                    xmax = 79, // How far on the X axis to scan
                    ystart = 0, // Y position to start scanning
                    ymax = 6; // How far on the Y axis to scan.
                double yincrement = 0; // Amount to increment to scan next option line

                if (dir == Direction.RightDown)
                {
                    ystart = slotlocation.Y + 12;
                    yincrement = 11.5;
                }
                else if (dir == Direction.RightUp)
                {
                    ystart = slotlocation.Y - 128;
                    yincrement = -11.5;
                }

                cg.updateScreen();
                // Scan -the number of the variable max- lines.
                for (int mi = 0, yii = ystart; mi < max; mi++, yii = ystart + (int)(yincrement * (mi))) // Mi is the line to scan, yii is the Y coordinate of line mi.
                {
                    // Get bitmap of option
                    Bitmap work = cg.bmp.Clone(new Rectangle(xstart, yii, xmax, ymax), cg.bmp.PixelFormat);

                    for (int xi = 0; xi < work.Width; xi++)
                        for (int yi = 0; yi < work.Height; yi++)
                        {
                            int fade = 80;
                            int[] textcolor = new int[] { 169, 169, 169 };
                            if (work.CompareColor(xi, yi, textcolor, fade))
                                work.SetPixel(xi, yi, Color.Black);
                            else
                                work.SetPixel(xi, yi, Color.White);
                        }
                    work.Save(savelocation + "markup-" + (mi + savestart).ToString() + ".png");
                    work.Dispose();
                }

                // Close the menu.
                cg.LeftClick(400, 500, 100);
                cg.LeftClick(500, 500, 100);
                cg.ResetMouse();
            }

            /// <summary>
            /// Scans a player menu for an option.
            /// </summary>
            /// <param name="slot">Slot's menu to scan.</param>
            /// <param name="markup">Bitmap markup of option to scan for.</param>
            /// <param name="minimumPercent">Minimum percent the markup has to match an option in the menu.</param>
            /// <param name="max">Maximum options to scan.</param>
            /// <returns>Returns true if the option in the markup has been found, else returns false.</returns>
            /// <exception cref="InvalidSlotException">Thrown if slot argument is out of range.</exception>
            public bool MenuOptionScan(int slot, Bitmap markup, int minimumPercent, int max)
            {
                if (slot < 0 || slot > Queueid + 5)
                    throw new InvalidSlotException(string.Format("Slot {0} is out of range.", slot));

                Point slotlocation = FindSlotLocation(slot);
                if (slotlocation.IsEmpty)
                    return false;
                OpenSlotMenu(slotlocation);

                Direction dir = Getmenudirection(slotlocation); // Get menu direction after opening menu.
                int[] offset = OffsetViaDirection(dir);

                int xstart = slotlocation.X + 14,  // X position to start scanning.
                    xmax = 79,  // How far on the X axis to scan.
                    xselect = slotlocation.X + 10, // Amount to increment X when selecting the option after the option has been found.
                    ystart = 0, // Y position to start scanning.
                    ymax = 6; // How far on the Y axis to scan.
                double yincrement = 0; // Amount to increment Y after scanning option
                if (dir == Direction.RightDown)
                {
                    ystart = slotlocation.Y + 12;
                    yincrement = 11.5;
                }
                else if (dir == Direction.RightUp)
                {
                    ystart = slotlocation.Y - 128;
                    yincrement = -11.5;
                }

                cg.updateScreen();
                List<int> percentResults = new List<int>();
                // Scan each option and get the likelyhood in percent of the option being the markup.
                for (int mi = 0, yii = ystart; mi < max; mi++, yii = ystart + (int)(yincrement * mi)) // Mi is the line to scan, yii is the Y coordinate of line mi.
                {
                    int success = 0;
                    int total = 0;

                    Bitmap work = cg.bmp.Clone(new Rectangle(xstart, yii, xmax, ymax), cg.bmp.PixelFormat); // Get the option.
                    for (int xi = 0; xi < work.Width; xi++)
                        for (int yi = 0; yi < work.Height; yi++)
                        {
                            if (work.CompareColor(xi, yi, new int[] { 169, 169, 169 }, 80))
                                work.SetPixel(xi, yi, Color.Black);
                            else
                                work.SetPixel(xi, yi, Color.White);
                            total++;
                            if (work.GetPixelAt(xi, yi) == markup.GetPixelAt(xi, yi))
                                success++;
                        }
                    work.Dispose();
                    int percent = (int)((Convert.ToDouble(success) / Convert.ToDouble(total)) * 100);
                    percentResults.Add(percent);
                }

                // Check if option has been found.
                int maxpercent = percentResults.IndexOf(percentResults.Max());
                if (percentResults[maxpercent] > minimumPercent)
                {
                    // If yes, click the option found.
                    SelectMenuOption(xselect, ystart + (int)(yincrement * maxpercent));
                    cg.ResetMouse();
                    return true;
                }
                else
                {
                    // If not, close menu
                    cg.LeftClick(400, 500, 100);
                    cg.LeftClick(500, 500, 100);
                    cg.ResetMouse();
                    return false;
                }
            }

            internal Bitmap RemoveFromGameMarkup = new Bitmap(Properties.Resources.remove_from_game); // Get remove from game markup
            /// <summary>
            /// Removes player from the game
            /// </summary>
            /// <param name="slot">Slot to remove from game.</param>
            /// <returns>Returns true if removing from game was successful</returns>
            /// <exception cref="InvalidSlotException">Thrown if slot argument is out of range.</exception>
            public bool RemoveFromGame(int slot)
            {
                return MenuOptionScan(slot, RemoveFromGameMarkup, 80, 3); // Attempt to remove slot from game
            }

            internal Bitmap SwapToRedMarkup = new Bitmap(Properties.Resources.swap_to_red);
            /// <summary>
            /// Swaps player to red team.
            /// </summary>
            /// <param name="slot">Slot to swap to red team.</param>
            /// <returns>Returns true when swapping player to red team is successful.</returns>
            /// <exception cref="InvalidSlotException">Thrown if slot argument is out of range.</exception>
            public bool SwapToRed(int slot)
            {
                return MenuOptionScan(slot, SwapToRedMarkup, 80, 4); // Attempt to remove slot from game
            }


            internal Bitmap SwapToBlueMarkup = new Bitmap(Properties.Resources.swap_to_blue);
            /// <summary>
            /// Swaps player to blue team.
            /// </summary>
            /// <param name="slot">Slot to swap to blue team.</param>
            /// <returns>Returns true when swapping player to blue team is successful.</returns>
            /// <exception cref="InvalidSlotException">Thrown if slot argument is out of range.</exception>
            public bool SwapToBlue(int slot)
            {
                return MenuOptionScan(slot, SwapToBlueMarkup, 80, 4); // Attempt to remove slot from game
            }

            /// <summary>
            /// Swaps the team of a player.
            /// </summary>
            /// <param name="slot">Slot to swap team.</param>
            /// <returns>Returns true if swapping team is successful.</returns>
            /// <exception cref="InvalidSlotException">Thrown if slot argument is out of range.</exception>
            public bool SwapTeam(int slot)
            {
                if (slot <= 5)
                    return MenuOptionScan(slot, SwapToRedMarkup, 80, 4); // Swap blue player to red
                else if (slot <= 11)
                    return MenuOptionScan(slot, SwapToBlueMarkup, 80, 4); // Swap red player to blue
                else if (slot >= Queueid)
                {
                    QueueTeam team = cg.PlayerInfo.GetQueueTeam(slot);
                    if (team == QueueTeam.Blue)
                        return MenuOptionScan(slot, SwapToRedMarkup, 80, 4);
                    else if (team == QueueTeam.Red)
                        return MenuOptionScan(slot, SwapToBlueMarkup, 80, 4);
                }
                return false;
            }

            internal Bitmap SwapToSpectatorsMarkup = new Bitmap(Properties.Resources.swap_to_spectators);
            /// <summary>
            /// Swaps player to spectator.
            /// </summary>
            /// <param name="slot">Slot to swap to spectator.</param>
            /// <returns>Returns true if swapping to spectators is successful.</returns>
            /// <exception cref="InvalidSlotException">Thrown if slot argument is out of range.</exception>
            public bool SwapToSpectators(int slot)
            {
                return MenuOptionScan(slot, SwapToSpectatorsMarkup, 80, 6);
            }

            internal Bitmap SwapToNeutralMarkup = new Bitmap(Properties.Resources.swap_to_neutral_team);
            /// <summary>
            /// Swaps player in queue to the neutral team.
            /// </summary>
            /// <param name="slot">Slot to swap to neutral.</param>
            /// <returns>Returns true if swapping player to neutral is successful.</returns>
            /// <exception cref="InvalidSlotException">Thrown if slot argument is out of range.</exception>
            public bool SwapToNeutral(int slot)
            {
                return MenuOptionScan(slot, SwapToNeutralMarkup, 80, 6);
            }

            internal Bitmap RemoveAllBotsMarkup = new Bitmap(Properties.Resources.remove_all_bots);
            /// <summary>
            /// Removes all AI from the game. Input slot must be an AI.
            /// </summary>
            /// <param name="slot">Slot of an AI.</param>
            /// <returns>Returns true if removing all AI is successful.</returns>
            /// <exception cref="InvalidSlotException">Thrown if slot argument is out of range.</exception>
            public bool RemoveAllBots(int slot)
            {
                return MenuOptionScan(slot, RemoveAllBotsMarkup, 80, 6);
            }

            /// <summary>
            /// Swap 2 slots with eachother.
            /// </summary>
            /// <param name="slot1">Target 1</param>
            /// <param name="slot2">Target 2</param>
            /// <returns>Returns true if swapping slots is successful.</returns>
            /// <exception cref="InvalidSlotException">Thrown if the slot1 or slot2 argument is out of range of possible slots to move.</exception>
            // Swap slots
            public bool Move(int slot1, int slot2)
            {
                if (slot1 < 0 || slot1 > Queueid + 5)
                    throw new InvalidSlotException(string.Format("slot1 argument '{0}' is out of range.", slot1));
                if (slot2 < 0 || slot2 > Queueid + 5)
                    throw new InvalidSlotException(string.Format("slot2 argument '{0}' is out of range.", slot2));

                cg.updateScreen();
                if (cg.addbutton()) cg.LeftClick(661, 180, 25);
                else cg.LeftClick(717, 180, 25);

                Point slot1loc = FindSlotLocation(slot1); // Get the location of the target
                Point slot2loc = FindSlotLocation(slot2); // Get the location of the destination
                cg.LeftClick(slot1loc.X, slot1loc.Y, 25); // Click the target
                cg.LeftClick(slot2loc.X, slot2loc.Y, 25); // Click the destination

                cg.updateScreen();

                if (cg.bmp.CompareColor(717, 180, new int[] { 160, 124, 80 }, 30)) cg.LeftClick(717, 183, 25);
                else cg.LeftClick(660, 180, 25);

                return true;
            }

            /// <summary>
            /// Swaps players on both teams.
            /// </summary>
            public void SwapAll()
            {
                bool aistatus = cg.addbutton();

                // click move
                if (aistatus) cg.LeftClick(661, 180, 25); // with add AI button
                else cg.LeftClick(717, 180, 25); // without add AI button

                // click swap all
                if (aistatus) cg.LeftClick(617, 180, 25); // with add AI button
                else cg.LeftClick(678, 180, 25); // without add AI button

                cg.updateScreen();

                // Click done
                if (cg.bmp.CompareColor(717, 180, new int[] { 160, 124, 80 }, 30)) cg.LeftClick(717, 183, 25); // with add AI button
                else cg.LeftClick(660, 180, 25); // without add AI button
            }
        }
    }
}
