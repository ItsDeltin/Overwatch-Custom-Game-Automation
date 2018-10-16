using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System;
using Deltin.CustomGameAutomation;

namespace ZombieBot
{
    partial class Program
    {
        public static bool Ingame(CustomGame cg)
        {
            int[] messageStamps = new int[] { 300, 240, 180, 120, 60, 30, 15 };
            int[] timeStamps = new int[] { 30, 60, 60, 60, 60, 30, 15 };
            int ti = 0;
            Stopwatch game = new Stopwatch();
            game.Start();

            new Task(() => 
            {
                cg.Chat.SendChatMessage("If you can't move, you are a zombie. You will be able to move when the preperation phase is over.");
                Thread.Sleep(5000);
                cg.Chat.SendChatMessage("Survivors win when time runs out. Survivors are converted to zombies when they die. Zombies win when all survivors are converted.");
                Thread.Sleep(5000);
                cg.Chat.SendChatMessage("Zombies will be released when preperation phase is over.");
            }).Start();

            while (true)
            {
                Thread.Sleep(100);

                if (Join == JoinType.Abyxa)
                    a.Update();

                if (cg.IsDisconnected() || cg.HasExited())
                    return false;

                // Swap killed survivors to red
                List<int> playersDead = cg.PlayerInfo.PlayersDead();
                for (int i = 0; i < playersDead.Count(); i++)
                    if (CustomGame.IsSlotBlue(playersDead[i]))
                        cg.Interact.SwapToRed(playersDead[i]);

                // end game if winning condition is met
                bool endgame = false;
                if (game.ElapsedMilliseconds >= 330 * 1000) // if time runs out, survivors win
                {
                    Console.WriteLine("Game Over: Survivors win.");
                    cg.Chat.SendChatMessage("The survivors defend long enough for help to arrive. Survivors win.");
                    endgame = true;
                    Thread.Sleep(2000);
                }
                if (cg.BlueCount == 0) // blue is empty, zombies win
                {
                    Console.WriteLine("Game Over: Zombies win.");
                    cg.Chat.SendChatMessage("The infection makes its way to the last human. Zombies win.");
                    endgame = true;
                    Thread.Sleep(2000);
                }
                if (endgame == true)
                {
                    cg.Chat.SendChatMessage("Resetting, please wait...");

                    // ti will equal 0 if the game ends before mccree bots are removed, so remove the bots.
                    if (ti == 0)
                        cg.AI.RemoveAllBotsAuto();
                    Thread.Sleep(500);

                    cg.ToggleMap(ToggleAction.EnableAll);

                    cg.RestartGame();

                    if (Join == JoinType.Abyxa)
                    {
                        string currentMap = cg.GetCurrentMap()?.FirstOrDefault()?.ShortName;
                        if (currentMap != null)
                            a.SetMap(currentMap);
                    }

                    if (cg.AllCount < 7 && Join == JoinType.ServerBrowser)
                    {
                        MatchIsPublic = true;
                        cg.Settings.SetJoinSetting(Deltin.CustomGameAutomation.Join.Everyone);
                    }
                    else
                        MatchIsPublic = false;

                    Thread.Sleep(1000);
                    return true;
                }

                /*
                 * ti is short for time index
                 * the ti variable determines which time remaining message to use from the timeStamps variable.
                 */
                if (ti < timeStamps.Length)
                {
                    if (game.ElapsedMilliseconds >= Extra.SquashArray(timeStamps, ti) * 1000)
                    {
                        if (messageStamps[ti] > 60) cg.Chat.SendChatMessage((messageStamps[ti] / 60) + " minutes remaining.");
                        if (messageStamps[ti] == 60) cg.Chat.SendChatMessage("1 minute remaining.");
                        if (messageStamps[ti] < 60) cg.Chat.SendChatMessage(messageStamps[ti] + " seconds remaining.");
                        ti++;
                        if (ti == 1)
                        {
                            // remove bots
                            cg.AI.RemoveAllBotsAuto();
                            cg.Chat.SendChatMessage("Zombies have been released. Good luck.");

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
