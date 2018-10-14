using System;
using System.Diagnostics;
using System.Threading;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Deltin.CustomGameAutomation
{
    partial class CustomGame
    {
        // WIP way to create overwatch process without username and password.
        /// <summary>
        /// Creates an Overwatch process using the currently logged in battle.net account.
        /// </summary>
        /// <param name="processInfo">Parameters for creating the process.</param>
        /// <returns>The created Overwatch process.</returns>
        public static Process CreateOverwatchProcessAutomatically(OverwatchProcessInfoAuto processInfo = null)
        {
            if (processInfo == null)
                processInfo = new OverwatchProcessInfoAuto();

            if (!File.Exists(processInfo.BattlenetExecutableFilePath))
                throw new FileNotFoundException(string.Format("Battle.net.exe's executable at {0} was not found. " +
                    "Change battlenetExeLocation to the location of the battle.net.exe executable.", processInfo.BattlenetExecutableFilePath));

            Stopwatch startTime = new Stopwatch();

            // If battle.net is not started, start it.
            if (Process.GetProcessesByName("battle.net").Length == 0)
            {
                Debug.WriteLine(DebugHeader + "No battle.net process found, starting battle.net.");

                Process battlenet = new Process();
                battlenet.StartInfo.FileName = processInfo.BattlenetExecutableFilePath;
                battlenet.Start();

                startTime.Start();
                // The battle.net app is fully started when there are 3 battle.net processes. Loop while there are less than 3.
                while (Process.GetProcessesByName("battle.net").Length < 3)
                {
                    if (startTime.ElapsedMilliseconds >= processInfo.MaxBattlenetStartTime || processInfo.MaxBattlenetStartTime == -1)
                    {
                        Debug.WriteLine(DebugHeader + "Error: Battle.net took too long to start.");
                        throw new OverwatchStartFailedException("Battle.net took too long to start.");
                    }
                    Thread.Sleep(200);
                }

                Debug.WriteLine(DebugHeader + "Finished starting Battle.net.");
            }
            else
                Debug.WriteLine(DebugHeader + "Battle.net process found.");

            Debug.WriteLine(DebugHeader + "Starting the Overwatch process.");

            Process[] processList = Process.GetProcessesByName("Overwatch");

            // Set the video settings.
            var initialSettings = ChangeVideoSettings(processInfo.OverwatchSettingsFilePath, VideoSettings.Item1, VideoSettings.Item2);

            Process battlenetOW = new Process();
            // The arguments to start the game directly before August 2018:
            // battlenet.StartInfo.FileName = "battlenet://Pro";
            // The arguments after:
            battlenetOW.StartInfo.FileName = processInfo.BattlenetExecutableFilePath;
            battlenetOW.StartInfo.Arguments = "--exec=\"launch Pro\"";
            battlenetOW.Start();

            startTime.Restart();

            while (startTime.ElapsedMilliseconds < processInfo.MaxOverwatchStartTime || processInfo.MaxOverwatchStartTime == -1)
            {
                Process[] newProcessList = Process.GetProcessesByName("Overwatch");

                for (int i = 0; i < newProcessList.Length; i++)
                    if (processList.Contains(newProcessList[i]) == false)
                    {
                        Process owProcess = newProcessList[i];

                        WaitForVisibleProcessWindow(owProcess);
                        RestoreVideoSettings(processInfo.OverwatchSettingsFilePath, initialSettings);

                        Bitmap bmp = null;
                        if (WaitForMainMenu(processInfo.ScreenshotMethod, owProcess.MainWindowHandle, bmp, processInfo.MaxWaitForMenuTime))
                        {
                            Debug.WriteLine(DebugHeader + "Finished starting Overwatch.");
                            if (processInfo.AutomaticallyCreateCustomGame)
                                CreateCustomGame(owProcess.MainWindowHandle);
                            return newProcessList[i];
                        }
                        else
                        {
                            Debug.WriteLine(DebugHeader + "Could not start Overwatch, main menu did not load.");
                            if (bmp != null)
                                bmp.Dispose();
                            if (processInfo.CloseOverwatchProcessOnFailure)
                            {
                                owProcess.CloseMainWindow();
                                owProcess.Close();
                            }
                            throw new OverwatchStartFailedException("Could not start Overwatch, main menu did not load.");
                        }
                    }

                Thread.Sleep(200);
            }

            Debug.WriteLine(DebugHeader + "Error: Overwatch took too long to start.");
            RestoreVideoSettings(processInfo.OverwatchSettingsFilePath, initialSettings);
            throw new OverwatchStartFailedException("Overwatch took too long to start.");
        }

        /// <summary>
        /// Creates a new Overwatch process by logging into an account. I strongly reccommend using <see cref="CreateOverwatchProcessAutomatically(OverwatchProcessInfoAuto)"/> instead.
        /// </summary>
        /// <param name="processInfo">Parameters for creating the process.</param>
        /// <returns>The created Overwatch process.</returns>
        public static Process CreateOverwatchProcessManually(OverwatchProcessInfoManual processInfo)
        {
            if (processInfo == null)
                throw new ArgumentNullException("processInfo");

            int maxWaitTime = 5000;

            if (!File.Exists(processInfo.OverwatchExecutableFilePath))
                throw new FileNotFoundException(string.Format("Overwatch's executable at {0} was not found. " +
                    "Change OverwatchProcessInfo.OverwatchExecutableFilePath to the location of the Overwatch executable.", processInfo.OverwatchExecutableFilePath));

            if (!File.Exists(processInfo.OverwatchSettingsFilePath))
                throw new FileNotFoundException(string.Format("Overwatch's settings at {0} was not found. " +
                    "Change OverwatchProcessInfo.OverwatchSettingsFilePath to the location of Overwatch's settings.", processInfo.OverwatchSettingsFilePath));

            // Set the video settings.
            var initialSettings = ChangeVideoSettings(processInfo.OverwatchSettingsFilePath, VideoSettings.Item1, VideoSettings.Item2);

            Process OWProcess = new Process();
            OWProcess.StartInfo.FileName = processInfo.OverwatchExecutableFilePath;
            OWProcess.StartInfo.Arguments = "-Displaymode 0";
            OWProcess.Start();

            // Wait for the window to start
            WaitForVisibleProcessWindow(OWProcess);

            // Show the window
            SetupWindow(OWProcess.MainWindowHandle, processInfo.ScreenshotMethod);

            Stopwatch elapsed = new Stopwatch();
           
            Bitmap bmp = null;
            elapsed.Start();
            while (true)
            {
                Screenshot(processInfo.ScreenshotMethod, OWProcess.MainWindowHandle, ref bmp);

                if (elapsed.ElapsedMilliseconds >= maxWaitTime)
                {
                    bmp.Dispose();
                    ProcessCreateError(initialSettings, processInfo, OWProcess, bmp, new OverwatchStartFailedException("Failed to start Overwatch."));
                }

                // If the text input for the log in info is found, break out of the loop.
                if (bmp.CompareColor(407, 384, new int[] { 168, 168, 170 }, 10))
                {
                    break;
                }
                // If the log in button is yellow, there is not a connection.
                else if (bmp.CompareColor(Points.PRE_MAIN_MENU_LOGIN, Colors.CONFIRM, Fades.CONFIRM))
                {
                    ProcessCreateError(initialSettings, processInfo, OWProcess, bmp, new OverwatchStartFailedException("Could not log in; no internet connection."));
                }
                Thread.Sleep(500);
            }
            elapsed.Reset();

            Thread.Sleep(100);

            // At this point login info is ready to be inputed
            TextInput(OWProcess.MainWindowHandle, processInfo.Username);
            KeyPress(OWProcess.MainWindowHandle, Keys.Tab);
            TextInput(OWProcess.MainWindowHandle, processInfo.Password);

            // Log in
            Thread.Sleep(50);
            Screenshot(processInfo.ScreenshotMethod, OWProcess.MainWindowHandle, ref bmp);
            if (bmp.CompareColor(Points.PRE_MAIN_MENU_LOGIN, Colors.CONFIRM, Fades.CONFIRM))
                KeyPress(OWProcess.MainWindowHandle, Keys.Enter);
            else
            {
                ProcessCreateError(initialSettings, processInfo, OWProcess, bmp, new OverwatchStartFailedException("Could not log in with the input username or password."));
            }

            Thread.Sleep(500);

            elapsed.Start();
            while (true)
            {
                if (elapsed.ElapsedMilliseconds >= maxWaitTime)
                {
                    ProcessCreateError(initialSettings, processInfo, OWProcess, bmp, new OverwatchStartFailedException("Failed to start Overwatch."));
                }

                Screenshot(processInfo.ScreenshotMethod, OWProcess.MainWindowHandle, ref bmp);
                if (bmp.CompareColor(469, 437, Colors.CONFIRM, Fades.CONFIRM) == false)
                    break;
                Thread.Sleep(500);
            }
            elapsed.Reset();

            Thread.Sleep(500);

            Screenshot(processInfo.ScreenshotMethod, OWProcess.MainWindowHandle, ref bmp);
            // Check if login failed
            // s0 will equal true and s1 will equal false if the login failed. s0 and s1 will equal true if an authenticator is required.
            bool s0 = bmp.CompareColor(518, 482, Colors.CONFIRM, Fades.CONFIRM); // "Cancel" button
            bool s1 = bmp.CompareColor(605, 475, Colors.CONFIRM, Fades.CONFIRM); // "Authenticate" button.
            if (s0 && !s1)
            {
                ProcessCreateError(initialSettings, processInfo, OWProcess, bmp, new OverwatchStartFailedException("Could not log in with the input username or password."));
            }

            // Enter authenticator code if it is required
            if (s0 && s1)
            {
                if (String.IsNullOrEmpty(processInfo.Authenticator))
                {
                    ProcessCreateError(initialSettings, processInfo, OWProcess, bmp, new OverwatchStartFailedException("Authenticator is required"));
                }

                TextInput(OWProcess.MainWindowHandle, processInfo.Authenticator);
                Thread.Sleep(10);
                KeyPress(OWProcess.MainWindowHandle, Keys.Enter);
                Thread.Sleep(500);

                elapsed.Start();
                while (true)
                {
                    if (elapsed.ElapsedMilliseconds >= maxWaitTime)
                    {
                        ProcessCreateError(initialSettings, processInfo, OWProcess, bmp, new OverwatchStartFailedException("Failed to start Overwatch."));
                    }

                    Screenshot(processInfo.ScreenshotMethod, OWProcess.MainWindowHandle, ref bmp);
                    if (bmp.CompareColor(469, 437, Colors.CONFIRM, Fades.CONFIRM) == false)
                        break;
                    Thread.Sleep(500);
                }
                Thread.Sleep(500);

                Screenshot(processInfo.ScreenshotMethod, OWProcess.MainWindowHandle, ref bmp);
                if (bmp.CompareColor(518, 482, Colors.CONFIRM, Fades.CONFIRM))
                {
                    ProcessCreateError(initialSettings, processInfo, OWProcess, bmp, new OverwatchStartFailedException(string.Format("Authenticator number \"{0}\" is invalid.", processInfo.Authenticator)));
                }
            }

            if(!WaitForMainMenu(processInfo.ScreenshotMethod, OWProcess.MainWindowHandle, bmp, maxWaitTime))
                ProcessCreateError(initialSettings, processInfo, OWProcess, bmp, new OverwatchStartFailedException("Failed to start Overwatch."));

            if (processInfo.AutomaticallyCreateCustomGame)
                CreateCustomGame(OWProcess.MainWindowHandle);

            // Reset the contrast to its initial value
            RestoreVideoSettings(processInfo.OverwatchSettingsFilePath, initialSettings);

            return OWProcess;
        }

        private static List<Tuple<string, string>> ChangeVideoSettings(string settingsFilePath, string[] settings, string[] setTo)
        {
            string[] allSettings = File.ReadAllLines(settingsFilePath);

            List<Tuple<string, string>> originalSettings = new List<Tuple<string, string>>();

            for (int i = 0; i < allSettings.Length; i++)
            {
                string[] settingSplit = allSettings[i].Split('=');
                string settingCheck = settingSplit[0].Trim();

                for (int s = 0; s < settings.Length; s++)
                    if (settingCheck == settings[s])
                    {
                        allSettings[i] = settingCheck + " = \"" + setTo[s] + "\"";
                        if (settingSplit.Length > 1)
                            originalSettings.Add(new Tuple<string, string>(settingCheck, settingSplit[1].Trim(' ', '\"')));
                        else
                            originalSettings.Add(new Tuple<string, string>(settingCheck, ""));
                        break;
                    }
            }

            File.WriteAllLines(settingsFilePath, allSettings);

            return originalSettings;
        }

        private static void RestoreVideoSettings(string settingsFilePath, List<Tuple<string, string>> settingsList)
        {
            List<string> settings = new List<string>();
            List<string> setTo = new List<string>();
            foreach(var s in settingsList)
            {
                settings.Add(s.Item1);
                setTo.Add(s.Item2);
            }
            ChangeVideoSettings(settingsFilePath, settings.ToArray(), setTo.ToArray());
        }

        private static void ProcessCreateError(List<Tuple<string, string>> initialSettings, OverwatchProcessInfoManual info, Process process, Bitmap bmp, Exception ex)
        {
            if (info.CloseOverwatchProcessOnFailure)
            {
                process.CloseMainWindow();
                process.Close();
            }
            if (bmp != null)
                bmp.Dispose();
            RestoreVideoSettings(info.OverwatchSettingsFilePath, initialSettings);
            throw ex;
        }

        private static void WaitForVisibleProcessWindow(Process process)
        {
            while (string.IsNullOrEmpty(process.MainWindowTitle))
            {
                Thread.Sleep(100);
                process.Refresh();
            }
        }

        private static bool WaitForMainMenu(ScreenshotMethod screenshotMethod, IntPtr hwnd, Bitmap bmp, int maxTime)
        {
            Stopwatch elapsed = new Stopwatch();
            elapsed.Start();
            while (elapsed.ElapsedMilliseconds < maxTime || maxTime == -1)
            {
                Screenshot(screenshotMethod, hwnd, ref bmp);
                if (bmp.CompareColor(Points.MAIN_MENU_OVERWATCH_WATERMARK, Colors.WHITE, 10))
                {
                    Thread.Sleep(2000);
                    return true;
                }
                Thread.Sleep(500);
            }

            return false;
        }

        private static void CreateCustomGame(IntPtr hwnd)
        {
            KeyPress(hwnd, 100, Keys.Tab, Keys.Space);
            Thread.Sleep(500);
            KeyPress(hwnd, 100, Keys.Tab, Keys.Tab, Keys.Tab, Keys.Tab, Keys.Space);
            Thread.Sleep(500);
            KeyPress(hwnd, 100, Keys.Tab, Keys.Tab, Keys.Tab, Keys.Tab, Keys.Space);
            Thread.Sleep(500);
        }

        /// <summary>
        /// <para>Item1 = the name of the setting.</para>
        /// <para>Item2 = what the setting should equal.</para>
        /// </summary>
        private static readonly Tuple<string[], string[]> VideoSettings = new Tuple<string[], string[]>(
            new string[] { "RenderContrast", "RenderBrightness", "RenderGamma", "ColorblindMode", "FullscreenWindow", "FullscreenWindowEnabled", "MaximizedWindow" }, 
            new string[] { "0.5",            "0",                "2.2",         "0",              "0",                "0",                       "0" });

        /// <summary>
        /// Gets a running Overwatch process.
        /// </summary>
        /// <returns>The running Overwatch process. Returns null if there are none.</returns>
        public static Process GetOverwatchProcess()
        {
            return Process.GetProcessesByName("Overwatch").FirstOrDefault();
        }
    }

    /// <summary>
    /// Data for creating an Overwatch process manually.
    /// </summary>
    public class OverwatchProcessInfoManual
    {
        /// <summary>
        /// Data for creating an Overwatch process manually.
        /// </summary>
        /// <param name="username">Username of the account for the new Overwatch process.</param>
        /// <param name="password">Password of the account for the new Overwatch process.</param>
        /// <param name="authenticator">Authenticator number from the Authenticator app. Only required if the account is hooked up to the Blizzard Authenticator app.</param>
        public OverwatchProcessInfoManual(string username, string password, string authenticator = null)
        {
            Username = username;
            Password = password;
            Authenticator = authenticator;
        }

        // Required
        /// <summary>
        /// Username of the account for the new Overwatch process.
        /// </summary>
        public string Username;
        /// <summary>
        /// Password of the account for the new Overwatch process.
        /// </summary>
        public string Password;
        // Required if user has authenticator linked to their account
        /// <summary>
        /// Authenticator number from the Authenticator app.
        /// </summary>
        public string Authenticator;

        // Optional
        /// <summary>
        /// If true, the Overwatch process will automatically create a Custom Game.
        /// </summary>
        public bool AutomaticallyCreateCustomGame = true;
        /// <summary>
        /// Closes the Overwatch process if it fails to log in.
        /// </summary>
        public bool CloseOverwatchProcessOnFailure = true;
        /// <summary>
        /// The path to the Overwatch executable.
        /// </summary>
        public string OverwatchExecutableFilePath = @"C:\Program Files (x86)\Overwatch\Overwatch.exe";
        /// <summary>
        /// The path to Overwatch's settings file
        /// </summary>
        public string OverwatchSettingsFilePath = @"C:\Users\" + Environment.UserName + @"\Documents\Overwatch\Settings\Settings_v0.ini";

        /// <summary>
        /// The method that is used to take screenshots of the Overwatch window.
        /// </summary>
        public ScreenshotMethod ScreenshotMethod = ScreenshotMethod.BitBlt;
    }

    /// <summary>
    /// Data for creating an Overwatch process automatically.
    /// </summary>
    public class OverwatchProcessInfoAuto
    {
        /// <summary>
        /// Data for creating an Overwatch process automatically.
        /// </summary>
        public OverwatchProcessInfoAuto()
        {

        }

        /// <summary>
        /// If true, the Overwatch process will automatically create a Custom Game.
        /// </summary>
        public bool AutomaticallyCreateCustomGame = true;
        /// <summary>
        /// Closes the Overwatch process if it fails to log in.
        /// </summary>
        public bool CloseOverwatchProcessOnFailure = true;
        /// <summary>
        /// The path to the battle.net executable. Defaults to "C:\Program Files (x86)\Blizzard App\Battle.net.exe"
        /// </summary>
        public string BattlenetExecutableFilePath = @"C:\Program Files (x86)\Blizzard App\Battle.net.exe";
        /// <summary>
        /// The path to Overwatch's settings file. Defaults to "C:\Users\(EnvironmentName)\Documents\Overwatch\Settings\Settings_v0.ini"
        /// </summary>
        public string OverwatchSettingsFilePath = @"C:\Users\" + Environment.UserName + @"\Documents\Overwatch\Settings\Settings_v0.ini";
        /// <summary>
        /// The method that is used to take screenshots of the Overwatch window.
        /// </summary>
        public ScreenshotMethod ScreenshotMethod = ScreenshotMethod.BitBlt;
        /// <summary>
        /// The maximum amount of time to wait for the menu to load.
        /// </summary>
        public int MaxWaitForMenuTime = 20000;
        /// <summary>
        /// The maximum amount of time to wait for Overwatch to start.
        /// </summary>
        public int MaxOverwatchStartTime = 10000;
        /// <summary>
        /// The maximum amount of time to wait for Battle.net to start.
        /// </summary>
        public int MaxBattlenetStartTime = 10000;
    }
}
