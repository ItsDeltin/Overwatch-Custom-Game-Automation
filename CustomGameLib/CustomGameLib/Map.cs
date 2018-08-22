﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Reflection;
using System.Windows.Forms;
using System.Drawing;

namespace Deltin.CustomGameAutomation
{
    partial class CustomGame
    {
        /// <summary>
        /// Modes enabled in custom games.
        /// </summary>
        public ModesEnabled ModesEnabled = null;
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
        /// Toggles maps in Overwatch.
        /// </summary>
        /// <param name="ta">Determines if all maps should be enabled, disabled or neither before toggling.</param>
        /// <param name="maps">Maps that should be toggled.</param>
        /// <exception cref="ArgumentNullException">Thrown if <see cref="ModesEnabled"/> is null.</exception>
        /// <remarks>
        /// <see cref="ModesEnabled"/> must be set to use this method. <see cref="CurrentOverwatchEvent"/> must be set if a seasonal Overwatch event is on.
        /// <include file="docs.xml" path="doc/getMaps" />
        /// </remarks>
        /// <example>
        /// The code below will disable all maps but Hanamura, Gibraltar, and Ilios.
        /// <code>
        /// using Deltin.CustomGameAutomation;
        /// 
        /// public class ToggleMapExample
        /// {
        ///     public static void SetEnabledMaps(CustomGame cg) 
        ///     {
        ///         cg.ModesEnabled = new ModesEnabled()
        ///         {
        ///             Assault = true,
        ///             AssaultEscort = true,
        ///             Control = true,
        ///             Escort = true
        ///         }
        ///         cg.CurrentOverwatchEvent = Event.None;
        ///         cg.ToggleMap(ToggleAction.DisableAll, Map.A_Hanamura, E_Gibraltar, C_Ilios);
        ///     }
        /// }
        /// </code>
        /// </example>
        /// <seealso cref="Map"/>
        /// <seealso cref="ToggleMap(ModesEnabled, Event, ToggleAction, Map[])"/>
        public void ToggleMap(ToggleAction ta, params Map[] maps)
        {
            if (ModesEnabled == null)
                throw new ArgumentNullException("CustomGame.ModesEnabled", "The field CustomGame.ModesEnabled must be set in order to use ToggleMap.");
            ToggleMap(ModesEnabled, CurrentOverwatchEvent, ta, maps);
        }

