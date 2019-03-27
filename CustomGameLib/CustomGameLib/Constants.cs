using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using Deltin.CustomGameAutomation.Properties;

namespace Deltin.CustomGameAutomation
{
    internal static class Colors
    {
        public static readonly int[] WHITE = new int[] { 191, 191, 191 };
        public static readonly int[] LOADING_BLACK = new int[] { 64, 64, 64 };

        public static readonly int[] CONFIRM = new int[] { 176, 141, 89 };

        public static readonly int[] LOADING_ENTERING_GAME = new int[] { 176, 141, 89 };

        public static readonly int[] LOBBY_START_GAME = new int[] { 150, 127, 96 };
        public static readonly int[] LOBBY_CHANGE = new int[] { 126, 158, 181 };
        public static readonly int[] LOBBY_INVITE_PLAYERS_TO_GROUP_MIN = new int[] { 77, 130, 165 };
        public static readonly int[] LOBBY_INVITE_PLAYERS_TO_GROUP_MAX = new int[] { 95, 145, 180 };
        public static readonly int[] LOBBY_JOIN_BOX = new int[] { 92, 110, 124 };

        public static readonly int[] SETTINGS_PRESETS_LOADABLE_PRESET = new int[] { 126, 128, 134 };

        public static readonly int[] DEAD_PLAYER = new int[] { 118, 74, 76 }; // Spectator UI red X
        public static readonly int[] HERO_CHOSEN_BLUE = new int[] { 83, 110, 123 };
        public static readonly int[] HERO_CHOSEN_RED = new int[] { 114, 77, 81 };

        public static readonly int[] MODERATOR_ICON = new int[] { 143, 155, 80 };
        public static readonly int[] SPECTATOR_MODERATOR_ICON = new int[] { 149, 183, 89 };

        public static readonly int[] ENDING_COMMEND_DEFEAT = new int[] { 120, 70, 74 };

        public static readonly int[] SETTINGS_ERROR = new int[] { 151, 119, 81 };
        public static readonly int[] SETTINGS_MODES_ENABLED = new int[] { 125, 127, 135 };

        public static readonly int[] EXIT_TO_DESKTOP = new int[] { 83, 124, 152 };
    }

    internal static class Fades
    {
        public static readonly int SETTINGS_ERROR = 20;
        public static readonly int DEAD_PLAYER = 15;

        public static readonly int LOADING_ENTERING_GAME = 20;

        public static readonly int SLOT_FADE = 20;

        public static readonly int LOBBY_START_GAME = 30;
        public static readonly int LOBBY_CHANGE = 50;
        public static readonly int LOBBY_INVITE_PLAYERS_TO_GROUP_COMPARE = 15;
        public static readonly int LOBBY_JOIN_BOX = 15;

        public static readonly int SETTINGS_PRESETS_LOADABLE_PRESET = 30;
        public static readonly int SETTINGS_MODES_ENABLED = 30;

        public static readonly int CONFIRM = 50;

        public static readonly int HEROES_CHOSEN = 15;

        public static readonly int ENDING_COMMEND_DEFEAT = 10;

        public static readonly int EXIT_TO_DESKTOP = 15;
    }

    internal static class Points
    {
        public static readonly Point RESET_POINT = new Point(500, 500);

        public static readonly Point PRE_MAIN_MENU_LOGIN = new Point(419, 473);

        public static readonly Point MAIN_MENU_OVERWATCH_WATERMARK = new Point(53, 68);

        public static readonly Point OPTIONS_APPLY = new Point(400, 500);
        public static readonly Point OPTIONS_BACK = new Point(500, 500);

        public static readonly Point LOADING_ENTERING_GAME = new Point(450, 325);

