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
        static Rectangle shotarea = new Rectangle(0, 0, 960, 540);

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
        public CustomGame(IntPtr overwatchHandle = new IntPtr(), ScreenshotMethods screenshotMethod = ScreenshotMethods.BitBlt, bool openChatIsDefault = true)
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
            updateScreen();

            OpenChatIsDefault = openChatIsDefault;
            if (OpenChatIsDefault)
                Chat.OpenChat();

            SetupGameOverCheck();
        }

        static void SetupWindow(IntPtr hWnd, ScreenshotMethods method)
        {
            if (method == ScreenshotMethods.ScreenCopy)
                User32.SetForegroundWindow(hWnd);
            else
                User32.ShowWindow(hWnd, User32.nCmdShow.SW_SHOWNOACTIVATE);
            User32.MoveWindow(hWnd, -7, 0, shotarea.Width, shotarea.Height, false);
        }

        void ResetMouse()
        {
            Thread.Sleep(100);
            Cursor = new Point(500, 500);
            Thread.Sleep(200);
        }

        /// <summary>
        /// Waits for the slots in overwatch to change.
        /// </summary>
        /// <param name="maxtime">Time to wait. Set to -1 to wait forever.</param>
        /// <returns>Returns true if Overwatch's slots changed. Returns false if the time ran out.</returns>
        public bool WaitForSlotUpdate(int maxtime = 1000)
        {
            Stopwatch time = new Stopwatch();
            List<int> preslots = TotalPlayerSlots;
            time.Start();
            while (time.ElapsedMilliseconds < maxtime || maxtime == -1)
            {
                List<int> newslots = TotalPlayerSlots;
                if (preslots.Count != newslots.Count)
                    return true;
                else
                    for (int i = 0; i < preslots.Count; i++)
                        if (preslots[i] != newslots[i])
                            return true;
                Thread.Sleep(100);
            }
            return false;
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
            if (CompareColor(CALData.StartGameLocation.X, CALData.StartGameLocation.Y, CALData.StartGameColor, CALData.StartGameFade)) // Get "START GAME" color
                return GameState.InLobby;

            // Check if waiting
            if (CompareColor(599, 456, CALData.LobbyChangeColor, 50)) // Check if "START GAMEMODE" button exists.
                return GameState.Waiting;

            if (CompareColor(53, 62, new int[] { 120, 70, 74 }, 10)) // Check if commending by testing red color of defeat at top left corner
                return GameState.Ending_Commend;

            if (CompareColor(394, 457, CALData.LobbyChangeColor, 50)) // Check if ingame by checking if "START GAMEMODE" button does not exist and the "BACK TO LOBBY" button does.
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

    public enum BotTeam
    {
        Both,
        Blue,
        Red
    }
    public enum InviteTeam
    {
        Blue,
        Red,
        Spectator,
        Both
    }
    public enum PlayerTeam
    {
        Blue,
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
