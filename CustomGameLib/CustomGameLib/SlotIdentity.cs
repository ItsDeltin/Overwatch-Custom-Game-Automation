using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;

namespace Deltin.CustomGameAutomation
{
    partial class CustomGame
    {
        public List<int> GetUpdatedSlots(SlotInfo slotInfo)
        {
            List<int> changedSlots = new List<int>();

            var allSlots = AllSlots;

            //for (int slot = 0; slot < CustomGame.SlotCount; slot++)
            Parallel.For(0, SlotCount, (slot) =>
            {
                SlotIdentity slotIdentity = allSlots.Contains(slot) ? GetSlotIdentity(slot) : null;

                bool changed = true;

                // Slot emptied
                if (slotIdentity == null && slotInfo.SlotIdentities[slot] != null)
                {
                    slotInfo.SlotIdentities[slot].Dispose();
                    slotInfo.SlotIdentities[slot] = null;
                }
                // Slot joined
                else if (slotIdentity != null && slotInfo.SlotIdentities[slot] == null)
                {
                    slotInfo.SlotIdentities[slot] = slotIdentity;
                }
                else if (slotIdentity == null && slotInfo.SlotIdentities[slot] == null)
                {
                    changed = false;
                }
                // Slot swapped
                else if (!slotInfo.SlotIdentities[slot].CompareIdentities(slotIdentity))
                {
                    slotInfo.SlotIdentities[slot].Dispose();
                    slotInfo.SlotIdentities[slot] = slotIdentity;
                }
                else
                {
                    slotIdentity.Dispose();
                    changed = false;
                }

                if (changed)
                    changedSlots.Add(slot);
            });

            return changedSlots;
        }

        private SlotIdentity GetSlotIdentity(int slot)
        {
            if (!AllSlots.Contains(slot))
                return null;

            if (slot == 5 && OpenChatIsDefault)
                Chat.CloseChat();
            if (slot == 0)
                ResetMouse();

            Point origin = Point.Empty;
            int width = 0;
            int height = 0;

            if (IsSlotBlueOrRed(slot))
            {
                width = 158;
                height = Distances.LOBBY_SLOT_HEIGHT;

                int comp = slot;
                if (IsSlotBlue(slot))
                {
                    origin = new Point(145, 239);
                }
                else if (IsSlotRed(slot))
                {
                    origin = new Point(372, 239);
                    comp -= 6;
                }
                origin.Y += Distances.LOBBY_SLOT_DISTANCE * comp;
            }
            else if (IsSlotSpectatorOrQueue(slot))
            {
                width = 158;
                height = Distances.LOBBY_SPECTATOR_SLOT_HEIGHT;
                origin = new Point(666, 245);

                int comp = slot;
                if (IsSlotSpectator(slot))
                {
                    origin.Y += FindSpectatorOffset(true);
                    comp -= Spectatorid;
                }
                else if (IsSlotInQueue(slot))
                {
                    origin.Y -= Distances.LOBBY_QUEUE_OFFSET;
                    comp -= Queueid;
                }
                origin.Y += Distances.LOBBY_SPECTATOR_SLOT_DISTANCE * comp;
            }

            updateScreen();
            DirectBitmap identity = Capture.Clone(origin.X, origin.Y, width, height);

            if (slot == 5 && OpenChatIsDefault)
                Chat.OpenChat();

            return new SlotIdentity(identity, slot);
        }
    }

    public class SlotInfo
    {
        public SlotInfo() { }
        internal SlotIdentity[] SlotIdentities = new SlotIdentity[CustomGame.SlotCount];
    }
    internal class SlotIdentity : Identity
    {
        internal SlotIdentity(DirectBitmap identity, int slot) : base(identity)
        {
            Slot = slot;
        }

        public int Slot { get; private set; }

        public bool CompareIdentities(SlotIdentity other)
        {
            return Identity.CompareIdentities(this, other);
        }
    }
}
