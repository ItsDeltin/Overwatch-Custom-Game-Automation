using System;
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
        public Gamemode ModesEnabled = Gamemode.Assault | Gamemode.AssaultEscort | Gamemode.Control | Gamemode.Escort;
        /// <summary>
        /// The current event occuring in Overwatch.
        /// </summary>
        public OWEvent CurrentEvent = OWEvent.None;
        /// <summary>
        /// Gets the current Overwatch event. This compares the current date with past event's start and end times, so specific time may be a little off.
        /// </summary>
        /// <returns>The current Overwatch event as the Event enum.</returns>
        public OWEvent GetCurrentEvent()
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
                return OWEvent.SummerGames;

            if (currentdate >= htStart && currentdate <= htEnd)
                return OWEvent.HalloweenTerror;

            if (currentdate >= wwStart && currentdate <= wwEnd)
                return OWEvent.WinterWonderland;

            if (currentdate >= lnyStart && currentdate <= lnyEnd)
                return OWEvent.LunarNewYear;

            if (currentdate >= uStart && currentdate <= uEnd)
                return OWEvent.Uprising;

            if (currentdate >= aStart && currentdate <= aEnd)
                return OWEvent.Aniversary;

            return OWEvent.None;
        }

        /// <summary>
        /// Toggles maps in Overwatch.
        /// </summary>
        /// <param name="ta">Determines if all maps should be enabled, disabled or neither before toggling.</param>
        /// <param name="maps">Maps that should be toggled.</param>
        /// <exception cref="ArgumentNullException">Thrown if <see cref="ModesEnabled"/> is null.</exception>
        /// <remarks>
        /// <see cref="ModesEnabled"/> must be set to use this method. <see cref="CurrentEvent"/> must be set if a seasonal Overwatch event is on.
        /// <include file="docs.xml" path="doc/getMaps" />
        /// </remarks>
        /// <include file='docs.xml' path='doc/ToggleMap1/example'></include>
        /// <seealso cref="Map"/>
        /// <seealso cref="ToggleMap(Gamemode, OWEvent, ToggleAction, Map[])"/>
        public void ToggleMap(ToggleAction ta, params Map[] maps)
        {
            ToggleMap(ModesEnabled, CurrentEvent, ta, maps);
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
        /// <include file='docs.xml' path='doc/ToggleMap2/example'></include>
        /// <seealso cref="Map"/>
        /// <seealso cref="ToggleMap(ToggleAction, Map[])"/>
        public void ToggleMap(Gamemode modesEnabled, OWEvent currentOverwatchEvent, ToggleAction ta, params Map[] maps)
        {
            using (LockHandler.Interactive)
            {
                int waitTime = 1;

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
                    modesEnabled.HasFlag(Gamemode.Assault),
                    modesEnabled.HasFlag(Gamemode.AssaultEscort),
                    modesEnabled.HasFlag(Gamemode.CaptureTheFlag),
                    modesEnabled.HasFlag(Gamemode.Control),
                    modesEnabled.HasFlag(Gamemode.Deathmatch),
                    modesEnabled.HasFlag(Gamemode.Elimination),
                    modesEnabled.HasFlag(Gamemode.Escort),
                    modesEnabled.HasFlag(Gamemode.JunkensteinsRevenge),
                    modesEnabled.HasFlag(Gamemode.Lucioball),
                    modesEnabled.HasFlag(Gamemode.MeisSnowballOffensive),
                    modesEnabled.HasFlag(Gamemode.Skirmish),
                    modesEnabled.HasFlag(Gamemode.TeamDeathmatch),
                    modesEnabled.HasFlag(Gamemode.YetiHunter),
                };

                if (enabledModes.Length != Enum.GetNames(typeof(Gamemode)).Length)
                    throw new NotImplementedException("The length of enabledModes does not equal the length of the gamemodes in the Gamemode enum.");

                List<int> selectMap = new List<int>();
                int mapcount = 0;
                // For each enabled mode...
                for (int i = 0; i < enabledModes.Length; i++)
                    if (enabledModes[i])
                    {
                        Gamemode emi = (Gamemode)Enum.Parse(typeof(Gamemode), Enum.GetNames(typeof(Gamemode))[i]); //enabledmodesindex

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
                            Thread.Sleep(waitTime);
                        }
                    KeyPress(Keys.Down);
                    Thread.Sleep(waitTime);
                }

                GoBack(2, 0);
            }
        }

        internal Point GetModeLocation(Gamemode mode, OWEvent owevent)
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

            OWEvent[] gamemodeEvents = new OWEvent[]
            {
                OWEvent.None,             // Assault
                OWEvent.None,             // AssaultEscort
                OWEvent.None,             // Control
                OWEvent.None,             // Escort

                OWEvent.None,             // CaptureTheFlag
                OWEvent.None,             // Deathmatch
                OWEvent.None,             // Elimination
                OWEvent.HalloweenTerror,  // JunkensteinsRevenge
                OWEvent.SummerGames,      // Lucioball
                OWEvent.WinterWonderland, // MeisSnowballOffensive
                OWEvent.None,             // TeamDeathmatch
                OWEvent.WinterWonderland, // YetiHunter

                OWEvent.None,             // Skirmish
            };

            for (int i = 0; i < gamemodeEvents.Length; i++)
                if (gamemodeEvents[i] == OWEvent.None || gamemodeEvents[i] == owevent)
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

        /// <summary>
        /// Gets the modes enabled in the Overwatch custom game.
        /// </summary>
        /// <param name="currentOverwatchEvent">The current Overwatch event.</param>
        /// <returns></returns>
        public Gamemode GetModesEnabled(OWEvent currentOverwatchEvent)
        {
            using (LockHandler.Interactive)
            {
                NavigateToModesMenu();

                ResetMouse();

                Thread.Sleep(100);

                UpdateScreen();

                Gamemode modesEnabled = new Gamemode();

                foreach (Gamemode gamemode in Enum.GetValues(typeof(Gamemode)))
                {
                    Point gamemodeIconLocation = GetModeLocation(gamemode, currentOverwatchEvent);
                    if (gamemodeIconLocation != Point.Empty && Capture.CompareColor(gamemodeIconLocation, Colors.SETTINGS_MODES_ENABLED, Fades.SETTINGS_MODES_ENABLED))
                        modesEnabled |= gamemode;
                }

                GoBack(2);

                return modesEnabled;
            }
        }

        /// <summary>
        /// Gets the markup of the current selected map.
        /// </summary>
        /// <returns></returns>
        public Bitmap GetMapMarkup()
        {
            UpdateScreen();
            return Capture.CloneAsBitmap(Rectangles.LOBBY_MAP);
        }

        /// <summary>
        /// Gets the current map being played.
        /// </summary>
        /// <returns>All Map values that are being played. For instance, if Route 66 is being played, this will return <see cref="Map.E_Route66"/> and <see cref="Map.SKIRM_Route66"/>.</returns>
        public Map[] GetCurrentMap()
        {
            using (LockHandler.Passive)
            {
                UpdateScreen();

                Tuple<double, MapMarkup> mostLikely = null;

                foreach (MapMarkup mapMarkup in Markups.MAP_MARKUPS)
                {
                    double total = 0;
                    double success = 0;
                    double fail = 0;
                    bool failed = false;

                    for (int x = 0; x < mapMarkup.Markup.Width && !failed; x++)
                        for (int y = 0; y < mapMarkup.Markup.Height && !failed; y++)
                        {
                            total++;
                            if (Capture.CompareColor(Rectangles.LOBBY_MAP.X + x, Rectangles.LOBBY_MAP.Y + y, mapMarkup.Markup.GetPixel(x, y).ToInt(), MapFade))
                                success++;
                            else
                                fail++;

                            failed = fail > MaximumMapIncorrectCount;
                        }
                    if (failed)
                        continue;

                    double result = success / total * 100;

                    if (result >= MinimumMapRatio && (mostLikely == null || (mostLikely.Item1 < result)))
                        mostLikely = new Tuple<double, MapMarkup>(result, mapMarkup);
                }

                return mostLikely?.Item2.Maps;
            }
        }
        private static readonly int MapFade = 10;
        private static readonly int MinimumMapRatio = 98;
        private static readonly int MaximumMapIncorrectCount = (int)(Rectangles.LOBBY_MAP.Width * Rectangles.LOBBY_MAP.Height * ((double)(100 - MinimumMapRatio) / 100));
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
        public static readonly Map A_Hanamura                       = new Map(Gamemode.Assault,               "A_Hanamura",                       OWEvent.None);
        public static readonly Map A_Hanamura_Winter                = new Map(Gamemode.Assault,               "A_Hanamura_Winter",                OWEvent.WinterWonderland);
        public static readonly Map A_HorizonLunarColony             = new Map(Gamemode.Assault,               "A_HorizonLunarColony",             OWEvent.None);
        public static readonly Map A_TempleOfAnubis                 = new Map(Gamemode.Assault,               "A_TempleOfAnubis",                 OWEvent.None);
        public static readonly Map A_VolskayaIndustries             = new Map(Gamemode.Assault,               "A_VolskayaIndustries",             OWEvent.None);

        // AssaultEscort
        public static readonly Map AE_BlizzardWorld                 = new Map(Gamemode.AssaultEscort,         "AE_BlizzardWorld",                 OWEvent.None);
        public static readonly Map AE_Eichenwalde                   = new Map(Gamemode.AssaultEscort,         "AE_Eichenwalde",                   OWEvent.None);
        public static readonly Map AE_Eichenwalde_Halloween         = new Map(Gamemode.AssaultEscort,         "AE_Eichenwalde_Halloween",         OWEvent.HalloweenTerror);
        public static readonly Map AE_Hollywood                     = new Map(Gamemode.AssaultEscort,         "AE_Hollywood",                     OWEvent.None);
        public static readonly Map AE_Hollywood_Halloween           = new Map(Gamemode.AssaultEscort,         "AE_Hollywood_Halloween",           OWEvent.HalloweenTerror);
        public static readonly Map AE_KingsRow                      = new Map(Gamemode.AssaultEscort,         "AE_KingsRow",                      OWEvent.None);
        public static readonly Map AE_KingsRow_Winter               = new Map(Gamemode.AssaultEscort,         "AE_KingsRow_Winter",               OWEvent.WinterWonderland);
        public static readonly Map AE_Numbani                       = new Map(Gamemode.AssaultEscort,         "AE_Numbani",                       OWEvent.None);

        // Capture The Flag
        public static readonly Map CTF_Ayutthaya                    = new Map(Gamemode.CaptureTheFlag,        "CTF_Ayutthaya",                    OWEvent.None);
        public static readonly Map CTF_Ilios_Lighthouse             = new Map(Gamemode.CaptureTheFlag,        "CTF_Ilios_Lighthouse",             OWEvent.None);
        public static readonly Map CTF_Ilios_Ruins                  = new Map(Gamemode.CaptureTheFlag,        "CTF_Ilios_Ruins",                  OWEvent.None);
        public static readonly Map CTF_Ilios_Well                   = new Map(Gamemode.CaptureTheFlag,        "CTF_Ilios_Well",                   OWEvent.None);
        public static readonly Map CTF_Lijiang_ControlCenter        = new Map(Gamemode.CaptureTheFlag,        "CTF_Lijiang_ControlCenter",        OWEvent.None);
        public static readonly Map CTF_Lijiang_ControlCenter_Lunar  = new Map(Gamemode.CaptureTheFlag,        "CTF_Lijiang_ControlCenter_Lunar",  OWEvent.LunarNewYear);
        public static readonly Map CTF_Lijiang_Garden               = new Map(Gamemode.CaptureTheFlag,        "CTF_Lijiang_Garden",               OWEvent.None);
        public static readonly Map CTF_Lijiang_Garden_Lunar         = new Map(Gamemode.CaptureTheFlag,        "CTF_Lijiang_Garden_Lunar",         OWEvent.LunarNewYear);
        public static readonly Map CTF_Lijiang_NightMarket          = new Map(Gamemode.CaptureTheFlag,        "CTF_Lijiang_NightMarket",          OWEvent.None);
        public static readonly Map CTF_Lijiang_NightMarket_Lunar    = new Map(Gamemode.CaptureTheFlag,        "CTF_Lijiang_NightMarket_Lunar",    OWEvent.LunarNewYear);
        public static readonly Map CTF_Nepal_Sanctum                = new Map(Gamemode.CaptureTheFlag,        "CTF_Nepal_Sanctum",                OWEvent.None);
        public static readonly Map CTF_Nepal_Shrine                 = new Map(Gamemode.CaptureTheFlag,        "CTF_Nepal_Shrine",                 OWEvent.None);
        public static readonly Map CTF_Nepal_Village                = new Map(Gamemode.CaptureTheFlag,        "CTF_Nepal_Village",                OWEvent.None);
        public static readonly Map CTF_Oasis_CityCenter             = new Map(Gamemode.CaptureTheFlag,        "CTF_Oasis_CityCenter",             OWEvent.None);
        public static readonly Map CTF_Oasis_Gardens                = new Map(Gamemode.CaptureTheFlag,        "CTF_Oasis_Gardens",                OWEvent.None);
        public static readonly Map CTF_Oasis_University             = new Map(Gamemode.CaptureTheFlag,        "CTF_Oasis_University",             OWEvent.None);

        // Control
        public static readonly Map C_Busan                          = new Map(Gamemode.Control,               "C_Busan",                          OWEvent.None);
        public static readonly Map C_Ilios                          = new Map(Gamemode.Control,               "C_Ilios",                          OWEvent.None);
        public static readonly Map C_Lijiang                        = new Map(Gamemode.Control,               "C_Lijiang",                        OWEvent.None);
        public static readonly Map C_Lijiang_Lunar                  = new Map(Gamemode.Control,               "C_Lijiang_Lunar",                  OWEvent.LunarNewYear);
        public static readonly Map C_Nepal                          = new Map(Gamemode.Control,               "C_Nepal",                          OWEvent.None);
        public static readonly Map C_Oasis                          = new Map(Gamemode.Control,               "C_Oasis",                          OWEvent.None);

        // Deathmatch
        public static readonly Map DM_BlackForest                   = new Map(Gamemode.Deathmatch,            "DM_BlackForest",                   OWEvent.None);
        public static readonly Map DM_BlackForest_Winter            = new Map(Gamemode.Deathmatch,            "DM_BlackForest_Winter",            OWEvent.WinterWonderland);
        public static readonly Map DM_BlizzardWorld                 = new Map(Gamemode.Deathmatch,            "DM_BlizzardWorld",                 OWEvent.None);
        public static readonly Map DM_Castillo                      = new Map(Gamemode.Deathmatch,            "DM_Castillo",                      OWEvent.None);
        public static readonly Map DM_ChateauGuillard               = new Map(Gamemode.Deathmatch,            "DM_ChateauGuillard",               OWEvent.None);
        public static readonly Map DM_ChateauGuillard_Halloween     = new Map(Gamemode.Deathmatch,            "DM_ChateauGuillard_Halloween",     OWEvent.HalloweenTerror);
        public static readonly Map DM_Dorado                        = new Map(Gamemode.Deathmatch,            "DM_Dorado",                        OWEvent.None);
        public static readonly Map DM_Antarctica                    = new Map(Gamemode.Deathmatch,            "DM_Antarctica",                    OWEvent.None);
        public static readonly Map DM_Antarctica_Winter             = new Map(Gamemode.Deathmatch,            "DM_Antarctica_Winter",             OWEvent.WinterWonderland);
        public static readonly Map DM_Eichenwalde                   = new Map(Gamemode.Deathmatch,            "DM_Eichenwalde",                   OWEvent.None);
        public static readonly Map DM_Eichenwalde_Halloween         = new Map(Gamemode.Deathmatch,            "DM_Eichenwalde_Halloween",         OWEvent.HalloweenTerror);
        public static readonly Map DM_Hanamura                      = new Map(Gamemode.Deathmatch,            "DM_Hanamura",                      OWEvent.None);
        public static readonly Map DM_Hanamura_Winter               = new Map(Gamemode.Deathmatch,            "DM_Hanamura_Winter",               OWEvent.WinterWonderland);
        public static readonly Map DM_Hollywood                     = new Map(Gamemode.Deathmatch,            "DM_Hollywood",                     OWEvent.None);
        public static readonly Map DM_Hollywood_Halloween           = new Map(Gamemode.Deathmatch,            "DM_Hollywood_Halloween",           OWEvent.HalloweenTerror);
        public static readonly Map DM_HorizonLunarColony            = new Map(Gamemode.Deathmatch,            "DM_HorizonLunarColony",            OWEvent.None);
        public static readonly Map DM_Ilios_Lighthouse              = new Map(Gamemode.Deathmatch,            "DM_Ilios_Lighthouse",              OWEvent.None);
        public static readonly Map DM_Ilios_Ruins                   = new Map(Gamemode.Deathmatch,            "DM_Ilios_Ruins",                   OWEvent.None);
        public static readonly Map DM_Ilios_Well                    = new Map(Gamemode.Deathmatch,            "DM_Ilios_Well",                    OWEvent.None);
        public static readonly Map DM_KingsRow                      = new Map(Gamemode.Deathmatch,            "DM_KingsRow",                      OWEvent.None);
        public static readonly Map DM_KingsRow_Winter               = new Map(Gamemode.Deathmatch,            "DM_KingsRow_Winter",               OWEvent.WinterWonderland);
        public static readonly Map DM_Lijiang_ControlCenter         = new Map(Gamemode.Deathmatch,            "DM_Lijiang_ControlCenter",         OWEvent.None);
        public static readonly Map DM_Lijiang_ControlCenter_Lunar   = new Map(Gamemode.Deathmatch,            "DM_Lijiang_ControlCenter_Lunar",   OWEvent.LunarNewYear);
        public static readonly Map DM_Lijiang_Garden                = new Map(Gamemode.Deathmatch,            "DM_Lijiang_Garden",                OWEvent.None);
        public static readonly Map DM_Lijiang_Garden_Lunar          = new Map(Gamemode.Deathmatch,            "DM_Lijiang_Garden_Lunar",          OWEvent.LunarNewYear);
        public static readonly Map DM_Lijiang_NightMarket           = new Map(Gamemode.Deathmatch,            "DM_Lijiang_NightMarket",           OWEvent.None);
        public static readonly Map DM_Lijiang_NightMarket_Lunar     = new Map(Gamemode.Deathmatch,            "DM_Lijiang_NightMarket_Lunar",     OWEvent.LunarNewYear);
        public static readonly Map DM_Necropolis                    = new Map(Gamemode.Deathmatch,            "DM_Necropolis",                    OWEvent.None);
        public static readonly Map DM_Nepal_Sanctum                 = new Map(Gamemode.Deathmatch,            "DM_Nepal_Sanctum",                 OWEvent.None);
        public static readonly Map DM_Nepal_Shrine                  = new Map(Gamemode.Deathmatch,            "DM_Nepal_Shrine",                  OWEvent.None);
        public static readonly Map DM_Nepal_Village                 = new Map(Gamemode.Deathmatch,            "DM_Nepal_Village",                 OWEvent.None);
        public static readonly Map DM_Oasis_CityCenter              = new Map(Gamemode.Deathmatch,            "DM_Oasis_CityCenter",              OWEvent.None);
        public static readonly Map DM_Oasis_Gardens                 = new Map(Gamemode.Deathmatch,            "DM_Oasis_Gardens",                 OWEvent.None);
        public static readonly Map DM_Oasis_University              = new Map(Gamemode.Deathmatch,            "DM_Oasis_University",              OWEvent.None);
        public static readonly Map DM_Petra                         = new Map(Gamemode.Deathmatch,            "DM_Petra",                         OWEvent.None);
        public static readonly Map DM_TempleOfAnubis                = new Map(Gamemode.Deathmatch,            "DM_TempleOfAnubis",                OWEvent.None);
        public static readonly Map DM_VolskayaIndustries            = new Map(Gamemode.Deathmatch,            "DM_VolskayaIndustries",            OWEvent.None);

        // Elimination
        public static readonly Map ELIM_Ayutthaya                   = new Map(Gamemode.Elimination,           "ELIM_Ayutthaya",                   OWEvent.None);
        public static readonly Map ELIM_BlackForest                 = new Map(Gamemode.Elimination,           "ELIM_BlackForest",                 OWEvent.None);
        public static readonly Map ELIM_BlackForest_Winter          = new Map(Gamemode.Elimination,           "ELIM_BlackForest_Winter",          OWEvent.WinterWonderland);
        public static readonly Map ELIM_Castillo                    = new Map(Gamemode.Elimination,           "ELIM_Castillo",                    OWEvent.None);
        public static readonly Map ELIM_Antarctica                  = new Map(Gamemode.Elimination,           "ELIM_Antarctica",                  OWEvent.None);
        public static readonly Map ELIM_Antarctica_Winter           = new Map(Gamemode.Elimination,           "ELIM_Antarctica_Winter",           OWEvent.WinterWonderland);
        public static readonly Map ELIM_Ilios_Lighthouse            = new Map(Gamemode.Elimination,           "ELIM_Ilios_Lighthouse",            OWEvent.None);
        public static readonly Map ELIM_Ilios_Ruins                 = new Map(Gamemode.Elimination,           "ELIM_Ilios_Ruins",                 OWEvent.None);
        public static readonly Map ELIM_Ilios_Well                  = new Map(Gamemode.Elimination,           "ELIM_Ilios_Well",                  OWEvent.None);
        public static readonly Map ELIM_Lijiang_ControlCenter       = new Map(Gamemode.Elimination,           "ELIM_Lijiang_ControlCenter",       OWEvent.None);
        public static readonly Map ELIM_Lijiang_ControlCenter_Lunar = new Map(Gamemode.Elimination,           "ELIM_Lijiang_ControlCenter_Lunar", OWEvent.LunarNewYear);
        public static readonly Map ELIM_Lijiang_Garden              = new Map(Gamemode.Elimination,           "ELIM_Lijiang_Garden",              OWEvent.None);
        public static readonly Map ELIM_Lijiang_Garden_Lunar        = new Map(Gamemode.Elimination,           "ELIM_Lijiang_Garden_Lunar",        OWEvent.LunarNewYear);
        public static readonly Map ELIM_Lijiang_NightMarket         = new Map(Gamemode.Elimination,           "ELIM_Lijiang_NightMarket",         OWEvent.None);
        public static readonly Map ELIM_Lijiang_NightMarket_Lunar   = new Map(Gamemode.Elimination,           "ELIM_Lijiang_NightMarket_Lunar",   OWEvent.LunarNewYear);
        public static readonly Map ELIM_Necropolis                  = new Map(Gamemode.Elimination,           "ELIM_Necropolis",                  OWEvent.None);
        public static readonly Map ELIM_Nepal_Sanctum               = new Map(Gamemode.Elimination,           "ELIM_Nepal_Sanctum",               OWEvent.None);
        public static readonly Map ELIM_Nepal_Shrine                = new Map(Gamemode.Elimination,           "ELIM_Nepal_Shrine",                OWEvent.None);
        public static readonly Map ELIM_Nepal_Village               = new Map(Gamemode.Elimination,           "ELIM_Nepal_Village",               OWEvent.None);
        public static readonly Map ELIM_Oasis_CityCenter            = new Map(Gamemode.Elimination,           "ELIM_Oasis_CityCenter",            OWEvent.None);
        public static readonly Map ELIM_Oasis_Gardens               = new Map(Gamemode.Elimination,           "ELIM_Oasis_Gardens",               OWEvent.None);
        public static readonly Map ELIM_Oasis_University            = new Map(Gamemode.Elimination,           "ELIM_Oasis_University",            OWEvent.None);

        // Escort
        public static readonly Map E_Dorado                         = new Map(Gamemode.Escort,                "E_Dorado",                         OWEvent.None);
        public static readonly Map E_Junkertown                     = new Map(Gamemode.Escort,                "E_Junkertown",                     OWEvent.None);
        public static readonly Map E_Rialto                         = new Map(Gamemode.Escort,                "E_Rialto",                         OWEvent.None);
        public static readonly Map E_Route66                        = new Map(Gamemode.Escort,                "E_Route66",                        OWEvent.None);
        public static readonly Map E_Gibraltar                      = new Map(Gamemode.Escort,                "E_Gibraltar",                      OWEvent.None);

        // Junkensteins Revenge
        public static readonly Map JR_Adlersbrunn                   = new Map(Gamemode.JunkensteinsRevenge,   "JR_Adlersbrunn",                   OWEvent.HalloweenTerror);

        // Lucioball
        public static readonly Map LB_EstasioDasRas                 = new Map(Gamemode.Lucioball,             "LB_EstasioDasRas",                 OWEvent.SummerGames);
        public static readonly Map LB_SydneyHarbourArena            = new Map(Gamemode.Lucioball,             "LB_SydneyHarbourArena",            OWEvent.SummerGames);

        // Meis Snowball Offensive
        public static readonly Map MSO_BlackForest_Winter           = new Map(Gamemode.MeisSnowballOffensive, "MSO_BlackForest_Winter",           OWEvent.WinterWonderland);
        public static readonly Map MSO_Antarctica_Winter            = new Map(Gamemode.MeisSnowballOffensive, "MSO_Antarctica_Winter",            OWEvent.WinterWonderland);

        // Skirmish
        public static readonly Map SKIRM_BlizzardWorld              = new Map(Gamemode.Skirmish,              "SKIRM_BlizzardWorld",              OWEvent.None);
        public static readonly Map SKIRM_Busan                      = new Map(Gamemode.Skirmish,              "SKIRM_Busan",                      OWEvent.None);
        public static readonly Map SKIRM_Dorado                     = new Map(Gamemode.Skirmish,              "SKIRM_Dorado",                     OWEvent.None);
        public static readonly Map SKIRM_Eichenwalde                = new Map(Gamemode.Skirmish,              "SKIRM_Eichenwalde",                OWEvent.None);
        public static readonly Map SKIRM_Eichenwalde_Halloween      = new Map(Gamemode.Skirmish,              "SKIRM_Eichenwalde_Halloween",      OWEvent.HalloweenTerror);
        public static readonly Map SKIRM_Hanamura                   = new Map(Gamemode.Skirmish,              "SKIRM_Hanamura",                   OWEvent.None);
        public static readonly Map SKIRM_Hanamura_Winter            = new Map(Gamemode.Skirmish,              "SKIRM_Hanamura_Winter",            OWEvent.WinterWonderland);
        public static readonly Map SKIRM_Hollywood                  = new Map(Gamemode.Skirmish,              "SKIRM_Hollywood",                  OWEvent.None);
        public static readonly Map SKIRM_Hollywood_Halloween        = new Map(Gamemode.Skirmish,              "SKIRM_Hollywood_Halloween",        OWEvent.HalloweenTerror);
        public static readonly Map SKIRM_HorizonLunarColony         = new Map(Gamemode.Skirmish,              "SKIRM_HorizonLunarColony",         OWEvent.None);
        public static readonly Map SKIRM_Ilios                      = new Map(Gamemode.Skirmish,              "SKIRM_Ilios",                      OWEvent.None);
        public static readonly Map SKIRM_Junkertown                 = new Map(Gamemode.Skirmish,              "SKIRM_Junkertown",                 OWEvent.None);
        public static readonly Map SKIRM_KingsRow                   = new Map(Gamemode.Skirmish,              "SKIRM_KingsRow",                   OWEvent.None);
        public static readonly Map SKIRM_KingsRow_Winter            = new Map(Gamemode.Skirmish,              "SKIRM_KingsRow_Winter",            OWEvent.WinterWonderland);
        public static readonly Map SKIRM_Lijiang                    = new Map(Gamemode.Skirmish,              "SKIRM_Lijiang",                    OWEvent.None);
        public static readonly Map SKIRM_Lijiang_Lunar              = new Map(Gamemode.Skirmish,              "SKIRM_Lijiang_Lunar",              OWEvent.LunarNewYear);
        public static readonly Map SKIRM_Nepal                      = new Map(Gamemode.Skirmish,              "SKIRM_Nepal",                      OWEvent.None);
        public static readonly Map SKIRM_Numbani                    = new Map(Gamemode.Skirmish,              "SKIRM_Numbani",                    OWEvent.None);
        public static readonly Map SKIRM_Oasis                      = new Map(Gamemode.Skirmish,              "SKIRM_Oasis",                      OWEvent.None);
        public static readonly Map SKIRM_Rialto                     = new Map(Gamemode.Skirmish,              "SKIRM_Rialto",                     OWEvent.None);
        public static readonly Map SKIRM_Route66                    = new Map(Gamemode.Skirmish,              "SKIRM_Route66",                    OWEvent.None);
        public static readonly Map SKIRM_TempleOfAnubis             = new Map(Gamemode.Skirmish,              "SKIRM_TempleOfAnubis",             OWEvent.None);
        public static readonly Map SKIRM_VolskayaIndustries         = new Map(Gamemode.Skirmish,              "SKIRM_VolskayaIndustries",         OWEvent.None);
        public static readonly Map SKIRM_Gibraltar                  = new Map(Gamemode.Skirmish,              "SKIRM_Gibraltar",                  OWEvent.None);

        // Team Deathmatch
        public static readonly Map TDM_BlackForest                  = new Map(Gamemode.TeamDeathmatch,        "TDM_BlackForest",                  OWEvent.None);
        public static readonly Map TDM_BlackForest_Winter           = new Map(Gamemode.TeamDeathmatch,        "TDM_BlackForest_Winter",           OWEvent.WinterWonderland);
        public static readonly Map TDM_BlizzardWorld                = new Map(Gamemode.TeamDeathmatch,        "TDM_BlizzardWorld",                OWEvent.None);
        public static readonly Map TDM_Castillo                     = new Map(Gamemode.TeamDeathmatch,        "TDM_Castillo",                     OWEvent.None);
        public static readonly Map TDM_ChateauGuillard              = new Map(Gamemode.TeamDeathmatch,        "TDM_ChateauGuillard",              OWEvent.None);
        public static readonly Map TDM_ChateauGuillard_Halloween    = new Map(Gamemode.TeamDeathmatch,        "TDM_ChateauGuillard_Halloween",    OWEvent.HalloweenTerror);
        public static readonly Map TDM_Dorado                       = new Map(Gamemode.TeamDeathmatch,        "TDM_Dorado",                       OWEvent.None);
        public static readonly Map TDM_Antarctica                   = new Map(Gamemode.TeamDeathmatch,        "TDM_Antarctica",                   OWEvent.None);
        public static readonly Map TDM_Antarctica_Winter            = new Map(Gamemode.TeamDeathmatch,        "TDM_Antarctica_Winter",            OWEvent.WinterWonderland);
        public static readonly Map TDM_Eichenwalde                  = new Map(Gamemode.TeamDeathmatch,        "TDM_Eichenwalde",                  OWEvent.None);
        public static readonly Map TDM_Eichenwalde_Halloween        = new Map(Gamemode.TeamDeathmatch,        "TDM_Eichenwalde_Halloween",        OWEvent.HalloweenTerror);
        public static readonly Map TDM_Hanamura                     = new Map(Gamemode.TeamDeathmatch,        "TDM_Hanamura",                     OWEvent.None);
        public static readonly Map TDM_Hanamura_Winter              = new Map(Gamemode.TeamDeathmatch,        "TDM_Hanamura_Winter",              OWEvent.WinterWonderland);
        public static readonly Map TDM_Hollywood                    = new Map(Gamemode.TeamDeathmatch,        "TDM_Hollywood",                    OWEvent.None);
        public static readonly Map TDM_Hollywood_Halloween          = new Map(Gamemode.TeamDeathmatch,        "TDM_Hollywood_Halloween",          OWEvent.HalloweenTerror);
        public static readonly Map TDM_HorizonLunarColony           = new Map(Gamemode.TeamDeathmatch,        "TDM_HorizonLunarColony",           OWEvent.None);
        public static readonly Map TDM_Ilios_Lighthouse             = new Map(Gamemode.TeamDeathmatch,        "TDM_Ilios_Lighthouse",             OWEvent.None);
        public static readonly Map TDM_Ilios_Ruins                  = new Map(Gamemode.TeamDeathmatch,        "TDM_Ilios_Ruins",                  OWEvent.None);
        public static readonly Map TDM_Ilios_Well                   = new Map(Gamemode.TeamDeathmatch,        "TDM_Ilios_Well",                   OWEvent.None);
        public static readonly Map TDM_KingsRow                     = new Map(Gamemode.TeamDeathmatch,        "TDM_KingsRow",                     OWEvent.None);
        public static readonly Map TDM_KingsRow_Winter              = new Map(Gamemode.TeamDeathmatch,        "TDM_KingsRow_Winter",              OWEvent.WinterWonderland);
        public static readonly Map TDM_Lijiang_ControlCenter        = new Map(Gamemode.TeamDeathmatch,        "TDM_Lijiang_ControlCenter",        OWEvent.None);
        public static readonly Map TDM_Lijiang_ControlCenter_Lunar  = new Map(Gamemode.TeamDeathmatch,        "TDM_Lijiang_ControlCenter_Lunar",  OWEvent.LunarNewYear);
        public static readonly Map TDM_Lijiang_Garden               = new Map(Gamemode.TeamDeathmatch,        "TDM_Lijiang_Garden",               OWEvent.None);
        public static readonly Map TDM_Lijiang_Garden_Lunar         = new Map(Gamemode.TeamDeathmatch,        "TDM_Lijiang_Garden_Lunar",         OWEvent.LunarNewYear);
        public static readonly Map TDM_Lijiang_NightMarket          = new Map(Gamemode.TeamDeathmatch,        "TDM_Lijiang_NightMarket",          OWEvent.None);
        public static readonly Map TDM_Lijiang_NightMarket_Lunar    = new Map(Gamemode.TeamDeathmatch,        "TDM_Lijiang_NightMarket_Lunar",    OWEvent.LunarNewYear);
        public static readonly Map TDM_Necropolis                   = new Map(Gamemode.TeamDeathmatch,        "TDM_Necropolis",                   OWEvent.None);
        public static readonly Map TDM_Nepal_Sanctum                = new Map(Gamemode.TeamDeathmatch,        "TDM_Nepal_Sanctum",                OWEvent.None);
        public static readonly Map TDM_Nepal_Shrine                 = new Map(Gamemode.TeamDeathmatch,        "TDM_Nepal_Shrine",                 OWEvent.None);
        public static readonly Map TDM_Nepal_Village                = new Map(Gamemode.TeamDeathmatch,        "TDM_Nepal_Village",                OWEvent.None);
        public static readonly Map TDM_Oasis_CityCenter             = new Map(Gamemode.TeamDeathmatch,        "TDM_Oasis_CityCenter",             OWEvent.None);
        public static readonly Map TDM_Oasis_Gardens                = new Map(Gamemode.TeamDeathmatch,        "TDM_Oasis_Gardens",                OWEvent.None);
        public static readonly Map TDM_Oasis_University             = new Map(Gamemode.TeamDeathmatch,        "TDM_Oasis_University",             OWEvent.None);
        public static readonly Map TDM_Petra                        = new Map(Gamemode.TeamDeathmatch,        "TDM_Petra",                        OWEvent.None);
        public static readonly Map TDM_TempleOfAnubis               = new Map(Gamemode.TeamDeathmatch,        "TDM_TempleOfAnubis",               OWEvent.None);
        public static readonly Map TDM_VolskayaIndustries           = new Map(Gamemode.TeamDeathmatch,        "TDM_VolskayaIndustries",           OWEvent.None);

        // Yeti Hunter
        public static readonly Map YH_Nepal_Village                 = new Map(Gamemode.YetiHunter,            "YH_Nepal_Village",                 OWEvent.WinterWonderland);
