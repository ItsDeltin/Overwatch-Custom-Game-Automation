using System.Threading;

namespace Deltin.CustomGameAutomation
{
    partial class CustomGame
    {
        /// <summary>
        /// Invites a player to the game via battletag.
        /// </summary>
        /// <param name="playerName">Battletag of the player to invite. Is case sensitive. Ex: Tracer#1818</param>
        /// <param name="team">Team that the invited player joins.</param>
        /// <returns></returns>
        public bool InvitePlayer(string playerName, InviteTeam team = InviteTeam.Both)
        {
            updateScreen();
            // check if the add AI button is there.
            // because the invite button gets moved if it is/isnt there.
            if (DoesAddButtonExist())
            {
                LeftClick(Points.LOBBY_INVITE_IF_ADD_BUTTON_PRESENT, 250);
            }
            else
            {
                LeftClick(Points.LOBBY_INVITE_IF_ADD_BUTTON_NOT_PRESENT, 250);
            }

            LeftClick(Points.INVITE_VIA_BATTLETAG, 100);

            TextInput(playerName);

            if (team != InviteTeam.Both)
            {
                LeftClick(Points.INVITE_TEAM_DROPDOWN);
                if (team == InviteTeam.Blue)
                {
                    LeftClick(Points.INVITE_TEAM_BLUE);
                }
                else if (team == InviteTeam.Red)
                {
                    LeftClick(Points.INVITE_TEAM_RED);
                }
                else if (team == InviteTeam.Spectator)
                {
                    LeftClick(Points.INVITE_TEAM_SPECTATOR);
                }
            }

            Thread.Sleep(200);

            updateScreen();

            if (CompareColor(Points.INVITE_INVITE, Colors.CONFIRM, Fades.CONFIRM)) //fade was 30
            {
                LeftClick(Points.INVITE_INVITE);
                ResetMouse();
                return true;
            }
            else
            {
                LeftClick(Points.INVITE_BACK);
                ResetMouse();
                return false;
            }

        }
    }
}
