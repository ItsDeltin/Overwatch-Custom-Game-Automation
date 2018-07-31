using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Deltin.CustomGameAutomation
{
    internal class Points
    {
        public static readonly Point PRE_MAIN_MENU_LOGIN = new Point(419, 473);

        public static readonly Point MAIN_MENU_OVERWATCH_WATERMARK = new Point(53, 68);

        public static readonly Point LOBBY_ADD_AI = new Point(835, 182);

        public static readonly Point LOBBY_MY_PLAYER_ICON = new Point(744, 62);

        public static readonly Point LOBBY_CHATBOX = new Point(105, 504);
        public static readonly Point LOBBY_CHAT_TYPE_INDICATOR = new Point(50, 505);

        public static readonly Point EDIT_AI_CONFIRM = new Point(447, 354);
    }

    internal class Rectangles
    {
        public static readonly Rectangle LOBBY_CHATBOX = new Rectangle(50, 461, 169, 26);

        public static readonly Rectangle LOBBY_CAREER_PROFILE = new Rectangle(46, 101, 265, 82);
    }
}