        public static readonly Point LOBBY_ADD_AI = new Point(835, 182);
        public static readonly Point LOBBY_BACK_TO_LOBBY = new Point(394, 457);
        public static readonly Point LOBBY_CHATBOX = new Point(105, 504);
        public static readonly Point LOBBY_CHAT_TYPE_INDICATOR = new Point(50, 505);
        public static readonly Point LOBBY_GAME_NAME = new Point(209, 165);
        public static readonly Point LOBBY_INVITE_IF_ADD_BUTTON_PRESENT = new Point(778, 180);
        public static readonly Point LOBBY_INVITE_IF_ADD_BUTTON_NOT_PRESENT = new Point(835, 180);
        public static readonly Point LOBBY_JOIN_DROPDOWN = new Point(280, 198);
        public static readonly Point LOBBY_JOIN_EVERYONE = new Point(280, 220);
        public static readonly Point LOBBY_JOIN_FRIENDS = new Point(280, 240);
        public static readonly Point LOBBY_JOIN_INVITE = new Point(280, 260);
        public static readonly Point LOBBY_JOIN_FRIENDS_CHECK = new Point(273, 196);
        public static readonly Point LOBBY_JOIN_INVITE_CHECK = new Point(264, 196);
        public static readonly Point LOBBY_JOIN_UPDATING_CHECK = new Point(316, 196);
        public static readonly Point LOBBY_MOVE_IF_ADD_BUTTON_PRESENT = new Point(661, 180);
        public static readonly Point LOBBY_MOVE_IF_ADD_BUTTON_NOT_PRESENT = new Point(717, 180);
        public static readonly Point LOBBY_SETTINGS_IF_ADD_BUTTON_PRESENT = new Point(716, 180);
        public static readonly Point LOBBY_SETTINGS_IF_ADD_BUTTON_NOT_PRESENT = new Point(774, 180);
        public static readonly Point LOBBY_START_GAME = new Point(451, 458);
        public static readonly Point LOBBY_START_FROM_WAITING_FOR_PLAYERS = new Point(570, 455);
        public static readonly Point LOBBY_START_GAMEMODE = new Point(599, 456);
        public static readonly Point LOBBY_RESTART = new Point(500, 455);
        public static readonly Point LOBBY_SWAP_ALL_IF_ADD_BUTTON_PRESENT = new Point(617, 180);
        public static readonly Point LOBBY_SWAP_ALL_IF_ADD_BUTTON_NOT_PRESENT = new Point(678, 180);
        public static readonly Point LOBBY_MY_PLAYER_ICON = new Point(744, 62);
        public static readonly Point LOBBY_BLUE_NAME = new Point(159, 229);
        public static readonly Point LOBBY_RED_NAME = new Point(458, 230);
        public static readonly Point LOBBY_PAUSED = new Point(441, 268);
        public static readonly Point LOBBY_INVITE_PLAYERS_TO_GROUP = new Point(703, 51);
        public static readonly Point LOBBY_INVITE_PLAYERS_TO_GROUP_COMPARE = new Point(703, 40);
        public static readonly Point LOBBY_CUSTOM_GAME_INFO = new Point(119, 180);
        public static readonly Point LOBBY_NAV_CREATEGAME = new Point(48, 107);
        public static readonly Point LOBBY_NAV_ESCAPEMENU = new Point(310, 60);
        public static readonly Point LOBBY_NAV_MAINMENU = new Point(48, 50);
        public static readonly Point LOBBY_ELIMINATION_ROUND_OVER = new Point(393, 142);

        public static readonly Point EDIT_AI_CONFIRM = new Point(447, 354);

        public static readonly Point INVITE_INVITE = new Point(460, 434);
        public static readonly Point INVITE_BACK = new Point(412, 434);
        public static readonly Point INVITE_VIA_BATTLETAG = new Point(572, 171);
        public static readonly Point INVITE_TEAM_DROPDOWN = new Point(475, 398);
        public static readonly Point INVITE_TEAM_BLUE = new Point(475, 447);
        public static readonly Point INVITE_TEAM_RED = new Point(475, 466);
        public static readonly Point INVITE_TEAM_SPECTATOR = new Point(475, 483);

        public static readonly Point SETTINGS_PRESETS = new Point(103, 183);
        public static readonly Point SETTINGS_LOBBY = new Point(297, 183);
        public static readonly Point SETTINGS_MODES = new Point(494, 178);
        public static readonly Point SETTINGS_MAPS = new Point(103, 300);
        public static readonly Point SETTINGS_HEROES = new Point(351, 311);
        public static readonly Point SETTINGS_ERROR = new Point(522, 320);
        public static readonly Point SETTINGS_BACK = new Point(855, 507);
        public static readonly Point SETTINGS_DISCARD = new Point(436, 318);

