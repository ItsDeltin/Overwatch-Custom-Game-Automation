using System.Threading;
using System.Windows.Forms;

namespace Deltin.CustomGameAutomation
{
    partial class CustomGame
    {
        public CG_Pause Pause;
        public class CG_Pause
        {
            private CustomGame cg;
            internal CG_Pause(CustomGame cg)
            { this.cg = cg; }

            // Pause/Unpause the game

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
                cg.updateScreen();
                // Check if the pause text is there. If not, toggle pause.
                if (cg.CompareColor(441, 268, new int[] { 187, 138, 79 }, 10) == false)
                    TogglePause();
            }
            /// <summary>
            /// Unpauses the game.
            /// </summary>
            public void Unpause()
            {
                cg.updateScreen();
                // Check if the pause text is there. If it is, toggle pause.
                if (cg.CompareColor(441, 268, new int[] { 187, 138, 79 }, 10))
                    TogglePause();
            }
        }
    }
}
