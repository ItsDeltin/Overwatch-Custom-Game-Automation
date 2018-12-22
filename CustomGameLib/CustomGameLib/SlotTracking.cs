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
        private SlotIdentity GetSlotIdentity(int slot)
        {
            using (LockHandler.Passive)
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
                        comp -= SpectatorID;
                    }
                    else if (IsSlotInQueue(slot))
                    {
                        origin.Y -= Distances.LOBBY_QUEUE_OFFSET;
                        comp -= QueueID;
                    }
                    origin.Y += Distances.LOBBY_SPECTATOR_SLOT_DISTANCE * comp;
                }

                UpdateScreen();
                DirectBitmap identity = Capture.Clone(origin.X, origin.Y, width, height);

                if (slot == 5 && OpenChatIsDefault)
                    Chat.OpenChat();

                return new SlotIdentity(identity, slot);
            }
        }

        /// <summary>
        /// Gets the slots that have changed.
        /// </summary>
        /// <param name="slotInfo">The data of the last scan.</param>
        /// <param name="slotFlags">Flags for the slots to check.</param>
        /// <returns>A List of slots that changed.</returns>
        /// <remarks>
        /// <para>The returned slots are slots that have been swapped, removed, or added to the game.</para>
        /// <para>The data of the scan is saved in <paramref name="slotInfo"/>. It is compared to next time it is used with this method.
        /// A new <see cref="SlotInfo"/> object will return every slot.</para>
        /// </remarks>
        /// <seealso cref="SlotInfo"/>
        public List<int> GetUpdatedSlots(SlotInfo slotInfo, SlotFlags slotFlags = SlotFlags.All)
        {
            using (LockHandler.Passive)
            {
                object listLock = new object();
                List<int> changedSlots = new List<int>();

                var slots = GetSlots(slotFlags);

                //for (int slot = 0; slot < SlotCount; slot++)
                Parallel.For(0, SlotCount, (slot) =>
                {
                    SlotIdentity slotIdentity = slots.Contains(slot) ? GetSlotIdentity(slot) : null;

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
                    // Slot swapped
                    else if (slotIdentity != null && slotInfo.SlotIdentities[slot] != null && !slotInfo.SlotIdentities[slot].CompareIdentities(slotIdentity))
                    {
                        slotInfo.SlotIdentities[slot].Dispose();
                        slotInfo.SlotIdentities[slot] = slotIdentity;
                    }
                    // Slot did not change
                    else
                    {
                        if (slotIdentity != null) slotIdentity.Dispose();
                        changed = false;
                    }

                    if (changed)
                        lock (listLock)
                            changedSlots.Add(slot);
                });

                return changedSlots;
            }
        }

        /// <summary>
        /// Tracks player slots.
        /// </summary>
        /// <param name="playerTracker"></param>
        /// <param name="slotFlags">Slots to track.</param>
        public void TrackPlayers(PlayerTracker playerTracker, SlotFlags slotFlags = SlotFlags.All)
        {
            if (playerTracker == null)
                throw new ArgumentNullException(nameof(playerTracker));

            using (LockHandler.Interactive)
            {
                List<int> changedSlots = GetUpdatedSlots(playerTracker.SlotInfo, slotFlags);

                var slots = GetSlots(slotFlags);

                foreach (int slot in changedSlots)
                    if (slots.Contains(slot))
                    {
                        PlayerIdentity newIdentity = Commands.GetPlayerIdentity(slot);
                        if (newIdentity == null)
                            continue;

                        var pi = playerTracker._players.FirstOrDefault(p => newIdentity.CompareIdentities(p.PlayerIdentity));

                        // New player joined the game
                        if (pi == null)
                        {
                            playerTracker._players.Add(new PlayerTrackerSlot(newIdentity, slot));
                        }
                        // Player swapped slots
                        else
                        {
                            pi.Slot = slot;
                            newIdentity.Dispose();
                        }
                    }

                // Remove players that left the game.
                for (int i = playerTracker._players.Count - 1; i >= 0; i--)
                    if (!slots.Contains(playerTracker._players[i].Slot))
                    {
                        playerTracker._players[i].PlayerIdentity.Dispose();
                        playerTracker._players.RemoveAt(i);
                    }
            }
        }

        /// <summary>
        /// Waits for the slots in overwatch to change.
        /// </summary>
        /// <param name="maxtime">Time to wait. Set to -1 to wait forever.</param>
        /// <returns>Returns true if Overwatch's slots changed. Returns false if the time ran out.</returns>
        public bool WaitForSlotUpdate(int maxtime = 1000)
        {
            using (LockHandler.Passive)
            {
                using (SlotInfo slotInfo = new SlotInfo())
                {
                    GetUpdatedSlots(slotInfo);

                    Stopwatch time = new Stopwatch();
                    time.Start();
                    while (time.ElapsedMilliseconds < maxtime || maxtime == -1)
                    {
                        if (GetUpdatedSlots(slotInfo).Count > 0)
                            return true;
                        Thread.Sleep(10);
                    }
                    return false;
                }
            }
        }
    }

    /// <summary>
    /// Data about the saved slots.
    /// </summary>
    /// <seealso cref="CustomGame.GetUpdatedSlots(SlotInfo, SlotFlags)"/>
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
                if (SlotIdentities[i] != null)
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

    /// <summary>
    /// Tracks player's slots.
    /// </summary>
    /// <seealso cref="CustomGame.TrackPlayers(PlayerTracker, SlotFlags)"/>
    public class PlayerTracker : IDisposable
    {
        /// <summary>
        /// Tracks player's slots.
        /// </summary>
        public PlayerTracker()
        {

        }

        /// <summary>
        /// Gets the player's slot from a player's identity.
        /// </summary>
        /// <param name="identity">The identity of the player.</param>
        /// <returns>Returns the slot of the player. Returns -1 if it is not found.</returns>
        public int SlotFromPlayerIdentity(PlayerIdentity identity)
        {
            if (identity == null)
                throw new ArgumentNullException(nameof(identity), $"{nameof(identity)} was null.");

            foreach (var player in _players)
                if (player.PlayerIdentity.CompareIdentities(identity))
                    return player.Slot;
            return -1;
        }

        internal SlotInfo SlotInfo = new SlotInfo();
        internal List<PlayerTrackerSlot> _players = new List<PlayerTrackerSlot>();

        /// <summary>
        /// List of players in the game, including their slot and player identity.
        /// </summary>
        public IReadOnlyList<PlayerTrackerSlot> Players { get { return _players.AsReadOnly(); } }

        /// <summary>
        /// Disposes data used by the object.
        /// </summary>
        public void Dispose()
        {
            SlotInfo.Dispose();
        }
    }

    public class PlayerTrackerSlot
    {
        public PlayerTrackerSlot(PlayerIdentity identity, int slot)
        {
            PlayerIdentity = identity;
            Slot = slot;
        }

        public PlayerIdentity PlayerIdentity { get; private set; }
        public int Slot { get; internal set; }
    }
}