        public static readonly Point PRESETS_FIRST_PRESET = new Point(86, 155);
        public static readonly Point PRESETS_CONFIRM = new Point(480, 327);

        public static readonly Point SETTINGS_LOBBY_BLUE_MAX_PLAYERS = new Point(500, 269);
        public static readonly Point SETTINGS_LOBBY_RED_MAX_PLAYERS = new Point(500, 290);
        public static readonly Point SETTINGS_LOBBY_FFA_MAX_PLAYERS = new Point(500, 311);
        public static readonly Point SETTINGS_LOBBY_MAX_SPECTATORS = new Point(500, 333);

        public static readonly Point SETTINGS_MAPS_DISABLE_ALL = new Point(640, 125);
        public static readonly Point SETTINGS_MAPS_ENABLE_ALL = new Point(600, 125);

        public static readonly Point SETTINGS_HEROES_GENERAL = new Point(80, 146);
        public static readonly Point SETTINGS_HEROES_ROSTER = new Point(287, 158);

        public static readonly Point SETTINGS_HEROES_SETTINGS_TEAM_DROPDOWN = new Point(572, 126);
        public static readonly Point SETTINGS_HEROES_SETTINGS_TEAM_BLUE = new Point(572, 173);
        public static readonly Point SETTINGS_HEROES_SETTINGS_TEAM_RED = new Point(572, 192);

        public static readonly Point SETTINGS_HEROES_ROSTER_TEAM_DROPDOWN = new Point(492, 127);
        public static readonly Point SETTINGS_HEROES_ROSTER_TEAM_BLUE = new Point(484, 173);
        public static readonly Point SETTINGS_HEROES_ROSTER_TEAM_RED = new Point(484, 193);
        public static readonly Point SETTINGS_HEROES_ROSTER_DISABLE_ALL = new Point(635, 130);
        public static readonly Point SETTINGS_HEROES_ROSTER_ENABLE_ALL = new Point(597, 130);

        public static readonly Point INFO_SAVE = new Point(720, 390);

        public static readonly Point ENDING_COMMEND_DEFEAT = new Point(53, 62);

        public static readonly Point EXIT_TO_DESKTOP = new Point(130, 505);

        public static readonly Point[] SLOT_LOCATIONS = new Point[]
        {
            // Blue
            new Point(51, 255), // Slot 0
            new Point(51, 283), // Slot 1
            new Point(51, 311), // Slot 2
            new Point(51, 341), // Slot 3
            new Point(51, 369), // Slot 4
            new Point(51, 384), // Slot 5
            // Red
            new Point(621, 255), // Slot 6
            new Point(621, 283), // Slot 7
            new Point(621, 311), // Slot 8
            new Point(621, 341), // Slot 9
            new Point(621, 369), // Slot 10
            new Point(621, 397), // Slot 11
            // Spectator
            new Point(896, 248), // slot 12
            new Point(896, 264), // slot 13
            new Point(896, 277), // slot 14
            new Point(896, 290), // slot 15
            new Point(896, 304), // slot 16
            new Point(896, 317), // slot 17
        };

        public static readonly int[] KILLED_PLAYER_MARKERS = new int[]
        {
            66, // slot 0
            115, // slot 1
            164, // slot 2
            214, // slot 3
            263, // slot 4
            312, // slot 5

            633, // slot 6
            682, // slot 7
            731, // slot 8
            780, // slot 9
            830, // slot 10
            879, // slot 11
        };
        public static readonly int KILLED_PLAYER_MARKER_Y = 98;

        public static readonly Point[] MODERATOR_ICON_LOCATIONS = new Point[]
        {
            // blue
            new Point(60, 257),
            new Point(60, 286),
            new Point(60, 315),
            new Point(60, 343),
            new Point(60, 372),
            new Point(60, 400),
            // red
            new Point(607, 257),
            new Point(607, 286),
            new Point(607, 315),
            new Point(607, 343),
            new Point(607, 372),
            new Point(607, 400),
            // spectator
            new Point(885, 252),
            new Point(885, 265),
            new Point(885, 279),
            new Point(885, 292),
            new Point(885, 305),
            new Point(885, 318)
        };

