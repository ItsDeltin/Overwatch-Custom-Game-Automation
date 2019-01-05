using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Drawing;
using System.Diagnostics;

namespace Deltin.CustomGameAutomation
{
    partial class CustomGame
    {
        /// <summary>
        /// Invites a player to the game via battletag.
        /// </summary>
        /// <param name="playerName">Battletag of the player to invite. Is case sensitive. Ex: Tracer#1818</param>
        /// <param name="team">Team that the invited player will join.</param>
        /// <returns>Returns true if <paramref name="playerName"/> is a valid battletag.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="playerName"/> is null.</exception>
        public bool InvitePlayer(string playerName, Team team)
        {
            using (LockHandler.Interactive)
            {
                if (playerName == null)
                    throw new ArgumentNullException(nameof(playerName));
                if (team.HasFlag(Team.Queue))
                    throw new ArgumentOutOfRangeException(nameof(team), team, "Team cannot be Queue.");

                UpdateScreen();
                // check if the add AI button is there.
                // because the invite button gets moved if it is/isnt there.
                if (DoesAddButtonExist())
                {
                    LeftClick(Points.LOBBY_INVITE_IF_ADD_BUTTON_PRESENT, 250); // click invite
                }
                else
                {
                    LeftClick(Points.LOBBY_INVITE_IF_ADD_BUTTON_NOT_PRESENT, 250); // click invite
                }

                LeftClick(Points.INVITE_VIA_BATTLETAG, 100); // click via battletag

                TextInput(playerName);

                if (team != Team.BlueAndRed)
                {
                    LeftClick(Points.INVITE_TEAM_DROPDOWN);
                    if (team.HasFlag(Team.Blue))
                    {
                        LeftClick(Points.INVITE_TEAM_BLUE);
                    }
                    else if (team.HasFlag(Team.Red))
                    {
                        LeftClick(Points.INVITE_TEAM_RED);
                    }
                    else if (team.HasFlag(Team.Spectator))
                    {
                        LeftClick(Points.INVITE_TEAM_SPECTATOR);
                    }
                }

                Thread.Sleep(200);

                UpdateScreen();

                if (Capture.CompareColor(Points.INVITE_INVITE, Colors.CONFIRM, Fades.CONFIRM))
                {
                    LeftClick(Points.INVITE_INVITE); // invite player
                    //ResetMouse();
                    return true;
                }
                else
                {
                    LeftClick(Points.INVITE_BACK); // click back
                    //ResetMouse();
                    return false;
                }
            }
        }

