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
        /// <summary>
        /// Gets the slots that have changed.
        /// </summary>
        /// <param name="slotInfo">The data of the last scan.</param>
        /// <returns>A List of slots that changed.</returns>
        /// <remarks>
        /// <para>The returned slots are slots that have been swapped, removed, or added to the game.</para>
        /// <para>The data of the scan is saved in <paramref name="slotInfo"/>. It is compared to next time it is used with this method.
        /// A new <see cref="SlotInfo"/> object will return every slot.</para>
        /// </remarks>
        /// <seealso cref="SlotInfo"/>
        public List<int> GetUpdatedSlots(SlotInfo slotInfo)
        {
            using (LockHandler.Passive)
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

    /// <summary>
    /// Data about the saved slots.
    /// </summary>
    /// <seealso cref="CustomGame.GetUpdatedSlots(SlotInfo)"/>
    public class SlotInfo : IDisposable
    {
        /// <summary>
        /// Creates a new SlotInfo object.
        /// </summary>
        public SlotInfo() { }
        internal SlotIdentity[] SlotIdentities = new SlotIdentity[CustomGame.SlotCount];

        /// <summary>
        /// Disposes data used by the object.
        /// </summary>
        public void Dispose()
        {
            for (int i = 0; i < SlotIdentities.Length; i++)
                SlotIdentities[i].Dispose();
        }
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
