using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace Deltin.CustomGameAutomation
{
    internal static class CALData
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

        // * Settings Error
        // <image url="$(ProjectDir)\ImageComments\ColorAndLocationData.cs\Error.png" scale="1" />
        // Works with every invalid setting occurence, for example no hero chosen, no map chosen, or no mode chosen.
        public static Point ErrorLocation = new Point(522, 320);
        public static int[] ErrorColor = new int[] { 151, 119, 81 };
        public static int ErrorFade = 20;
        // *
    }
}
