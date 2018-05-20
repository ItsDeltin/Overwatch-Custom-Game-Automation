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

            public static int[] HeroChosenColor = new int[] { 122, 124, 125 }; // Hero chosen color
            public static int[] HeroChosenLocations = new int[] { 45, 94, 143, 192, 241, 290, 610, 659, 708, 757, 807, 856 };
            public static int HeroChosenY = 95;
            public static int HeroChosenFade = 5;

            public static int[] ModeratorIconColor = new int[] { 156, 188, 111 }; // Moderator icon aka green crown
            public static int[] SpectatorModeratorIconColor = new int[] { 149, 183, 89 }; // Moderator icon color for spectators.
            public static int[] ConfirmColor = new int[] { 176, 141, 89 }; // The yellow confirm color.
            public static int[] LobbyChangeColor = new int[] { 126, 158, 181 };

            public static Point ChatLocation = new Point(50, 505);
            public static int ChatFade = 20;
            public static int[] TeamChatColor = new int[] { 65, 139, 162 };
            public static int[] MatchChatColor = new int[] { 161, 122, 91 };
            public static int[] GeneralChatColor = new int[] { 161, 161, 162 };
            public static int[] GroupChatColor = new int[] { 0, 0, 0 }; // TODO: Get this color
            public static int[] PrivateMessageChatColor = new int[] { 160, 118, 167 };
            public static int[][] ChatColors = new int[][] { TeamChatColor, MatchChatColor, GeneralChatColor, GroupChatColor, PrivateMessageChatColor };

            public static int[] StartGameColor = new int[] { 150, 127, 96 }; // The yellow button in the lobby that starts the game
            public static int StartGameFade = 30;
            public static Point StartGameLocation = new Point(426, 457);

            public static Point ErrorLocation = new Point(522, 320);
            public static int[] ErrorColor = new int[] { 151, 119, 81 };
            public static int ErrorFade = 20;
        }
    }
}
