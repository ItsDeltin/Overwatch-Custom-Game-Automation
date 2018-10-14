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
        /// <summary>
        /// The number where the queue slots start.
        /// </summary>
        public const int Queueid = 18;
        /// <summary>
        /// The number where the spectator slots start.
        /// </summary>
        public const int Spectatorid = 12;

        /// <summary>
        /// Changes a player's state in Overwatch.
        /// </summary>
        public Interact Interact;
    }
    /// <summary>
    /// Changes a player's state in Overwatch.
    /// </summary>
    /// <remarks>
    /// The Interact class is accessed in a CustomGame object on the <see cref="CustomGame.Interact"/> field.
    /// </remarks>
    public class Interact : CustomGameBase
    {
        internal Interact(CustomGame cg) : base(cg) { }

        internal Point FindSlotLocation(int slot)
        {
            int yoffset = 0;
            int xoffset = 0;
            if (CustomGame.IsSlotInQueue(slot) && !cg.QueueSlots.Contains(slot)) return Point.Empty; // If a queue slot is selected and there is no one in that queue slot, return empty.
            if (CustomGame.IsSlotSpectator(slot)) yoffset = cg.FindSpectatorOffset(); // If there is players in the queue, the spectator slots move down. Find the offset in pixels to spectator.
            if (CustomGame.IsSlotSpectatorOrQueue(slot)) xoffset = -100; // Prevents the player context menu from orientating left for slots in the spectator and queue.
            if (CustomGame.IsSlotInQueue(slot)) slot = slot - 6; // selecting a person in the queue where spectator slots are normally at.

            return new Point(CustomGame.SlotLocations[slot].X + xoffset, CustomGame.SlotLocations[slot].Y + yoffset); // Blue, Red, Spectators, and all of queue except for the first slot.
        }

        internal Point OpenSlotMenu(int slot)
        {
            Point slotlocation = FindSlotLocation(slot); // Get location of slot
            if (slotlocation.IsEmpty)
                return Point.Empty;
            //if (slot > 11)
              //  slotlocation.X += -100; // If the slot selected is a spectator or in queue, this prevents the selected slot from sliding to the left.
            
            // Open slot menu by right clicking on slot.
            cg.RightClick(slotlocation);
            cg.updateScreen();
            return slotlocation;
        }

        // Selects an option in the slot menu.
        internal bool SelectMenuOption(Point point)
        {
            cg.MoveMouseTo(point); // Select the option
            Thread.Sleep(100);
            // <image url="$(ProjectDir)\ImageComments\Interact.cs\OptionSelect.png" scale="0.7" />
            cg.updateScreen();
            if (cg.CompareColor(point, new int[] { 83, 133, 155 }, 20)) // Detects if the blue color of the selected option is there, clicks then returns true
            {
                cg.LeftClick(point, 0);
                //cg.//ResetMouse();
                return true;
            }
            return false;
        }

        internal enum Direction
        {
            RightDown,
            RightUp,
        }
        internal Direction Getmenudirection(Point point)
        {
            cg.updateScreen();

            // Tests for the blue outline for the first option selection.
            if (cg.CompareColor(point.X + 12, point.Y + 9, new int[] { 75, 106, 120 }, 5))
                return Direction.RightDown;
            // Tests for the border of the option menu for right/left-up
            else if (cg.CompareColor(point.X + 5, point.Y - 5, new int[] { 166, 165, 166 }, 50))
                return Direction.RightUp;
            else
                return Direction.RightDown;
        }

        public object MenuOptionScan(Point scanLocation, OptionScanFlags flags, string savelocation, Bitmap markup)
        {
            lock (cg.CustomGameLock)
            {
                if (flags.HasFlag(OptionScanFlags.OpenMenu))
                    cg.RightClick(scanLocation);

                // Get direction opened menu is going.
                Direction dir = Getmenudirection(scanLocation);

                double yincrement = 11.65; // Pixel distance between options.
                int xstart = scanLocation.X + 14,  // X position to start scanning
                    xmax = 79, // How far on the X axis to scan
                    ystart = 0, // Y position to start scanning
                    ymax = 6; // How far on the Y axis to scan.

                if (dir == Direction.RightDown)
                {
                    ystart = scanLocation.Y + 12;
                }
                else if (dir == Direction.RightUp)
                {
                    ystart = scanLocation.Y - 18;
                    yincrement = -yincrement;
                }

                cg.updateScreen();
                List<int> percentResults = new List<int>();
                /*
                for (int mi = 0, yii = ystart;
                    mi < max && yii > 15 && yii < cg.bmp.Height - ymax - 15;
                    mi++, yii = ystart + (int)(yincrement * mi)) // Mi is the line to scan, yii is the Y coordinate of line mi.
                    */

                List<Point> optionLocations = new List<Point>();

                int optionIndex = 0;
                int yoffset = 0;
                while (true)
                {
                    int yii = ystart + yoffset + (int)(yincrement * optionIndex);

                    if (10 > yii || yii > cg.bmp.Height - 10)
                        break;

                    // Test for menu split
                    if (optionIndex > 0)
                    {
                        if (dir == Direction.RightDown)
                        {
                            if (cg.CompareColor(xstart - 1, yii - 1, new int[] { 102, 102, 103 }, 25))
                            {
                                yoffset += 3;
                                yii += 3;
                            }
                            else if (cg.CompareColor(xstart - 1, yii - 2, new int[] { 102, 102, 103 }, 25))
                            {
                                yoffset += 2;
                                yii += 2;
                            }
                        }
                        else if (dir == Direction.RightUp)
                        {
                            if (cg.CompareColor(xstart - 1, yii + 8, new int[] { 102, 102, 103 }, 25))
                            {
                                yoffset -= 2;
                                yii -= 2;
                            }
                            else if (cg.CompareColor(xstart - 1, yii + 7, new int[] { 102, 102, 103 }, 25))
                            {
                                yoffset -= 3;
                                yii -= 3;
                            }
                        }
                    }

                    // Get bitmap of option
                    if (savelocation != null)
                    {
                        Bitmap work = cg.BmpClone(xstart, yii, xmax, ymax);

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
                        work.Save(savelocation + "markup-" + optionIndex.ToString() + ".png");
                        work.Dispose();
                    }

                    if (markup != null)
                    {
                        int success = 0;
                        int total = 0;

                        for (int xi = 0; xi < markup.Width; xi++)
                            for (int yi = 0; yi < markup.Height; yi++)
                            {
                                total++;

                                bool bmpPixelIsBlack = cg.CompareColor(xstart + xi, yii + yi, new int[] { 170, 170, 170 }, 80);
                                bool markupPixelIsBlack = markup.GetPixelAt(xi, yi) == Color.FromArgb(0, 0, 0);

                                if (bmpPixelIsBlack == markupPixelIsBlack)
                                    success++;
                            }
                        int percent = (int)(Convert.ToDouble(success) / Convert.ToDouble(total) * 100);
                        percentResults.Add(percent);
                    }

                    optionLocations.Add(new Point(xstart + 12, yii));

                    optionIndex++;
                }

                Point optionLocation = Point.Empty;

                if (markup != null)
                {
                    int maxpercent = percentResults.IndexOf(percentResults.Max());
                    if (percentResults[maxpercent] > 80)
                        optionLocation = optionLocations[maxpercent];
                }

                if (flags.HasFlag(OptionScanFlags.Click))
                {
                    SelectMenuOption(optionLocation);
                }

                // Close the menu.
                if (flags.HasFlag(OptionScanFlags.CloseMenu) || (flags.HasFlag(OptionScanFlags.CloseIfNotFound) && optionLocation == Point.Empty))
                    cg.CloseOptionMenu();

                if (flags.HasFlag(OptionScanFlags.ReturnFound))
                    return optionLocation != Point.Empty;
                else if (flags.HasFlag(OptionScanFlags.ReturnLocation))
                    return optionLocation;
                return null;
            }
        }
        public object MenuOptionScan(int slot, OptionScanFlags flags, string savelocation, Bitmap markup)
        {
            return MenuOptionScan(FindSlotLocation(slot), flags, savelocation, markup);
        }

        public bool PeakOption(Point scanLocation, Bitmap markup)
        {
            return (bool)MenuOptionScan(scanLocation, OptionScanFlags.OpenMenu | OptionScanFlags.CloseMenu | OptionScanFlags.ReturnFound, null, markup);
        }
        public bool PeakOption(int slot, Bitmap markup)
        {
            return PeakOption(FindSlotLocation(slot), markup);
        }

        public bool ClickOption(Point scanLocation, Bitmap markup)
        {
            return (bool)MenuOptionScan(scanLocation, OptionScanFlags.OpenMenu | OptionScanFlags.CloseIfNotFound | OptionScanFlags.ReturnFound | OptionScanFlags.Click, null, markup);
        }
        public bool ClickOption(int slot, Bitmap markup)
        {
            return ClickOption(FindSlotLocation(slot), markup);
        }

        /// <summary>
        /// Removes player from the game
        /// </summary>
        /// <param name="slot">Slot to remove from game.</param>
        /// <returns>Returns true if removing from game was successful</returns>
        /// <include file='docs.xml' path='doc/exceptions/invalidslot/exception'/>
        /// <seealso cref="AI.RemoveFromGameIfAI(int)"/>
        public bool RemoveFromGame(int slot)
        {
            return ClickOption(slot, Markups.REMOVE_FROM_GAME); // Attempt to remove slot from game
        }

        /// <summary>
        /// Swaps player to red team.
        /// </summary>
        /// <param name="slot">Slot to swap to red team.</param>
        /// <returns>Returns true when swapping player to red team is successful.</returns>
        /// <include file='docs.xml' path='doc/exceptions/invalidslot/exception'/>
        public bool SwapToRed(int slot)
        {
            return ClickOption(slot, Markups.SWAP_TO_RED); // Attempt to remove slot from game
        }

        /// <summary>
        /// Swaps player to blue team.
        /// </summary>
        /// <param name="slot">Slot to swap to blue team.</param>
        /// <returns>Returns true when swapping player to blue team is successful.</returns>
        /// <include file='docs.xml' path='doc/exceptions/invalidslot/exception'/>
        public bool SwapToBlue(int slot)
        {
            return ClickOption(slot, Markups.SWAP_TO_BLUE); // Attempt to remove slot from game
        }

        /// <summary>
        /// Swaps the team of a player.
        /// </summary>
        /// <param name="slot">Slot to swap team.</param>
        /// <returns>Returns true if swapping team is successful.</returns>
        /// <include file='docs.xml' path='doc/exceptions/invalidslot/exception'/>
        public bool SwapTeam(int slot)
        {
            // If the player is in blue, use the swap to red option.
            if (CustomGame.IsSlotBlue(slot))
                return ClickOption(slot, Markups.SWAP_TO_RED); // Swap blue player to red
            // If the player is in red, use the swap to blue option.
            else if (CustomGame.IsSlotRed(slot))
                return ClickOption(slot, Markups.SWAP_TO_BLUE); // Swap red player to blue
            else if (CustomGame.IsSlotInQueue(slot))
            {
                QueueTeam team = cg.PlayerInfo.GetQueueTeam(slot);
                if (team == QueueTeam.Blue)
                    return ClickOption(slot, Markups.SWAP_TO_RED);
                else if (team == QueueTeam.Red)
                    return ClickOption(slot, Markups.SWAP_TO_BLUE);
            }
            return false;
        }

        /// <summary>
        /// Swaps player to spectator.
        /// </summary>
        /// <param name="slot">Slot to swap to spectator.</param>
        /// <returns>Returns true if swapping to spectators is successful.</returns>
        /// <include file='docs.xml' path='doc/exceptions/invalidslot/exception'/>
        public bool SwapToSpectators(int slot)
        {
            return ClickOption(slot, Markups.SWAP_TO_SPECTATORS);
        }

        /// <summary>
        /// Swaps player in queue to the neutral team.
        /// </summary>
        /// <param name="slot">Slot to swap to neutral.</param>
        /// <returns>Returns true if swapping player to neutral is successful.</returns>
        /// <include file='docs.xml' path='doc/exceptions/invalidslot/exception'/>
        public bool SwapToNeutral(int slot)
        {
            return ClickOption(slot, Markups.SWAP_TO_NEUTRAL);
        }

        /// <summary>
        /// Removes all AI from the game. Input slot must be an AI.
        /// </summary>
        /// <param name="slot">Slot of an AI.</param>
        /// <returns>Returns true if removing all AI is successful.</returns>
        /// <include file='docs.xml' path='doc/exceptions/invalidslot/exception'/>
        /// <remarks>
        /// <paramref name="slot"/> must be the slot of an AI. If you do not know if the slot will be an AI, use <see cref="AI.RemoveAllBotsAuto"/>.
        /// </remarks>
        /// <seealso cref="AI.RemoveAllBotsAuto"/>
        public bool RemoveAllBots(int slot)
        {
            return ClickOption(slot, Markups.REMOVE_ALL_BOTS);
        }

        /// <summary>
        /// Swap 2 slots with eachother.
        /// </summary>
        /// <param name="targetSlot">Target 1</param>
        /// <param name="destinationSlot">Target 2</param>
        /// <exception cref="InvalidSlotException">Thrown if the <paramref name="targetSlot"/> or <paramref name="destinationSlot"/> argument is out of range of possible slots to move.</exception>
        public void Move(int targetSlot, int destinationSlot)
        {
            lock (cg.CustomGameLock)
            {
                if (!CustomGame.IsSlotValid(targetSlot))
                    throw new InvalidSlotException(string.Format("targetSlot argument '{0}' is out of range.", targetSlot));
                if (!CustomGame.IsSlotValid(destinationSlot))
                    throw new InvalidSlotException(string.Format("destinationSlot argument '{0}' is out of range.", destinationSlot));

                //cg.//ResetMouse();

                cg.updateScreen();
                if (cg.DoesAddButtonExist())
                {
                    cg.LeftClick(Points.LOBBY_MOVE_IF_ADD_BUTTON_PRESENT, 250);
                }
                else
                {
                    cg.LeftClick(Points.LOBBY_MOVE_IF_ADD_BUTTON_NOT_PRESENT, 250);
                }

                Point targetSlotLoc = FindSlotLocation(targetSlot);
                Point destinationSlotLoc = FindSlotLocation(destinationSlot);
                cg.LeftClick(targetSlotLoc, 250);
                cg.LeftClick(destinationSlotLoc, 250);

                Thread.Sleep(200);

                ExitMoveMenu();
            }
        }

        /// <summary>
        /// Swaps players on both teams.
        /// </summary>
        public void SwapAll()
        {
            lock (cg.CustomGameLock)
            {
                bool aistatus = cg.DoesAddButtonExist();

                // click move
                if (aistatus)
                {
                    cg.LeftClick(Points.LOBBY_MOVE_IF_ADD_BUTTON_PRESENT, 25);
                }
                else
                {
                    cg.LeftClick(Points.LOBBY_MOVE_IF_ADD_BUTTON_NOT_PRESENT, 25);
                }

                // click swap all
                if (aistatus)
                {
                    cg.LeftClick(Points.LOBBY_SWAP_ALL_IF_ADD_BUTTON_PRESENT, 25);
                }
                else
                {
                    cg.LeftClick(Points.LOBBY_SWAP_ALL_IF_ADD_BUTTON_NOT_PRESENT, 25);
                }

                ExitMoveMenu();
            }
        }

        private void ExitMoveMenu()
        {
            cg.updateScreen();

            // Can't use DoesAddButtonExist here because the color of the buttons change

            Color color = cg.GetPixelAt(661, 175);
            if (color.R - color.B > 40)
                cg.LeftClick(Points.LOBBY_MOVE_IF_ADD_BUTTON_PRESENT, 50);
            else
                cg.LeftClick(Points.LOBBY_MOVE_IF_ADD_BUTTON_NOT_PRESENT, 50);

            cg.ResetMouse();
        }
    }

    [Flags]
    public enum OptionScanFlags
    {
        None = 0,
        OpenMenu = 1 << 0,
        CloseMenu = 1 << 1,
        CloseIfNotFound = 1 << 2,
        Click = 1 << 3,
        ReturnLocation = 1 << 4,
        ReturnFound = 1 << 5
    }
}
