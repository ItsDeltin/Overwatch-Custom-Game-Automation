using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace Deltin.CustomGameAutomation
{
    partial class CustomGame
    {
        private static class CALData
        {
            public static int[] WhiteColor = new int[] { 191, 191, 191 };

            public static int[] DeadPlayerColor = new int[] { 118, 74, 76 }; // Spectator UI red X
            public static int DeadPlayerFade = 15;

            //public static int[] HeroChosenLocations = new int[] { 91, 140, 189, 239, 288, 337, 644, 695, 744, 792, 843, 888 };
            public static int HeroChosenY = 75;
            public static int[] HeroChosenBlue = new int[] { 83, 110, 123 };
            public static int[] HeroChosenRed = new int[] { 114, 77, 81 };
            public static int HeroChosenFade = 10;

            public static int[] ModeratorIconColor = new int[] { 143, 155, 80 }; // Moderator icon aka green crown
            public static int[] SpectatorModeratorIconColor = new int[] { 149, 183, 89 }; // Moderator icon color for spectators.
            public static int[] ConfirmColor = new int[] { 176, 141, 89 }; // The yellow confirm color.
            public static int[] LobbyChangeColor = new int[] { 126, 158, 181 };

            // * Start Game button
            // <image url="$(ProjectDir)\ImageComments\ColorAndLocationData.cs\StartGame.png" scale="1" />
            public static int[] StartGameColor = new int[] { 150, 127, 96 }; // The yellow button in the lobby that starts the game
            public static int StartGameFade = 30;
            public static Point StartGameLocation = new Point(426, 457);
            // *

            public static int[] LoadablePreset = new int[] { 126, 128, 134 };

            // * Settings Error
            // <image url="$(ProjectDir)\ImageComments\ColorAndLocationData.cs\Error.png" scale="1" />
            // Works with every invalid setting occurence, for example no hero chosen, no map chosen, or no mode chosen.
            public static Point ErrorLocation = new Point(522, 320);
            public static int[] ErrorColor = new int[] { 151, 119, 81 };
            public static int ErrorFade = 20;
            // *
        }
    }

    internal class Points
    {
        public static readonly Point RESET_POINT = new Point(500, 500);

        public static readonly Point PRE_MAIN_MENU_LOGIN = new Point(419, 473);

        public static readonly Point MAIN_MENU_OVERWATCH_WATERMARK = new Point(53, 68);

        public static readonly Point OPTIONS_APPLY = new Point(400, 500);
        public static readonly Point OPTIONS_BACK = new Point(500, 500);

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
        public static readonly Point LOBBY_SWAP_ALL_IF_ADD_BUTTON_PRESENT = new Point(617, 180);
        public static readonly Point LOBBY_SWAP_ALL_IF_ADD_BUTTON_NOT_PRESENT = new Point(678, 180);
        public static readonly Point LOBBY_MY_PLAYER_ICON = new Point(744, 62);
        public static readonly Point LOBBY_START_GAMEMODE = new Point(599, 456);
        public static readonly Point LOBBY_BLUE_NAME = new Point(159, 229);
        public static readonly Point LOBBY_RED_NAME = new Point(458, 230);

        public static readonly Point EDIT_AI_CONFIRM = new Point(447, 354);

        public static readonly Point INVITE_INVITE = new Point(460, 434);
        public static readonly Point INVITE_BACK = new Point(412, 434);
        public static readonly Point INVITE_VIA_BATTLETAG = new Point(572, 171);
        public static readonly Point INVITE_TEAM_DROPDOWN = new Point(475, 398);
        public static readonly Point INVITE_TEAM_BLUE = new Point(475, 447);
        public static readonly Point INVITE_TEAM_RED = new Point(475, 466);
        public static readonly Point INVITE_TEAM_SPECTATOR = new Point(475, 483);

        public static readonly Point SETTINGS_HEROES = new Point(351, 311);
        public static readonly Point SETTINGS_PRESETS = new Point(103, 183);
        public static readonly Point SETTINGS_LOBBY = new Point(297, 183);

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

        public static readonly Point SETTINGS_LOBBY_BLUE_MAX_PLAYERS = new Point(500, 269);
        public static readonly Point SETTINGS_LOBBY_RED_MAX_PLAYERS = new Point(500, 290);
        public static readonly Point SETTINGS_LOBBY_FFA_MAX_PLAYERS = new Point(500, 311);
        public static readonly Point SETTINGS_LOBBY_MAX_SPECTATORS = new Point(500, 333);


        public static readonly Point PRESETS_CONFIRM = new Point(480, 327);

        public static readonly Point ENDING_COMMEND_DEFEAT = new Point(53, 62);
    }

    internal class Rectangles
    {
        public static readonly Rectangle ENTIRE_SCREEN = new Rectangle(0, 0, 960, 540);

        public static readonly Rectangle LOBBY_CHATBOX = new Rectangle(50, 461, 169, 26);
        public static readonly Rectangle LOBBY_CAREER_PROFILE = new Rectangle(46, 101, 265, 82);


    }

}