        /// <summary>
        /// Invites a player to the game via battletag.
        /// </summary>
        /// <param name="playerName">Battletag of the player to invite. Is case sensitive. Ex: Tracer#1818</param>
        /// <param name="slot">Slot that the invited player will join.</param>
        /// <returns>Returns true if <paramref name="playerName"/> is a valid battletag.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="playerName"/> is null.</exception>
        public bool InvitePlayer(string playerName, int slot)
        {
            using (LockHandler.Interactive)
            {
                if (playerName == null)
                    throw new ArgumentNullException(nameof(playerName));

                if (!IsSlotValid(slot))
                    throw new InvalidSlotException(slot);

                if (IsSlotInQueue(slot))
                    throw new InvalidSlotException("slot cannot be in queue.");

                if (AllSlots.Contains(slot))
                    return false;

                LeftClick(Interact.FindSlotLocation(slot));

                LeftClick(Points.INVITE_VIA_BATTLETAG, 100); // click via battletag

                TextInput(playerName);

                Thread.Sleep(200);

                UpdateScreen();

                if (Capture.CompareColor(Points.INVITE_INVITE, Colors.CONFIRM, Fades.CONFIRM))
                {
                    LeftClick(Points.INVITE_INVITE); // invite player
                    //ResetMouse();
                    return true;
                }
                else
                {
                    LeftClick(Points.INVITE_BACK); // click back
                    //ResetMouse();
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets the slots that are invited.
        /// </summary>
        /// <returns>The list of slots invited.</returns>
        public List<int> GetInvitedSlots()
        {
            using (LockHandler.Interactive)
            {
                // Get all non-AI players
                List<int> players = GetSlots(SlotFlags.Blue | SlotFlags.Red | SlotFlags.Spectators | SlotFlags.Queue);

                // Close the chat if it is opened
                if (OpenChatIsDefault && players.Contains(5))
                    Chat.CloseChat();

                List<InviteScanData> slotData = new List<InviteScanData>();

                Stopwatch sw = new Stopwatch();
                sw.Start();

                double iterations = 0;

                while (sw.ElapsedMilliseconds <= InviteScanData.MSToScan)
                {
                    UpdateScreen();
                    foreach (int slot in players)
                    {
                        // Get the data relating to the slot.
                        InviteScanData previousSlotData = slotData.FirstOrDefault(v => v.Slot == slot);

                        // Copy a 20x20 (10x10 if spectator) pixel square of the invite icon animation.
                        Point scanAt = AddToSlotOrigin(IsSlotBlueOrRed(slot) ? InviteScanData.Origin : InviteScanData.SpectatorOrigin, slot, true);
                        int range = IsSlotBlueOrRed(slot) ? InviteScanData.PlayerRange : InviteScanData.SpectatorRange;
                        DirectBitmap markup = Capture.Clone(scanAt.X - range, scanAt.Y - range, range * 2, range * 2);

                        // If there is no previous record of the animation for the slot, create it.
                        if (previousSlotData == null)
                        {
                            slotData.Add(new InviteScanData(slot, markup));
                        }
                        else
                        {
                            // If there is, compare it to the previous one.

                            bool matching = previousSlotData.Markup.CompareTo(markup, InviteScanData.MarkupFade, InviteScanData.MarkupDifference, DBCompareFlags.Multithread);

                            previousSlotData.Markup.Dispose();
                            previousSlotData.Markup = markup;

                            if (!matching)
                            {
                                previousSlotData.Changes++;
                            }
                        }
                    }

                    iterations++;
                    Thread.Sleep(20);
                }

                for (int i = 0; i < slotData.Count; i++)
                    slotData[i].Markup.Dispose();

                if (OpenChatIsDefault && players.Contains(5))
                    Chat.OpenChat();

                return slotData.Where(slot => slot.Changes / iterations * 100 >
                (IsSlotBlueOrRed(slot.Slot) ? InviteScanData.Player_MarkInvitedWithPercentageChanged : InviteScanData.Spectator_MarkInvitedWithPercentageChanged))
                .Select(slot => slot.Slot).ToList();
            }
        }

        /// <summary>
        /// Gets the number of slots that are invited.
        /// </summary>
        /// <returns>The number of slots invited.</returns>
        public int GetInvitedCount()
        {
            return GetInvitedSlots().Count;
        }

        internal Point AddToSlotOrigin(Point origin, int slot, bool noUpdate = false)
        {
            Point newPoint = new Point(origin.X, origin.Y);

            if (IsSlotBlueOrRed(slot))
            {
                if (IsSlotRed(slot))
                    newPoint.X += Distances.LOBBY_TEAM_SLOT_DISTANCE;

                newPoint.Y += Distances.LOBBY_SLOT_DISTANCE * (IsSlotBlue(slot) ? slot : slot - 6);
            }
            else
            {
                if (IsSlotSpectator(slot))
                {
                    newPoint.Y += FindSpectatorOffset(noUpdate) + (Distances.LOBBY_SPECTATOR_SLOT_DISTANCE * (slot - SpectatorID));
                }
                else if (IsSlotInQueue(slot))
                {
                    newPoint.Y += (Distances.LOBBY_SPECTATOR_SLOT_DISTANCE * (slot - QueueID)) - Distances.LOBBY_QUEUE_OFFSET;
                }
            }

            return newPoint;
        }

        internal class InviteScanData
        {
            public static readonly Point Origin = new Point(176, 252);
            public const int PlayerRange = 10;
            public static readonly Point SpectatorOrigin = new Point(779, 251);
            public const int SpectatorRange = 6;

            public const double MarkupDifference = 99;
            public const int MarkupFade = 3;
            public const int MSToScan = 250;
            public const int Player_MarkInvitedWithPercentageChanged = 30; // 50%
            public const int Spectator_MarkInvitedWithPercentageChanged = 15; // 25%
            
            public InviteScanData(int slot, DirectBitmap markup)
            {
                Slot = slot;
                Markup = markup;
            }

            public int Slot;
            public DirectBitmap Markup;
            public double Changes = 0;
        }
    }
}
