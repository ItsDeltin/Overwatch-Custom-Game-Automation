using System;
using System.Diagnostics;
using System.Threading;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;

namespace Deltin.CustomGameAutomation
{
    partial class CustomGame
    {
#if false
        public static IntPtr CreateOverwatchProcess()
        {
            Process[] processList = Process.GetProcessesByName("Overwatch");

            Process battlenet = new Process();
            battlenet.StartInfo.FileName = "battlenet://Pro";
            battlenet.Start();

            Process[] newProcessList;
            while (processList.Length == (newProcessList = Process.GetProcessesByName("Overwatch")).Length)
            {
                Thread.Sleep(200);
            }

            for (int n = 0; n < newProcessList.Length; n++)
            {
                bool contains = false;
                for (int o = 0; o < processList.Length; o++)
                    if (newProcessList[n] == processList[o])
                        contains = true;
                if (!contains)
                    return newProcessList[n].MainWindowHandle;
            }

            return IntPtr.Zero;
        }
#endif

        /// <summary>
        /// Creates a new Overwatch process.
        /// </summary>
        /// <param name="processInfo">The info of the process being started.</param>
        /// <returns>The main window handle of the Overwatch process.</returns>
        public static IntPtr CreateOverwatchProcess(OverwatchProcessInfo processInfo)
        {
            if (!File.Exists(processInfo.OverwatchExecutableFilePath))
                throw new FileNotFoundException(string.Format("Overwatch's executable at {0} was not found. " +
                    "Change OverwatchProcessInfo.OverwatchExecutableFilePath to the location of the Overwatch executable.", processInfo.OverwatchExecutableFilePath));

            if (!File.Exists(processInfo.OverwatchSettingsFilePath))
                throw new FileNotFoundException(string.Format("Overwatch's settings at {0} was not found. " +
                    "Change OverwatchProcessInfo.OverwatchSettingsFilePath to the location of Overwatch's settings.", processInfo.OverwatchSettingsFilePath));

            // Set the contrast to 0.5
            var initialSettings = ChangeVideoSettings(processInfo.OverwatchSettingsFilePath, 
                new string[] { "RenderContrast", "RenderBrightness", "RenderGamma", "ColorblindMode", "FullscreenWindow", "FullscreenWindowEnabled", "MaximizedWindow" },
                new string[] { "0.5",            "0",                "2.2",          "0",             "0",                "0",                       "0" });

            Process OWProcess = new Process();
            OWProcess.StartInfo.FileName = processInfo.OverwatchExecutableFilePath;
            OWProcess.StartInfo.Arguments = "-Displaymode 0";
            OWProcess.Start();

            // Wait for the window to start
            while (string.IsNullOrEmpty(OWProcess.MainWindowTitle))
            {
                Thread.Sleep(100);
                OWProcess.Refresh();
            }

            // Show the window
            SetupWindow(OWProcess.MainWindowHandle, processInfo.ScreenshotMethod);

            Bitmap bmp = null;
            while (true)
            {
                Screenshot(processInfo.ScreenshotMethod, OWProcess.MainWindowHandle, ref bmp);
                if (bmp.CompareColor(407, 384, new int[] { 168, 168, 170 }, 10))
                {
                    break;
                }
                // If the log in button is yellow, there is not a connection.
                else if (bmp.CompareColor(419, 473, CALData.ConfirmColor, 50))
                {
                    if (processInfo.CloseOverwatchProcessOnFailure)
                    {
                        OWProcess.CloseMainWindow();
                        OWProcess.Close();
                    }
                    RestoreVideoSettings(processInfo.OverwatchSettingsFilePath, initialSettings);
                    throw new LoginFailedException("Could not log in; no internet connection.");
                }
                Thread.Sleep(500);
            }

            Thread.Sleep(100);

            // At this point login info is ready to be inputed
            TextInput(OWProcess.MainWindowHandle, processInfo.Username);
            KeyPress(OWProcess.MainWindowHandle, Keys.Tab);
            TextInput(OWProcess.MainWindowHandle, processInfo.Password);

            // Log in
            Thread.Sleep(50);
            Screenshot(processInfo.ScreenshotMethod, OWProcess.MainWindowHandle, ref bmp);
            if (bmp.CompareColor(419, 473, CALData.ConfirmColor, 50))
                KeyPress(OWProcess.MainWindowHandle, Keys.Enter);
            else
            {
                if (processInfo.CloseOverwatchProcessOnFailure)
                {
                    OWProcess.CloseMainWindow();
                    OWProcess.Close();
                }
                RestoreVideoSettings(processInfo.OverwatchSettingsFilePath, initialSettings);
                throw new LoginFailedException("Could not log in with the input username or password.");
            }

            Thread.Sleep(500);

            while (true)
            {
                Screenshot(processInfo.ScreenshotMethod, OWProcess.MainWindowHandle, ref bmp);
                if (bmp.CompareColor(469, 437, CALData.ConfirmColor, 50) == false)
                    break;
                Thread.Sleep(500);
            }

            Thread.Sleep(500);

            Screenshot(processInfo.ScreenshotMethod, OWProcess.MainWindowHandle, ref bmp);
            // Check if login failed
            bool s0 = bmp.CompareColor(518, 482, CALData.ConfirmColor, 50);
            bool s1 = bmp.CompareColor(605, 475, CALData.ConfirmColor, 50);
            if (s0 && !s1)
            {
                if (processInfo.CloseOverwatchProcessOnFailure)
                {
                    OWProcess.CloseMainWindow();
                    OWProcess.Close();
                }
                RestoreVideoSettings(processInfo.OverwatchSettingsFilePath, initialSettings);
                throw new LoginFailedException("Could not log in with the input username or password.");
            }

            // Enter authenticator code if it is required
            if (s0 && s1)
            {
                if (String.IsNullOrEmpty(processInfo.Authenticator))
                {
                    if (processInfo.CloseOverwatchProcessOnFailure)
                    {
                        OWProcess.CloseMainWindow();
                        OWProcess.Close();
                    }
                    RestoreVideoSettings(processInfo.OverwatchSettingsFilePath, initialSettings);
                    throw new LoginFailedException("Authenticator is required");
                }

                TextInput(OWProcess.MainWindowHandle, processInfo.Authenticator);
                Thread.Sleep(10);
                KeyPress(OWProcess.MainWindowHandle, Keys.Enter);
                Thread.Sleep(500);

                while (true)
                {
                    Screenshot(processInfo.ScreenshotMethod, OWProcess.MainWindowHandle, ref bmp);
                    if (bmp.CompareColor(469, 437, CALData.ConfirmColor, 50) == false)
                        break;
                    Thread.Sleep(500);
                }
                Thread.Sleep(500);

                Screenshot(processInfo.ScreenshotMethod, OWProcess.MainWindowHandle, ref bmp);
                if (bmp.CompareColor(518, 482, CALData.ConfirmColor, 50))
                {
                    if (processInfo.CloseOverwatchProcessOnFailure)
                    {
                        OWProcess.CloseMainWindow();
                        OWProcess.Close();
                    }
                    RestoreVideoSettings(processInfo.OverwatchSettingsFilePath, initialSettings);
                    throw new LoginFailedException(string.Format("Authenticator number \"{0}\" is invalid.", processInfo.Authenticator));
                }
            }

            while (true)
            {
                Screenshot(processInfo.ScreenshotMethod, OWProcess.MainWindowHandle, ref bmp);
                if (bmp.CompareColor(53, 68, CALData.WhiteColor, 10))
                    break;
                Thread.Sleep(500);
            }

            Thread.Sleep(2000);

            if (processInfo.AutomaticallyCreateCustomGame)
            {
                KeyPress(OWProcess.MainWindowHandle, Keys.Tab, Keys.Space);
                Thread.Sleep(500);
                KeyPress(OWProcess.MainWindowHandle, Keys.Tab, Keys.Tab, Keys.Tab, Keys.Tab, Keys.Space);
                Thread.Sleep(500);
                KeyPress(OWProcess.MainWindowHandle, Keys.Tab, Keys.Tab, Keys.Tab, Keys.Tab, Keys.Space);
                Thread.Sleep(500);
            }

            // Reset the contrast to its initial value
            RestoreVideoSettings(processInfo.OverwatchSettingsFilePath, initialSettings);

            return OWProcess.MainWindowHandle;
        }

        static List<Tuple<string, string>> ChangeVideoSettings(string settingsFilePath, string[] settings, string[] setTo)
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

        static void RestoreVideoSettings(string settingsFilePath, List<Tuple<string, string>> settingsList)
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
    }

    public class OverwatchProcessInfo
    {
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
        public ScreenshotMethods ScreenshotMethod = ScreenshotMethods.BitBlt;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username">Username of the account for the new Overwatch process.</param>
        /// <param name="password">Password of the account for the new Overwatch process.</param>
        /// <param name="authenticator">Authenticator number from the Authenticator app. Only required if the account is hooked up to the Blizzard Authenticator app.</param>
        public OverwatchProcessInfo(string username, string password, string authenticator = null)
        {
            Username = username;
            Password = password;
            Authenticator = authenticator;
        }
    }
}
