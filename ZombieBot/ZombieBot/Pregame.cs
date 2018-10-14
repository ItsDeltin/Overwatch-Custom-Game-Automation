using System;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using Deltin.CustomGameAutomation;

namespace ZombieBot
{
    partial class Program
    {
        static bool MatchIsPublic = false;

        public static bool Pregame(CustomGame cg, InfectionMap[] maps)
        {
            int prevPlayerCount = 0;
            Stopwatch pregame = new Stopwatch();
            Stopwatch skirmish = new Stopwatch();
            skirmish.Start();

            while (true)
            {
                if (Join == JoinType.Abyxa)
                    a.Update();

                if (cg.IsDisconnected() || cg.HasExited())
                    return false;

                if (skirmish.ElapsedMilliseconds >= 300 * 1000)
                {
                    cg.RestartGame();
                    prevPlayerCount = 0;
                    skirmish.Restart();
                    cg.Chat.SwapChannel(Channel.Match);
                }

                int totalPlayerCount = cg.AllCount - 1; // Get total number of players in server
                
                int addamount = 7 - totalPlayerCount; // get the total number of players that can be added to the game.

                var spectatorslots = cg.SpectatorSlots;
                // If players are in spectator and slots are available, switch them to blue/red
                for (int i = 0; i < addamount && i < spectatorslots.Count - 1; i++)
                {
                    if (cg.Interact.SwapToBlue(spectatorslots[i - 1]))
                        addamount--;
                    else if (cg.Interact.SwapToRed(spectatorslots[i - 1]))
                        addamount--;
                }

                // invite players to game
                string[] queue = new string[0];
                if (Join == JoinType.Abyxa)
                {
                    queue = a.Queuelist(); // get list of player in queue

                    for (int i = 0; i < addamount && i < queue.Length; i++)
                    {
                        // Get player data
                        string[] data = queue[i].Split(' ');

                        // wait=true: check if there is enough players that are waiting and are ingame.
                        // wait=false: just invite them.
                        if ((totalPlayerCount + queue.Length >= minimumPlayers && data[1] == "true") || data[1] == "false")
                        {
                            Console.WriteLine("Inviting the player " + data[0] + "...");
                            // invite player to game
                            cg.InvitePlayer(data[0], Team.BlueAndRed); // invite player to game
                            cg.WaitForSlotUpdate();
                            totalPlayerCount = cg.AllCount - 1;
                            a.RemoveFromQueue(data[0]); // remove player from queue
                        }
                    }
                }

                int invitedcount = cg.GetInvitedCount();
                int playingCount = cg.GetSlots(SlotFlags.BlueTeam | SlotFlags.RedTeam | SlotFlags.Queue).Count - invitedcount;
                int allCountWithoutInvited = totalPlayerCount - invitedcount;

                // update server
                if (Join == JoinType.Abyxa)
                {
                    a.SetMode(0);
                    a.SetPlayerCount(allCountWithoutInvited);
                    a.SetInviteCount(invitedcount);
                }

                // Send a message when someone joins
                if (allCountWithoutInvited > prevPlayerCount)
                {
                    int wait = minimumPlayers - totalPlayerCount;
                    if (wait > 1) cg.Chat.SendChatMessage("Welcome to Zombies! Waiting for " + wait + " more players. I am a bot, source is at the github repository ItsDeltin/Overwatch-Custom-Game-Automation");
                    if (wait == 1) cg.Chat.SendChatMessage("Welcome to Zombies! Waiting for " + wait + " more player. I am a bot, source is at the github repository ItsDeltin/Overwatch-Custom-Game-Automation");
                    if (wait < 0) cg.Chat.SendChatMessage("Welcome to Zombies! Game will be starting soon. I am a bot, source is at the github repository ItsDeltin/Overwatch-Custom-Game-Automation");
                }
                prevPlayerCount = allCountWithoutInvited;

                // if enough players join, start the timer.
                /*
                 * Start the game start timer if:
                 * 1. The timer isn't already running
                 * 2. and the number of players in blue and red is more greater than or equal to the minimum required players
                 * 3. and if the game isnt full, there isnt anyone in the queue.
                */

                if (
                    !pregame.IsRunning // 1
                    && playingCount >= minimumPlayers // 2
                    && !(playingCount < 7 && (queue.Length > 0 || cg.SpectatorCount > 1)) // 3
                    )
                {
                    cg.Chat.SendChatMessage("Enough players have joined, starting game in 15 seconds.");
                    pregame.Start();
                }

                // if too many players leave, cancel the countdown.
                if (pregame.IsRunning == true && allCountWithoutInvited < minimumPlayers)
                {
                    cg.Chat.SendChatMessage("Players left, waiting for " + (minimumPlayers - allCountWithoutInvited) + " more players, please wait.");
                    pregame.Reset();
                }

                if (Join == JoinType.ServerBrowser && MatchIsPublic == true && playingCount >= 7)
                {
                    MatchIsPublic = false;
                    cg.Settings.SetJoinSetting(Deltin.CustomGameAutomation.Join.InviteOnly);
                }
                else if (Join == JoinType.ServerBrowser && MatchIsPublic == false && playingCount < 7)
                {
                    MatchIsPublic = true;
                    cg.Settings.SetJoinSetting(Deltin.CustomGameAutomation.Join.Everyone);
                }

                // if the amount of players equals 7 or the queue list is empty and there is enough players,
                // and the pregame timer elapsed 15 seconds,
                // and there is no one invited and loading,
                // start the game.
                if ((playingCount >= 7 || (queue.Length == 0 && cg.SpectatorCount == 1 && playingCount >= minimumPlayers)) && pregame.ElapsedMilliseconds >= 15 * 1000)
                {
                    if (Join == JoinType.ServerBrowser) cg.Settings.SetJoinSetting(Deltin.CustomGameAutomation.Join.InviteOnly);
                    MatchIsPublic = false;
                    cg.SendServerToLobby();

                    // If there is too many players, swap some to spectators. If they can't be swapped to spectators, remove them from the game.
                    var playingSlots = cg.GetSlots(SlotFlags.BlueTeam | SlotFlags.RedTeam | SlotFlags.Queue);
                    for (int pc = playingSlots.Count; pc > 7; pc--)
                    {
                        int slotChosen = playingSlots[rnd.Next(playingSlots.Count - 1)];
                        // Swap the extra player to spectator. If they cannot be switched, remove them from the game.
                        if (!cg.Interact.SwapToSpectators(slotChosen))
                            cg.Interact.RemoveFromGame(slotChosen);
                    }

                    if (Join == JoinType.Abyxa)
                    {
                        a.SetMode(-1);
                        a.SetPlayerCount(playingCount);
                        a.SetInviteCount(0);
                    }               

                    int[] votemap = new int[VoteCount]; // The index of maps that can be voted for. 3 is the amount of maps chosen that can be voted for.
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

                    // Send the maps to vote for to the chat.
                    cg.Chat.SendChatMessage(FormatMessage(
                        "Vote for map! (15 seconds)",
                        maps[votemap[0]].ShortenedName + " - $VOTE 1",
                        maps[votemap[1]].ShortenedName + " - $VOTE 2",
                        maps[votemap[2]].ShortenedName + " - $VOTE 3"));

                    // Listen to the "$VOTE" command for 15 seconds.
                    ListenTo listenTo = new ListenTo("$VOTE", true, false, false, OnVote);
                    cg.Commands.ListenTo.Add(listenTo);
                    cg.Commands.Listen = true;
                    Thread.Sleep(15000);
                    cg.Commands.Listen = false;
                    cg.Commands.ListenTo.Remove(listenTo);
                    // Get results
                    int[] results = new int[VoteCount]
                    {
                        VoteResults.Count(vr => vr.VotingFor == 1),
                        VoteResults.Count(vr => vr.VotingFor == 2),
                        VoteResults.Count(vr => vr.VotingFor == 3),
                    };

                    int winningmap = votemap[Array.IndexOf(results, results.Max())];

                    // Dispose all chat identities.
                    foreach (Vote voteResult in VoteResults) voteResult.ChatIdentity.Dispose();
                    VoteResults = new List<Vote>();

                    // Print the results to the chat
                    cg.Chat.SendChatMessage(String.Format("{0}: {1} votes, {2}: {3} votes, {4}: {5} votes", 
                        maps[votemap[0]].ShortenedName, results[0], 
                        maps[votemap[1]].ShortenedName, results[1], 
                        maps[votemap[2]].ShortenedName, results[2]));
                    cg.Chat.SendChatMessage("Next map: " + maps[winningmap].ShortenedName);
                    cg.ToggleMap(ToggleAction.DisableAll, maps[winningmap].Map);
                    // Update map on website if jointype is Abyxa.
                    if (Join == JoinType.Abyxa)
                        a.SetMap(maps[winningmap].ShortenedName.ToLower());

                    // Swap everyone in red to blue.
                    var redslots = cg.RedSlots;
                    while (redslots.Count > 0)
                    {
                        cg.Interact.SwapToBlue(redslots[0]);
                        cg.WaitForSlotUpdate();
                        redslots = cg.RedSlots;
                    }

                    cg.AI.AddAI(AIHero.McCree, Difficulty.Easy, Team.Red, 6); // fill team 2 with mccree bots
                    cg.WaitForSlotUpdate();

                    int zombies = 2;
                    for (int i = 0; i < zombies; i++)
                    {
                        var blueSlots = cg.BlueSlots;
                        int choose = rnd.Next(0, blueSlots.Count);
                        cg.Interact.SwapToRed(choose);
                    }

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
        } // Pregame

        private static void OnVote(CommandData commandData)
        {
            // converts a string like "$VOTE 2" to an integer 2.
            if (int.TryParse(commandData.Command.Split(' ').ElementAtOrDefault(1), out int voteFor)
                && 1 <= voteFor && voteFor <= VoteCount) // If the number is a valid map to vote for.
            {
                // Test if the player already voted for a map. If they did, update the map they are voting for.
                for (int i = 0; i < VoteResults.Count; i++)
                    if (ChatIdentity.CompareChatIdentities(commandData.ChatIdentity, VoteResults[i].ChatIdentity))
                    {
                        Console.WriteLine(string.Format("Player #{0} changing their vote to: {1}", i, voteFor));
                        VoteResults[i].VotingFor = voteFor;
                        return;
                    }

                // If they didn't already vote for a map, add their vote to the VoteResults list.

                Console.WriteLine(string.Format("New vote from player #{0}: {1}", VoteResults.Count, voteFor));
                VoteResults.Add(new Vote(voteFor, commandData.ChatIdentity));
            }
        }

        // Makes each line of text a new line for Overwatch.
        private static string FormatMessage(params string[] text)
        {
            string newLine = string.Concat(Enumerable.Repeat("\u3000", 30));

            string result = "";
            for (int i = 0; i < text.Length; i++)
            {
                if (i < text.Length - 1)
                    result += text[i] + " " + (text[i].Length * 0.80 < newLine.Length ? newLine.Substring((int)(text[i].Length * 0.80)) : "");
                else
                    result += text[i];
            }

            return result;
        }

        private const int VoteCount = 3;
        private static List<Vote> VoteResults = new List<Vote>();
        private class Vote
        {
            public Vote(int votingFor, ChatIdentity chatIdentity)
            {
                VotingFor = votingFor;
                ChatIdentity = chatIdentity;
            }

            public int VotingFor = -1;
            public ChatIdentity ChatIdentity;
        }
    }
}