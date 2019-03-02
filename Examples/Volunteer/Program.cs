using System;
using System.Threading.Tasks;
using System.Threading;
using Deltin.CustomGameAutomation;

namespace CGL_Testing
{
    class Program
    {
        static void Main(string[] args)
        {
            CustomGame cg = new CustomGame();

            Team volunteerForTeam = Team.Red;

            cg.Chat.SendChatMessage($"Type $VOLUNTEER to volunteer for {volunteerForTeam}.");

            PlayerTracker tracker = new PlayerTracker();
            cg.Commands.Listen = true;
            cg.Commands.ListenTo.Add(new ListenTo("$VOLUNTEER", true, true, true, (cd) => Volunteer(cg, tracker, cd, volunteerForTeam)));

            Task.Run(() =>
            {
                while (true)
                {
                    cg.TrackPlayers(tracker);
                    Thread.Sleep(25);
                }
            });

            while (true)
            {
                cg.Chat.SendChatMessage(Console.ReadLine());
            }
        }

        static void Volunteer(CustomGame cg, PlayerTracker tracker, CommandData cd, Team volunteerForTeam)
        {
            cg.TrackPlayers(tracker);
            int slot = tracker.SlotFromPlayerIdentity(cd.PlayerIdentity);

            if (CustomGame.IsSlotValid(slot) && !CustomGame.IsSlot(slot, volunteerForTeam))
            {
                cg.Interact.SwapTeam(slot);
                cg.Chat.SendChatMessage($"Thanks for volunteering, {FormatName(cd.PlayerName)}!");
            }
        }

        static string FormatName(string name)
        {
            // CommandData.PlayerName is all captilized, this makes the name lowercase except for the first character.
            // JOHN > John
            char[] c = name.ToLower().ToCharArray();
            c[0] = char.ToUpper(c[0]);
            return new string(c);
        }
    }
}