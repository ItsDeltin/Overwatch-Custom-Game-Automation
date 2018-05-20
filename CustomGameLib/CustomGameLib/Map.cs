using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Reflection;
using System.Windows.Forms;

namespace Deltin.CustomGameAutomation
{
    partial class CustomGame
    {
        /// <summary>
        /// Modes enabled in custom games.
        /// </summary>
        public ModesEnabled ModesEnabled = new ModesEnabled();
        /// <summary>
        /// The current event occuring in Overwatch.
        /// </summary>
        public Event CurrentOverwatchEvent = Event.None;
        /// <summary>
        /// Gets the current Overwatch event. This compares the current date with past event's start and end times, so specific time may be a little off.
        /// </summary>
        /// <returns>The current Overwatch event as the Event enum.</returns>
        public Event GetCurrentOverwatchEvent()
        {
            DateTime cdt = DateTime.UtcNow;
            DateTime currentdate = new DateTime(1, cdt.Month, cdt.Day);

            // new DateTime(1, MONTH, DAY)
            // Summer Games
            DateTime sgStart = new DateTime(1, 8, 8);
            DateTime sgEnd = new DateTime(1, 8, 28);
            // Halloween Terror
            DateTime htStart = new DateTime(1, 10, 10);
            DateTime htEnd = new DateTime(1, 11, 1);
            // Winter Wonderland
            DateTime wwStart = new DateTime(1, 12, 12);
            DateTime wwEnd = new DateTime(2, 1, 1); // 2 because next year
            // Lunar New Year
            DateTime lnyStart = new DateTime(1, 2, 8);
            DateTime lnyEnd = new DateTime(1, 3, 5);
            // Uprising
            DateTime uStart = new DateTime(1, 4, 11);
            DateTime uEnd = new DateTime(1, 5, 1);
            // Aniversary
            DateTime aStart = new DateTime(1, 5, 23);
            DateTime aEnd = new DateTime(1, 6, 12);

            if (currentdate >= sgStart && currentdate <= sgEnd)
                return Event.SummerGames;

            if (currentdate >= htStart && currentdate <= htEnd)
                return Event.HalloweenTerror;

            if (currentdate >= wwStart && currentdate <= wwEnd)
                return Event.WinterWonderland;

            if (currentdate >= lnyStart && currentdate <= lnyEnd)
                return Event.LunarNewYear;

            if (currentdate >= uStart && currentdate <= uEnd)
                return Event.Uprising;

            if (currentdate >= aStart && currentdate <= aEnd)
                return Event.Aniversary;

            return Event.None;
        }

        /// <summary>
        /// Fields related to Overwatch maps.
        /// </summary>
        public CG_Maps Maps;
        /// <summary>
        /// Fields related to Overwatch maps.
        /// </summary>
        public class CG_Maps
        {
            private CustomGame cg;
            internal CG_Maps(CustomGame cg)
            { this.cg = cg; }

