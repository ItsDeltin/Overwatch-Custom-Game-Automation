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
        private IntPtr OverwatchHandle = IntPtr.Zero;
        internal DefaultKeys DefaultKeys;

        internal DirectBitmap Capture = null;
        internal bool Disposed = false;

        /// <summary>
        /// The Overwatch Process being used in the CustomGame class.
        /// </summary>
        public Process OverwatchProcess { get; private set; } = null;

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
            OverwatchProcess.Exited += InvokeOnExit;

            SetupWindow(OverwatchHandle, ScreenshotMethod);
            Thread.Sleep(500);

#if DEBUG && DEBUG_WINDOW
            SetupDebugWindow();
#endif

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

        private static void SetupWindow(IntPtr hWnd, ScreenshotMethod method)
        {
            if (!Validate(hWnd))
                return;

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
            using (LockHandler.SemiPassive)
            {
                // There is an Overwatch glitch where exiting some menus will cause the first slot to become highlighted.
                // This will mess with some color detection, so this will move the mouse to an unused spot on the Overwatch window
                // to tell the process where the cursor is. This will make the first slot become unhighlighted.
                Thread.Sleep(100);
                MoveMouseTo(Points.RESET_POINT);
                Thread.Sleep(100);
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
            using (LockHandler.Passive)
            {
                UpdateScreen();

                // Check if in lobby
                if (Capture.CompareColor(Points.LOBBY_START_GAME, Colors.LOBBY_START_GAME, Fades.LOBBY_START_GAME)) // Get "START GAME" color
                    return GameState.InLobby;

                // Check if waiting
                if (Capture.CompareColor(Points.LOBBY_START_GAMEMODE, Colors.LOBBY_CHANGE, Fades.LOBBY_CHANGE)) // Check if "START GAMEMODE" button exists.
                    return GameState.Waiting;

                if (Capture.CompareColor(Points.ENDING_COMMEND_DEFEAT, Colors.ENDING_COMMEND_DEFEAT, Fades.ENDING_COMMEND_DEFEAT)) // Check if commending by testing red color of defeat at top left corner
                    return GameState.Ending_Commend;

                if (Capture.CompareColor(Points.LOBBY_BACK_TO_LOBBY, Colors.LOBBY_CHANGE, Fades.LOBBY_CHANGE)) // Check if ingame by checking if "START GAMEMODE" button does not exist and the "BACK TO LOBBY" button does.
                    return GameState.Ingame;

                return GameState.Unknown;
            }
        }

        /// <summary>
        /// Disposes of all resources being used by the CustomGame instance.
        /// </summary>
        public void Dispose()
        {
            using (LockHandler.Interactive)
            {
                Disposed = true;
                Commands.StopScanning();
                DisposePersistentScanningThread();

                if (Capture != null)
                    Capture.Dispose();
            }
        }

        /// <summary>
        /// Checks if a player account exists via battletag. Is case sensitive.
        /// </summary>
        /// <param name="battletag">Battletag of player to check. Is case sensitive.</param>
        /// <returns>Returns true if player exists, else returns false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="battletag"/> is null.</exception>
        public static bool PlayerExists(string battletag)
        {
            if (battletag == null)
                throw new ArgumentNullException("battletag", "Battletag was null.");

            // If the website "https://playoverwatch.com/en-us/career/pc/(BATTLETAGNAME)-(BATTLETAGID)" exists, then the player exists.
            try
            {
                string playerprofile = "https://playoverwatch.com/en-us/career/pc/" + battletag.Replace('#', '-');

                using (var wc = new System.Net.WebClient())
                {
                    string pageinfo = wc.DownloadString(playerprofile);
                    wc.Dispose();

                    // Check if the career profile page exists by checking if the title of the page starts with C in Career profile.
                    // If it doesn't, it will be a "page doesn't exist" page with the title starting with O in Overwatch.
                    if (pageinfo[pageinfo.IndexOf("<title>") + 7] == 'C')
                        return true;
                }
            }
            catch (System.Net.WebException) { }

            return false;
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
        /// <summary>
        /// The captured screen.
        /// </summary>
        protected DirectBitmap Capture { get { return cg.Capture; } }
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
}
