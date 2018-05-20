using System;
using Deltin.CustomGameAutomation;

namespace ZombieBot
{
    partial class Program
    {
        public static void Setup()
        {
            int map = rnd.Next(maps.Length);
            Console.WriteLine("Map chosen: " + maps[map]);
            cg.Maps.ToggleMap(ToggleAction.DisableAll, CustomGame.CG_Maps.MapIDFromName(maps[map]));

            // Update map on website if jointype is abyxa.
            if (Join == JoinType.Abyxa)
                a.SetMap(mapsSend[map].ToLower());

            // Make game public if jointype is serverbrowser and there is less than 7 players.
            if (Join == JoinType.ServerBrowser)
            {
                if (cg.TotalPlayerCount < 7)
                {
                    cg.GameSettings.SetJoinSetting(Deltin.CustomGameAutomation.Join.Everyone);
                    MatchIsPublic = true;
                }
                else
                {
                    cg.GameSettings.SetJoinSetting(Deltin.CustomGameAutomation.Join.InviteOnly);
                    MatchIsPublic = false;
                }
            }

            cg.StartGame();

            cg.Chat.SwapChannel(Channel.Match);
        }
    }
}