            /// <summary>
            /// Toggles maps in Overwatch.
            /// </summary>
            /// <param name="ta">Determines if all maps should be enabled, disabled or neither before toggling.</param>
            /// <param name="maps">Maps that should be toggled.</param>
            public void ToggleMap(ToggleAction ta, params Map[] maps)
            {
                cg.gotosettings();

                cg.LeftClick(103, 300, 1000); // Clicks "Maps" button (SETTINGS/MAPS/)

                // Click Disable All or Enable All in custom games if mta doesnt equal MapToggleAction.None.
                if (ta == ToggleAction.DisableAll)
                    cg.LeftClick(640, 125);
                else if (ta == ToggleAction.EnableAll)
                    cg.LeftClick(600, 125);

                // Get the modes enabled state in a bool in alphabetical order.
                bool[] enabledModes = new bool[]
                {
                    cg.ModesEnabled.Assault,
                    cg.ModesEnabled.AssaultEscort,
                    cg.ModesEnabled.CaptureTheFlag,
                    cg.ModesEnabled.Control,
                    cg.ModesEnabled.Deathmatch,
                    cg.ModesEnabled.Elimination,
                    cg.ModesEnabled.Escort,
                    cg.ModesEnabled.JunkensteinsRevenge,
                    cg.ModesEnabled.Lucioball,
                    cg.ModesEnabled.MeisSnowballOffensive,
                    cg.ModesEnabled.Skirmish,
                    cg.ModesEnabled.TeamDeathmatch,
                    cg.ModesEnabled.YetiHunter
                };

                List<int> selectMap = new List<int>();
                int mapcount = 0;
                // For each enabled mode...
                for (int i = 0; i < enabledModes.Length; i++)
                    if (enabledModes[i])
                    {
                        Gamemode emi = (Gamemode)i; //enabledmodesindex
                        List<Map> allowedmaps = GetAllowedMaps(emi);
                        // ...And for each map...
                        for (int mi = 0; mi < maps.Length; mi++)
                            // ...Check if the maps mode equals the enabledModes index and check if the map is in allowed maps...
                            if (maps[mi].GameMode == emi && allowedmaps.Contains(maps[mi]))
                            {
                                // ...then add the map index to the selectmap list. 1, 5, 10 will toggle the first map in overwatch, the fifth, then the tenth...
                                selectMap.Add(mapcount + allowedmaps.IndexOf(maps[mi]) + 1);
                            }
                        // ...then finally add the number of maps in the mode to the mapcount.
                        mapcount += allowedmaps.Count;
                    }
                mapcount++;

                if (cg.OpenChatIsDefault)
                {
                    cg.Chat.CloseChat();
                }

                // Toggle maps
                for (int i = 0; i < mapcount; i++)
                {
                    for (int mi = 0; mi < selectMap.Count; mi++)
                        if (selectMap[mi] == i)
                        {
                            cg.KeyPress(Keys.Space);
                            Thread.Sleep(1);
                        }
                    cg.KeyPress(Keys.Down);
                    Thread.Sleep(1);
                }

                if (cg.OpenChatIsDefault)
                    cg.Chat.OpenChat();

                cg.GoBack(2, 0);
            }

            /// <summary>
            /// Gets map ID from map name.
            /// </summary>
            /// <param name="map">Map name.</param>
            /// <returns>Returns map ID.</returns>
            public static Map MapIDFromName(string map)
            {
                FieldInfo[] fi = GetMapFieldInfo();
                for (int i = 0; i < fi.Length; i++)
                    if (fi[i].Name.ToLower() == map.ToLower())
                        return MaparFromFieldInfo(fi[i]);
                return null;
            }

            /// <summary>
            /// Gets map name from map ID.
            /// </summary>
            /// <param name="map">Map ID.</param>
            /// <returns>Returns map name.</returns>
            public static string MapNameFromID(Map map)
            {
                FieldInfo[] fi = GetMapFieldInfo();
                for (int i = 0; i < fi.Length; i++)
                    if (MaparFromFieldInfo(fi[i]) == map)
                        return fi[i].Name;
                return null;
            }

            private static FieldInfo[] GetMapFieldInfo()
            {
                return typeof(Map).GetFields(BindingFlags.Public | BindingFlags.Static);
            }
            private static Map MaparFromFieldInfo(FieldInfo fi)
            {
                return (Map)fi.GetValue(null);
            }
            private static List<Map> GetMaps()
            {
                List<Map> allmaps = new List<Map>();
                FieldInfo[] fi = GetMapFieldInfo();
                for (int i = 0; i < fi.Length; i++)
                    allmaps.Add(MaparFromFieldInfo(fi[i]));
                return allmaps;
            }
            private List<Map> GetAllowedMaps(Gamemode gamemode)
            {
                List<Map> allmaps = GetMaps();

                List<Map> selected = new List<Map>();
                for (int i = 0; i < allmaps.Count; i++)
                    if ((allmaps[i].GameEvent == cg.CurrentOverwatchEvent || allmaps[i].GameEvent == Event.None) && allmaps[i].GameMode == gamemode)
                        selected.Add(allmaps[i]);
                return selected;
            }
            /*
            private static string[] GetGamemodeMaps(Gamemode gamemode)
            {
                var maps = GetAllMapNames();
                List<string> gamemodeMaps = new List<string>();
                for (int i = 0; i < maps.Length; i++)
                    if (maps[i].Split('_')[0] == GamemodeAcronyms[(int)gamemode])
                        gamemodeMaps.Add(maps[i]);
                return gamemodeMaps.ToArray();
            }
            */
            private static string[] GetAllMapNames()
            {
                return typeof(Map).GetFields().Select(field => field.Name).ToArray();
            }
            private static string[] GamemodeAcronyms = new string[]
            {
                "A",
                "AE",
                "CTF",
                "C",
                "DM",
                "ELIM",
                "E",
                "JR",
                "LB",
                "MSO",
                "SKIRM",
                "TDM",
                "YH"
            };
        }
    }

