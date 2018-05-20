using System;
using System.Linq;
using System.Collections.Generic;
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
        public static string[] maps = new string[] // Must be the same length as mapsSend
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
        public static string[] mapsSend = new string[] // Must be the same length as maps. Shorter versions of the map names to send to overwatch chat.
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
        public static bool NoInviteWait = false; // Determines if players invited is considered ingame, false=not.
        public static bool InstantStart = false; // Determines if the game should start right away, ignoring minimum player count.
        public static bool NoHeroChosenSwap = true; // If true, survivors who did not choose a hero when the zombies are released is swapped to zombies.

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
            bool autoStart = false; // Determines if the game should autostart or wait for user input.
            List<string> mapsSet = null; // Maps to vote for.
            List<string> mapsSendSet = null; // Maps to send to chat during vote.
            string mode = "TeamDeathmatch"; // The mode enabled in the Custom Game.
            Event? owevent = null; // The current overwatch event
            ScreenshotMethods screenshotMethod = ScreenshotMethods.BitBlt;

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

                        if (lineSplit[0] == "NoInviteWait")
                        {
                            if (bool.TryParse(lineSplit[1], out bool set))
                                NoInviteWait = set;
                        }

                        else if (lineSplit[0] == "InstantStart")
                        {
                            if (bool.TryParse(lineSplit[1], out bool set))
                                InstantStart = set;
                        }

                        else if (lineSplit[0] == "local")
                        {
                            if (bool.TryParse(lineSplit[1], out bool set))
                                local = set;
                        }

                        else if (lineSplit[0] == "minimumPlayers")
                        {
                            if (int.TryParse(lineSplit[1], out int set) && set > 0 && set <= 7)
                                minimumPlayers = set;
                        }

                        else if (lineSplit[0] == "name")
                            name = lineSplit[1];

                        else if (lineSplit[0] == "region" && ValidRegions.Contains(lineSplit[1]))
                            region = lineSplit[1];

                        else if (lineSplit[0] == "DefaultMode")
                        {
                            if (Enum.TryParse(lineSplit[1], out JoinType jointype))
                                Join = jointype;
                        }

                        else if (lineSplit[0] == "AutoStart")
                        {
                            if (bool.TryParse(lineSplit[1], out bool set))
                                autoStart = set;
                        }

                        else if (lineSplit[0] == "NoHeroChosenSwap")
                        {
                            if (bool.TryParse(lineSplit[1], out bool set))
                                NoHeroChosenSwap = set;
                        }

                        else if (lineSplit[0] == "Event")
                        {
                            if (Enum.TryParse(lineSplit[1], out Event setowevent))
                                owevent = setowevent;
                        }

                        else if (lineSplit[0] == "Maps")
                        {
                            mapsSet = new List<string>();
                            string[] set = lineSplit[1].Split(',');
                            for (int setindex = 0; setindex < set.Length; setindex++)
                            {
                                set[setindex] = set[setindex].Trim(' ');
                                if (CustomGame.CG_Maps.MapIDFromName(set[setindex]) != null)
                                    mapsSet.Add(set[setindex]);
                                else
                                {
                                    Console.WriteLine("Config parse error: Map \"{0}\" does not exist.", set[setindex]);
                                    mapsSend = null;
                                    break;
                                }
                            }
                        }

                        else if (lineSplit[0] == "MapsSend")
                        {
                            mapsSendSet = new List<string>();
                            string[] set = lineSplit[1].Split(',');
                            for (int setindex = 0; setindex < set.Length; setindex++)
                                mapsSendSet.Add(set[setindex].Trim(' '));
                        }

                        else if (lineSplit[0] == "CustomGameMode")
                        {
                            string[] validmodes = new string[] { "TeamDeathmatch", "Elimination" };
                            string setmode = lineSplit[1].Trim(' ');
                            if (validmodes.Contains(setmode))
                                mode = setmode;
                            else if (setmode != "")
                                Console.WriteLine("Mode \"{0}\" in CustomGameMode in config.txt is not a valid mode.", setmode);
                        }

                        else if (lineSplit[0] == "ScreenshotMethod")
                        {
                            if (Enum.TryParse(lineSplit[1], out ScreenshotMethods set))
                                screenshotMethod = set;
                        }

                    }
                }

            if (mapsSet != null && mapsSendSet != null)
            {
                if (mapsSet.Count == mapsSendSet.Count)
                {
                    maps = mapsSet.ToArray();
                    mapsSend = mapsSendSet.ToArray();
                }
                else
                    Console.WriteLine("\"Maps\" and \"MapsSend\" must be the same length in config.txt, using default maps.");
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

            IntPtr useHwnd = new IntPtr();

            while (true)
            {
                Process[] overwatchProcesses = Process.GetProcessesByName("Overwatch");

                if (overwatchProcesses.Length == 0)
                {
                    Console.WriteLine("No Overwatch processes found, press enter to recheck.");
                    continue;
                }

                else if (overwatchProcesses.Length == 1)
                {
                    useHwnd = overwatchProcesses[0].MainWindowHandle;
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
                                useHwnd = hwnd;
                                lookingForWindow = false;
                                break;
                            }
                        System.Threading.Thread.Sleep(500);
                    }
                    break;
                }
            }

            if (!autoStart)
            {
                Console.WriteLine("Press return to start.");
                Console.ReadLine();
                Console.WriteLine("Starting...");
            }

            cg = new CustomGame(useHwnd, screenshotMethod);
            // Set the mode enabled
            if (mode == "TeamDeathmatch")
                cg.ModesEnabled.TeamDeathmatch = true;
            else if (mode == "Elimination")
                cg.ModesEnabled.Elimination = true;
            // Set event
            if (owevent == null)
                cg.CurrentOverwatchEvent = cg.GetCurrentOverwatchEvent();
            else
                cg.CurrentOverwatchEvent = (Event)owevent;
            cg.Command.ListenTo.Add("$VOTE", true);
            cg.Command.SameExecutorCommandUpdate = true;
            cg.Chat.BlockGeneralChat = true;

            a = null;
            if (Join == JoinType.Abyxa)
            {
                a = new Abyxa(name, region, local);
                a.SetMinimumPlayerCount(minimumPlayers);
                cg.GameSettings.SetJoinSetting(Deltin.CustomGameAutomation.Join.InviteOnly);
            }

            Setup();

            while (true)
            {
                bool pregame = Pregame();
                if (pregame)
                {
                    Ingame();
                }
                else
                {
                    Setup();
                }
            }
        }
    }
}