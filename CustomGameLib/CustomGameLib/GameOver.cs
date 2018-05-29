using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;

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
            PlayerTeam? currentWinningTeamCheck = null;
            Stopwatch checkTime = new Stopwatch();
            int checkLength = (int)(1.5 * 1000); // 1.5 seconds in milliseconds
            bool executed = false;

            while (KeepGameOverCheckScanning)
            {
                if (OnGameOver != null)
                {
                    updateScreen(); // Start

                    PlayerTeam? thisCheck = null;

                    for (int x = 110; x < 450; x++)
                        // Test for a straight line '|'
                        if (CompareColor(x, 295, new int[] { 132, 117, 87 }, 7) && CompareColor(x, 267, new int[] { 132, 117, 87 }, 7))
                        {
                            thisCheck = PlayerTeam.Blue;
                            break;
                        }
                        // Test for just the top '*'
                        else if (CompareColor(x, 267, new int[] { 132, 117, 87 }, 7))
                        {
                            thisCheck = PlayerTeam.Red;
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
                            OnGameOver(this, new GameOverArgs((PlayerTeam)thisCheck));
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

                Thread.Sleep(10); // End
            }
        }

        /// <summary>
        /// Events that are executed when the game is over.
        /// To get the winning team, blue team must have "\" on the start of their name, and red needs "*" on the start of their name.
        /// </summary>
        public event EventHandler<GameOverArgs> OnGameOver;
    }

    public class GameOverArgs : EventArgs
    {
        private PlayerTeam WinningTeam;

        public GameOverArgs(PlayerTeam winningteam)
        {
            WinningTeam = winningteam;
        }

        public PlayerTeam GetWinningTeam()
        {
            return WinningTeam;
        }
    }
}
