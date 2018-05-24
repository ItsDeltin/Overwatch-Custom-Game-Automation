using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Diagnostics;

namespace ZombieBot
{
    partial class Program
    {
        public static void Ingame()
        {
            int[] messageStamps = new int[] { 300, 240, 180, 120, 60, 30, 15 };
            int[] timeStamps = new int[] { 30, 60, 60, 60, 60, 30, 15 };
            int ti = 0;
            Stopwatch game = new Stopwatch();
            game.Start();

            while (true)
            {
                Thread.Sleep(10);

                if (Join == JoinType.Abyxa)
                    a.Update();

                // Swap killed survivors to red
                List<int> playersDead = cg.PlayerInfo.PlayersDead();
                for (int i = 0; i < playersDead.Count(); i++)
                    if (playersDead[i] < 6)
                        cg.Interact.SwapToRed(playersDead[i]);

                // end game if winning condition is met
                bool endgame = false;
                if (game.ElapsedMilliseconds >= 330 * 1000) // if time runs out, survivors win
                {
                    cg.Chat.Chat("The survivors defend long enough for help to arrive. Survivors win.");
                    endgame = true;
                    Thread.Sleep(2000);
                }
                if (cg.BlueCount == 0) // blue is empty, zombies win
                {
                    cg.Chat.Chat("The infection makes its way to the last human. Zombies win.");
                    endgame = true;
                    Thread.Sleep(2000);
                }
                if (endgame == true)
                {
                    cg.Chat.Chat("Resetting, please wait...");

                    game.Reset();

                    // ti will equal 0 if the game ends before mccree bots are removed, so remove the bots.
                    if (ti == 0)
                    {
                        cg.AI.RemoveAllBotsAuto();
                    }
                    Thread.Sleep(500);

                    ti = 0;

                    cg.RestartGame();

                    if (cg.TotalPlayerCount < 7 && Join == JoinType.ServerBrowser)
                    {
                        MatchIsPublic = true;
                        cg.GameSettings.SetJoinSetting(Deltin.CustomGameAutomation.Join.Everyone);
                    }
                    else
                        MatchIsPublic = false;

                    Thread.Sleep(1000);
                    return;
                }

                /*
                 * ti is short for time index
                 * the ti variable determines which time remaining message to use from the timeStamps variable.
                 */
                if (ti < timeStamps.Length)
                {
                    if (game.ElapsedMilliseconds >= Extra.SquashArray(timeStamps, ti) * 1000)
                    {
                        if (messageStamps[ti] > 60) cg.Chat.Chat((messageStamps[ti] / 60) + " minutes remaining.");
                        if (messageStamps[ti] == 60) cg.Chat.Chat("1 minute remaining.");
                        if (messageStamps[ti] < 60) cg.Chat.Chat(messageStamps[ti] + " seconds remaining.");
                        ti++;
                        if (ti == 1)
                        {
                            // remove bots
                            cg.AI.RemoveAllBotsAuto();
                            cg.Chat.Chat("Zombies have been released. Good luck.");

                            // Swap blue players who didn't choose a hero to red if the version is TDM.
                            if (version == 1)
                            {
                                var blueslots = cg.BlueSlots;
                                for (int i = 0; i < blueslots.Count; i++)
                                    if (cg.PlayerInfo.IsHeroChosen(blueslots[i]) == false)
                                        cg.Interact.SwapToRed(blueslots[i]);
                            }
                        }
                        Thread.Sleep(500);
                    }
                }

                if (Join == JoinType.Abyxa)
                {
                    a.SetSurvivorCount(cg.BlueCount.ToString());
                }

            }
        }
    }
}
