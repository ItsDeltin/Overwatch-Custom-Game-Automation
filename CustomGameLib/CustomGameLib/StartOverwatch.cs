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
            if (processInfo == null)
                processInfo = new OverwatchInfoAuto();

            if (!File.Exists(processInfo.BattlenetExecutableFilePath))
                throw new FileNotFoundException(string.Format("Battle.net.exe's executable at {0} was not found. " +
                    "Change battlenetExeLocation to the location of the battle.net.exe executable.", processInfo.BattlenetExecutableFilePath));

            Stopwatch startTime = new Stopwatch();

            // If battle.net is not started, start it.
            if (Process.GetProcessesByName("battle.net").Length == 0)
            {
#if DEBUG
                CustomGameDebug.WriteLine("No battle.net process found, starting battle.net.");
#endif

                Process battlenet = new Process();
                battlenet.StartInfo.FileName = processInfo.BattlenetExecutableFilePath;
                battlenet.Start();

                startTime.Start();
                // The battle.net app is fully started when there are 3 battle.net processes. Loop while there are less than 3.
                while (Process.GetProcessesByName("battle.net").Length < 3)
                {
                    if (startTime.ElapsedMilliseconds >= processInfo.MaxBattlenetStartTime || processInfo.MaxBattlenetStartTime == -1)
                    {
#if DEBUG
                        CustomGameDebug.WriteLine("Error: Battle.net took too long to start.");
#endif
                        throw new OverwatchStartFailedException("Battle.net took too long to start.");
                    }
                    Thread.Sleep(200);
                }
#if DEBUG
                CustomGameDebug.WriteLine("Finished starting Battle.net.");
#endif
            }
#if DEBUG
            else
                CustomGameDebug.WriteLine("Battle.net process found.");

            CustomGameDebug.WriteLine("Starting the Overwatch process.");
#endif

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

                        if (processInfo.AutomaticallyCreateCustomGame)
                        {
                            CustomGame cg = new CustomGame(new CustomGameBuilder()
                            {
                                OpenChatIsDefault = false,
                                ScreenshotMethod = processInfo.ScreenshotMethod,
                                OverwatchProcess = owProcess
                            });

                            DirectBitmap bmp = null;
                            if (WaitForMainMenu(cg, processInfo.MaxWaitForMenuTime))
                            {
#if DEBUG
                                CustomGameDebug.WriteLine("Finished starting Overwatch.");
#endif
                                cg.CreateCustomGame();
                                if (bmp != null)
                                    bmp.Dispose();
                            }
                            else
                            {
#if DEBUG
                                CustomGameDebug.WriteLine("Could not start Overwatch, main menu did not load.");
#endif
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
#if DEBUG
                        else
                            CustomGameDebug.WriteLine("Finished starting Overwatch.");
#endif

                        return newProcessList[i];
                    }

                Thread.Sleep(200);
            }

#if DEBUG
            CustomGameDebug.WriteLine("Error: Overwatch took too long to start.");
#endif
            RestoreVideoSettings(processInfo.OverwatchSettingsFilePath, initialSettings);
            throw new OverwatchStartFailedException("Overwatch took too long to start.");
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

        private static void WaitForVisibleProcessWindow(Process process)
        {
            while (string.IsNullOrEmpty(process.MainWindowTitle))
            {
                Thread.Sleep(100);
                process.Refresh();
            }
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
            new string[] { "RenderContrast", "RenderBrightness", "RenderGamma", "ColorblindMode", "FullscreenWindow", "FullscreenWindowEnabled", "MaximizedWindow" }, 
            new string[] { "0.5",            "0",                "2.2",         "0",              "0",                "0",                       "0" });

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