        public static readonly Point[] ULTIMATE_LOCATIONS = new Point[]
        {
            new Point(59, 72),
            new Point(108, 72),
            new Point(157, 72),
            new Point(207, 72),
            new Point(255, 72),
            new Point(305, 72),

            new Point(612, 72),
            new Point(661, 72),
            new Point(710, 75),
            new Point(759, 75),
            new Point(808, 75),
            new Point(857, 75)
        };

        public static readonly int[] HERO_LOCATIONS = new int[]
        {
            76,
            125,
            175,
            224,
            273,
            322,

            629,
            678,
            727,
            777,
            826,
            875
        };
        public static readonly int HERO_Y = 73;
    }

    internal static class Rectangles
    {
        public static readonly Rectangle ENTIRE_SCREEN = new Rectangle(0, 0, 960, 540);

        public static readonly Rectangle LOBBY_CHATBOX = new Rectangle(50, 440, 200, 47);
        public static readonly Rectangle LOBBY_CAREER_PROFILE = new Rectangle(46, 101, 265, 82);
        public static readonly Rectangle LOBBY_MAP = new Rectangle(52, 153, 139, 56);

        public static readonly Rectangle SETTINGS_PRESET_OPTION = new Rectangle(0, 0, 128, 20);
    }

    internal static class Distances
    {
        public static readonly int LOBBY_SLOT_DISTANCE = 29;
        public static readonly int LOBBY_TEAM_SLOT_DISTANCE = 319;
        public static readonly int LOBBY_SPECTATOR_SLOT_DISTANCE = 13;
        public static readonly int LOBBY_QUEUE_OFFSET = 6; // Did you know that the queue list is 6 pixels higher than the spectator list?
        public static readonly int LOBBY_SLOT_HEIGHT = 26;
        public static readonly int LOBBY_SPECTATOR_SLOT_HEIGHT = 11;

        public static readonly int LOBBY_SLOT_DM_BLUE_X_OFFSET = 17;
        public static readonly int LOBBY_SLOT_DM_RED_X_OFFSET = -17;
        public static readonly int LOBBY_SLOT_DM_Y_OFFSET = -20;

        public static readonly int PRESET_DISTANCE_X = 144;
        public static readonly int PRESET_DISTANCE_Y = 33;
    }

    internal static class Markups
    {
        // Markups for the option menu
        public static readonly DirectBitmap REMOVE_FROM_GAME    = new DirectBitmap(Resources.O_RemoveFromGame, true);
        public static readonly DirectBitmap SWAP_TO_RED         = new DirectBitmap(Resources.O_SwapToRed, true);
        public static readonly DirectBitmap SWAP_TO_BLUE        = new DirectBitmap(Resources.O_SwapToBlue, true);
        public static readonly DirectBitmap SWAP_TO_SPECTATORS  = new DirectBitmap(Resources.O_SwapToSpectators, true);
        public static readonly DirectBitmap SWAP_TO_NEUTRAL     = new DirectBitmap(Resources.O_SwapToNeutral, true);
        public static readonly DirectBitmap REMOVE_ALL_BOTS     = new DirectBitmap(Resources.O_RemoveAllBots, true);
        public static readonly DirectBitmap VIEW_CAREER_PROFILE = new DirectBitmap(Resources.O_ViewCareerProfile, true);
        public static readonly DirectBitmap SEND_FRIEND_REQUEST = new DirectBitmap(Resources.O_SendFriendRequest, true);
        //public static readonly DirectBitmap REMOVE_FRIEND       = new DirectBitmap(Resources.O_RemoveFriend, true);

        // Navigation
        public static readonly DirectBitmap NAV_LOBBY = new DirectBitmap(Resources.N_CreateGame, true);
        public static readonly DirectBitmap NAV_ESCAPEMENU = new DirectBitmap(Resources.N_EscapeMenu, true);
        public static readonly DirectBitmap NAV_MAINMENU = new DirectBitmap(Resources.N_MainMenu, true);