        /// <summary>
        /// Toggles maps in Overwatch.
        /// </summary>
        /// <param name="modesEnabled">The modes enabled in the overwatch game.</param>
        /// <param name="currentOverwatchEvent">The current Overwatch event.</param>
        /// <param name="ta">Determines if all maps should be enabled, disabled or neither before toggling.</param>
        /// <param name="maps">Maps that should be toggled.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="modesEnabled"/> is null.</exception>
        /// <remarks>
        /// <include file="docs.xml" path="doc/getMaps" />
        /// </remarks>
        /// <example>
        /// The code below will disable all maps but Hanamura, Gibraltar, and Ilios.
        /// <code>
        /// using Deltin.CustomGameAutomation;
        /// 
        /// public class ToggleMapExample
        /// {
        ///     public static void SetEnabledMaps(CustomGame cg) 
        ///     {
        ///         cg.ToggleMap(
        ///             new ModesEnabled() 
        ///             {
        ///                 Assault = true,
        ///                 AssaultEscort = true,
        ///                 Control = true,
        ///                 Escort = true
        ///             },
        ///             Event.None,
        ///             ToggleAction.DisableAll, 
        ///             Map.A_Hanamura, E_Gibraltar, C_Ilios);
        ///     }
        /// }
        /// </code>
        /// </example>
        /// <seealso cref="Map"/>
        /// <seealso cref="ToggleMap(ToggleAction, Map[])"/>
        public void ToggleMap(ModesEnabled modesEnabled, Event currentOverwatchEvent, ToggleAction ta, params Map[] maps)
        {
            if (modesEnabled == null)
                throw new ArgumentNullException("modesEnabled");

            GoToSettings();

            LeftClick(Points.SETTINGS_MAPS, 1000); // Clicks "Maps" button (SETTINGS/MAPS/)

            // Click Disable All or Enable All in custom games if ta doesnt equal ToggleAction.None.
            if (ta == ToggleAction.DisableAll)
                LeftClick(Points.SETTINGS_MAPS_DISABLE_ALL, 250);
            else if (ta == ToggleAction.EnableAll)
                LeftClick(Points.SETTINGS_MAPS_ENABLE_ALL, 250);

            // Get the modes enabled state in a bool in alphabetical order.
            bool[] enabledModes = new bool[]
            {
                modesEnabled.Assault,
                modesEnabled.AssaultEscort,
                modesEnabled.CaptureTheFlag,
                modesEnabled.Control,
                modesEnabled.Deathmatch,
                modesEnabled.Elimination,
                modesEnabled.Escort,
                modesEnabled.JunkensteinsRevenge,
                modesEnabled.Lucioball,
                modesEnabled.MeisSnowballOffensive,
                modesEnabled.Skirmish,
                modesEnabled.TeamDeathmatch,
                modesEnabled.YetiHunter
            };

            List<int> selectMap = new List<int>();
            int mapcount = 0;
            // For each enabled mode...
            for (int i = 0; i < enabledModes.Length; i++)
                if (enabledModes[i])
                {
                    Gamemode emi = (Gamemode)i; //enabledmodesindex
                    List<Map> allowedmaps = Map.GetMapsInGamemode(emi, currentOverwatchEvent).ToList();
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

            // Toggle maps
            for (int i = 0; i < mapcount; i++)
            {
                for (int mi = 0; mi < selectMap.Count; mi++)
                    if (selectMap[mi] == i)
                    {
                        KeyPress(Keys.Space);
                        Thread.Sleep(1);
                    }
                KeyPress(Keys.Down);
                Thread.Sleep(1);
            }

            GoBack(2, 0);
        }

        internal Point GetModeLocation(Gamemode mode, Event owevent)
        {
            List<Gamemode> enabledGamemodes = new List<Gamemode>();

            Gamemode[] gamemodeOrder = new Gamemode[]
            {
                // Core gamemodes
                Gamemode.Assault,
                Gamemode.AssaultEscort,
                Gamemode.Control,
                Gamemode.Escort,

                // Followed by arcade gamemodes
                Gamemode.CaptureTheFlag,
                Gamemode.Deathmatch,
                Gamemode.Elimination,
                Gamemode.JunkensteinsRevenge, // < Fix later when Junkensteins revenge is live, the position is a guess
                Gamemode.Lucioball,           // < Fix later when lucioball is live, the position is a guess
                Gamemode.MeisSnowballOffensive, // < Fix later when winter wonderland is live, the position is a guess
                Gamemode.TeamDeathmatch,
                Gamemode.YetiHunter, // < Fix later when winter wonderland is live, the position is a guess

                // Followed by skirmish
                Gamemode.Skirmish
            };

            Event[] gamemodeEvents = new Event[]
            {
                Event.None,             // Assault
                Event.None,             // AssaultEscort
                Event.None,             // Control
                Event.None,             // Escort

                Event.None,             // CaptureTheFlag
                Event.None,             // Deathmatch
                Event.None,             // Elimination
                Event.HalloweenTerror,  // JunkensteinsRevenge
                Event.SummerGames,      // Lucioball
                Event.WinterWonderland, // MeisSnowballOffensive
                Event.None,             // TeamDeathmatch
                Event.WinterWonderland, // YetiHunter

                Event.None,             // Skirmish
            };

            for (int i = 0; i < gamemodeEvents.Length; i++)
                if (gamemodeEvents[i] == Event.None || gamemodeEvents[i] == owevent)
                    enabledGamemodes.Add(gamemodeOrder[i]);

            int eventModeIndex = Array.IndexOf(enabledGamemodes.ToArray(), mode) + 1;

            if (eventModeIndex == 0)
                return Point.Empty;

            int y = 129;
            int x = -1;

            int[] columns = new int[] { 83, 227, 370, 515 };
            int rowHeight = 107;
            
            while (true)
            {
                if (eventModeIndex < 4)
                {
                    x = columns[eventModeIndex];
                    break;
                }
                else
                {
                    y += rowHeight;
                    eventModeIndex -= 4;
                }
            }

            return new Point(x, y);
        }
    }