    /// <summary>
    /// Maps in Overwatch.
    /// </summary>
    public class Map
    {
        // This is all possible map variants that can be selected in custom games. All static fields must be a Map value.
#pragma warning disable CS1591
        // Assault
        public static Map A_Hanamura = new Map(Gamemode.Assault, 0, Event.None);
        public static Map A_Hanamura_Winter = new Map(Gamemode.Assault, 1, Event.WinterWonderland);
        public static Map A_HorizonLunarColony = new Map(Gamemode.Assault, 2, Event.None);
        public static Map A_TempleOfAnubis = new Map(Gamemode.Assault, 3, Event.None);
        public static Map A_VolskayaIndustries = new Map(Gamemode.Assault, 4, Event.None);

        // AssaultEscort
        public static Map AE_BlizzardWorld = new Map(Gamemode.AssaultEscort, 0, Event.None);
        public static Map AE_Eichenwalde = new Map(Gamemode.AssaultEscort, 1, Event.None);
        public static Map AE_Eichenwalde_Halloween = new Map(Gamemode.AssaultEscort, 2, Event.HalloweenTerror);
        public static Map AE_Hollywood = new Map(Gamemode.AssaultEscort, 3, Event.None);
        public static Map AE_Hollywood_Halloween = new Map(Gamemode.AssaultEscort, 4, Event.HalloweenTerror);
        public static Map AE_KingsRow = new Map(Gamemode.AssaultEscort, 5, Event.None);
        public static Map AE_KingsRow_Winter = new Map(Gamemode.AssaultEscort, 6, Event.WinterWonderland);
        public static Map AE_Numbani = new Map(Gamemode.AssaultEscort, 7, Event.None);

        // Capture The Flag
        public static Map CTF_Ayutthaya = new Map(Gamemode.CaptureTheFlag, 0, Event.None);
        public static Map CTF_Ilios_Lighthouse = new Map(Gamemode.CaptureTheFlag, 1, Event.None);
        public static Map CTF_Ilios_Ruins = new Map(Gamemode.CaptureTheFlag, 2, Event.None);
        public static Map CTF_Ilios_Well = new Map(Gamemode.CaptureTheFlag, 3, Event.None);
        public static Map CTF_Lijiang_ControlCenter = new Map(Gamemode.CaptureTheFlag, 4, Event.None);
        public static Map CTF_Lijiang_ControlCenter_Lunar = new Map(Gamemode.CaptureTheFlag, 5, Event.LunarNewYear);
        public static Map CTF_Lijiang_Garden = new Map(Gamemode.CaptureTheFlag, 6, Event.None);
        public static Map CTF_Lijiang_Garden_Lunar = new Map(Gamemode.CaptureTheFlag, 7, Event.LunarNewYear);
        public static Map CTF_Lijiang_NightMarket = new Map(Gamemode.CaptureTheFlag, 8, Event.None);
        public static Map CTF_Lijiang_NightMarket_Lunar = new Map(Gamemode.CaptureTheFlag, 9, Event.LunarNewYear);
        public static Map CTF_Nepal_Sanctum = new Map(Gamemode.CaptureTheFlag, 10, Event.None);
        public static Map CTF_Nepal_Shrine = new Map(Gamemode.CaptureTheFlag, 11, Event.None);
        public static Map CTF_Nepal_Village = new Map(Gamemode.CaptureTheFlag, 12, Event.None);
        public static Map CTF_Oasis_CityCenter = new Map(Gamemode.CaptureTheFlag, 13, Event.None);
        public static Map CTF_Oasis_Gardens = new Map(Gamemode.CaptureTheFlag, 14, Event.None);
        public static Map CTF_Oasis_University = new Map(Gamemode.CaptureTheFlag, 15, Event.None);

