﻿using System;
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
            if (!IsLobbyOpened())
            {
                Activate(OverwatchHandle);
                Thread.Sleep(250);
                KeyPress(Keys.L);
                Thread.Sleep(250);
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
                /*
                // for debugging
                Console.WriteLine("Start color: {0}\nEntering Game color: {1}\nBlack Screen: {2}\nLoading logo: {3}",
                    CompareColor(CALData.StartGameLocation.X, CALData.StartGameLocation.Y, CALData.StartGameColor, CALData.StartGameFade),
                    CompareColor(450, 325, new int[] { 176, 141, 89 }, 20),
                    CompareColor(400, 300, new int[] { 64, 64, 64 }, 15),
                    CompareColor(853, 483, new int[] { 154, 157, 157 }, 15));
                */
                if (
                    CompareColor(CALData.StartGameLocation.X, CALData.StartGameLocation.Y, CALData.StartGameColor, CALData.StartGameFade) || // Test for "START" button color

                    CompareColor(450, 325, new int[] { 176, 141, 89 }, 20) || // Test for "ENTERING GAME" color

                    CompareColor(400, 300, new int[] { 64, 64, 64 }, 15) // Test for black screen color

                    //CompareColor(853, 483, new int[] { 154, 157, 157 }, 15) // Test for overwatch loading logo
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
            Thread.Sleep(1000);
            //Console.WriteLine("DONE"); // for debugging
        }

        /// <summary>
        /// Restarts the game.
        /// </summary>
        public void RestartGame()
        {
            if (OpenChatIsDefault)
                Chat.CloseChat();

            LeftClick(500, 455);
            LoadStall();
            BackToMenu();

            if (OpenChatIsDefault)
                Chat.OpenChat();
        }

        /// <summary>
        /// Starts the game.
        /// </summary>
        public void StartGame()
        {
            if (OpenChatIsDefault)
                Chat.CloseChat();

            LeftClick(451, 458, 3000);
            LoadStall();
            BackToMenu();

            if (OpenChatIsDefault)
                Chat.OpenChat();
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
        internal void GoToSettings()
        {
            updateScreen();
            // The "Add AI" button moves the "Settings" button, this detects if that happens.
            if (DoesAddButtonExist())
                LeftClick(716, 180, 250); // "Add AI" Button
            else
                LeftClick(774, 180, 250); // No "Add AI" Button
        }

        private bool DoesAddButtonExist()
        {
            updateScreen();

            return CompareColor(
                700, 182, // Location of the "MOVE" button
                715, 182, // Location of the "SETTINGS" button
                20);
        }

        internal void GoBack(int settingpages, params int[] checkForErrorsAt)
        {
            for (int i = 0; i < settingpages; i++)
            {
                LeftClick(855, 507);
                if (checkForErrorsAt.Contains(i))
                {
                    updateScreen();
                    if (CompareColor(CALData.ErrorLocation.X, CALData.ErrorLocation.Y, CALData.ErrorColor, CALData.ErrorFade))
                        LeftClick(436, 318);
                }
            }
        }

        internal bool IsLobbyOpened()
        {
            updateScreen();
            return CompareColor(835, 179, new int[] { 121, 152, 184 }, 50) // Test for blue button (move/settings) at the top of the screen
                    && CompareColor(704, 67, new int[] { 78, 122, 158 }, 30); // Test for invite players to group button
        }

        internal void NavigateToModesMenu()
        {
            GoToSettings();
            LeftClick(494, 178);
        }
    }
}