    /// <summary>
    /// Maps in Overwatch.
    /// </summary>
    public class Map : IEquatable<Map>
    {
        // This is all possible map variants that can be selected in custom games. All static fields must be a Map value.
        #region Maps
#pragma warning disable CS1591
        // Assault
        public static Map A_Hanamura                       = new Map(Gamemode.Assault,               "A_Hanamura",                       Event.None);
        public static Map A_Hanamura_Winter                = new Map(Gamemode.Assault,               "A_Hanamura_Winter",                Event.WinterWonderland);
        public static Map A_HorizonLunarColony             = new Map(Gamemode.Assault,               "A_HorizonLunarColony",             Event.None);
        public static Map A_TempleOfAnubis                 = new Map(Gamemode.Assault,               "A_TempleOfAnubis",                 Event.None);
        public static Map A_VolskayaIndustries             = new Map(Gamemode.Assault,               "A_VolskayaIndustries",             Event.None);

        // AssaultEscort
        public static Map AE_BlizzardWorld                 = new Map(Gamemode.AssaultEscort,         "AE_BlizzardWorld",                 Event.None);
        public static Map AE_Eichenwalde                   = new Map(Gamemode.AssaultEscort,         "AE_Eichenwalde",                   Event.None);
        public static Map AE_Eichenwalde_Halloween         = new Map(Gamemode.AssaultEscort,         "AE_Eichenwalde_Halloween",         Event.HalloweenTerror);
        public static Map AE_Hollywood                     = new Map(Gamemode.AssaultEscort,         "AE_Hollywood",                     Event.None);
        public static Map AE_Hollywood_Halloween           = new Map(Gamemode.AssaultEscort,         "AE_Hollywood_Halloween",           Event.HalloweenTerror);
        public static Map AE_KingsRow                      = new Map(Gamemode.AssaultEscort,         "AE_KingsRow",                      Event.None);
        public static Map AE_KingsRow_Winter               = new Map(Gamemode.AssaultEscort,         "AE_KingsRow_Winter",               Event.WinterWonderland);
        public static Map AE_Numbani                       = new Map(Gamemode.AssaultEscort,         "AE_Numbani",                       Event.None);

        // Capture The Flag
        public static Map CTF_Ayutthaya                    = new Map(Gamemode.CaptureTheFlag,        "CTF_Ayutthaya",                    Event.None);
        public static Map CTF_Ilios_Lighthouse             = new Map(Gamemode.CaptureTheFlag,        "CTF_Ilios_Lighthouse",             Event.None);
        public static Map CTF_Ilios_Ruins                  = new Map(Gamemode.CaptureTheFlag,        "CTF_Ilios_Ruins",                  Event.None);
        public static Map CTF_Ilios_Well                   = new Map(Gamemode.CaptureTheFlag,        "CTF_Ilios_Well",                   Event.None);
        public static Map CTF_Lijiang_ControlCenter        = new Map(Gamemode.CaptureTheFlag,        "CTF_Lijiang_ControlCenter",        Event.None);
        public static Map CTF_Lijiang_ControlCenter_Lunar  = new Map(Gamemode.CaptureTheFlag,        "CTF_Lijiang_ControlCenter_Lunar",  Event.LunarNewYear);
        public static Map CTF_Lijiang_Garden               = new Map(Gamemode.CaptureTheFlag,        "CTF_Lijiang_Garden",               Event.None);
        public static Map CTF_Lijiang_Garden_Lunar         = new Map(Gamemode.CaptureTheFlag,        "CTF_Lijiang_Garden_Lunar",         Event.LunarNewYear);
        public static Map CTF_Lijiang_NightMarket          = new Map(Gamemode.CaptureTheFlag,        "CTF_Lijiang_NightMarket",          Event.None);
        public static Map CTF_Lijiang_NightMarket_Lunar    = new Map(Gamemode.CaptureTheFlag,        "CTF_Lijiang_NightMarket_Lunar",    Event.LunarNewYear);
        public static Map CTF_Nepal_Sanctum                = new Map(Gamemode.CaptureTheFlag,        "CTF_Nepal_Sanctum",                Event.None);
        public static Map CTF_Nepal_Shrine                 = new Map(Gamemode.CaptureTheFlag,        "CTF_Nepal_Shrine",                 Event.None);
        public static Map CTF_Nepal_Village                = new Map(Gamemode.CaptureTheFlag,        "CTF_Nepal_Village",                Event.None);
        public static Map CTF_Oasis_CityCenter             = new Map(Gamemode.CaptureTheFlag,        "CTF_Oasis_CityCenter",             Event.None);
        public static Map CTF_Oasis_Gardens                = new Map(Gamemode.CaptureTheFlag,        "CTF_Oasis_Gardens",                Event.None);
        public static Map CTF_Oasis_University             = new Map(Gamemode.CaptureTheFlag,        "CTF_Oasis_University",             Event.None);

