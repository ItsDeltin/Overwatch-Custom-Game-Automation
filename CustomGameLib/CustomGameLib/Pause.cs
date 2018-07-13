using System.Threading;
using System.Windows.Forms;

namespace Deltin.CustomGameAutomation
{
    partial class CustomGame
    {
        /// <summary>
        /// Controls Overwatch's pause feature.
        /// </summary>
        public CG_Pause Pause;
        /// <summary>
        /// Controls Overwatch's pause feature.
        /// </summary>
        public class CG_Pause
        {
            private CustomGame cg;
            internal CG_Pause(CustomGame cg)
            { this.cg = cg; }

            /// <summary>
            /// Toggle pause.
            /// </summary>
            public void TogglePause()
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
            /// <summary>
            /// Pauses the game.
            /// </summary>
            public void Pause()
            {
                if (!IsPaused())
                    TogglePause();
            }
            /// <summary>
            /// Unpauses the game.
            /// </summary>
            public void Unpause()
            {
                if (IsPaused())
                    TogglePause();
            }
            /// <summary>
            /// Determines if the game is paused.
            /// </summary>
            public bool IsPaused()
            {
                cg.updateScreen();
                // Check if the pause text is there.
                return cg.CompareColor(441, 268, new int[] { 187, 138, 79 }, 10);
            }
        }
    }
}
