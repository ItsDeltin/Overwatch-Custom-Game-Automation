using System;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using Deltin.CustomGameAutomation;

namespace ZombieBot
{
    partial class Program
    {
        static bool MatchIsPublic = false;

        public static bool Pregame()
        {
            int prevPlayerCount = 0;
            Stopwatch pregame = new Stopwatch();
            Stopwatch skirmish = new Stopwatch();
            skirmish.Start();

            while (true)
            {
                if (Join == JoinType.Abyxa)
                    a.Update();

                if (skirmish.ElapsedMilliseconds >= 300 * 1000)
                {
                    cg.RestartGame();
                    prevPlayerCount = 0;
                    skirmish.Restart();
                    cg.Chat.SwapChannel(Channel.Match);
                }

                int totalPlayerCount = cg.TotalPlayerCount - 1; // Get total number of players in server

                // update server
                if (Join == JoinType.Abyxa)
                {
                    int invitedcount = cg.GetInvitedCount();
                    a.SetMode(0);
                    a.SetPlayerCount(totalPlayerCount - invitedcount);
                    a.SetInviteCount(invitedcount);
                }

                // invite players to game
                string[] queue = new string[0];
                if (Join == JoinType.Abyxa)
                {
                    queue = a.Queuelist(); // get list of player in queue

                    int addamount = 7 - (totalPlayerCount); // get the total number of players that can be added to the game.
                    for (int i = 0; i < addamount && i < queue.Length; i++)
                    {
                        // Make sure server doesn't invite more players than it needs to
                        if (totalPlayerCount < 7 && queue.Length > 0 && cg.SpectatorCount == 1) // if there is less than 7 players, invite players.
                        {
                            // Get player data
                            string[] data = queue[i].Split(' ');

                            // wait=true: check if there is enough players that are waiting and are ingame.
                            // wait=false: just invite them.
                            if ((totalPlayerCount + queue.Length >= minimumPlayers && data[1] == "true") || data[1] == "false")
                            {
                                Console.WriteLine("Inviting the player " + data[0] + "...");
                                // invite player to game
                                cg.InvitePlayer(data[0]); // invite player to game
                                a.RemoveFromQueue(data[0]); // remove player from queue
                            }
                        }
                        Thread.Sleep(500);
                        totalPlayerCount = cg.TotalPlayerCount;
                    }
                }

                // If players are in spectator and slots are available, switch them to blue/red
                var spectatorslots = cg.SpectatorSlots;
                if (spectatorslots.Count > 1 && cg.PlayerCount + cg.QueueCount < 7) 
                {
                    if (cg.BlueCount < 6) cg.Interact.SwapToBlue(spectatorslots[1]);
                    else if (cg.RedCount < 6) cg.Interact.SwapToRed(spectatorslots[1]);
                    Thread.Sleep(500);
                }

                // Send a message when someone joins
                var playerslots = cg.PlayerSlots;
                int loading = cg.GetInvitedCount();
                if (playerslots.Count - loading > prevPlayerCount)
                {
                    int wait = minimumPlayers - playerslots.Count;
                    if (wait > 1) cg.Chat.SendChatMessage("Welcome to Zombies! Waiting for " + wait + " more players. I am a bot, source is at the github repository ItsDeltin/Overwatch-Custom-Game-Automation");
                    if (wait == 1) cg.Chat.SendChatMessage("Welcome to Zombies! Waiting for " + wait + " more player. I am a bot, source is at the github repository ItsDeltin/Overwatch-Custom-Game-Automation");
                    if (wait < 0) cg.Chat.SendChatMessage("Welcome to Zombies! Game will be starting soon. I am a bot, source is at the github repository ItsDeltin/Overwatch-Custom-Game-Automation");
                    Thread.Sleep(500);
                }
                prevPlayerCount = playerslots.Count - loading;
                if (prevPlayerCount < 0)
                    prevPlayerCount = 0;

                int playercount = cg.PlayerCount;

                // if enough players join, start the timer.
                /*
                 * Start the game start timer if:
                 * 1. The timer isn't already running
                 * 2. and the number of players in blue and red is more greater than or equal to the minimum required players
                 * 3. and if the game isnt full, there isnt anyone in the queue.
                */

                if (
                    pregame.IsRunning == false // 1
                    && playercount >= minimumPlayers // 2
                    && (playercount < 7 && (queue.Length > 0 || cg.SpectatorCount > 1)) == false // 3
                    )
                {
                    cg.Chat.SendChatMessage("Enough players have joined, starting game in 15 seconds.");
                    pregame.Start();
                    Thread.Sleep(500);
                }

                // if too many players leave, cancel the countdown.
                if (pregame.IsRunning == true && playercount < minimumPlayers)
                {
                    cg.Chat.SendChatMessage("Players left, waiting for " + (minimumPlayers - playercount) + " more players, please wait.");
                    pregame.Reset();
                }

                if (Join == JoinType.ServerBrowser && MatchIsPublic == true && cg.TotalPlayerCount >= 7)
                {
                    MatchIsPublic = false;
                    cg.Settings.SetJoinSetting(Deltin.CustomGameAutomation.Join.InviteOnly);
                }

                else if (Join == JoinType.ServerBrowser && MatchIsPublic == false && cg.TotalPlayerCount < 7)
                {
                    MatchIsPublic = true;
                    cg.Settings.SetJoinSetting(Deltin.CustomGameAutomation.Join.Everyone);
                }

                // if the amount of players equals 7 or the queue list is empty and there is enough players,
                // and the pregame timer elapsed 15 seconds,
                // and there is no one invited and loading,
                // start the game.
                if ((playercount >= 7 || (queue.Length == 0 && playercount >= minimumPlayers)) && pregame.ElapsedMilliseconds >= 15 * 1000 && cg.GetInvitedCount() == 0)
                {
                    if (Join == JoinType.Abyxa)
                    {
                        a.SetMode(-1);
                        a.SetPlayerCount(playercount);
                        a.SetInviteCount(0);
                    }

                    skirmish.Reset();
                    pregame.Reset();
                    prevPlayerCount = 0;

                    if (Join == JoinType.ServerBrowser) cg.Settings.SetJoinSetting(Deltin.CustomGameAutomation.Join.InviteOnly);
                    MatchIsPublic = false;

                    cg.SendServerToLobby();

                    // If players are in spectator and slots are available, switch spectator to blue.
                    playerslots = cg.PlayerSlots;
                    spectatorslots = cg.SpectatorSlots;
                    for (int i = 1; i < spectatorslots.Count && playerslots.Count + cg.QueueCount < 7; i++)
                    {
                        cg.Interact.SwapToBlue(spectatorslots[i]);
                        cg.WaitForSlotUpdate();
                        Thread.Sleep(500);
                        playerslots = cg.PlayerSlots;
                        spectatorslots = cg.SpectatorSlots;
                    }

                    // If there is too many players, swap some to spectators. If they can't be swapped to spectators, remove them from the game.
                    for (int i = playerslots.Count + cg.QueueCount; i > 7; i--)
                    {
                        int remove = playerslots[rnd.Next(playerslots.Count)];
                        bool swap = cg.Interact.SwapToSpectators(remove);
                        if (!swap)
                            cg.Interact.RemoveFromGame(remove);
                        Thread.Sleep(500);
                        playerslots = cg.PlayerSlots;
                        i = playerslots.Count + cg.QueueCount + 1;
                    }

                    // Vote for map.
                    int[] votemap = new int[3]; // The index of maps that can be voted for. 3 is the amount of maps chosen that can be voted for.
                    // Choose random maps to be added to the votemap variable.
                    for (int i = 0; i < votemap.Length; i++)
                    {
                        int choose;
                        while (true)
                        {
                            // Make sure there are no duplicates when choosing the next map to be added to the votemap array
                            choose = rnd.Next(maps.Length);
                            if (votemap.Contains(choose))
                                continue;
                            break;
                        }
                        votemap[i] = choose;
                    }
                    string type = "Vote for map! (15 seconds)                                      " + mapsSend[votemap[0]] + " - $VOTE 1                               " + mapsSend[votemap[1]] + " - $VOTE 2                               " + mapsSend[votemap[2]] + " - $VOTE 3";
                    cg.Chat.SendChatMessage(type);
                    // Listen for chat commands for 15 seconds.
                    cg.Commands.Listen = true;
                    Thread.Sleep(15000);
                    cg.Commands.Listen = false;
                    // Get results
                    int[] results = new int[3];
                    var commands = cg.Commands.ExecutedCommands;
                    for (int i = 0; i < commands.Count; i++)
                    {
                        string[] commandSplit = commands[i].Command.Split(' ');
                        if (commandSplit.Length >= 2)
                            if (commandSplit[0] == "$VOTE")
                            {
                                int votefor = Int32.Parse(commands[i].Command.Split(' ')[1]) - 1;
                                if (votefor >= 0 && votefor < results.Length)
                                    results[votefor]++;
                            }
                    }
                    int winningmap = votemap[results.ToList().IndexOf(results.Max())];
                    cg.Commands.DisposeAllExecutedCommands();
                    cg.Chat.SendChatMessage(String.Format("{0}: {1} votes, {2}: {3} votes, {4}: {5} votes", mapsSend[votemap[0]], results[0], mapsSend[votemap[1]], results[1], mapsSend[votemap[2]], results[2]));
                    cg.Chat.SendChatMessage("Next map: " + mapsSend[winningmap]);
                    Map mapid = Map.MapIDFromName(maps[winningmap]);
                    cg.ToggleMap(ToggleAction.DisableAll, mapid);
                    // Update map on website if jointype is Abyxa.
                    if (Join == JoinType.Abyxa)
                        a.SetMap(mapsSend[winningmap].ToLower());

                    // Swap everyone in red to blue.
                    var redslots = cg.RedSlots;
                    while (redslots.Count > 0)
                    {
                        cg.Interact.SwapToBlue(redslots[0]);
                        redslots = cg.RedSlots;
                    }

                    cg.AI.AddAI(AIHero.McCree, Difficulty.Easy, BotTeam.Red, 6); // fill team 2 with mccree bots

                    Thread.Sleep(1500);

                    int zombies = 2; // if there is less than 6 players, choose one zombie

                    int attempts = 0;

                    while (zombies > 0)
                    {
                        try
                        {
                            if (attempts >= 5)
                            {
                                cg.AI.RemoveAllBotsAuto();
                                return false;
                            }
                            var blueslots = cg.BlueSlots;
                            int choose = rnd.Next(0, blueslots.Count);
                            if (cg.Interact.SwapToRed(blueslots[choose]))
                                zombies--;
                            else
                                attempts++;
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            attempts++;
                        }
                    }

                    cg.Chat.SendChatMessage("If you can't move, you are a zombie. You will be able to move when the preperation phase is over.");
                    Thread.Sleep(5000);
                    cg.Chat.SendChatMessage("Survivors win when time runs out. Survivors are converted to zombies when they die. Zombies win when all survivors are converted.");
                    Thread.Sleep(5000);
                    cg.Chat.SendChatMessage("Zombies will be released when preperation phase is over.");

                    // Start game
                    cg.Chat.SendChatMessage("Starting game...");

                    cg.StartGame();

                    if (Join == JoinType.Abyxa)
                    {
                        a.SetGameEnd(DateTime.UtcNow.AddMinutes(5.5));
                        a.SetMode(1);
                        a.SetPlayerCount(cg.PlayerCount + cg.QueueCount - 6);
                    }

                    cg.Chat.SendChatMessage("Zombies will be released in 30 seconds.");

                    return true;
                }
            }
        }
    }
}