        // Control
        public static Map C_Ilios                          = new Map(Gamemode.Control,               "C_Ilios",                          Event.None);
        public static Map C_Lijiang                        = new Map(Gamemode.Control,               "C_Lijiang",                        Event.None);
        public static Map C_Lijiang_Lunar                  = new Map(Gamemode.Control,               "C_Lijiang_Lunar",                  Event.LunarNewYear);
        public static Map C_Nepal                          = new Map(Gamemode.Control,               "C_Nepal",                          Event.None);
        public static Map C_Oasis                          = new Map(Gamemode.Control,               "C_Oasis",                          Event.None);

        // Deathmatch
        public static Map DM_BlackForest                   = new Map(Gamemode.Deathmatch,            "DM_BlackForest",                   Event.None);
        public static Map DM_BlackForest_Winter            = new Map(Gamemode.Deathmatch,            "DM_BlackForest_Winter",            Event.WinterWonderland);
        public static Map DM_Castillo                      = new Map(Gamemode.Deathmatch,            "DM_Castillo",                      Event.None);
        public static Map DM_ChateauGuillard               = new Map(Gamemode.Deathmatch,            "DM_ChateauGuillard",               Event.None);
        public static Map DM_Dorado                        = new Map(Gamemode.Deathmatch,            "DM_Dorado",                        Event.None);
        public static Map DM_Antarctica                    = new Map(Gamemode.Deathmatch,            "DM_Antarctica",                    Event.None);
        public static Map DM_Antarctica_Winter             = new Map(Gamemode.Deathmatch,            "DM_Antarctica_Winter",             Event.WinterWonderland);
        public static Map DM_Eichenwalde                   = new Map(Gamemode.Deathmatch,            "DM_Eichenwalde",                   Event.None);
        public static Map DM_Eichenwalde_Halloween         = new Map(Gamemode.Deathmatch,            "DM_Eichenwalde_Halloween",         Event.HalloweenTerror);
        public static Map DM_Hanamura                      = new Map(Gamemode.Deathmatch,            "DM_Hanamura",                      Event.None);
        public static Map DM_Hanamura_Winter               = new Map(Gamemode.Deathmatch,            "DM_Hanamura_Winter",               Event.WinterWonderland);
        public static Map DM_Hollywood                     = new Map(Gamemode.Deathmatch,            "DM_Hollywood",                     Event.None);
        public static Map DM_Hollywood_Halloween           = new Map(Gamemode.Deathmatch,            "DM_Hollywood_Halloween",           Event.HalloweenTerror);
        public static Map DM_HorizonLunarColony            = new Map(Gamemode.Deathmatch,            "DM_HorizonLunarColony",            Event.None);
        public static Map DM_Ilios_Lighthouse              = new Map(Gamemode.Deathmatch,            "DM_Ilios_Lighthouse",              Event.None);
        public static Map DM_Ilios_Ruins                   = new Map(Gamemode.Deathmatch,            "DM_Ilios_Ruins",                   Event.None);
        public static Map DM_Ilios_Well                    = new Map(Gamemode.Deathmatch,            "DM_Ilios_Well",                    Event.None);
        public static Map DM_KingsRow                      = new Map(Gamemode.Deathmatch,            "DM_KingsRow",                      Event.None);
        public static Map DM_KingsRow_Winter               = new Map(Gamemode.Deathmatch,            "DM_KingsRow_Winter",               Event.WinterWonderland);
        public static Map DM_Lijiang_ControlCenter         = new Map(Gamemode.Deathmatch,            "DM_Lijiang_ControlCenter",         Event.None);
        public static Map DM_Lijiang_ControlCenter_Lunar   = new Map(Gamemode.Deathmatch,            "DM_Lijiang_ControlCenter_Lunar",   Event.LunarNewYear);
        public static Map DM_Lijiang_Garden                = new Map(Gamemode.Deathmatch,            "DM_Lijiang_Garden",                Event.None);
        public static Map DM_Lijiang_Garden_Lunar          = new Map(Gamemode.Deathmatch,            "DM_Lijiang_Garden_Lunar",          Event.LunarNewYear);
        public static Map DM_Lijiang_NightMarket           = new Map(Gamemode.Deathmatch,            "DM_Lijiang_NightMarket",           Event.None);
        public static Map DM_Lijiang_NightMarket_Lunar     = new Map(Gamemode.Deathmatch,            "DM_Lijiang_NightMarket_Lunar",     Event.LunarNewYear);
        public static Map DM_Necropolis                    = new Map(Gamemode.Deathmatch,            "DM_Necropolis",                    Event.None);
        public static Map DM_Nepal_Sanctum                 = new Map(Gamemode.Deathmatch,            "DM_Nepal_Sanctum",                 Event.None);
        public static Map DM_Nepal_Shrine                  = new Map(Gamemode.Deathmatch,            "DM_Nepal_Shrine",                  Event.None);
        public static Map DM_Nepal_Village                 = new Map(Gamemode.Deathmatch,            "DM_Nepal_Village",                 Event.None);
        public static Map DM_Oasis_CityCenter              = new Map(Gamemode.Deathmatch,            "DM_Oasis_CityCenter",              Event.None);
        public static Map DM_Oasis_Gardens                 = new Map(Gamemode.Deathmatch,            "DM_Oasis_Gardens",                 Event.None);
        public static Map DM_Oasis_University              = new Map(Gamemode.Deathmatch,            "DM_Oasis_University",              Event.None);
        public static Map DM_Petra                         = new Map(Gamemode.Deathmatch,            "DM_Petra",                         Event.None);
        public static Map DM_TempleOfAnubis                = new Map(Gamemode.Deathmatch,            "DM_TempleOfAnubis",                Event.None);
        public static Map DM_VolskayaIndustries            = new Map(Gamemode.Deathmatch,            "DM_VolskayaIndustries",            Event.None);

