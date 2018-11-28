using System;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Deltin.CustomGameAutomation;

namespace ZombieBot
{
    partial class Program
    {
        private static readonly Map[] ElimMaps = new Map[]
        {
            Map.ELIM_Ayutthaya,
            Map.ELIM_Ilios_Well,
            Map.ELIM_Ilios_Ruins,
            Map.ELIM_Ilios_Lighthouse,
            Map.ELIM_Lijiang_ControlCenter,
            Map.ELIM_Lijiang_Garden,
            Map.ELIM_Nepal_Sanctum,
            Map.ELIM_Nepal_Shrine,
            Map.ELIM_Nepal_Village,
            Map.ELIM_Oasis_CityCenter,
        };
        private static readonly Map[] TdmMaps = new Map[]
        {
            Map.TDM_Dorado,
            Map.TDM_Eichenwalde,
            Map.TDM_Hanamura,
            Map.TDM_Hollywood,
            Map.TDM_HorizonLunarColony,
            Map.TDM_KingsRow,
            Map.TDM_TempleOfAnubis,
            Map.TDM_VolskayaIndustries,
            Map.TDM_Ilios_Well,
            Map.TDM_Ilios_Ruins
        };

        static void Main(string[] args)
        {
            string header = "Zombiebot - https://github.com/ItsDeltin/Overwatch-Custom-Game-Automation";
            Console.Title = header;
            Console.WriteLine(header);

            Config config = Config.ParseConfig();

            Abyxa abyxa = null;
            if (config.DefaultMode == "abyxa")
            {
                abyxa = new Abyxa(config.Name, config.Region, config.Local);
                abyxa.ZombieServer.MinimumPlayerCount = config.MinimumPlayers;
            }
            bool serverBrowser = config.DefaultMode == "serverbrowser";

            Task.Run(() =>
            {
                while (true)
                {
                    CustomGame cg = new CustomGame(new CustomGameBuilder()
                    {
                        OverwatchProcess = CustomGame.GetOverwatchProcess() ?? CustomGame.CreateOverwatchProcessAutomatically(),
                        ScreenshotMethod = config.ScreenshotMethod
                    });
                    cg.Commands.Listen = true;

                    cg.ModesEnabled = config.Version == 0 ? Gamemode.Elimination : Gamemode.TeamDeathmatch;
                    cg.CurrentEvent = config.OverwatchEvent;

                    Map[] maps = config.Version == 0 ? ElimMaps : TdmMaps;

                    Setup(abyxa, serverBrowser, cg, maps, config.Preset, config.Name);

                    while (true)
                    {
                        if (!Pregame(abyxa, serverBrowser, cg, maps, config.MinimumPlayers))
                            break;
                        if (!Ingame(abyxa, serverBrowser, cg, config.Version))
                            break;
                    }

                    Console.WriteLine("Resetting (1/4)...");
                    if (!cg.HasExited())
                    {
                        cg.OverwatchProcess.Close();
                    }
                    Console.WriteLine("Resetting (2/4)...");
                    cg.Dispose();
                    Console.WriteLine("Resetting (3/4)...");
                    Thread.Sleep(30000);
                    Console.WriteLine("Resetting (4/4)...");
                } // Bot loop
            });

            while (true)
            {
                Console.Write(">");
                string[] input = Console.ReadLine().Split(' ');
                input[0] = input[0].ToLower();

                switch(input[0])
                {
                    case "help":
                        Console.WriteLine("invite <battletag>");
                        break;

                    case "invite":
                        string invitePlayer = input.ElementAtOrDefault(1);
                        if (invitePlayer != null)
                            cg.InvitePlayer(invitePlayer, Team.BlueAndRed);
                        break;

                    default:
                        break;
                }
            }
        } // Main()

        public static string UpdateMap(Abyxa abyxa, CustomGame cg)
        {
            string currentMap = cg.GetCurrentMap()?.FirstOrDefault()?.ShortName;
            if (currentMap != null && abyxa != null)
            {
                abyxa.ZombieServer.Map = currentMap;
                abyxa.Update();
            }
            return currentMap;
        }
    }
}