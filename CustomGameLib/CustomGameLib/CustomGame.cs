using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Deltin.CustomGameAutomation
{
    /// <summary>
    /// Automates Overwatch's custom games.
    /// </summary>
    public partial class CustomGame : IDisposable
    {
        internal const string DebugHeader = "[CGL] ";

        static int KeyPressWait = 50;

        Bitmap bmp = null;

        internal bool debugmode = false;
        internal Form debug;
        internal Graphics g;

        IntPtr OverwatchHandle = IntPtr.Zero;
        Process OverwatchProcess = null;
        internal DefaultKeys DefaultKeys;

        internal object CustomGameLock = new object();

        /// <summary>
        /// Creates new CustomGame object.
        /// </summary>
        public CustomGame(CustomGameBuilder customGameBuilder = default)
        {
            if (customGameBuilder == null)
                customGameBuilder = new CustomGameBuilder();

            if (customGameBuilder.OverwatchProcess != null)
            {
                OverwatchProcess = customGameBuilder.OverwatchProcess;
            }
            else
            {
                // Get the overwatch process
                OverwatchProcess = Process.GetProcessesByName("Overwatch").FirstOrDefault();
            }

            if (OverwatchProcess == null)
                throw new MissingOverwatchProcessException("Could not find any Overwatch processes running.");

            OverwatchHandle = OverwatchProcess.MainWindowHandle;

            OverwatchProcess.EnableRaisingEvents = true;
            OverwatchProcess.Exited += InvokeOnOverwatchProcessExit;

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

            Commands = new Commands(this);
            AI = new AI(this);
            Chat = new Chat(this);
            Pause = new Pause(this);
            PlayerInfo = new PlayerInfo(this);
            Interact = new Interact(this);
            Settings = new Settings(this);

            ScreenshotMethod = customGameBuilder.ScreenshotMethod;
            OpenChatIsDefault = customGameBuilder.OpenChatIsDefault;
            DefaultKeys = customGameBuilder.DefaultKeys;

            if (OpenChatIsDefault)
                Chat.OpenChat();

            StartPersistentScanning();
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
        /// Disables input for the Overwatch window.
        /// </summary>
        /// <remarks>
        /// Input must be re-enabled with <see cref="EnableOverwatchWindowInput"/>.
        /// </remarks>
        /// <seealso cref="EnableOverwatchWindowInput"/>
        public void DisableOverwatchWindowInput()
        {
            User32.EnableWindow(OverwatchHandle, false);
        }

        /// <summary>
        /// Enables input for the Overwatch window after disabling it with <see cref="DisableOverwatchWindowInput"/>.
        /// </summary>
        /// <seealso cref="DisableOverwatchWindowInput"/>
        public void EnableOverwatchWindowInput()
        {
            User32.EnableWindow(OverwatchHandle, true);
        }

        internal void ResetMouse()
        {
            lock (CustomGameLock)
            {
                // There is an Overwatch glitch where exiting some menus will cause the first slot to become highlighted.
                // This will mess with some color detection, so this will move the mouse to an unused spot on the Overwatch window
                // to tell the process where the cursor is. This will make the first slot become unhighlighted.
                Thread.Sleep(100);
                Cursor = Points.RESET_POINT;
                Thread.Sleep(100);
            }
        }

        internal void CloseOptionMenu()
        {
            lock (CustomGameLock)
            {
                LeftClick(400, 500, 100);
                LeftClick(500, 500, 100);
                //ResetMouse();
            }
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
            lock (CustomGameLock)
            {
                updateScreen();

                // Check if in lobby
                if (CompareColor(Points.LOBBY_START_GAME, Colors.LOBBY_START_GAME, Fades.LOBBY_START_GAME)) // Get "START GAME" color
                    return GameState.InLobby;

                // Check if waiting
                if (CompareColor(Points.LOBBY_START_GAMEMODE, Colors.LOBBY_CHANGE, Fades.LOBBY_CHANGE)) // Check if "START GAMEMODE" button exists.
                    return GameState.Waiting;

                if (CompareColor(Points.ENDING_COMMEND_DEFEAT, Colors.ENDING_COMMEND_DEFEAT, Fades.ENDING_COMMEND_DEFEAT)) // Check if commending by testing red color of defeat at top left corner
                    return GameState.Ending_Commend;

                if (CompareColor(Points.LOBBY_BACK_TO_LOBBY, Colors.LOBBY_CHANGE, Fades.LOBBY_CHANGE)) // Check if ingame by checking if "START GAMEMODE" button does not exist and the "BACK TO LOBBY" button does.
                    return GameState.Ingame;

                return GameState.Unknown;
            }
        } 

        /// <summary>
        /// The Overwatch Process being used in the CustomGame class.
        /// </summary>
        public Process UsingOverwatchProcess
        {
            get
            {
                return OverwatchProcess;
            }
        }

        internal bool Disposed = false;
        /// <summary>
        /// Disposes of all resources being used by the CustomGame instance.
        /// </summary>
        public void Dispose()
        {
            lock (CustomGameLock)
            {
                Disposed = true;
                // Stop scanning commands
                Commands.StopScanning();
                if (bmp != null)
                    bmp.Dispose();
                DisposePersistentScanningThread();
            }
        }

    } // CustomGame class

    /// <summary>
    /// Base type for CustomGame interaction members.
    /// </summary>
    public class CustomGameBase
    {
        /// <summary>
        /// Base type for CustomGame interaction members.
        /// </summary>
        protected CustomGameBase(CustomGame cg)
        {
            this.cg = cg;
        }
        /// <summary>
        /// The <see cref="CustomGame"/> object to use.
        /// </summary>
        protected CustomGame cg;
    }

    /// <summary>
    /// Overwatch's keybinds.
    /// </summary>
    public class DefaultKeys
    {
        /// <summary>
        /// The key used to open the Custom Game lobby. Is Keys.L by default.
        /// </summary>
        public Keys OpenCustomGameLobbyKey = Keys.L;
        /// <summary>
        /// The key used to open the chat. The default in Overwatch is Enter, but we recommend using Delete. Is Keys.Delete by default.
        /// </summary>
        public Keys OpenChat = Keys.Delete;
    }

    /// <summary>
    /// CustomGame object builder.
    /// </summary>
    public class CustomGameBuilder
    {
        /// <summary>
        /// The handle of the Overwatch process to use. Leave at default to use the first Overwatch process found.
        /// </summary>
        public Process OverwatchProcess = null;
        /// <summary>
        /// The screenshot method the CustomGame class will use.
        /// </summary>
        public ScreenshotMethod ScreenshotMethod = ScreenshotMethod.BitBlt;
        /// <summary>
        /// Determines if the chat should always be opened. Command scanning is more reliable if true.
        /// </summary>
        public bool OpenChatIsDefault = true;
        /// <summary>
        /// The default keys set in Overwatch's settings.
        /// </summary>
        public DefaultKeys DefaultKeys = new DefaultKeys();
    }

    /// <summary>
    /// Teams in Overwatch.
    /// </summary>
    [Flags]
    public enum Team
    {
        /// <summary>
        /// The blue team.
        /// </summary>
        Blue = 1 << 0,
        /// <summary>
        /// The red team.
        /// </summary>
        Red = 1 << 1,
        /// <summary>
        /// The blue and red team.
        /// </summary>
        BlueAndRed = Blue | Red,
        /// <summary>
        /// The spectators.
        /// </summary>
        Spectator = 1 << 2,
        /// <summary>
        /// The queue.
        /// </summary>
        Queue = 1 << 3
    }

    /// <summary>
    /// Teams in the queue.
    /// </summary>
    public enum QueueTeam
    {
        /// <summary>
        /// Queueing for both blue and red.
        /// </summary>
        Neutral,
        /// <summary>
        /// Queueing for blue.
        /// </summary>
        Blue,
        /// <summary>
        /// Queueing for red.
        /// </summary>
        Red
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