        // Elimination
        public static Map ELIM_Ayutthaya                   = new Map(Gamemode.Elimination,           "ELIM_Ayutthaya",                   Event.None);
        public static Map ELIM_BlackForest                 = new Map(Gamemode.Elimination,           "ELIM_BlackForest",                 Event.None);
        public static Map ELIM_BlackForest_Winter          = new Map(Gamemode.Elimination,           "ELIM_BlackForest_Winter",          Event.WinterWonderland);
        public static Map ELIM_Castillo                    = new Map(Gamemode.Elimination,           "ELIM_Castillo",                    Event.None);
        public static Map ELIM_Antarctica                  = new Map(Gamemode.Elimination,           "ELIM_Antarctica",                  Event.None);
        public static Map ELIM_Antarctica_Winter           = new Map(Gamemode.Elimination,           "ELIM_Antarctica_Winter",           Event.WinterWonderland);
        public static Map ELIM_Ilios_Lighthouse            = new Map(Gamemode.Elimination,           "ELIM_Ilios_Lighthouse",            Event.None);
        public static Map ELIM_Ilios_Ruins                 = new Map(Gamemode.Elimination,           "ELIM_Ilios_Ruins",                 Event.None);
        public static Map ELIM_Ilios_Well                  = new Map(Gamemode.Elimination,           "ELIM_Ilios_Well",                  Event.None);
        public static Map ELIM_Lijiang_ControlCenter       = new Map(Gamemode.Elimination,           "ELIM_Lijiang_ControlCenter",       Event.None);
        public static Map ELIM_Lijiang_ControlCenter_Lunar = new Map(Gamemode.Elimination,           "ELIM_Lijiang_ControlCenter_Lunar", Event.LunarNewYear);
        public static Map ELIM_Lijiang_Garden              = new Map(Gamemode.Elimination,           "ELIM_Lijiang_Garden",              Event.None);
        public static Map ELIM_Lijiang_Garden_Lunar        = new Map(Gamemode.Elimination,           "ELIM_Lijiang_Garden_Lunar",        Event.LunarNewYear);
        public static Map ELIM_Lijiang_NightMarket         = new Map(Gamemode.Elimination,           "ELIM_Lijiang_NightMarket",         Event.None);
        public static Map ELIM_Lijiang_NightMarket_Lunar   = new Map(Gamemode.Elimination,           "ELIM_Lijiang_NightMarket_Lunar",   Event.LunarNewYear);
        public static Map ELIM_Necropolis                  = new Map(Gamemode.Elimination,           "ELIM_Necropolis",                  Event.None);
        public static Map ELIM_Nepal_Sanctum               = new Map(Gamemode.Elimination,           "ELIM_Nepal_Sanctum",               Event.None);
        public static Map ELIM_Nepal_Shrine                = new Map(Gamemode.Elimination,           "ELIM_Nepal_Shrine",                Event.None);
        public static Map ELIM_Nepal_Village               = new Map(Gamemode.Elimination,           "ELIM_Nepal_Village",               Event.None);
        public static Map ELIM_Oasis_CityCenter            = new Map(Gamemode.Elimination,           "ELIM_Oasis_CityCenter",            Event.None);
        public static Map ELIM_Oasis_Gardens               = new Map(Gamemode.Elimination,           "ELIM_Oasis_Gardens",               Event.None);
        public static Map ELIM_Oasis_University            = new Map(Gamemode.Elimination,           "ELIM_Oasis_University",            Event.None);

