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
                    CompareColor(Points.LOBBY_START_GAME, Colors.LOBBY_START_GAME, Fades.LOBBY_START_GAME) || // Test for "START" button color

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

            LeftClick(Points.LOBBY_RESTART);
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
            LeftClick(Points.LOBBY_START_GAME, 3000);
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
            LeftClick(Points.LOBBY_BACK_TO_LOBBY, 750);
            WaitForColor(Points.LOBBY_START_GAME, Colors.LOBBY_START_GAME, Fades.LOBBY_START_GAME, 5000);
            ResetMouse();
        }

        /// <summary>
        /// Starts the game when waiting for players.
        /// </summary>
        public void StartGamemode()
        {
            LeftClick(Points.LOBBY_START_FROM_WAITING_FOR_PLAYERS, 1000);
        }

        internal void GoToSettings()
        {
            updateScreen();
            if (DoesAddButtonExist())
            {
                LeftClick(Points.LOBBY_SETTINGS_IF_ADD_BUTTON_PRESENT, 250);
            }
            else
            {
                LeftClick(Points.LOBBY_SETTINGS_IF_ADD_BUTTON_NOT_PRESENT, 250);
            }
        }

        private bool DoesAddButtonExist()
        {
            updateScreen();
            return CompareColor(Points.LOBBY_MOVE_IF_ADD_BUTTON_PRESENT, Points.LOBBY_SETTINGS_IF_ADD_BUTTON_PRESENT, 20);
        }

        internal void GoBack(int settingpages, params int[] checkForErrorsAt)
        {
            for (int i = 0; i < settingpages; i++)
            {
                LeftClick(855, 507); //todo, this is the back button in settings? That's saved as 500,500 in constants.
                if (checkForErrorsAt.Contains(i))
                {
                    updateScreen();
                    if (CompareColor(Points.SETTINGS_ERROR, Colors.SETTINGS_ERROR, Fades.SETTINGS_ERROR))
                        LeftClick(436, 318); //todo figure what this is, and add to constants.
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
            LeftClick(Points.SETTINGS_MODES);
        }
    }
}
