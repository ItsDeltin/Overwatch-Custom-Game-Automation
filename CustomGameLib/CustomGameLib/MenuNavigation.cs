using System;
using System.Threading;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace Deltin.CustomGameAutomation
{
    partial class CustomGame
    {

        static void Activate(IntPtr hWnd)
        {
            User32.PostMessage(hWnd, 0x0006, 2, 0);
            User32.PostMessage(hWnd, 0x0086, 1, 0);
            User32.PostMessage(hWnd, 0x0007, 0, 0);
        }

        /// <summary>
        /// Go back to executing position.
        /// </summary>
        void BackToMenu()
        {
            // Make sure the server lobby menu is not opened
            if (!(bmp.CompareColor(835, 179, new int[] { 121, 152, 184 }, 50) // Test for blue button at top of screen
                    && bmp.CompareColor(704, 67, new int[] { 78, 122, 158 }, 20))) // Test for invite players to group button
            {
                Activate(OverwatchHandle);
                if (OpenChatIsDefault)
                    KeyPress(Keys.Escape);
                Thread.Sleep(1000);
                Activate(OverwatchHandle);
                KeyPress(Keys.L);
                Thread.Sleep(1000);
                Activate(OverwatchHandle);

                if (OpenChatIsDefault)
                    Chat.OpenChat();
            }

            ResetMouse();
        }

        /// <summary>
        /// Stalls the program to wait for the map to finish loading.
        /// </summary>
        void LoadStall()
        {
            updateScreen();
            Stopwatch sw = new Stopwatch();
            Stopwatch sc = new Stopwatch();
            sw.Start();
            while (true)
            {
                // for debugging
                /*
                Console.WriteLine("Start color: {0}\nEntering Game color: {1}\nBlack Screen: {2}\nLoading logo: {3}",
                    bmp.CompareColor(CALData.StartGameLocation.X, CALData.StartGameLocation.Y, CALData.StartGameColor, CALData.StartGameFade),
                    bmp.CompareColor(450, 325, new int[] { 206, 169, 122 }, 15),
                    bmp.CompareColor(400, 300, new int[] { 64, 64, 64 }, 15),
                    bmp.CompareColor(853, 483, new int[] { 154, 157, 157 }, 15));
                */
                if (
                    bmp.CompareColor(CALData.StartGameLocation.X, CALData.StartGameLocation.Y, CALData.StartGameColor, CALData.StartGameFade) || // Test for "START" button color

                    bmp.CompareColor(450, 325, new int[] { 206, 169, 122 }, 15) || // Test for "ENTERING GAME" color

                    bmp.CompareColor(400, 300, new int[] { 64, 64, 64 }, 15) || // Test for black screen color

                    bmp.CompareColor(853, 483, new int[] { 154, 157, 157 }, 15) // Test for overwatch loading logo
                    )
                {
                    sc.Reset(); // reset timer
                }
                else
                {
                    sc.Start(); // (re)start timer.
                }
                if (sc.ElapsedMilliseconds > 3 * 1000 || sw.ElapsedMilliseconds >= 15 * 1000) break;
                //Console.WriteLine(sc.ElapsedMilliseconds / 1000 + "\n"); // for debugging
                Thread.Sleep(1000);
                updateScreen();
            }
            //Console.WriteLine("DONE"); // for debugging
        }

        /// <summary>
        /// Restarts the game.
        /// </summary>
        public void RestartGame()
        {
            LeftClick(500, 455);
            LoadStall();
            BackToMenu();
        }

        /// <summary>
        /// Starts the game.
        /// </summary>
        public void StartGame()
        {
            LeftClick(451, 458, 3000);
            LoadStall();
            BackToMenu();
        }

        /// <summary>
        /// Sends server to the lobby.
        /// </summary>
        public void SendServerToLobby()
        {
            LeftClick(400, 455, 750);
            WaitForColor(CALData.StartGameLocation.X, CALData.StartGameLocation.Y, CALData.StartGameColor, CALData.StartGameFade, 5000);
            ResetMouse();
        }

        /// <summary>
        /// Starts the game when waiting for players.
        /// </summary>
        public void StartGamemode()
        {
            LeftClick(570, 455, 1000);
        }

        // go to settings
        private void gotosettings()
        {
            updateScreen();
            // The "Add AI" button moves the "Settings" button, this detects if that happens.
            if (addbutton())
                LeftClick(716, 180, 250); // "Add AI" Button
            else
                LeftClick(774, 180, 250); // No "Add AI" Button
        }

        private bool addbutton()
        {
            updateScreen();
            int[] addAIcolor = new int[] { 121, 152, 184 };
            // The "Add AI" button moves the "Settings" button, this detects if that happens.
            if (bmp.CompareColor(659, 180, addAIcolor, 50)) return true; // "Add AI" Button
            else return false; // No "Add AI" Button
        }

        private void GoBack(int settingpages, params int[] checkForErrorsAt)
        {
            for (int i = 0; i < settingpages; i++)
            {
                LeftClick(855, 507);
                if (checkForErrorsAt.Contains(i))
                {
                    updateScreen();
                    if (bmp.CompareColor(CALData.ErrorLocation.X, CALData.ErrorLocation.Y, CALData.ErrorColor, CALData.ErrorFade))
                        LeftClick(436, 318);
                }
            }
        }
    }
}