        // Escort
        public static Map E_Dorado                         = new Map(Gamemode.Escort,                "E_Dorado",                         Event.None);
        public static Map E_Junkertown                     = new Map(Gamemode.Escort,                "E_Junkertown",                     Event.None);
        public static Map E_Rialto                         = new Map(Gamemode.Escort,                "E_Rialto",                         Event.None);
        public static Map E_Route66                        = new Map(Gamemode.Escort,                "E_Route66",                        Event.None);
        public static Map E_Gibraltar                      = new Map(Gamemode.Escort,                "E_Gibraltar",                      Event.None);

        // Junkensteins Revenge
        public static Map JR_Adlersbrunn                   = new Map(Gamemode.JunkensteinsRevenge,   "JR_Adlersbrunn",                   Event.HalloweenTerror);

        // Lucioball
        public static Map LB_EstasioDasRas                 = new Map(Gamemode.Lucioball,             "LB_EstasioDasRas",                 Event.SummerGames);
        public static Map LB_SydneyHarbourArena            = new Map(Gamemode.Lucioball,             "LB_SydneyHarbourArena",            Event.SummerGames);

        // Meis Snowball Offensive
        public static Map MSO_BlackForest_Winter           = new Map(Gamemode.MeisSnowballOffensive, "MSO_BlackForest_Winter",           Event.WinterWonderland);
        public static Map MSO_Antarctica_Winter            = new Map(Gamemode.MeisSnowballOffensive, "MSO_Antarctica_Winter",            Event.WinterWonderland);

        // Skirmish
        public static Map SKIRM_BlizzardWorld              = new Map(Gamemode.Skirmish,              "SKIRM_BlizzardWorld",              Event.None);
        public static Map SKIRM_Dorado                     = new Map(Gamemode.Skirmish,              "SKIRM_Dorado",                     Event.None);
        public static Map SKIRM_Eichenwalde                = new Map(Gamemode.Skirmish,              "SKIRM_Eichenwalde",                Event.None);
        public static Map SKIRM_Eichenwalde_Halloween      = new Map(Gamemode.Skirmish,              "SKIRM_Eichenwalde_Halloween",      Event.HalloweenTerror);
        public static Map SKIRM_Hanamura                   = new Map(Gamemode.Skirmish,              "SKIRM_Hanamura",                   Event.None);
        public static Map SKIRM_Hanamura_Winter            = new Map(Gamemode.Skirmish,              "SKIRM_Hanamura_Winter",            Event.WinterWonderland);
        public static Map SKIRM_Hollywood                  = new Map(Gamemode.Skirmish,              "SKIRM_Hollywood",                  Event.None);
        public static Map SKIRM_Hollywood_Halloween        = new Map(Gamemode.Skirmish,              "SKIRM_Hollywood_Halloween",        Event.HalloweenTerror);
        public static Map SKIRM_HorizonLunarColony         = new Map(Gamemode.Skirmish,              "SKIRM_HorizonLunarColony",         Event.None);
        public static Map SKIRM_Ilios                      = new Map(Gamemode.Skirmish,              "SKIRM_Ilios",                      Event.None);
        public static Map SKIRM_Junkertown                 = new Map(Gamemode.Skirmish,              "SKIRM_Junkertown",                 Event.None);
        public static Map SKIRM_KingsRow                   = new Map(Gamemode.Skirmish,              "SKIRM_KingsRow",                   Event.None);
        public static Map SKIRM_KingsRow_Winter            = new Map(Gamemode.Skirmish,              "SKIRM_KingsRow_Winter",            Event.WinterWonderland);
        public static Map SKIRM_Lijiang                    = new Map(Gamemode.Skirmish,              "SKIRM_Lijiang",                    Event.None);
        public static Map SKIRM_Lijiang_Lunar              = new Map(Gamemode.Skirmish,              "SKIRM_Lijiang_Lunar",              Event.LunarNewYear);
        public static Map SKIRM_Nepal                      = new Map(Gamemode.Skirmish,              "SKIRM_Nepal",                      Event.None);
        public static Map SKIRM_Numbani                    = new Map(Gamemode.Skirmish,              "SKIRM_Numbani",                    Event.None);
        public static Map SKIRM_Oasis                      = new Map(Gamemode.Skirmish,              "SKIRM_Oasis",                      Event.None);
        public static Map SKIRM_Railto                     = new Map(Gamemode.Skirmish,              "SKIRM_Railto",                     Event.None);
        public static Map SKIRM_Route66                    = new Map(Gamemode.Skirmish,              "SKIRM_Route66",                    Event.None);
        public static Map SKIRM_TempleOfAnubis             = new Map(Gamemode.Skirmish,              "SKIRM_TempleOfAnubis",             Event.None);
        public static Map SKIRM_VolskayaIndustries         = new Map(Gamemode.Skirmish,              "SKIRM_VolskayaIndustries",         Event.None);
        public static Map SKIRM_Gibraltar                  = new Map(Gamemode.Skirmish,              "SKIRM_Gibraltar",                  Event.None);

