using System;
using System.Threading;
using System.Linq;
using Deltin.CustomGameAutomation;

namespace ZombieBot
{
    partial class Program
    {
        public static void Setup(Abyxa abyxa, bool serverBrowser, CustomGame cg, Map[] maps, int preset, string name)
        {
            cg.AI.RemoveAllBotsAuto();

            if (abyxa != null)
                cg.Settings.SetJoinSetting(Join.InviteOnly);

            if (preset > -1)
                cg.Settings.LoadPreset(preset);

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
            UpdateMap(abyxa, cg);

            cg.StartGame();

            cg.Chat.SwapChannel(Channel.Match);

            // Make game publc if there is less than 7 players.
            if (serverBrowser)
            {
                if (cg.AllCount < 7)
                {
                    cg.Settings.SetJoinSetting(Join.Everyone);
                    MatchIsPublic = true;
                }
                else
                {
                    cg.Settings.SetJoinSetting(Join.InviteOnly);
                    MatchIsPublic = false;
                }
            }
        }
    }
}
