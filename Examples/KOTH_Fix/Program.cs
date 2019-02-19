/*

King of the Hill games in Overwatch have an end-of-round animation that can mess with scanning in the Custom Game class.
This code will lock all CustomGame methods during the animation.

*/

using System;
using System.Threading;
using Deltin.CustomGameAutomation;

namespace KOTH_Fix
{
    class Program
    {
        static void Main(string[] args)
        {
            CustomGame cg = new CustomGame();

            cg.OnRoundOver += CustomGame_OnRoundOver;

            Console.ReadLine();
        }

        private static void CustomGame_OnRoundOver(object sender, EventArgs e)
        {
            CustomGame cg = (CustomGame)sender;
            using (cg.LockHandler.Interactive)
            {
                cg.Chat.SendChatMessage("Round over!");
                Thread.Sleep(7000);
            }
        }
    }
}