        // Control
        public static Map C_Ilios = new Map(Gamemode.Control, 0, Event.None);
        public static Map C_Lijiang = new Map(Gamemode.Control, 1, Event.None);
        public static Map C_Lijiang_Lunar = new Map(Gamemode.Control, 2, Event.LunarNewYear);
        public static Map C_Nepal = new Map(Gamemode.Control, 3, Event.None);
        public static Map C_Oasis = new Map(Gamemode.Control, 4, Event.None);

        // Deathmatch
        public static Map DM_BlackForest = new Map(Gamemode.Deathmatch, 0, Event.None);
        public static Map DM_BlackForest_Winter = new Map(Gamemode.Deathmatch, 1, Event.WinterWonderland);
        public static Map DM_Castillo = new Map(Gamemode.Deathmatch, 2, Event.None);
        public static Map DM_ChateauGuillard = new Map(Gamemode.Deathmatch, 3, Event.None);
        public static Map DM_Dorado = new Map(Gamemode.Deathmatch, 4, Event.None);
        public static Map DM_Antarctica = new Map(Gamemode.Deathmatch, 5, Event.None);
        public static Map DM_Antarctica_Winter = new Map(Gamemode.Deathmatch, 6, Event.WinterWonderland);
        public static Map DM_Eichenwalde = new Map(Gamemode.Deathmatch, 7, Event.None);
        public static Map DM_Eichenwalde_Halloween = new Map(Gamemode.Deathmatch, 8, Event.HalloweenTerror);
        public static Map DM_Hanamura = new Map(Gamemode.Deathmatch, 9, Event.None);
        public static Map DM_Hanamura_Winter = new Map(Gamemode.Deathmatch, 10, Event.WinterWonderland);
        public static Map DM_Hollywood = new Map(Gamemode.Deathmatch, 11, Event.None);
        public static Map DM_Hollywood_Halloween = new Map(Gamemode.Deathmatch, 12, Event.HalloweenTerror);
        public static Map DM_HorizonLunarColony = new Map(Gamemode.Deathmatch, 13, Event.None);
        public static Map DM_Ilios_Lighthouse = new Map(Gamemode.Deathmatch, 14, Event.None);
        public static Map DM_Ilios_Ruins = new Map(Gamemode.Deathmatch, 15, Event.None);
        public static Map DM_Ilios_Well = new Map(Gamemode.Deathmatch, 16, Event.None);
        public static Map DM_KingsRow = new Map(Gamemode.Deathmatch, 17, Event.None);
        public static Map DM_KingsRow_Winter = new Map(Gamemode.Deathmatch, 18, Event.WinterWonderland);
        public static Map DM_Lijiang_ControlCenter = new Map(Gamemode.Deathmatch, 19, Event.None);
        public static Map DM_Lijiang_ControlCenter_Lunar = new Map(Gamemode.Deathmatch, 20, Event.LunarNewYear);
        public static Map DM_Lijiang_Garden = new Map(Gamemode.Deathmatch, 21, Event.None);
        public static Map DM_Lijiang_Garden_Lunar = new Map(Gamemode.Deathmatch, 22, Event.LunarNewYear);
        public static Map DM_Lijiang_NightMarket = new Map(Gamemode.Deathmatch, 23, Event.None);
        public static Map DM_Lijiang_NightMarket_Lunar = new Map(Gamemode.Deathmatch, 24, Event.LunarNewYear);
        public static Map DM_Necropolis = new Map(Gamemode.Deathmatch, 25, Event.None);
        public static Map DM_Nepal_Sanctum = new Map(Gamemode.Deathmatch, 26, Event.None);
        public static Map DM_Nepal_Shrine = new Map(Gamemode.Deathmatch, 27, Event.None);
        public static Map DM_Nepal_Village = new Map(Gamemode.Deathmatch, 28, Event.None);
        public static Map DM_Oasis_CityCenter = new Map(Gamemode.Deathmatch, 29, Event.None);
        public static Map DM_Oasis_Gardens = new Map(Gamemode.Deathmatch, 30, Event.None);
        public static Map DM_Oasis_University = new Map(Gamemode.Deathmatch, 31, Event.None);
        public static Map DM_TempleOfAnubis = new Map(Gamemode.Deathmatch, 32, Event.None);
        public static Map DM_VolskayaIndustries = new Map(Gamemode.Deathmatch, 33, Event.None);