#pragma warning restore CS1591
        #endregion

        private Map(Gamemode gamemode, string mapName, OWEvent gameEvent)
        {
            GameEvent = gameEvent;
            GameMode = gamemode;
            MapName = mapName;
            ShortName = mapName.Substring(mapName.IndexOf('_') + 1);
        }

        /// <summary>
        /// The Overwatch event the map is on.
        /// </summary>
        public OWEvent GameEvent { get; private set; }
        /// <summary>
        /// Gamemode of the map.
        /// </summary>
        public Gamemode GameMode { get; private set; }
        /// <summary>
        /// Name of the map.
        /// </summary>
        public string MapName { get; private set; }
        /// <summary>
        /// Short name of the map.
        /// </summary>
        public string ShortName { get; private set; }

#pragma warning disable CS1591
        public bool Equals(Map other)
        {
            return MapName == other.MapName 
                && GameMode == other.GameMode 
                && GameEvent == other.GameEvent;
        }
        public override string ToString()
        {
            return MapName;
        }
#pragma warning restore CS1591

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
        public static Map[] GetMapsInGamemode(Gamemode gamemode, OWEvent owEvent = OWEvent.None)
        {
            return GetMaps().Where(v => (v.GameEvent == OWEvent.None || v.GameEvent == owEvent) && gamemode.HasFlag(v.GameMode)).ToArray();
        }

        private static FieldInfo[] GetMapFieldInfo()
        {
            return typeof(Map).GetFields(BindingFlags.Public | BindingFlags.Static);
        }
        private static Map MaparFromFieldInfo(FieldInfo fi)
        {
            return (Map)fi.GetValue(null);
        }
    }
}