        // Team Deathmatch
        public static Map TDM_BlackForest                  = new Map(Gamemode.TeamDeathmatch,        "TDM_BlackForest",                  Event.None);
        public static Map TDM_BlackForest_Winter           = new Map(Gamemode.TeamDeathmatch,        "TDM_BlackForest_Winter",           Event.WinterWonderland);
        public static Map TDM_Castillo                     = new Map(Gamemode.TeamDeathmatch,        "TDM_Castillo",                     Event.None);
        public static Map TDM_ChateauGuillard              = new Map(Gamemode.TeamDeathmatch,        "TDM_ChateauGuillard",              Event.None);
        public static Map TDM_Dorado                       = new Map(Gamemode.TeamDeathmatch,        "TDM_Dorado",                       Event.None);
        public static Map TDM_Antarctica                   = new Map(Gamemode.TeamDeathmatch,        "TDM_Antarctica",                   Event.None);
        public static Map TDM_Antarctica_Winter            = new Map(Gamemode.TeamDeathmatch,        "TDM_Antarctica_Winter",            Event.WinterWonderland);
        public static Map TDM_Eichenwalde                  = new Map(Gamemode.TeamDeathmatch,        "TDM_Eichenwalde",                  Event.None);
        public static Map TDM_Eichenwalde_Halloween        = new Map(Gamemode.TeamDeathmatch,        "TDM_Eichenwalde_Halloween",        Event.HalloweenTerror);
        public static Map TDM_Hanamura                     = new Map(Gamemode.TeamDeathmatch,        "TDM_Hanamura",                     Event.None);
        public static Map TDM_Hanamura_Winter              = new Map(Gamemode.TeamDeathmatch,        "TDM_Hanamura_Winter",              Event.WinterWonderland);
        public static Map TDM_Hollywood                    = new Map(Gamemode.TeamDeathmatch,        "TDM_Hollywood",                    Event.None);
        public static Map TDM_Hollywood_Halloween          = new Map(Gamemode.TeamDeathmatch,        "TDM_Hollywood_Halloween",          Event.HalloweenTerror);
        public static Map TDM_HorizonLunarColony           = new Map(Gamemode.TeamDeathmatch,        "TDM_HorizonLunarColony",           Event.None);
        public static Map TDM_Ilios_Lighthouse             = new Map(Gamemode.TeamDeathmatch,        "TDM_Ilios_Lighthouse",             Event.None);
        public static Map TDM_Ilios_Ruins                  = new Map(Gamemode.TeamDeathmatch,        "TDM_Ilios_Ruins",                  Event.None);
        public static Map TDM_Ilios_Well                   = new Map(Gamemode.TeamDeathmatch,        "TDM_Ilios_Well",                   Event.None);
        public static Map TDM_KingsRow                     = new Map(Gamemode.TeamDeathmatch,        "TDM_KingsRow",                     Event.None);
        public static Map TDM_KingsRow_Winter              = new Map(Gamemode.TeamDeathmatch,        "TDM_KingsRow_Winter",              Event.WinterWonderland);
        public static Map TDM_Lijiang_ControlCenter        = new Map(Gamemode.TeamDeathmatch,        "TDM_Lijiang_ControlCenter",        Event.None);
        public static Map TDM_Lijiang_ControlCenter_Lunar  = new Map(Gamemode.TeamDeathmatch,        "TDM_Lijiang_ControlCenter_Lunar",  Event.LunarNewYear);
        public static Map TDM_Lijiang_Garden               = new Map(Gamemode.TeamDeathmatch,        "TDM_Lijiang_Garden",               Event.None);
        public static Map TDM_Lijiang_Garden_Lunar         = new Map(Gamemode.TeamDeathmatch,        "TDM_Lijiang_Garden_Lunar",         Event.LunarNewYear);
        public static Map TDM_Lijiang_NightMarket          = new Map(Gamemode.TeamDeathmatch,        "TDM_Lijiang_NightMarket",          Event.None);
        public static Map TDM_Lijiang_NightMarket_Lunar    = new Map(Gamemode.TeamDeathmatch,        "TDM_Lijiang_NightMarket_Lunar",    Event.LunarNewYear);
        public static Map TDM_Necropolis                   = new Map(Gamemode.TeamDeathmatch,        "TDM_Necropolis",                   Event.None);
        public static Map TDM_Nepal_Sanctum                = new Map(Gamemode.TeamDeathmatch,        "TDM_Nepal_Sanctum",                Event.None);
        public static Map TDM_Nepal_Shrine                 = new Map(Gamemode.TeamDeathmatch,        "TDM_Nepal_Shrine",                 Event.None);
        public static Map TDM_Nepal_Village                = new Map(Gamemode.TeamDeathmatch,        "TDM_Nepal_Village",                Event.None);
        public static Map TDM_Oasis_CityCenter             = new Map(Gamemode.TeamDeathmatch,        "TDM_Oasis_CityCenter",             Event.None);
        public static Map TDM_Oasis_Gardens                = new Map(Gamemode.TeamDeathmatch,        "TDM_Oasis_Gardens",                Event.None);
        public static Map TDM_Oasis_University             = new Map(Gamemode.TeamDeathmatch,        "TDM_Oasis_University",             Event.None);
        public static Map TDM_Petra                        = new Map(Gamemode.TeamDeathmatch,        "TDM_Petra",                        Event.None);
        public static Map TDM_TempleOfAnubis               = new Map(Gamemode.TeamDeathmatch,        "TDM_TempleOfAnubis",               Event.None);
        public static Map TDM_VolskayaIndustries           = new Map(Gamemode.TeamDeathmatch,        "TDM_VolskayaIndustries",           Event.None);