        // Elimination
        public static Map ELIM_Ayutthaya = new Map(Gamemode.Elimination, 0, Event.None);
        public static Map ELIM_BlackForest = new Map(Gamemode.Elimination, 1, Event.None);
        public static Map ELIM_BlackForest_Winter = new Map(Gamemode.Elimination, 2, Event.WinterWonderland);
        public static Map ELIM_Castillo = new Map(Gamemode.Elimination, 3, Event.None);
        public static Map ELIM_Antarctica = new Map(Gamemode.Elimination, 4, Event.None);
        public static Map ELIM_Antarctica_Winter = new Map(Gamemode.Elimination, 5, Event.WinterWonderland);
        public static Map ELIM_Ilios_Lighthouse = new Map(Gamemode.Elimination, 6, Event.None);
        public static Map ELIM_Ilios_Ruins = new Map(Gamemode.Elimination, 7, Event.None);
        public static Map ELIM_Ilios_Well = new Map(Gamemode.Elimination, 8, Event.None);
        public static Map ELIM_Lijiang_ControlCenter = new Map(Gamemode.Elimination, 9, Event.None);
        public static Map ELIM_Lijiang_ControlCenter_Lunar = new Map(Gamemode.Elimination, 10, Event.LunarNewYear);
        public static Map ELIM_Lijiang_Garden = new Map(Gamemode.Elimination, 11, Event.None);
        public static Map ELIM_Lijiang_Garden_Lunar = new Map(Gamemode.Elimination, 12, Event.LunarNewYear);
        public static Map ELIM_Lijiang_NightMarket = new Map(Gamemode.Elimination, 13, Event.None);
        public static Map ELIM_Lijiang_NightMarket_Lunar = new Map(Gamemode.Elimination, 14, Event.LunarNewYear);
        public static Map ELIM_Necropolis = new Map(Gamemode.Elimination, 15, Event.None);
        public static Map ELIM_Nepal_Sanctum = new Map(Gamemode.Elimination, 16, Event.None);
        public static Map ELIM_Nepal_Shrine = new Map(Gamemode.Elimination, 17, Event.None);
        public static Map ELIM_Nepal_Village = new Map(Gamemode.Elimination, 18, Event.None);
        public static Map ELIM_Oasis_CityCenter = new Map(Gamemode.Elimination, 19, Event.None);
        public static Map ELIM_Oasis_Gardens = new Map(Gamemode.Elimination, 20, Event.None);
        public static Map ELIM_Oasis_University = new Map(Gamemode.Elimination, 21, Event.None);

        // Escort
        public static Map E_Dorado = new Map(Gamemode.Escort, 0, Event.None);
        public static Map E_Junkertown = new Map(Gamemode.Escort, 1, Event.None);
        public static Map E_Rialto = new Map(Gamemode.Escort, 2, Event.None);
        public static Map E_Route66 = new Map(Gamemode.Escort, 3, Event.None);
        public static Map E_Gibraltar = new Map(Gamemode.Escort, 4, Event.None);

        // Junkensteins Revenge
        public static Map JR_Adlersbrunn = new Map(Gamemode.JunkensteinsRevenge, 0, Event.HalloweenTerror);

        // Lucioball
        public static Map LB_EstasioDasRas = new Map(Gamemode.Lucioball, 0, Event.SummerGames);
        public static Map LB_SydneyHarbourArena = new Map(Gamemode.Lucioball, 1, Event.SummerGames);

        // Meis Snowball Offensive
        public static Map MSO_BlackForest_Winter = new Map(Gamemode.MeisSnowballOffensive, 0, Event.WinterWonderland);
        public static Map MSO_Antarctica_Winter = new Map(Gamemode.MeisSnowballOffensive, 1, Event.WinterWonderland);

