using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Deltin.CustomGameAutomation
{
#pragma warning disable CS1591
    [Serializable]
    public abstract class Identity : IDisposable
    {
        internal Identity(DirectBitmap identityMarkup)
        {
            IdentityMarkup = identityMarkup;
        }

        internal DirectBitmap IdentityMarkup;

        public static bool Compare(Identity i1, Identity i2)
        {
            if (i1.IdentityMarkup.Width != i2.IdentityMarkup.Width || i1.IdentityMarkup.Height != i2.IdentityMarkup.Height || i1.GetType() != i2.GetType())
                return false;

            return i1.IdentityMarkup.CompareTo(i2.IdentityMarkup, i1.Fade, i1.PercentMatches, DBCompareFlags.Multithread);
        }

        protected virtual int PercentMatches { get { return 97; } }
        protected virtual int Fade { get { return 50; } }

        public void Dispose()
        {
            Disposed = true;
            if (!Disposed && IdentityMarkup != null)
                IdentityMarkup.Dispose();
        }
        private bool Disposed = false;
    }
#pragma warning restore CS1591

    /// <summary>
    /// Contains data for identifying players who executed a command.
    /// </summary>
    [Serializable]
    public class PlayerIdentity : Identity
    {
        internal PlayerIdentity(DirectBitmap careerProfileMarkup) : base(careerProfileMarkup) { }
    }

    /// <summary>
    /// Contains data for identifying players who executed a command.
    /// </summary>
    [Serializable]
    public class ChatIdentity : Identity
    {
        internal ChatIdentity(DirectBitmap chatMarkup) : base(chatMarkup) { }
    }

    partial class CustomGame
    {
        /// <summary>
        /// Gets the player identity of a slot.
        /// </summary>
        /// <param name="slot">Slot to check.</param>
        /// <returns>The player identity of the slot.</returns>
        public PlayerIdentity GetPlayerIdentity(int slot)
        {
            using (LockHandler.Interactive)
            {
                bool careerProfileOpenSuccess = Interact.ClickOption(slot, Markups.VIEW_CAREER_PROFILE);
                if (!careerProfileOpenSuccess)
                    return null;

                WaitForCareerProfileToLoad();

                UpdateScreen();

                DirectBitmap careerProfile = Capture.Clone(Rectangles.LOBBY_CAREER_PROFILE);

                GoBack(1);

                Thread.Sleep(500);

                return new PlayerIdentity(careerProfile);
            }
        }

        /// <summary>
        /// Gets the identity and name of a slot.
        /// </summary>
        /// <param name="slot">Slot to check.</param>
        /// <param name="pi">The <see cref="PlayerIdentity" /> of the slot.</param>
        /// <param name="name">The name of the slot.</param>
        public void GetPlayerIdentityAndName(int slot, out PlayerIdentity pi, out string name)
        {
            using (LockHandler.Interactive)
            {
                bool careerProfileOpenSuccess = Interact.ClickOption(slot, Markups.VIEW_CAREER_PROFILE);
                if (!careerProfileOpenSuccess)
                {
                    pi = null;
                    name = null;
                    return;
                }

                WaitForCareerProfileToLoad();

                UpdateScreen();

                pi = new PlayerIdentity(Capture.Clone(Rectangles.LOBBY_CAREER_PROFILE));
                name = GetPlayerName();

                GoBack(1);

                Thread.Sleep(500);
            }
        }
    }
}
