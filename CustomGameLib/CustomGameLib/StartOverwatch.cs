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
        /// <summary>
        /// Creates an Overwatch process using the currently logged in battle.net account.
        /// </summary>
        /// <param name="processInfo">Parameters for creating the process.</param>
        /// <returns>The created Overwatch process.</returns>
        public static Process StartOverwatch(OverwatchInfoAuto processInfo = null)
        {
            // Return any Overwatch process that might already be running.
            Process foundProcess = GetOverwatchProcess();
            if (foundProcess != null)
                return foundProcess;

            #region Argument check
            if (processInfo == null)
                processInfo = new OverwatchInfoAuto();

            // Check if the files in processInfo exist.
            if (!File.Exists(processInfo.BattlenetExecutableFilePath))
                throw new FileNotFoundException($"Battle.net.exe's executable at {processInfo.BattlenetExecutableFilePath} was not found. " +
                    "Change OverwatchInfoAuto.BattlenetExecutableFilePath to the location of the battle.net.exe executable.");

            if (!File.Exists(processInfo.OverwatchSettingsFilePath))
                throw new FileNotFoundException($"Overwatch's settings file at {processInfo.OverwatchSettingsFilePath} was not found. " +
                    "Change OverwatchInfoAuto.OverwatchSettingsFilePath to the location of Overwatch's settings file.");
            #endregion

            #region Start battle.net
            // If battle.net is not started, start it.
            if (Process.GetProcessesByName("battle.net").Length == 0)
            {
                CustomGameDebug.WriteLine("No battle.net process found, starting battle.net.");

                Process battlenet = new Process();
                battlenet.StartInfo.FileName = processInfo.BattlenetExecutableFilePath;
                battlenet.Start();

                // The battle.net app is fully started when there are 3 battle.net processes. Loop while there are less than 3.
                if (!SpinWait.SpinUntil(() => 
                {
                    return Process.GetProcessesByName("battle.net").Length >= 3;
                }, processInfo.MaxBattlenetStartTime))
                {
                    // This code block will run if battle.net isn't finished setting up before the specified maximum time.
                    CustomGameDebug.WriteLine("Error: Battle.net took too long to start.");
                    throw new OverwatchStartFailedException("Battle.net took too long to start.");
                }
                CustomGameDebug.WriteLine("Finished starting Battle.net.");
            }
            else
                CustomGameDebug.WriteLine("Battle.net process found.");
            #endregion

            CustomGameDebug.WriteLine("Starting the Overwatch process.");

            // Set the video settings.
            var initialSettings = ChangeVideoSettings(processInfo.OverwatchSettingsFilePath, VideoSettings.Item1, VideoSettings.Item2);

            try
            {
                try
                {
                    Process battlenetOW = new Process();
                    // The arguments to start the game directly before August 2018:
                    // battlenet.StartInfo.FileName = "battlenet://Pro";
                    // The arguments after:
                    battlenetOW.StartInfo.FileName = processInfo.BattlenetExecutableFilePath;
                    battlenetOW.StartInfo.Arguments = "--exec=\"launch Pro\"";
                    battlenetOW.Start();

                    Process owProcess = null;

                    TimeSpan overwatchStartTimeSpan = new TimeSpan(0, 0, 0, 0, processInfo.MaxOverwatchStartTime);

                    if (!SpinWait.SpinUntil(() =>
                    {
                        owProcess = GetOverwatchProcess();
                        return owProcess != null;
                    }, overwatchStartTimeSpan))
                    {
                        CustomGameDebug.WriteLine("Error: Overwatch took too long to start.");
                        throw new OverwatchStartFailedException("Overwatch took too long to start.");
                    }

                    // Wait for the window to be visible.
                    if (!SpinWait.SpinUntil(() =>
                    {
                        owProcess.Refresh();
                        return !string.IsNullOrEmpty(owProcess.MainWindowTitle);
                    }, overwatchStartTimeSpan))
                    {
                        CustomGameDebug.WriteLine("Error: Overwatch took too long to start.");
                        throw new OverwatchStartFailedException("Overwatch took too long to start.");
                    }

                    RestoreVideoSettings(processInfo.OverwatchSettingsFilePath, initialSettings);
                    initialSettings = null;

                    if (processInfo.AutomaticallyCreateCustomGame)
                    {
                        CustomGame cg = new CustomGame(new CustomGameBuilder()
                        {
                            OpenChatIsDefault = false,
                            ScreenshotMethod = processInfo.ScreenshotMethod,
                            OverwatchProcess = owProcess
                        });

                        if (WaitForMainMenu(cg, processInfo.MaxWaitForMenuTime))
                            cg.CreateCustomGame();
                        else
                        {
                            CustomGameDebug.WriteLine("Could not start Overwatch, main menu did not load.");

                            if (processInfo.CloseOverwatchProcessOnFailure)
                                owProcess.CloseMainWindow();

                            throw new OverwatchStartFailedException("Could not start Overwatch, main menu did not load.");
                        }
                    }

                    CustomGameDebug.WriteLine("Finished starting Overwatch.");
                    return owProcess;
                }
                finally
                {
                    CustomGameDebug.WriteLine("Restoring video settings.");
                    if (initialSettings != null)
                        RestoreVideoSettings(processInfo.OverwatchSettingsFilePath, initialSettings);
                }
            }
            catch (OverwatchClosedException)
            {
                CustomGameDebug.WriteLine("Could not start Overwatch, was closed during initialization.");
                throw new OverwatchStartFailedException("Could not start Overwatch, was closed during initialization.");
            }
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

        private static bool WaitForMainMenu(CustomGame cg, int maxtime)
        {
            return cg.WaitForColor(Points.MAIN_MENU_OVERWATCH_WATERMARK, Colors.WHITE, 10, maxtime);
        }

        /// <summary>
        /// <para>Item1 = the name of the setting.</para>
        /// <para>Item2 = what the setting should equal.</para>
        /// </summary>
        private static readonly Tuple<string[], string[]> VideoSettings = new Tuple<string[], string[]>(
            new string[] { "RenderContrast", "RenderBrightness", "RenderGamma", "ColorblindMode", "FullscreenWindow", "FullscreenWindowEnabled", "MaximizedWindow", "WindowedFullscreen", "WindowMode" }, 
            new string[] { "0.5",            "0",                "2.2",         "0",              "0",                "0",                       "0",               "1",                  "1" });

        /// <summary>
        /// Gets a running Overwatch process.
        /// </summary>
        /// <returns>The running Overwatch process. Returns null if there are none.</returns>
        public static Process GetOverwatchProcess()
        {
            return Process.GetProcessesByName("Overwatch").OrderBy(v => v.StartTime).FirstOrDefault();
        }
    }

    /// <summary>
    /// Data for creating an Overwatch process automatically.
    /// </summary>
    public class OverwatchInfoAuto
    {
        /// <summary>
        /// Data for creating an Overwatch process automatically.
        /// </summary>
        public OverwatchInfoAuto()
        {

        }

        /// <summary>
        /// If true, the Overwatch process will automatically create a Custom Game. Default is true.
        /// </summary>
        public bool AutomaticallyCreateCustomGame = true;
        /// <summary>
        /// Closes the Overwatch process if it fails to log in. Default is true.
        /// </summary>
        public bool CloseOverwatchProcessOnFailure = true;
        /// <summary>
        /// The path to the battle.net executable. Default is "C:\Program Files (x86)\Blizzard App\Battle.net.exe"
        /// </summary>
        public string BattlenetExecutableFilePath = @"C:\Program Files (x86)\Blizzard App\Battle.net.exe";
        /// <summary>
        /// The path to Overwatch's settings file. Default is "C:\Users\(EnvironmentName)\Documents\Overwatch\Settings\Settings_v0.ini"
        /// </summary>
        public string OverwatchSettingsFilePath = @"C:\Users\" + Environment.UserName + @"\Documents\Overwatch\Settings\Settings_v0.ini";
        /// <summary>
        /// The method that is used to take screenshots of the Overwatch window. Default is <see cref="ScreenshotMethod.BitBlt"/>
        /// </summary>
        public ScreenshotMethod ScreenshotMethod = ScreenshotMethod.BitBlt;
        /// <summary>
        /// The maximum amount of time to wait for the menu to load. Default is 20000.
        /// </summary>
        public int MaxWaitForMenuTime = 60000;
        /// <summary>
        /// The maximum amount of time to wait for Overwatch to start. Default is 10000.
        /// </summary>
        public int MaxOverwatchStartTime = 60000;
        /// <summary>
        /// The maximum amount of time to wait for Battle.net to start. Default is 10000.
        /// </summary>
        public int MaxBattlenetStartTime = 60000;
    }
}
