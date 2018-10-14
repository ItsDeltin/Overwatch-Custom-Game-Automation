using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

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

        public static readonly int LOBBY_START_GAME = 30;
        public static readonly int LOBBY_CHANGE = 50;
        public static readonly int LOBBY_INVITE_PLAYERS_TO_GROUP_COMPARE = 15;

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

        public static readonly Point ENDING_COMMEND_DEFEAT = new Point(53, 62);

        public static readonly Point EXIT_TO_DESKTOP = new Point(130, 505);
    }

    internal static class Rectangles
    {
        public static readonly Rectangle ENTIRE_SCREEN = new Rectangle(0, 0, 960, 540);

        public static readonly Rectangle LOBBY_CHATBOX = new Rectangle(50, 461, 169, 26);
        public static readonly Rectangle LOBBY_CAREER_PROFILE = new Rectangle(46, 101, 265, 82);

        public static readonly Rectangle SETTINGS_PRESET_OPTION = new Rectangle(0, 0, 128, 20);
    }

    internal static class Distances
    {
        public static readonly int LOBBY_SLOT_DISTANCE = 29;
        public static readonly int LOBBY_TEAM_SLOT_DISTANCE = 319; // 322
        public static readonly int LOBBY_SPECTATOR_SLOT_DISTANCE = 13;
        public static readonly int LOBBY_QUEUE_OFFSET = 6; // Did you know that the queue list is 6 pixels higher than the spectator list?
    }

    internal static class Markups
    {
        public static readonly Bitmap REMOVE_FROM_GAME    = Properties.Resources.remove_from_game;
        public static readonly Bitmap SWAP_TO_RED         = Properties.Resources.swap_to_red;
        public static readonly Bitmap SWAP_TO_BLUE        = Properties.Resources.swap_to_blue;
        public static readonly Bitmap SWAP_TO_SPECTATORS  = Properties.Resources.swap_to_spectators;
        public static readonly Bitmap SWAP_TO_NEUTRAL     = Properties.Resources.swap_to_neutral;
        public static readonly Bitmap REMOVE_ALL_BOTS     = Properties.Resources.remove_all_bots;
        public static readonly Bitmap VIEW_CAREER_PROFILE = Properties.Resources.view_career_profile;
        public static readonly Bitmap SEND_FRIEND_REQUEST = Properties.Resources.send_friend_request;
        public static readonly Bitmap REMOVE_FRIEND       = Properties.Resources.remove_friend;

        public static readonly Bitmap[] DIFFICULTY_MARKUPS = new Bitmap[]
        {
            Properties.Resources.easy_difficulty,
            Properties.Resources.medium_difficulty,
            Properties.Resources.hard_difficulty
        };
        public static readonly Bitmap[] HERO_MARKUPS = new Bitmap[]
        {
            Properties.Resources.ana_markup, // Ana
            Properties.Resources.bastion_markup, // Bastion
            Properties.Resources.brigitte_markup, // Brigitte
            Properties.Resources.dva_markup, // Dva
            Properties.Resources.doomfist_markup, // Doomfist
            Properties.Resources.gengi_markup, // Genji
            Properties.Resources.hanzo_markup, // Hanzo
            Properties.Resources.junkrat_markup, // Junkrat
            Properties.Resources.lucio_markup, // Lucio
            Properties.Resources.mccree_markup, // McCree
            Properties.Resources.mei_markup, // Mei
            Properties.Resources.mercy_markup, // Mercy
            Properties.Resources.moira_markup, // Moira
            Properties.Resources.orisa_markup, // Orisa
            Properties.Resources.pharah_markup, // Pharah
            Properties.Resources.reaper_markup, // Reaper
            Properties.Resources.reinhardt_markup, // Reinhardt
            Properties.Resources.roadhog_markup, // Roadhog
            Properties.Resources.soldier_markup, // Soldier: 76
            Properties.Resources.sombra_markup, // Sombra
            Properties.Resources.symmetra_markup, // Symmetra
            Properties.Resources.torbjorn_markup, // Torbjorn
            Properties.Resources.tracer_markup, // Tracer
            Properties.Resources.widowmaker_markup, // Widowmaker
            Properties.Resources.winston_markup, // Winston
            Properties.Resources.wreckingball_markup, // Wrecking Ball
            Properties.Resources.zarya_markup, // Zarya
            Properties.Resources.zenyatta_markup // Zenyatta
        };
    }
}