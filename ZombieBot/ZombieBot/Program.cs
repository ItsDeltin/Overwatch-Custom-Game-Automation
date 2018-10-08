using System;
using System.Linq;
using System.Diagnostics;
using Deltin.CustomGameAutomation;

namespace ZombieBot
{
    enum JoinType
    {
        ServerBrowser,
        Abyxa,
        Private
    }

    partial class Program
    {
        public static int minimumPlayers = 5; // Minimum players before the game starts
        public static JoinType? Join = null; // The way other players will join the game.
        public static Abyxa a;
        public static CustomGame cg;
        public static Random rnd = new Random();

        public static int version = 0;
        public static string[] maps = null;
        public static string[] mapsSend = null;
        static string[] ElimMaps = new string[]
        {
            "ELIM_Ayutthaya",
            "ELIM_Ilios_Ruins",
            "ELIM_Ilios_Well",
            "ELIM_Ilios_Lighthouse",
            "ELIM_Lijiang_ControlCenter",
            "ELIM_Lijiang_Garden",
            "ELIM_Nepal_Sanctum",
            "ELIM_Nepal_Shrine",
            "ELIM_Nepal_Village",
            "ELIM_Oasis_CityCenter"
        };
        static string[] ElimMapsSend = new string[]
        {
            "Ayutthaya",
            "Ilios-Ruins",
            "Ilios-Well",
            "Ilios-Lighthouse",
            "Lijiang-Control",
            "Lijiang-Garden",
            "Nepal-Sanctum",
            "Nepal-Shrine",
            "Nepal-Village",
            "Oasis-CC"
        };
        public static string[] DmMaps = new string[] // Must be the same length as mapsSend
        {
            "TDM_Dorado",
            "TDM_Eichenwalde",
            "TDM_Hanamura",
            "TDM_Hollywood",
            "TDM_HorizonLunarColony",
            "TDM_KingsRow",
            "TDM_TempleOfAnubis",
            "TDM_VolskayaIndustries",
            "TDM_Ilios_Well",
            "TDM_Ilios_Ruins"
        };
        public static string[] DmMapsSend = new string[] // Must be the same length as maps. Shorter versions of the map names to send to overwatch chat.
        {
            "Dorado",
            "Eichenwalde",
            "Hanamura",
            "Hollywood",
            "Horizon",
            "KingsRow",
            "TempleOfAnubis",
            "Volskaya",
            "Ilios-Well",
            "Ilios-Ruins"
        };

        public static string[] ValidRegions = new string[] { "us", "eu", "kr" };

