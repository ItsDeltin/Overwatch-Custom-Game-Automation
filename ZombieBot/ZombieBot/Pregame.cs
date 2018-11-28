using System;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using Deltin.CustomGameAutomation;

namespace ZombieBot
{
    partial class Program
    {
        static bool MatchIsPublic = false;
        static PlayerTracker PlayerTracker;
        static CustomGame cg;

        static readonly ListenTo SwapMeCommand = new ListenTo("$SWAPME", true, true, false, OnSwapMe);

        public static bool Pregame(Abyxa abyxa, bool serverBrowser, CustomGame cg, Map[] maps, int minimumPlayers)
        {
            int prevPlayerCount = 0;
            Stopwatch pregame = new Stopwatch();
            Stopwatch skirmish = new Stopwatch();
            skirmish.Start();

            Program.cg = cg;
            PlayerTracker = new PlayerTracker();
            cg.Commands.ListenTo.Add(SwapMeCommand);

            if (abyxa != null)
            {
                abyxa.ZombieServer.Mode = Abyxa.Pregame;
                abyxa.Update();
            }

            try
            {
                while (true)
                {
                    if (cg.IsDisconnected() || cg.HasExited())
                        return false;

                    if (abyxa != null)
                        abyxa.Update();

                    cg.TrackPlayers(PlayerTracker, SlotFlags.BlueAndRed | SlotFlags.IngameOnly);

                    if (skirmish.ElapsedMilliseconds >= 300 * 1000)
                    {
                        Console.Write("Restarting the game. New map: ");

                        cg.RestartGame();
                        prevPlayerCount = 0;
                        skirmish.Restart();
                        cg.Chat.SwapChannel(Channel.Match);

                        string currentMap = UpdateMap(abyxa, cg) ?? "Unknown";
                        Console.WriteLine(currentMap);
                    }

                    InviteQueueToGame(abyxa, cg, minimumPlayers);

                    // update server
                    if (abyxa != null)
                    {
                        abyxa.ZombieServer.PlayerCount = PlayingCountIngame;
                        abyxa.ZombieServer.InvitedCount = cg.GetInvitedCount();
                        abyxa.Update();
                    }

                    // Send a message when someone joins
                    int playerCount = PlayingCountIngame;
                    if (PlayingCountIngame > prevPlayerCount)
                    {
                        int wait = minimumPlayers - playerCount;
                        if (wait > 1) cg.Chat.SendChatMessage("Welcome to Zombies! Waiting for " + wait + " more players. I am a bot, source is at the github repository ItsDeltin/Overwatch-Custom-Game-Automation");
                        if (wait == 1) cg.Chat.SendChatMessage("Welcome to Zombies! Waiting for " + wait + " more player. I am a bot, source is at the github repository ItsDeltin/Overwatch-Custom-Game-Automation");
                        if (wait < 0) cg.Chat.SendChatMessage("Welcome to Zombies! Game will be starting soon. I am a bot, source is at the github repository ItsDeltin/Overwatch-Custom-Game-Automation");
                    }
                    prevPlayerCount = playerCount;

                    if (!pregame.IsRunning && playerCount >= minimumPlayers)
                    {
                        cg.Chat.SendChatMessage("Enough players have joined, starting game in 15 seconds.");
                        pregame.Start();
                    }

                    // if too many players leave, cancel the countdown.
                    if (pregame.IsRunning == true && playerCount < minimumPlayers)
                    {
                        cg.Chat.SendChatMessage("Players left, waiting for " + (minimumPlayers - playerCount) + " more players, please wait.");
                        pregame.Reset();
                    }

                    if (serverBrowser && MatchIsPublic && playerCount >= 7)
                    {
                        MatchIsPublic = false;
                        cg.Settings.SetJoinSetting(Join.InviteOnly);
                    }
                    else if (serverBrowser && !MatchIsPublic && playerCount < 7)
                    {
                        MatchIsPublic = true;
                        cg.Settings.SetJoinSetting(Join.Everyone);
                    }

                    // if the amount of players equals 7 or the queue list is empty and there is enough players,
                    // and the pregame timer elapsed 15 seconds,
                    // and there is no one invited and loading,
                    // start the game.
                    if (pregame.ElapsedMilliseconds >= 15 * 1000)
                    {
                        SetupGame(abyxa, serverBrowser, cg, maps);
                        return true;
                    }
                }
            }
            finally
            {
                PlayerTracker.Dispose();
                cg.Commands.ListenTo.Remove(SwapMeCommand);
            }
        } // Pregame