        // Skirmish
        public static Map SKIRM_BlizzardWorld = new Map(Gamemode.Skirmish, 0, Event.None);
        public static Map SKIRM_Dorado = new Map(Gamemode.Skirmish, 1, Event.None);
        public static Map SKIRM_Eichenwalde = new Map(Gamemode.Skirmish, 2, Event.None);
        public static Map SKIRM_Eichenwalde_Halloween = new Map(Gamemode.Skirmish, 3, Event.HalloweenTerror);
        public static Map SKIRM_Hanamura = new Map(Gamemode.Skirmish, 4, Event.None);
        public static Map SKIRM_Hanamura_Winter = new Map(Gamemode.Skirmish, 5, Event.WinterWonderland);
        public static Map SKIRM_Hollywood = new Map(Gamemode.Skirmish, 6, Event.None);
        public static Map SKIRM_Hollywood_Halloween = new Map(Gamemode.Skirmish, 7, Event.HalloweenTerror);
        public static Map SKIRM_HorizonLunarColony = new Map(Gamemode.Skirmish, 8, Event.None);
        public static Map SKIRM_Ilios = new Map(Gamemode.Skirmish, 9, Event.None);
        public static Map SKIRM_Junkertown = new Map(Gamemode.Skirmish, 10, Event.None);
        public static Map SKIRM_KingsRow = new Map(Gamemode.Skirmish, 11, Event.None);
        public static Map SKIRM_KingsRow_Winter = new Map(Gamemode.Skirmish, 12, Event.WinterWonderland);
        public static Map SKIRM_Lijiang = new Map(Gamemode.Skirmish, 13, Event.None);
        public static Map SKIRM_Lijiang_Lunar = new Map(Gamemode.Skirmish, 14, Event.LunarNewYear);
        public static Map SKIRM_Nepal = new Map(Gamemode.Skirmish, 15, Event.None);
        public static Map SKIRM_Numbani = new Map(Gamemode.Skirmish, 16, Event.None);
        public static Map SKIRM_Oasis = new Map(Gamemode.Skirmish, 17, Event.None);
        public static Map SKIRM_Railto = new Map(Gamemode.Skirmish, 18, Event.None);
        public static Map SKIRM_Route66 = new Map(Gamemode.Skirmish, 19, Event.None);
        public static Map SKIRM_TempleOfAnubis = new Map(Gamemode.Skirmish, 20, Event.None);
        public static Map SKIRM_VolskayaIndustries = new Map(Gamemode.Skirmish, 21, Event.None);
        public static Map SKIRM_Gibraltar = new Map(Gamemode.Skirmish, 22, Event.None);

        // Team Deathmatch
        public static Map TDM_BlackForest = new Map(Gamemode.TeamDeathmatch, 0, Event.None);
        public static Map TDM_BlackForest_Winter = new Map(Gamemode.TeamDeathmatch, 1, Event.WinterWonderland);
        public static Map TDM_Castillo = new Map(Gamemode.TeamDeathmatch, 2, Event.None);
        public static Map TDM_ChateauGuillard = new Map(Gamemode.TeamDeathmatch, 3, Event.None);
        public static Map TDM_Dorado = new Map(Gamemode.TeamDeathmatch, 4, Event.None);
        public static Map TDM_Antarctica = new Map(Gamemode.TeamDeathmatch, 5, Event.None);
        public static Map TDM_Antarctica_Winter = new Map(Gamemode.TeamDeathmatch, 6, Event.WinterWonderland);
        public static Map TDM_Eichenwalde = new Map(Gamemode.TeamDeathmatch, 7, Event.None);
        public static Map TDM_Eichenwalde_Halloween = new Map(Gamemode.TeamDeathmatch, 8, Event.HalloweenTerror);
        public static Map TDM_Hanamura = new Map(Gamemode.TeamDeathmatch, 9, Event.None);
        public static Map TDM_Hanamura_Winter = new Map(Gamemode.TeamDeathmatch, 10, Event.WinterWonderland);
        public static Map TDM_Hollywood = new Map(Gamemode.TeamDeathmatch, 11, Event.None);
        public static Map TDM_Hollywood_Halloween = new Map(Gamemode.TeamDeathmatch, 12, Event.HalloweenTerror);
        public static Map TDM_HorizonLunarColony = new Map(Gamemode.TeamDeathmatch, 13, Event.None);
        public static Map TDM_Ilios_Lighthouse = new Map(Gamemode.TeamDeathmatch, 14, Event.None);
        public static Map TDM_Ilios_Ruins = new Map(Gamemode.TeamDeathmatch, 15, Event.None);
        public static Map TDM_Ilios_Well = new Map(Gamemode.TeamDeathmatch, 16, Event.None);
        public static Map TDM_KingsRow = new Map(Gamemode.TeamDeathmatch, 17, Event.None);
        public static Map TDM_KingsRow_Winter = new Map(Gamemode.TeamDeathmatch, 18, Event.WinterWonderland);
        public static Map TDM_Lijiang_ControlCenter = new Map(Gamemode.TeamDeathmatch, 19, Event.None);
        public static Map TDM_Lijiang_ControlCenter_Lunar = new Map(Gamemode.TeamDeathmatch, 20, Event.LunarNewYear);
        public static Map TDM_Lijiang_Garden = new Map(Gamemode.TeamDeathmatch, 21, Event.None);
        public static Map TDM_Lijiang_Garden_Lunar = new Map(Gamemode.TeamDeathmatch, 22, Event.LunarNewYear);
        public static Map TDM_Lijiang_NightMarket = new Map(Gamemode.TeamDeathmatch, 23, Event.None);
        public static Map TDM_Lijiang_NightMarket_Lunar = new Map(Gamemode.TeamDeathmatch, 24, Event.LunarNewYear);
        public static Map TDM_Necropolis = new Map(Gamemode.TeamDeathmatch, 25, Event.None);
        public static Map TDM_Nepal_Sanctum = new Map(Gamemode.TeamDeathmatch, 26, Event.None);
        public static Map TDM_Nepal_Shrine = new Map(Gamemode.TeamDeathmatch, 27, Event.None);
        public static Map TDM_Nepal_Village = new Map(Gamemode.TeamDeathmatch, 28, Event.None);
        public static Map TDM_Oasis_CityCenter = new Map(Gamemode.TeamDeathmatch, 29, Event.None);
        public static Map TDM_Oasis_Gardens = new Map(Gamemode.TeamDeathmatch, 30, Event.None);
        public static Map TDM_Oasis_University = new Map(Gamemode.TeamDeathmatch, 31, Event.None);
        public static Map TDM_TempleOfAnubis = new Map(Gamemode.TeamDeathmatch, 32, Event.None);
        public static Map TDM_VolskayaIndustries = new Map(Gamemode.TeamDeathmatch, 33, Event.None);

