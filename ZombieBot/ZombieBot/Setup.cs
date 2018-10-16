using System;
using System.Threading;
using System.Linq;
using Deltin.CustomGameAutomation;

namespace ZombieBot
{
    partial class Program
    {
        public static void Setup(CustomGame cg, Map[] maps, int preset, string name)
        {
            cg.AI.RemoveAllBotsAuto();

            if (Join == JoinType.Abyxa)
                cg.Settings.SetJoinSetting(Deltin.CustomGameAutomation.Join.InviteOnly);

            if (preset > -1)
                cg.Settings.LoadPreset(preset);

            try
            {
                cg.Settings.SetGameName(name);
            }
            catch (Exception)
            {
                cg.Settings.SetGameName("Zombies - Infection");
            }
            cg.Settings.SetTeamName(Team.Blue, "Survivors");
            cg.Settings.SetTeamName(Team.Red, "Zombies");

            int moderatorSlot = cg.PlayerInfo.ModeratorSlot();
            if (moderatorSlot != -1)
            {
                if (moderatorSlot != 12)
                    cg.Interact.Move(moderatorSlot, 12);
            }
            else
            {
                var allSlots = cg.AllSlots;
                if (allSlots.Count == 1 && allSlots[0] != 12)
                    cg.Interact.Move(allSlots[0], 12);
            }

            cg.ToggleMap(ToggleAction.EnableAll);
            Thread.Sleep(500);

            // Update map on website if jointype is abyxa.
            if (Join == JoinType.Abyxa)
            {
                string currentMap = cg.GetCurrentMap()?.FirstOrDefault()?.ShortName;
                if (currentMap != null)
                    a.SetMap(currentMap);
            }

            // Make game public if jointype is serverbrowser and there is less than 7 players.
            if (Join == JoinType.ServerBrowser)
            {
                if (cg.AllCount < 7)
                {
                    cg.Settings.SetJoinSetting(Deltin.CustomGameAutomation.Join.Everyone);
                    MatchIsPublic = true;
                }
                else
                {
                    cg.Settings.SetJoinSetting(Deltin.CustomGameAutomation.Join.InviteOnly);
                    MatchIsPublic = false;
                }
            }

            cg.StartGame();

            cg.Chat.SwapChannel(Channel.Match);
        }
    }
}
