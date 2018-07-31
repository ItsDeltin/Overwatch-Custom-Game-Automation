using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Deltin.CustomGameAutomation
{
    /// <summary>
    /// Automates Overwatch's custom games.
    /// </summary>
    public partial class CustomGame : IDisposable
    {
        static int KeyPressWait = 50;

        Bitmap bmp = null;

        bool debugmode = false;
        Form debug;
        Graphics g;

        IntPtr OverwatchHandle = IntPtr.Zero;

        /// <summary>
        /// Creates new CustomGame object using an Overwatch process.
        /// </summary>
        /// <param name="overwatchHandle">Overwatch process handle to use. Leave at default to use the first Overwatch process found.</param>
        /// <param name="screenshotMethod">Method to take screenshots with.</param>
        /// <param name="openChatIsDefault">Determines if the chat should be opened at all times. Command scanning is more reliable if true.</param>
        public CustomGame(IntPtr overwatchHandle = new IntPtr(), ScreenshotMethod screenshotMethod = ScreenshotMethod.BitBlt, bool openChatIsDefault = true)
        {
            if (overwatchHandle == IntPtr.Zero)
            {
                // Get the overwatch process
                Process[] overwatchProcesses = Process.GetProcessesByName("Overwatch");
                if (overwatchProcesses.Length > 0)
                {
                    OverwatchHandle = overwatchProcesses[0].MainWindowHandle;
                }
            }
            else
                OverwatchHandle = overwatchHandle;

            if (OverwatchHandle == IntPtr.Zero)
                throw new MissingOverwatchProcessException("Could not find any Overwatch processes running.");

            SetupWindow(OverwatchHandle, ScreenshotMethod);
            Thread.Sleep(500);

            // Set up debug window if debugmode is set to true.
            if (debugmode)
                new Task(() => 
                {
                    debug = new Form();
                    debug.Width = 1500;
                    debug.Height = 1000;
                    debug.Show();
                    g = debug.CreateGraphics();
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                    Application.Run(debug);
                }).Start();

            Command = new Commands(this);
            AI = new CG_AI(this);
            Maps = new CG_Maps(this);
            Chat = new CG_Chat(this);
            Pause = new CG_Pause(this);
            PlayerInfo = new CG_PlayerInfo(this);
            Interact = new CG_Interact(this);
            GameSettings = new CG_Settings(this);

            // Create bitmap of overwatch client screen capture.
            ScreenshotMethod = screenshotMethod; // Set the screenshot method

            OpenChatIsDefault = openChatIsDefault;
            if (OpenChatIsDefault)
                Chat.OpenChat();

            SetupGameOverCheck();
        }

        /// <summary>
        /// Positions the Overwatch window to be usable by the CustomGame class.
        /// </summary>
        public void SetupOverwatchWindow()
        {
            SetupWindow(OverwatchHandle, ScreenshotMethod);
        }

        static void SetupWindow(IntPtr hWnd, ScreenshotMethod method)
        {
            if (method == ScreenshotMethod.ScreenCopy)
                User32.SetForegroundWindow(hWnd);
            else
                User32.ShowWindow(hWnd, User32.nCmdShow.SW_SHOWNOACTIVATE);
            User32.MoveWindow(hWnd, -7, 0, Rectangles.ENTIRE_SCREEN.Width, Rectangles.ENTIRE_SCREEN.Height, false);
        }

        /// <summary>
        /// Disables input for the Overwatch window to prevent accidentally hovering over it.
        /// </summary>
        public void DisableOverwatchWindowInput()
        {
            User32.EnableWindow(OverwatchHandle, false);
        }

        /// <summary>
        /// Enables input for the Overwatch window after disabling it with DisableOverwatchWindowInput().
        /// </summary>
        public void EnableOverwatchWindowInput()
        {
            User32.EnableWindow(OverwatchHandle, true);
        }

        internal void ResetMouse()
        {
            Thread.Sleep(100);
            Cursor = Points.RESET_POINT;
            Thread.Sleep(200);
        }

        internal void CloseOptionMenu()
        {
            LeftClick(Points.OPTIONS_APPLY, 100);
            LeftClick(Points.OPTIONS_BACK, 100);
            ResetMouse();
        }

        /// <summary>
        /// Returns the state of the game.
        /// <para>In lobby: GameState.InLobby</para>
        /// <para>Waiting for players: GameState.Waiting</para>
        /// <para>Ingame: GameState.Ingame</para>
        /// <para>Commending players: GameState.Ending_Commend</para>
        /// </summary>
        /// <returns>Returns the state of the game.</returns>
        public GameState GetGameState()
        {
            updateScreen();

            // Check if in lobby
            if (CompareColor(Points.LOBBY_START_GAME, Colors.LOBBY_START_GAME, Fades.LOBBY_START_GAME)) // Get "START GAME" color
                return GameState.InLobby;

            // Check if waiting
            if (CompareColor(Points.LOBBY_START_GAMEMODE, Colors.LOBBY_CHANGE, 50)) // Check if "START GAMEMODE" button exists.
                return GameState.Waiting;

            if (CompareColor(Points.ENDING_COMMEND_DEFEAT, new int[] { 120, 70, 74 }, 10)) // Check if commending by testing red color of defeat at top left corner
                return GameState.Ending_Commend;

            if (CompareColor(Points.LOBBY_BACK_TO_LOBBY, Colors.LOBBY_CHANGE, 50)) // Check if ingame by checking if "START GAMEMODE" button does not exist and the "BACK TO LOBBY" button does.
                return GameState.Ingame;

            return GameState.Unknown;
        } 

        internal bool Disposed = false;
        /// <summary>
        /// Disposes of all resources being used by the CustomGame instance.
        /// </summary>
        public void Dispose()
        {
            Disposed = true;
            // Stop scanning commands
            Command.StopScanning();
            // Remove data of all executed commands.
            Command.DisposeAllExecutedCommands();
            if (bmp != null)
                bmp.Dispose();
            DisposeGameOverCheck();
        }

    } // CustomGame class

    /// <summary>
    /// The team a bot will be added to.
    /// </summary>
    public enum BotTeam
    {
        /// <summary>
        /// Bot will be added to the smaller team, or Blue in case of even teams.
        /// </summary>
        Both,
        /// <summary>
        /// Bot will be added to Blue team.
        /// </summary>
        Blue,
        /// <summary>
        /// Bot will be added to Red team.
        /// </summary>
        Red
    }

    /// <summary>
    /// The team a player will be invited to.
    /// </summary>
    public enum InviteTeam
    {
        /// <summary>
        /// The player will be invited to Blue team.
        /// </summary>
        Blue,
        /// <summary>
        /// The player will be invited to Red team.
        /// </summary>
        Red,
        /// <summary>
        /// The player will be invited to be a spectator.
        /// </summary>
        Spectator,
        /// <summary>
        /// The player will be invited to the smaller team, or Blue in case of even teams.
        /// </summary>
        Both
    }

    /// <summary>
    /// The team a player is on.
    /// </summary>
    public enum PlayerTeam
    {
        /// <summary>
        /// The player is on Blue team.
        /// </summary>
        Blue,
        /// <summary>
        /// The player is on Red team.
        /// </summary>
        Red
    }
    public enum Team
    {
        Blue,
        Red,
        Spectator
    }
    /// <summary>
    /// Options for who can join the game.
    /// </summary>
    public enum Join
    {
        /// <summary>
        /// Everyone can join the game.
        /// </summary>
        Everyone,
        /// <summary>
        /// Only friends of the moderator can join the game.
        /// </summary>
        FriendsOnly,
        /// <summary>
        /// Only players invited can join the game.
        /// </summary>
        InviteOnly
    }
    /// <summary>
    /// Gets the current state of the game.
    /// </summary>
    public enum GameState
    {
        /// <summary>
        /// The custom game is in the lobby.
        /// </summary>
        InLobby,
        /// <summary>
        /// The custom game is waiting for players.
        /// </summary>
        Waiting,
        /// <summary>
        /// The custom game is currently ingame.
        /// </summary>
        Ingame,
        /// <summary>
        /// The custom game is at player commendation.
        /// </summary>
        Ending_Commend,
        /// <summary>
        /// Cannot recognize what state the game is on.
        /// </summary>
        Unknown
    }
    /// <summary>
    /// Enables/disables settings before toggling them.
    /// </summary>
    public enum ToggleAction
    {
        /// <summary>
        /// Do not enable/disable.
        /// </summary>
        None,
        /// <summary>
        /// Disable all options before toggling.
        /// </summary>
        DisableAll,
        /// <summary>
        /// Enable all options before toggling.
        /// </summary>
        EnableAll
    }
}