        // Yeti Hunter
        public static Map YH_Nepal_Village = new Map(Gamemode.YetiHunter, 0, Event.WinterWonderland);
#pragma warning restore CS1591

        /// <summary>
        /// Gamemode of the map.
        /// </summary>
        public Gamemode GameMode;
        /// <summary>
        /// ID of map in alphabetical order of other maps in the gamemode.
        /// </summary>
        public int MapID;
        /// <summary>
        /// The Overwatch event the map is on.
        /// </summary>
        public Event GameEvent;
        private Map(Gamemode gamemode, int mapID, Event gameEvent)
        {
            GameMode = gamemode;
            MapID = mapID;
            GameEvent = gameEvent;
        }
    }

    /// <summary>
    /// Overwatch's limited time events.
    /// </summary>
    public enum Event
    {
#pragma warning disable CS1591
        None,
        SummerGames,
        HalloweenTerror,
        WinterWonderland,
        LunarNewYear,
        Uprising,
        Aniversary
#pragma warning restore CS1591
    }
    /// <summary>
    /// Overwatch's gamemodes.
    /// </summary>
    public enum Gamemode
    {
#pragma warning disable CS1591
        Assault, // a
        AssaultEscort, // ae
        CaptureTheFlag, // ctf
        Control, // c
        Deathmatch, // dm
        Elimination, // elim
        Escort, // e
        JunkensteinsRevenge, // jr
        Lucioball, // lb
        MeisSnowballOffensive, // mso
        Skirmish, // skirm
        TeamDeathmatch, // tdm
        YetiHunter // yh
#pragma warning restore CS1591
    }
    /// <summary>
    /// Overwatch gamemodes that are enabled.
    /// </summary>
    public class ModesEnabled
    {
#pragma warning disable CS1591
        public bool Assault, // a
        AssaultEscort, // ae
        CaptureTheFlag, // ctf
        Control, // c
        Deathmatch, // dm
        Elimination, // elim
        Escort, // e
        JunkensteinsRevenge, // jr
        Lucioball, // lb
        MeisSnowballOffensive, // mso
        Skirmish, // skirm
        TeamDeathmatch, // tdm
        YetiHunter; // yh
#pragma warning restore CS1591
    }
}