        [STAThread]
        static void Main(string[] args)
        {
            string header = "Zombiebot v1.2";
            Console.Title = header;
            Console.WriteLine(header);

            string name = "Zombies - Infection"; // Default name for the Abyxa server.
            string region = "us"; // Default region for the Abyxa server.
            bool local = false; // Determines if the Abyxa website is on the local server.
            Event? owevent = null; // The current overwatch event
            ScreenshotMethod screenshotMethod = ScreenshotMethod.BitBlt;

            // Parse config file
            string[] config = null;
            string filecheck = Extra.GetExecutingDirectory() + "config.txt"; // File location of config file.
            try
            {
                config = System.IO.File.ReadAllLines(filecheck);
            }
            catch (Exception ex)
            {
                if (ex is System.IO.DirectoryNotFoundException || ex is System.IO.FileNotFoundException)
                    Console.WriteLine("Could not find configuration file at '{0}', using default settings.", filecheck);
                else
                    Console.WriteLine("Error getting configuration file at '{0}', using default settings.", filecheck);
            }
            if (config != null)
                for (int i = 0; i < config.Length; i++) // For each line in the config file
                {
                    string line = config[i].Trim(' ');
                    if (line.Length >= 2)
                        if (line[0] == '/' && line[1] == '/')
                            continue;

                    // Remove any text after "//"
                    int index = line.IndexOf("//");
                    if (index > 0)
                        line = line.Substring(0, index);
                    // Split line at "="
                    string[] lineSplit = line.Split(new string[] { "=" }, 2, StringSplitOptions.RemoveEmptyEntries);
                    // Trim whitespace
                    for (int lsi = 0; lsi < lineSplit.Length; lsi++)
                        lineSplit[lsi] = lineSplit[lsi].Trim(' ');

                    if (lineSplit.Length > 1)
                    {
                        switch (lineSplit[0])
                        {
                            case "local":
                                {
                                    if (bool.TryParse(lineSplit[1], out bool set))
                                        local = set;
                                }
                                break;

                            case "minimumPlayers":
                                {
                                    if (int.TryParse(lineSplit[1], out int set) && set >= 0 && set <= 7)
                                        minimumPlayers = set;
                                }
                                break;

                            case "name":
                                {
                                    name = lineSplit[1];
                                }
                                break;

                            case "region":
                                {
                                    if (lineSplit[0] == "region" && ValidRegions.Contains(lineSplit[1]))
                                        region = lineSplit[1];
                                }
                                break;

                            case "DefaultMode":
                                {
                                    if (Enum.TryParse(lineSplit[1], out JoinType jointype))
                                        Join = jointype;
                                }
                                break;

                            case "Event":
                                {
                                    if (Enum.TryParse(lineSplit[1], out Event setowevent))
                                        owevent = setowevent;
                                }
                                break;

                            case "ScreenshotMethod":
                                {
                                    if (Enum.TryParse(lineSplit[1], out ScreenshotMethod set))
                                        screenshotMethod = set;
                                }
                                break;

                            case "version":
                                {
                                    if (Int32.TryParse(lineSplit[1], out int set))
                                        if (set == 0 || set == 1)
                                            version = set;
                                }
                            break;
                        }
                    }
                }

            if (Join == null)
            {
                string joinmode = Extra.ConsoleInput("Abyxa or server browser (\"abyxa\"/\"sb\"/\"private\"): ", "abyxa", "sb", "private");
                if (joinmode == "abyxa")
                    Join = JoinType.Abyxa;
                else if (joinmode == "sb")
                    Join = JoinType.ServerBrowser;
                else if (joinmode == "private")
                    Join = JoinType.Private;
            }

            Process useProcess = null;

            while (true)
            {
                Process[] overwatchProcesses = Process.GetProcessesByName("Overwatch");

                if (overwatchProcesses.Length == 0)
                {
                    Console.WriteLine("No Overwatch processes found, press enter to recheck.");
                    Console.ReadLine();
                    continue;
                }

                else if (overwatchProcesses.Length == 1)
                {
                    useProcess = overwatchProcesses[0];
                    break;
                }

                else if (overwatchProcesses.Length > 1)
                {
                    Console.Write("Click on the Overwatch window to use... ");
                    bool lookingForWindow = true;
                    while (lookingForWindow)
                    {
                        IntPtr hwnd = Extra.GetForegroundWindow();
                        overwatchProcesses = Process.GetProcessesByName("Overwatch");
                        for (int i = 0; i < overwatchProcesses.Length; i++)
                            if (overwatchProcesses[i].MainWindowHandle == hwnd)
                            {
                                Console.WriteLine("Overwatch window found.");
                                useProcess = overwatchProcesses[i];
                                lookingForWindow = false;
                                break;
                            }
                        System.Threading.Thread.Sleep(500);
                    }
                    break;
                }
            }

            Console.WriteLine("Press return to start.");
            Console.ReadLine();
            Console.WriteLine("Starting...");

            cg = new CustomGame(new CustomGameBuilder() { OverwatchProcess = useProcess, ScreenshotMethod = screenshotMethod });

            // Set the mode enabled
            if (version == 0)
            {
                cg.ModesEnabled = Gamemode.Elimination;
                maps = ElimMaps;
                mapsSend = ElimMapsSend;
            }
            else if (version == 1)
            {
                cg.ModesEnabled = Gamemode.TeamDeathmatch;
                maps = DmMaps;
                mapsSend = DmMapsSend;
            }

            // Set event
            if (owevent == null)
                cg.CurrentOverwatchEvent = cg.GetCurrentOverwatchEvent();
            else
                cg.CurrentOverwatchEvent = (Event)owevent;

            cg.Commands.ListenTo.Add(new ListenTo("$VOTE", true, false, null));

            a = null;
            if (Join == JoinType.Abyxa)
            {
                a = new Abyxa(name, region, local);
                a.SetMinimumPlayerCount(minimumPlayers);
                cg.Settings.SetJoinSetting(Deltin.CustomGameAutomation.Join.InviteOnly);
            }

            Setup(true);

            while (true)
            {
                bool pregame = Pregame();
                if (pregame)
                {
                    Ingame();
                }
                else
                {
                    Setup(false);
                }
            }
        }
    }
}