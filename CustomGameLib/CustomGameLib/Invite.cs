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
            if (addbutton())
            {
                LeftClick(778, 180, 250); // click invite
            }
            else
            {
                LeftClick(835, 180, 250); // click invite
            }

            LeftClick(572, 171, 100); // click via battletag

            TextInput(playerName); // type the playername

            if (team != InviteTeam.Both)
            {
                LeftClick(475, 398);
                if (team == InviteTeam.Blue) LeftClick(475, 447); // click team1
                if (team == InviteTeam.Red) LeftClick(475, 466); // click team2
                if (team == InviteTeam.Spectator) LeftClick(475, 483); // click spectator
            }

            Thread.Sleep(200);

            updateScreen();

            if (bmp.CompareColor(460, 434, CALData.ConfirmColor, 30))
            {
                LeftClick(460, 434); // invite player
                ResetMouse();
                return true;
            }
            else
            {
                LeftClick(412, 434); // click back
                ResetMouse();
                return false;
            }

        }
        
    }
}
