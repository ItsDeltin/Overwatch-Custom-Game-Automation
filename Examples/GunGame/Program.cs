using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Deltin.CustomGameAutomation;

namespace GunGame
{
    class Program
    {
        static void Main(string[] args)
        {
            CustomGame cg = new CustomGame();

            cg.Settings.SetMaxPlayers(1, 1, 0, 2);

            Hero blueHero = Hero.Ana;
            Hero redHero = Hero.Ana;

            bool endGame = false;

            while (!endGame)
            {
                // Get the dead slots.
                var deadSlots = cg.PlayerInfo.GetDeadSlots();

                // 0 is the first blue slot.
                if (deadSlots.Contains(0))
                {
                    // If the red player is already at the last hero, the red player wins.
                    if (redHero == Hero.Zenyatta)
                    {
                        cg.Chat.SendChatMessage("Red Wins.");
                        endGame = true;
                    }
                    // If the red player is not at the last hero, advance their hero.
                    else
                    {
                        redHero++;
                        cg.Chat.SendChatMessage($"Red's next hero: {redHero}");
                    }
                }
                // Send the blue player back to the hero selection if they did not choose the correct hero.
                else if (cg.PlayerInfo.GetHero(0, out HeroResultInfo result) != blueHero && result != HeroResultInfo.NoHeroChosen)
                {
                    // Do this by swapping them to spectators then back.
                    cg.Interact.Move(0, 13);
                    cg.Interact.Move(13, 0);
                    cg.Chat.SendChatMessage($"Blue's next hero: {blueHero}");
                }

                // 6 is the first red slot.
                if (deadSlots.Contains(6))
                {
                    // If the blue player is already at the last hero, the blue player wins.
                    if (blueHero == Hero.Zenyatta)
                    {
                        cg.Chat.SendChatMessage("Blue Wins.");
                        endGame = true;
                    }
                    // If the blue player is not at the last hero, advance their hero.
                    else
                    {
                        blueHero++;
                        cg.Chat.SendChatMessage($"Blue's next hero: {blueHero}");
                    }
                }
                // Send the red player back to the hero selection if they did not choose the correct hero.
                else if (cg.PlayerInfo.GetHero(6, out HeroResultInfo result) != redHero && result != HeroResultInfo.NoHeroChosen)
                {
                    // Do this by swapping them to spectators then back.
                    cg.Interact.Move(6, 13);
                    cg.Interact.Move(13, 6);
                    cg.Chat.SendChatMessage($"Red's next hero: {redHero}");
                }

                Thread.Sleep(10);
            }
        }
    }
}