        // Events
        public static readonly DirectBitmap KOTH_ROUND_OVER = new DirectBitmap(Resources.E_KOTHRoundOver, true);
        public static readonly DirectBitmap ELIM_ROUND_OVER = new DirectBitmap(Resources.E_ELIMRoundOver, true);

        // Heroes
        public static readonly DirectBitmap[] HERO_MARKUPS = new DirectBitmap[]
        {
            new DirectBitmap(Resources.H_Ana, true), // Ana
            new DirectBitmap(Resources.H_Ashe, true), // Ashe
            new DirectBitmap(Resources.H_Baptiste, true), // Baptiste
            new DirectBitmap(Resources.H_Bastion, true), // Bastion
            new DirectBitmap(Resources.H_Brigitte, true), // Brigitte
            new DirectBitmap(Resources.H_Dva, true), // Dva
            new DirectBitmap(Resources.H_Doomfist, true), // Doomfist
            new DirectBitmap(Resources.H_Gengi, true), // Genji
            new DirectBitmap(Resources.H_Hanzo, true), // Hanzo
            new DirectBitmap(Resources.H_Junkrat, true), // Junkrat
            new DirectBitmap(Resources.H_Lucio, true), // Lucio
            new DirectBitmap(Resources.H_McCree, true), // McCree
            new DirectBitmap(Resources.H_Mei, true), // Mei
            new DirectBitmap(Resources.H_Mercy, true), // Mercy
            new DirectBitmap(Resources.H_Moira, true), // Moira
            new DirectBitmap(Resources.H_Orisa, true), // Orisa
            new DirectBitmap(Resources.H_Pharah, true), // Pharah
            new DirectBitmap(Resources.H_Reaper, true), // Reaper
            new DirectBitmap(Resources.H_Reinhardt, true), // Reinhardt
            new DirectBitmap(Resources.H_Roadhog, true), // Roadhog
            new DirectBitmap(Resources.H_Soldier76, true), // Soldier: 76
            new DirectBitmap(Resources.H_Sombra, true), // Sombra
            new DirectBitmap(Resources.H_Symmetra, true), // Symmetra
            new DirectBitmap(Resources.H_Torbjorn, true), // Torbjorn
            new DirectBitmap(Resources.H_Tracer, true), // Tracer
            new DirectBitmap(Resources.H_Widowmaker, true), // Widowmaker
            new DirectBitmap(Resources.H_Winston, true), // Winston
            new DirectBitmap(Resources.H_WreckingBall, true), // Wrecking Ball
            new DirectBitmap(Resources.H_Zarya, true), // Zarya
            new DirectBitmap(Resources.H_Zenyatta, true) // Zenyatta
        };

