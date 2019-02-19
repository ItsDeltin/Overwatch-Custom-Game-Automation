using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Deltin.CustomGameAutomation;

class MapVoting
{
    static void Main()
    {
        CustomGame cg = new CustomGame();

        Map[] voteForMaps = new Map[]
        {
            Map.AE_BlizzardWorld,
            Map.AE_Eichenwalde,
            Map.AE_Hollywood,
            Map.AE_KingsRow,
            Map.AE_Numbani
        };
        OWEvent currentEvent = CustomGame.GetCurrentEvent();
        Gamemode enabledGamemodes = cg.GetModesEnabled(currentEvent);

        VoteForMap(cg, voteForMaps, enabledGamemodes, currentEvent, true);

        Console.WriteLine("Done.");
        Console.ReadLine();
    }

    public static Map VoteForMap(CustomGame cg, Map[] maps, Gamemode enabledGamemodes, OWEvent currentEvent, bool logResults)
    {
        if (maps.Length < 3)
            throw new ArgumentException($"{nameof(maps)} must have at least 3 maps.", nameof(maps));

        Map[] voteForMaps = Get3RandomMaps(maps);

        List<Vote> voteResults = new List<Vote>();
        ListenTo voteCommand = new ListenTo("$VOTE", true, false, false, (cd) => OnVote(cd, voteResults, voteForMaps, logResults));

        cg.Commands.Listen = true;

        // Send the maps to vote for to the chat.
        cg.Chat.SwapChannel(Channel.Match); // Join the match channel
        cg.Chat.SendChatMessage(FormatMessage(
            "Vote for map! (15 seconds)",
            voteForMaps[0].ShortName + " - $VOTE 1",
            voteForMaps[1].ShortName + " - $VOTE 2",
            voteForMaps[2].ShortName + " - $VOTE 3"));

        // Listen to the "$VOTE" command for 15 seconds.
        cg.Commands.ListenTo.Add(voteCommand);
        Thread.Sleep(15000);
        cg.Commands.ListenTo.Remove(voteCommand);
        // Get results
        int[] results = new int[3]
        {
            voteResults.Count(vr => vr.VotingFor == 1),
            voteResults.Count(vr => vr.VotingFor == 2),
            voteResults.Count(vr => vr.VotingFor == 3)
        };

        Map winningmap = voteForMaps[Array.IndexOf(results, results.Max())];

        // Dispose all chat identities.
        foreach (Vote voteResult in voteResults) voteResult.ChatIdentity.Dispose();
        voteResults = new List<Vote>();

        // Print the results to the chat
        string mapResults = String.Format("{0}: {1} votes, {2}: {3} votes, {4}: {5} votes",
            voteForMaps[0].ShortName, results[0],
            voteForMaps[1].ShortName, results[1],
            voteForMaps[2].ShortName, results[2]);
        cg.Chat.SendChatMessage(mapResults);

        if (logResults)
        {
            Console.WriteLine(mapResults);
            Console.WriteLine("Next map: " + winningmap.ShortName);
        }
        cg.Chat.SendChatMessage("Next map: " + winningmap.ShortName);
        cg.ToggleMap(enabledGamemodes, currentEvent, ToggleAction.DisableAll, winningmap);

        return winningmap;
    }

    private static Map[] Get3RandomMaps(Map[] maps)
    {
        Random rnd = new Random();

        Map[] voteForMaps = new Map[3];
        for (int i = 0; i < 3; i++)
        {
            Map choose;
            while (true)
            {
                // Make sure there are no duplicates when choosing the next map to be added to the votemap array
                choose = maps[rnd.Next(maps.Length)];

                if (voteForMaps.Contains(choose))
                    continue;
                break;
            }
            voteForMaps[i] = choose;
        }

        return voteForMaps;
    }

    private static void OnVote(CommandData commandData, List<Vote> voteResults, Map[] maps, bool logResults)
    {
        // converts a string like "$VOTE 2" to an integer 2.
        if (int.TryParse(commandData.Command.Split(' ').ElementAtOrDefault(1), out int voteFor)
            && 1 <= voteFor && voteFor <= 3) // If the number is a valid map to vote for.
        {
            // Test if the player already voted for a map. If they did, update the map they are voting for.
            for (int i = 0; i < voteResults.Count; i++)
                if (commandData.ChatIdentity.CompareIdentities(voteResults[i].ChatIdentity))
                {
                    // Don't log if the player is voting for the same map again.
                    if (voteResults[i].VotingFor != voteFor)
                    {
                        if (logResults)
                            Console.WriteLine($"Player #{i} changing their vote to {maps[voteFor - 1]} ({voteFor})");
                        voteResults[i].VotingFor = voteFor;
                    }
                    return;
                }

            // If they didn't already vote for a map, add their vote to the VoteResults list.
            if (logResults)
                Console.WriteLine($"New vote from player #{voteResults.Count}: {maps[voteFor - 1]} ({voteFor})");
            voteResults.Add(new Vote(voteFor, commandData.ChatIdentity));
        }
    }

    // Formats an array of strings into a chat message. Each string represents a line.
    private static string FormatMessage(params string[] text)
    {
        string newLine = string.Concat(Enumerable.Repeat("\u3000", 30));

        string result = "";
        for (int i = 0; i < text.Length; i++)
        {
            double subLength = text[i].Length * 0.8;

            if (i < text.Length - 1) // If the index is not the last one
                // Add the required spaces to make the new line.
                result += text[i] + " " + (subLength < newLine.Length ? newLine.Substring((int)subLength) : "");
            else
                result += text[i];
        }

        return result;
    }

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