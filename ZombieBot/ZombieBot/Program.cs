using System;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
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
        public static Random rnd = new Random();

        public static int version = 0;
        private static InfectionMap[] ElimMaps = new InfectionMap[]
        {
            new InfectionMap(Map.ELIM_Ayutthaya, "Ayutthaya"),
            new InfectionMap(Map.ELIM_Ilios_Well, "Ilios-Well"),
            new InfectionMap(Map.ELIM_Ilios_Ruins, "Ilios-Ruins"),
            new InfectionMap(Map.ELIM_Ilios_Lighthouse, "Ilios-Lighthouse"),
            new InfectionMap(Map.ELIM_Lijiang_ControlCenter, "Lijiang-CC"),
            new InfectionMap(Map.ELIM_Lijiang_Garden, "Lijiang-Garden"),
            new InfectionMap(Map.ELIM_Nepal_Sanctum, "Nepal-Sanctum"),
            new InfectionMap(Map.ELIM_Nepal_Shrine, "Nepal-Shrine"),
            new InfectionMap(Map.ELIM_Nepal_Village, "Nepal-Village"),
            new InfectionMap(Map.ELIM_Oasis_CityCenter, "Oasis-CC")
        };
        private static InfectionMap[] TdmMaps = new InfectionMap[]
        {
            new InfectionMap(Map.TDM_Dorado, "Dorado"),
            new InfectionMap(Map.TDM_Eichenwalde, "Eichenwalde"),
            new InfectionMap(Map.TDM_Hanamura, "Hanamura"),
            new InfectionMap(Map.TDM_Hollywood, "Hollywood"),
            new InfectionMap(Map.TDM_HorizonLunarColony, "Horizon"),
            new InfectionMap(Map.TDM_KingsRow, "KingsRow"),
            new InfectionMap(Map.TDM_TempleOfAnubis, "TempleOfAnubis"),
            new InfectionMap(Map.TDM_VolskayaIndustries, "Volskaya"),
            new InfectionMap(Map.TDM_Ilios_Well, "Ilios-Well"),
            new InfectionMap(Map.TDM_Ilios_Ruins, "Ilios-Ruins")
        };

        public static string[] ValidRegions = new string[] { "us", "eu", "kr" };

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
            int preset = -1;

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

                            case "preset":
                                {
                                    if (Int32.TryParse(lineSplit[1], out int set))
                                        if (set == 0 || set == 1)
                                            preset = set;
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

            Console.WriteLine("Press return to start.");
            Console.ReadLine();
            Console.WriteLine("Starting...");

            while (true)
            {
                CustomGame cg = new CustomGame(new CustomGameBuilder()
                {
                    OverwatchProcess = CustomGame.GetOverwatchProcess() ?? CustomGame.CreateOverwatchProcessAutomatically(new OverwatchProcessInfoAuto()),
                    ScreenshotMethod = screenshotMethod
                });

                // Set the mode enabled
                if (version == 0)
                {
                    cg.ModesEnabled = Gamemode.Elimination;
                }
                else if (version == 1)
                {
                    cg.ModesEnabled = Gamemode.TeamDeathmatch;
                }

                // Set event
                if (owevent == null)
                    cg.CurrentOverwatchEvent = cg.GetCurrentOverwatchEvent();
                else
                    cg.CurrentOverwatchEvent = (Event)owevent;

                a = null;
                if (Join == JoinType.Abyxa)
                {
                    a = new Abyxa(name, region, local);
                    a.SetMinimumPlayerCount(minimumPlayers);
                    cg.Settings.SetJoinSetting(Deltin.CustomGameAutomation.Join.InviteOnly);
                }

                var maps = version == 0 ? ElimMaps : TdmMaps;

                Setup(cg, maps, preset, name);

                while (true)
                {
                    if (!Pregame(cg, maps))
                        break;
                    if (!Ingame(cg))
                        break;
                }

                if (!cg.HasExited())
                {
                    cg.UsingOverwatchProcess.Close();
                    cg.UsingOverwatchProcess.WaitForExit();
                }
                cg.Dispose();
                Thread.Sleep(30000);
            } // Bot loop
        } // Main()
    }

    class InfectionMap
    {
        public InfectionMap(Map map, string shortenedName)
        {
            Map = map;
            ShortenedName = shortenedName;
        }
        public Map Map { get; private set; }
        public string ShortenedName { get; private set; }
    }
}