        // Maps
        public static readonly MapMarkup[] MAP_MARKUPS = new MapMarkup[]
        {
            new MapMarkup(Resources.M_Antarctica,                Map.DM_Antarctica, Map.ELIM_Antarctica, Map.TDM_Antarctica),
            new MapMarkup(Resources.M_AntarcticaWinter,          Map.DM_Antarctica_Winter, Map.ELIM_Antarctica_Winter, Map.MSO_Antarctica_Winter, Map.TDM_Antarctica_Winter),
            new MapMarkup(Resources.M_Ayutthaya,                 Map.CTF_Ayutthaya, Map.ELIM_Ayutthaya),
            new MapMarkup(Resources.M_BlackForest,               Map.DM_BlackForest, Map.ELIM_BlackForest, Map.TDM_BlackForest),
            new MapMarkup(Resources.M_BlackForestWinter,         Map.DM_BlackForest_Winter, Map.ELIM_BlackForest_Winter, Map.MSO_BlackForest_Winter, Map.TDM_BlackForest_Winter),
            new MapMarkup(Resources.M_BlizzardWorld,             Map.AE_BlizzardWorld, Map.DM_BlizzardWorld, Map.SKIRM_BlizzardWorld, Map.TDM_BlizzardWorld),
            new MapMarkup(Resources.M_BlizzardWorldWinter,       Map.AE_BlizzardWorld_Winter, Map.DM_BlizzardWorld_Winter, Map.SKIRM_BlizzardWorld_Winter, Map.TDM_BlizzardWorld_Winter),
            new MapMarkup(Resources.M_Busan,                     Map.C_Busan, Map.SKIRM_Busan),
            new MapMarkup(Resources.M_Castillo,                  Map.DM_Castillo, Map.ELIM_Castillo, Map.TDM_Castillo),
            new MapMarkup(Resources.M_ChateauGuillard,           Map.DM_ChateauGuillard, Map.TDM_ChateauGuillard),
            new MapMarkup(Resources.M_ChateauGuillard_Halloween, Map.DM_ChateauGuillard_Halloween, Map.TDM_ChateauGuillard_Halloween),
            new MapMarkup(Resources.M_Dorado,                    Map.DM_Dorado, Map.E_Dorado, Map.SKIRM_Dorado, Map.TDM_Dorado),
            new MapMarkup(Resources.M_Eichenwalde,               Map.AE_Eichenwalde, Map.DM_Eichenwalde, Map.SKIRM_Eichenwalde, Map.TDM_Eichenwalde),
            new MapMarkup(Resources.M_Eichenwalde_Halloween,     Map.AE_Eichenwalde_Halloween, Map.DM_Eichenwalde_Halloween, Map.SKIRM_Eichenwalde_Halloween, Map.TDM_Eichenwalde_Halloween),
            new MapMarkup(Resources.M_Gibraltar,                 Map.E_Gibraltar, Map.SKIRM_Gibraltar),
            new MapMarkup(Resources.M_Hanamura,                  Map.A_Hanamura, Map.DM_Hanamura, Map.SKIRM_Hanamura, Map.TDM_Hanamura),
            new MapMarkup(Resources.M_HanamuraWinter,            Map.A_Hanamura_Winter, Map.DM_Hanamura_Winter, Map.SKIRM_Hanamura_Winter, Map.TDM_Hanamura_Winter),
            new MapMarkup(Resources.M_Hollywood,                 Map.AE_Hollywood, Map.DM_Hollywood, Map.SKIRM_Hollywood, Map.TDM_Hollywood),
            new MapMarkup(Resources.M_Hollywood_Halloween,       Map.AE_Hollywood_Halloween, Map.DM_Hollywood_Halloween, Map.SKIRM_Hollywood_Halloween, Map.TDM_Hollywood_Halloween),
            new MapMarkup(Resources.M_HorizonLunarColony,        Map.A_HorizonLunarColony, Map.DM_HorizonLunarColony, Map.SKIRM_HorizonLunarColony, Map.TDM_HorizonLunarColony),
            new MapMarkup(Resources.M_Ilios,                     Map.C_Ilios, Map.SKIRM_Ilios),
            new MapMarkup(Resources.M_Ilios_Lighthouse,          Map.CTF_Ilios_Lighthouse, Map.DM_Ilios_Lighthouse, Map.ELIM_Ilios_Lighthouse, Map.TDM_Ilios_Lighthouse),
            new MapMarkup(Resources.M_Ilios_Ruins,               Map.CTF_Ilios_Ruins, Map.DM_Ilios_Ruins, Map.ELIM_Ilios_Ruins, Map.TDM_Ilios_Ruins),
            new MapMarkup(Resources.M_Ilios_Well,                Map.CTF_Ilios_Well, Map.DM_Ilios_Well, Map.ELIM_Ilios_Well, Map.TDM_Ilios_Well),
            new MapMarkup(Resources.M_Junkertown,                Map.E_Junkertown, Map.SKIRM_Junkertown),
            new MapMarkup(Resources.M_KingsRow,                  Map.AE_KingsRow, Map.DM_KingsRow, Map.SKIRM_KingsRow, Map.TDM_KingsRow),
            new MapMarkup(Resources.M_KingsRowWinter,            Map.AE_KingsRow_Winter, Map.DM_KingsRow_Winter, Map.SKIRM_KingsRow_Winter, Map.TDM_KingsRow_Winter),
            new MapMarkup(Resources.M_Lijiang,                   Map.C_Lijiang, Map.SKIRM_Lijiang),
            new MapMarkup(Resources.M_Lijiang_ControlCenter,     Map.CTF_Lijiang_ControlCenter, Map.DM_Lijiang_ControlCenter, Map.ELIM_Lijiang_ControlCenter, Map.TDM_Lijiang_ControlCenter),
            new MapMarkup(Resources.M_Lijiang_Garden,            Map.CTF_Lijiang_Garden, Map.DM_Lijiang_Garden, Map.ELIM_Lijiang_Garden, Map.TDM_Lijiang_Garden),
            new MapMarkup(Resources.M_Lijiang_NightMarket,       Map.CTF_Lijiang_NightMarket, Map.DM_Lijiang_NightMarket, Map.ELIM_Lijiang_NightMarket, Map.TDM_Lijiang_NightMarket),
            new MapMarkup(Resources.M_Necropolis,                Map.DM_Necropolis, Map.ELIM_Necropolis, Map.TDM_Necropolis),
            new MapMarkup(Resources.M_Nepal,                     Map.C_Nepal, Map.SKIRM_Nepal),
            new MapMarkup(Resources.M_Nepal_Sanctum,             Map.CTF_Nepal_Sanctum, Map.DM_Nepal_Sanctum, Map.ELIM_Nepal_Sanctum, Map.TDM_Nepal_Sanctum),
            new MapMarkup(Resources.M_Nepal_Shrine,              Map.CTF_Nepal_Shrine, Map.DM_Nepal_Shrine, Map.ELIM_Nepal_Shrine, Map.TDM_Nepal_Shrine),
            new MapMarkup(Resources.M_Nepal_Village,             Map.CTF_Nepal_Village, Map.DM_Nepal_Village, Map.ELIM_Nepal_Village, Map.TDM_Nepal_Village),
            new MapMarkup(Resources.M_YH_Nepal_Village,          Map.YH_Nepal_Village),
            new MapMarkup(Resources.M_Numbani,                   Map.AE_Numbani, Map.SKIRM_Numbani),
            new MapMarkup(Resources.M_Oasis,                     Map.C_Oasis, Map.SKIRM_Oasis),
            new MapMarkup(Resources.M_Oasis_CityCenter,          Map.CTF_Oasis_CityCenter, Map.DM_Oasis_CityCenter, Map.ELIM_Oasis_CityCenter, Map.TDM_Oasis_CityCenter),
            new MapMarkup(Resources.M_Oasis_Gardens,             Map.CTF_Oasis_Gardens, Map.DM_Oasis_Gardens, Map.ELIM_Oasis_Gardens, Map.TDM_Oasis_Gardens),
            new MapMarkup(Resources.M_Oasis_University,          Map.CTF_Oasis_University, Map.DM_Oasis_University, Map.ELIM_Oasis_University, Map.TDM_Oasis_University),
            new MapMarkup(Resources.M_Petra,                     Map.DM_Petra, Map.TDM_Petra),
            new MapMarkup(Resources.M_Paris,                     Map.A_Paris, Map.A_Paris),
            new MapMarkup(Resources.M_Rialto,                    Map.E_Rialto, Map.SKIRM_Rialto),
            new MapMarkup(Resources.M_Route66,                   Map.E_Route66, Map.SKIRM_Route66),
            new MapMarkup(Resources.M_TempleOfAnubis,            Map.A_TempleOfAnubis, Map.DM_TempleOfAnubis, Map.SKIRM_TempleOfAnubis, Map.TDM_TempleOfAnubis),
            new MapMarkup(Resources.M_VolskayaIndustries,        Map.A_VolskayaIndustries, Map.DM_VolskayaIndustries, Map.SKIRM_VolskayaIndustries, Map.TDM_VolskayaIndustries),
        };
    }

    internal static class Timing
    {
        public static readonly int OPTION_MENU = 250;
        public static readonly int LOBBY_FADE = 1000;
    }

    internal class MapMarkup
    {
        public MapMarkup(Bitmap markup, params Map[] maps) : this(new DirectBitmap(markup, true), maps)
        {

        }
        public MapMarkup(DirectBitmap markup, params Map[] maps)
        {
            Markup = markup;
            Maps = maps;
        }
        public DirectBitmap Markup { get; private set; }
        public Map[] Maps { get; private set; }
    }
}