using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Deltin.CustomGameAutomation
{
    partial class CustomGame
    {
        void SetupGameOverCheck()
        {
            if (GameOverCheckTask != null)
                throw new Exception("GameOverCheckTask has already been created.");

            GameOverCheckTask = new Task(() => 
            {
                GameOverCheck();
            });
            GameOverCheckTask.Start();
        }

        void DisposeGameOverCheck()
        {
            KeepGameOverCheckScanning = false;
            GameOverCheckTask.Wait();
            GameOverCheckTask.Dispose();
        }

        Task GameOverCheckTask = null;
        bool KeepGameOverCheckScanning = true;

        // The blue team must have "\" on the start of their name.
        // The red team must have "*" on the start of their name.

        void GameOverCheck()
        {
            Team? currentWinningTeamCheck = null;
            Stopwatch checkTime = new Stopwatch();
            int checkLength = (int)(1.5 * 1000); // 1.5 seconds in milliseconds
            bool executed = false;

            InviteScan inviteData = new InviteScan();

            while (KeepGameOverCheckScanning)
            {
                if (OnGameOver != null)
                {
                    updateScreen(); // Start

                    Team? thisCheck = null;

                    for (int x = 110; x < 450; x++)
                        // Test for a straight line '|'
                        if (CompareColor(x, 295, new int[] { 132, 117, 87 }, 7) && CompareColor(x, 267, new int[] { 132, 117, 87 }, 7))
                        {
                            thisCheck = Team.Blue;
                            break;
                        }
                        // Test for just the top '*'
                        else if (CompareColor(x, 267, new int[] { 132, 117, 87 }, 7))
                        {
                            thisCheck = Team.Red;
                            break;
                        }

                    if (thisCheck == null)
                    {
                        executed = false;
                        currentWinningTeamCheck = null;
                        checkTime.Reset();
                    }

                    else if (currentWinningTeamCheck != thisCheck)
                    {
                        executed = false;
                        currentWinningTeamCheck = thisCheck;
                        checkTime.Restart();
                    }

                    else if (currentWinningTeamCheck == thisCheck)
                    {
                        if (!executed && checkTime.ElapsedMilliseconds >= checkLength)
                        {
                            OnGameOver(this, new GameOverArgs((Team)thisCheck));
                            executed = true;
                        }
                    }
                }
                else
                {
                    executed = false;
                    currentWinningTeamCheck = null;
                    checkTime.Reset();
                }

                ScanInvitedPlayers(inviteData);

                Thread.Sleep(10); // End
            }
        }

        private void ScanInvitedPlayers(InviteScan data)
        {
            // Get all non-AI players
            List<int> players = GetSlots(SlotFlags.BlueTeam | SlotFlags.RedTeam | SlotFlags.Spectators | SlotFlags.Queue | SlotFlags.NoAI);

            foreach(int slot in PlayerRange)
            {
                // Get the data relating to the slot.
                InviteScanSlotData previousSlotData = data.SlotData.FirstOrDefault(v => v.Slot == slot);

                // If the slot is empty, dispose of the data associated with it then continue to the next slot.
                if (!players.Contains(slot))
                {
                    if (previousSlotData != null)
                    {
                        previousSlotData.Markup.Dispose();
                        data.SlotData.Remove(previousSlotData);
                    }
                    continue;
                }

                // Copy a 20*20 pixel square of the invite icon animation.
                Point scanAt = new Point(InviteScan.Origin.X + ((slot / 6) * Distance.LOBBY_TEAM_SLOT_DISTANCE), InviteScan.Origin.Y + (slot * Distance.LOBBY_SLOT_DISTANCE));
                int range = 10;
                int markupFade = 30;
                Bitmap markup = BmpClone(scanAt.X - range, scanAt.Y - range, range * 2, range * 2);

                // If there is no previous record of the animation for the slot, create it.
                if (previousSlotData == null)
                {
                    data.SlotData.Add(new InviteScanSlotData(slot, markup));
                }
                else
                {
                    // If there is, compare it to the previous one.
                    double total = 0;
                    double success = 0;

                    for (int x = 0; x < markup.Width; x++)
                        for (int y = 0; y < markup.Height; y++)
                        {
                            total++;

                            if (previousSlotData.Markup.CompareColor(markup, x, y, markupFade))
                                success++;
                        }

                    double percentage = (success / total) * 100;

                    previousSlotData.Markup.Dispose();
                    previousSlotData.Markup = markup;

                    // If the markups are 90% similar, there is no animation so the player is in game and not invited.

                    if (percentage > 90)
                    {
                        previousSlotData.TimeSinceLastNoChange = DateTime.UtcNow;

                        if ((DateTime.UtcNow - previousSlotData.TimeSinceLastChange).Seconds >= 2)
                            previousSlotData.PredictedState = 1;
                    }
                    else
                    {
                        previousSlotData.TimeSinceLastChange = DateTime.UtcNow;

                        if ((DateTime.UtcNow - previousSlotData.TimeSinceLastNoChange).Seconds >= 2)
                            previousSlotData.PredictedState = 2;
                    }
                }
            }
        }

        /// <summary>
        /// Events that are executed when the game is over.
        /// To get the winning team, blue team must have "\" on the start of their name, and red needs "*" on the start of their name.
        /// </summary>
        /// <example>
        /// The example below will send a message to chat when the game is over.
        /// <code>
        /// using Deltin.CustomGameAutomation;
        /// 
        /// public class OnGameOverExample
        /// {
        ///     public static void SendMessageToChatWhenGameIsOver(CustomGame cg)
        ///     {
        ///         cg.GameSettings.SetTeamName(PlayerTeam.Blue, "\ Blue Team");
        ///         cg.GameSettings.SetTeamName(PlayerTeam.Red, "* Red Team");
        ///         cg.OnGameOver += Cg_OnGameOver;
        ///     }
        ///     
        ///     private static void Cg_OnGameOver(object sender, GameOverArgs e)
        ///     {
        ///         PlayerTeam winningTeam = e.GetWinningTeam();
        ///         (sender as CustomGame).Chat.Chat(string.Format("The game is over, Team {0} has won!", winningTeam.ToString()));
        ///     }
        /// }
        /// </code>
        /// </example>
        /// <seealso cref="GameOverArgs.GetWinningTeam"/>
        public event EventHandler<GameOverArgs> OnGameOver;
    }

    /// <summary>
    /// Arguments for the OnGameOver event that is executed when the game ends in Overwatch.
    /// </summary>
    /// <seealso cref="CustomGame.OnGameOver"/>
    public class GameOverArgs : EventArgs
    {
        private Team WinningTeam;

        /// <summary>
        /// Arguments for the OnGameOver event that is executed when the game ends in Overwatch.
        /// </summary>
        internal GameOverArgs(Team winningteam)
        {
            WinningTeam = winningteam;
        }

        /// <summary>
        /// Gets the team that won the Overwatch game.
        /// </summary>
        /// <returns>Returns the team that won the game.</returns>
        /// <seealso cref="CustomGame.OnGameOver"/>
        public Team GetWinningTeam()
        {
            return WinningTeam;
        }
    }

    internal class InviteScan
    {
        public static readonly Point Origin = new Point(176, 252);

        public List<InviteScanSlotData> SlotData = new List<InviteScanSlotData>();
    }

    internal class InviteScanSlotData
    {
        public InviteScanSlotData(int slot, Bitmap markup)
        {
            Slot = slot;
            Markup = markup;
        }

        public int Slot;
        public Bitmap Markup;

        public DateTime TimeSinceLastChange;
        public DateTime TimeSinceLastNoChange;
        public int PredictedState = 0; // 0 = unsure, 1 = ingame, 2 = invited.
    }
}
