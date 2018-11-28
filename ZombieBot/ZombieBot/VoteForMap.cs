using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Deltin.CustomGameAutomation;

namespace ZombieBot
{
    class MapVoting
    {
        private static readonly ListenTo VoteCommand = new ListenTo("$VOTE", true, false, false, OnVote);

        public static Map VoteForMap(CustomGame cg, Map[] maps)
        {
            Random rnd = new Random();

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
                maps[votemap[0]].ShortName + " - $VOTE 1",
                maps[votemap[1]].ShortName + " - $VOTE 2",
                maps[votemap[2]].ShortName + " - $VOTE 3"));

            // Listen to the "$VOTE" command for 15 seconds.
            cg.Commands.ListenTo.Add(VoteCommand);
            Thread.Sleep(15000);
            cg.Commands.ListenTo.Remove(VoteCommand);
            // Get results
            int[] results = new int[VoteCount]
            {
                VoteResults.Count(vr => vr.VotingFor == 1),
                VoteResults.Count(vr => vr.VotingFor == 2),
                VoteResults.Count(vr => vr.VotingFor == 3)
            };

            int winningmap = votemap[Array.IndexOf(results, results.Max())];

            // Dispose all chat identities.
            foreach (Vote voteResult in VoteResults) voteResult.ChatIdentity.Dispose();
            VoteResults = new List<Vote>();

            // Print the results to the chat
            string mapResults = String.Format("{0}: {1} votes, {2}: {3} votes, {4}: {5} votes",
                maps[votemap[0]].ShortName, results[0],
                maps[votemap[1]].ShortName, results[1],
                maps[votemap[2]].ShortName, results[2]);
            cg.Chat.SendChatMessage(mapResults);
            Console.WriteLine(mapResults);
            cg.Chat.SendChatMessage("Next map: " + maps[winningmap].ShortName);
            cg.ToggleMap(ToggleAction.DisableAll, maps[winningmap]);

            return maps[winningmap];
        }

        private static void OnVote(CommandData commandData)
        {
            // converts a string like "$VOTE 2" to an integer 2.
            if (int.TryParse(commandData.Command.Split(' ').ElementAtOrDefault(1), out int voteFor)
                && 1 <= voteFor && voteFor <= VoteCount) // If the number is a valid map to vote for.
            {
                // Test if the player already voted for a map. If they did, update the map they are voting for.
                for (int i = 0; i < VoteResults.Count; i++)
                    if (commandData.ChatIdentity.CompareIdentities(VoteResults[i].ChatIdentity))
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
