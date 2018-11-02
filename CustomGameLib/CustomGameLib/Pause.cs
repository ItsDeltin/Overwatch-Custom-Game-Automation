using System.Threading;
using System.Windows.Forms;

namespace Deltin.CustomGameAutomation
{
    partial class CustomGame
    {
        /// <summary>
        /// Controls Overwatch's pause feature.
        /// </summary>
        public Pause Pause { get; private set; }
    }
    /// <summary>
    /// Controls Overwatch's pause feature.
    /// </summary>
    /// <remarks>
    /// The Pause class is accessed in a CustomGame object on the <see cref="CustomGame.Pause"/> field.
    /// </remarks>
    public class Pause : CustomGameBase
    {
        internal Pause(CustomGame cg) : base(cg) { }

        /// <summary>
        /// Toggle pause.
        /// </summary>
        public void TogglePause()
        {
            using (cg.LockHandler.SemiPassive)
            {
                if (cg.OpenChatIsDefault)
                {
                    cg.Chat.CloseChat();
                    Thread.Sleep(250);
                }

                cg.KeyDown(Keys.Control);
                cg.KeyDown(Keys.Shift);
                cg.AlternateInput(0xBB);
                cg.KeyUp(Keys.Shift);
                cg.KeyUp(Keys.Control);

                if (cg.OpenChatIsDefault)
                    cg.Chat.OpenChat();
            }
        }
        /// <summary>
        /// Pauses the game.
        /// </summary>
        public void PauseGame()
        {
            using (cg.LockHandler.SemiPassive)
            {
                if (!IsPaused())
                    TogglePause();
            }
        }
        /// <summary>
        /// Unpauses the game.
        /// </summary>
        public void UnpauseGame()
        {
            using (cg.LockHandler.SemiPassive)
            {
                if (IsPaused())
                    TogglePause();
            }
        }
        /// <summary>
        /// Determines if the game is paused.
        /// </summary>
        public bool IsPaused()
        {
            using (cg.LockHandler.SemiPassive)
            {
                cg.updateScreen();
                // Check if the pause text is there.
                return Capture.CompareColor(Points.LOBBY_PAUSED, new int[] { 187, 138, 79 }, 10);
            }
        }
    }
}