        // Yeti Hunter
        public static Map YH_Nepal_Village                 = new Map(Gamemode.YetiHunter,            "YH_Nepal_Village",                 Event.WinterWonderland);
#pragma warning restore CS1591
        #endregion

        /// <summary>
        /// Gamemode of the map.
        /// </summary>
        public Gamemode GameMode;
        /// <summary>
        /// Name of the map.
        /// </summary>
        public string MapName;
        /// <summary>
        /// The Overwatch event the map is on.
        /// </summary>
        public Event GameEvent;
        private Map(Gamemode gamemode, string mapName, Event gameEvent)
        {
            GameMode = gamemode;
            MapName = mapName;
            GameEvent = gameEvent;
        }

        public bool Equals(Map other)
        {
            return MapName == other.MapName 
                && GameMode == other.GameMode 
                && GameEvent == other.GameEvent;
        }

        /// <summary>
        /// Gets map ID from map name.
        /// </summary>
        /// <param name="map">Map name.</param>
        /// <returns>Returns the map ID.</returns>
        public static Map MapIDFromName(string map)
        {
            var maps = GetMaps();
            for (int i = 0; i < maps.Length; i++)
                if (maps[i].MapName.ToLower() == map.ToLower())
                    return maps[i];
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
        /// <summary>
        /// Gets all maps.
        /// </summary>
        /// <returns>Returns all maps.</returns>
        public static Map[] GetMaps()
        {
            return GetMapFieldInfo().Select(v => (Map)v.GetValue(null)).ToArray();
        }
        /// <summary>
        /// Gets all maps in a gamemode.
        /// </summary>
        /// <param name="gamemode">The gamemode to get the maps from.</param>
        /// <param name="owEvent">Filter by Overwatch event.</param>
        /// <returns>An array of maps in the <paramref name="gamemode"/>.</returns>
        public static Map[] GetMapsInGamemode(Gamemode gamemode, Event owEvent = Event.None)
        {
            return GetMaps().Where(v => (v.GameEvent == Event.None || v.GameEvent == owEvent) && v.GameMode == gamemode).ToArray();
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
