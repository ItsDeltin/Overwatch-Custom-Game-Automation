using System;
using System.Threading;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace Deltin.CustomGameAutomation
{
    partial class CustomGame
    {
        /// <summary>
        /// Go back to executing position.
        /// </summary>
        private void BackToMenu()
        {
            if (!IsLobbyOpened())
            {
                Activate();
                Thread.Sleep(100);
                KeyPress(DefaultKeys.OpenCustomGameLobbyKey.Key);
                Thread.Sleep(Timing.LOBBY_FADE);
            }

            //ResetMouse();
        }

        /// <summary>
        /// Stalls the program to wait for the map to finish loading.
        /// </summary>
        private void LoadStall()
        {
            UpdateScreen();
            Stopwatch sw = new Stopwatch();
            Stopwatch sc = new Stopwatch();
            sw.Start();

            while (true)
            {
                if (
                    Capture.CompareColor(Points.LOBBY_START_GAME, Colors.LOBBY_START_GAME, Fades.LOBBY_START_GAME) || // Test for "START" button color
                    Capture.CompareColor(Points.LOADING_ENTERING_GAME, Colors.LOADING_ENTERING_GAME, Fades.LOADING_ENTERING_GAME) || // Test for "ENTERING GAME" color
                    Capture.CompareColor(400, 300, Colors.LOADING_BLACK, 15) // Test for black screen color
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
                UpdateScreen();
            }
            Thread.Sleep(1000);
        }

        /// <summary>
        /// Restarts the game.
        /// </summary>
        public void RestartGame()
        {
            using (LockHandler.Interactive)
            {
                if (OpenChatIsDefault)
                    Chat.CloseChat();

                LeftClick(Points.LOBBY_RESTART);
                LoadStall();
                BackToMenu();

                if (OpenChatIsDefault)
                    Chat.OpenChat();
            }
        }

        /// <summary>
        /// Starts the game.
        /// </summary>
        public void StartGame()
        {
            using (LockHandler.Interactive)
            {
                if (OpenChatIsDefault)
                    Chat.CloseChat();

                LeftClick(Points.LOBBY_START_GAME, 3000);
                LoadStall();
                BackToMenu();

                if (OpenChatIsDefault)
                    Chat.OpenChat();
            }
        }

        /// <summary>
        /// Sends server to the lobby.
        /// </summary>
        public void SendServerToLobby()
        {
            using (LockHandler.Interactive)
            {
                LeftClick(Points.LOBBY_BACK_TO_LOBBY, 750);
                WaitForColor(Points.LOBBY_START_GAME, Colors.LOBBY_START_GAME, Fades.LOBBY_START_GAME, 5000);
                //ResetMouse();
            }
        }

        /// <summary>
        /// Starts the game when waiting for players.
        /// </summary>
        public void StartGamemode()
        {
            using (LockHandler.Interactive)
            {
                LeftClick(Points.LOBBY_START_FROM_WAITING_FOR_PLAYERS, 1000);
            }
        }

        /// <summary>
        /// Resets the executing position in Overwatch.
        /// </summary>
        /// <returns>The state of Overwatch.</returns>
        public OverwatchState Reset()
        {
            using (LockHandler.Interactive)
            {
                Chat.CloseChat();

                for (int i = 0; i < 10; i++)
                {
                    UpdateScreen();
                    // Check if Ovewratch is disconnected.
                    if (IsDisconnected())
                    {
                        return OverwatchState.Disconnected;
                    }

                    // Check if Overwatch is in the main menu
                    if (Capture.CompareTo(Points.LOBBY_NAV_MAINMENU, Markups.NAV_MAINMENU, 60, 90, DBCompareFlags.IgnoreBlack))
                    {
                        // Overwatch is in the main menu.
                        if (OpenChatIsDefault)
                            Chat.OpenChat();
                        return OverwatchState.MainMenu;
                    }
                    
                    // Check if Overwatch is in the escape menu.
                    if (Capture.CompareTo(Points.LOBBY_NAV_ESCAPEMENU, Markups.NAV_ESCAPEMENU, 50, 95, DBCompareFlags.IgnoreBlack))
                    {
                        // Overwatch is in the escape menu.
                        KeyPress(DefaultKeys.OpenCustomGameLobbyKey.Key);
                        Thread.Sleep(Timing.LOBBY_FADE);
                        UpdateScreen();
                    }

                    // Check if Overwatch is in the custom game menu.
                    if (Capture.CompareTo(Points.LOBBY_NAV_CREATEGAME, Markups.NAV_LOBBY, new int[] { 152, 149, 151 }, 30, 90))
                    {
                        // Overwatch is in the custom game menu.
                        if (OpenChatIsDefault)
                            Chat.OpenChat();
                        return OverwatchState.Ready;
                    }

                    KeyPress(Keys.Escape);
                    Thread.Sleep(500);
                    Activate();
                }
                throw new UnknownOverwatchStateException();
            }
        }

        /// <summary>
        /// Creates a Custom Game from the main menu.
        /// </summary>
        public void CreateCustomGame()
        {
            OverwatchState gameState = Reset();

            if (gameState == OverwatchState.MainMenu)
            {
                KeyPress(100, Keys.Tab, Keys.Space);
                Thread.Sleep(500);
                KeyPress(100, Keys.Tab, Keys.Tab, Keys.Tab, Keys.Tab, Keys.Space);
                Thread.Sleep(500);
                KeyPress(100, Keys.Tab, Keys.Tab, Keys.Tab, Keys.Tab, Keys.Space);
                Thread.Sleep(500);
            }
        }

        internal void GoToSettings()
        {
            UpdateScreen();
            if (DoesAddButtonExist())
            {
                LeftClick(Points.LOBBY_SETTINGS_IF_ADD_BUTTON_PRESENT, 250);
            }
            else
            {
                LeftClick(Points.LOBBY_SETTINGS_IF_ADD_BUTTON_NOT_PRESENT, 250);
            }
        }

        internal bool DoesAddButtonExist()
        {
            UpdateScreen();

            return Capture.CompareColor(
                700, 182, // Location of the "MOVE" button
                715, 182, // Location of the "SETTINGS" button
                20);
        }

        internal void GoBack(int settingpages, params int[] checkForErrorsAt)
        {
            for (int i = 0; i < settingpages; i++)
            {
                LeftClick(Points.SETTINGS_BACK); // Clicks the back button
                if (checkForErrorsAt.Contains(i))
                {
                    UpdateScreen();
                    if (Capture.CompareColor(Points.SETTINGS_ERROR, Colors.SETTINGS_ERROR, Fades.SETTINGS_ERROR))
                        LeftClick(Points.SETTINGS_DISCARD);
                }
            }
        }

        internal bool IsLobbyOpened()
        {
            UpdateScreen();
            return Capture.CompareColor(Points.LOBBY_INVITE_PLAYERS_TO_GROUP, Colors.LOBBY_INVITE_PLAYERS_TO_GROUP_MIN, Colors.LOBBY_INVITE_PLAYERS_TO_GROUP_MAX) // Test for invite players to group button
                && !Capture.CompareColor(Points.LOBBY_INVITE_PLAYERS_TO_GROUP, Points.LOBBY_INVITE_PLAYERS_TO_GROUP_COMPARE, Fades.LOBBY_INVITE_PLAYERS_TO_GROUP_COMPARE); // Compare the invite players to group button with the area above it.
        }

        internal void NavigateToModesMenu()
        {
            GoToSettings();
            LeftClick(Points.SETTINGS_MODES);
        }

        internal void GridNavigator(int index, int columns = 4, int keyPressWait = 50)
        {
            int column = index % columns;
            int row = index / columns;

            for (int rowindex = 0; rowindex < row; rowindex++)
                KeyPress(keyPressWait, Keys.Down);
            for (int columnindex = 0; columnindex < column; columnindex++)
                KeyPress(keyPressWait, Keys.Right);
        }

        internal void GoToCustomGameInfo()
        {
            RightClick(Points.LOBBY_CUSTOM_GAME_INFO);
        }

        internal void WaitForCareerProfileToLoad()
        {
            WaitForColor(345, 164, new int[] { 85, 91, 108 }, 5, 10000);
            Thread.Sleep(250);
        }
    }
}