        private static void OnSwapMe(CommandData commandData)
        {
            cg.TrackPlayers(PlayerTracker, SlotFlags.BlueAndRed);
            int slot = PlayerTracker.SlotFromPlayerIdentity(commandData.PlayerIdentity);
            if (slot != -1)
                cg.Interact.SwapTeam(slot);
        }

        private static void SetupGame(Abyxa abyxa, bool serverBrowser, CustomGame cg, Map[] maps)
        {
            Console.WriteLine("Starting game...");

            if (abyxa != null)
            {
                abyxa.ZombieServer.Mode = Abyxa.SettingUpNextGame;
                abyxa.Update();
            }

            if (serverBrowser)
                cg.Settings.SetJoinSetting(Join.InviteOnly);
            MatchIsPublic = false;

            cg.SendServerToLobby();

            // If there is too many players, swap some to spectators. If they can't be swapped to spectators, remove them from the game.
            var playingSlots = PlayingSlots;
            Random rnd = new Random();
            for (int pc = playingSlots.Count; pc > 7; pc--)
            {
                int slotChosen = playingSlots[rnd.Next(playingSlots.Count - 1)];
                // Swap the extra player to spectator. If they cannot be switched, remove them from the game.
                if (!cg.Interact.SwapToSpectators(slotChosen))
                    cg.Interact.RemoveFromGame(slotChosen);
            }

            if (abyxa != null)
            {
                abyxa.ZombieServer.PlayerCount = PlayingCount;
                abyxa.ZombieServer.InvitedCount = 0;
                abyxa.Update();
            }

            Map chosenMap = MapVoting.VoteForMap(cg, maps);

            // Update map on website
            if (abyxa != null)
            {
                abyxa.ZombieServer.Map = chosenMap.ShortName.ToLower();
                abyxa.Update();
            }

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

            const int zombies = 2;
            for (int i = 0; i < zombies; i++)
            {
                var blueSlots = cg.BlueSlots;
                int choose = rnd.Next(0, blueSlots.Count);
                cg.Interact.SwapToRed(choose);
            }

            // Start game
            cg.Chat.SendChatMessage("Starting game...");

            cg.StartGame();
        }

        private static void InviteQueueToGame(Abyxa abyxa, CustomGame cg, int minimumPlayers)
        {
            var spectatorslots = cg.SpectatorSlots;
            // If players are in spectator and slots are available, switch them to blue/red
            for (int i = 1; i < spectatorslots.Count && PlayingCount < 7; i++)
            {
                if (!cg.Interact.SwapToBlue(spectatorslots[i]))
                    cg.Interact.SwapToRed(spectatorslots[i]);
            }

            if (abyxa != null)
            {
                List<QueueUser> queue = abyxa.GetQueue();

                for (int i = 0; i < queue.Count && PlayingCount < 7; i++)
                {
                    if (!queue[i].IsWaiting || (queue[i].IsWaiting && cg.AllCount + queue.Count >= minimumPlayers))
                    {
                        Console.WriteLine("Inviting the player " + queue[i].BattleTag + "...");
                        // invite player to game
                        cg.InvitePlayer(queue[i].BattleTag, Team.BlueAndRed); // invite player to game
                        cg.WaitForSlotUpdate(); // Wait for the slots to update
                        abyxa.RemoveFromQueue(queue[i].BattleTag); // remove player from queue
                    }
                }
            }
        }

        private static int PlayingCount { get { return cg.GetCount(SlotFlags.BlueAndRed | SlotFlags.Queue); } }
        private static int PlayingCountIngame { get { return cg.GetCount(SlotFlags.BlueAndRed | SlotFlags.Queue | SlotFlags.IngameOnly); } }
        private static List<int> PlayingSlots { get { return cg.GetSlots(SlotFlags.BlueAndRed | SlotFlags.Queue); } }
        private static List<int> PlayingSlotsIngame { get { return cg.GetSlots(SlotFlags.BlueAndRed | SlotFlags.Queue | SlotFlags.IngameOnly); } }
    }
} 