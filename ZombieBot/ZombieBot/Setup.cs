﻿using System;
using Deltin.CustomGameAutomation;

namespace ZombieBot
{
    partial class Program
    {
        public static void Setup(bool initial)
        {
            if (initial)
            {
                cg.GameSettings.SetGameName("Zombies - Infection");
                cg.GameSettings.SetTeamName(PlayerTeam.Blue, "Survivors");
                cg.GameSettings.SetTeamName(PlayerTeam.Red, "Zombies");

                int moderatorSlot = cg.PlayerInfo.ModeratorSlot();

                if (moderatorSlot != -1 && moderatorSlot != 12)
                {
                    cg.Interact.Move(moderatorSlot, 12);
                }
            }

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
