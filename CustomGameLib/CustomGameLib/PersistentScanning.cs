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
        private void StartPersistentScanning()
        {
            PersistentScanningTask = new Task(() =>
            {
                GameOverScan gameOverData = new GameOverScan();
                RoundOverScan roundOverData = new RoundOverScan();

                while (PersistentScan)
                {
                    SpinWait.SpinUntil(() => { return OnGameOver != null || OnRoundOver != null || OnDisconnect != null; });

                    using (LockHandler.Passive)
                    {
                        UpdateScreen();
                        ScanGameOver(gameOverData);
                        ScanRoundOver(roundOverData);
                        InvokeOnDisconnect();
                    }

                    Thread.Sleep(10); // End
                }
            });
            PersistentScanningTask.Start();
        }
        Task PersistentScanningTask = null;
        bool PersistentScan = true;

        #region On Game Over
        private void ScanGameOver(GameOverScan data)
        {
            // The blue team must have "\" on the start of their name.
            // The red team must have "*" on the start of their name.
            if (OnGameOver != null)
            {
                Team? thisCheck = null;

                for (int x = 110; x < 450; x++)
                    // Test for a straight line '|'
                    if (Capture.CompareColor(x, 295, new int[] { 132, 117, 87 }, 7) && Capture.CompareColor(x, 267, new int[] { 132, 117, 87 }, 7))
                    {
                        thisCheck = Team.Blue;
                        break;
                    }
                    // Test for just the top '*'
                    else if (Capture.CompareColor(x, 267, new int[] { 132, 117, 87 }, 7))
                    {
                        thisCheck = Team.Red;
                        break;
                    }

                if (thisCheck == null)
                {
                    data.Executed = false;
                    data.CurrentWinningTeamCheck = null;
                    data.CheckTime.Reset();
                }

                else if (data.CurrentWinningTeamCheck != thisCheck)
                {
                    data.Executed = false;
                    data.CurrentWinningTeamCheck = thisCheck;
                    data.CheckTime.Restart();
                }

                else if (data.CurrentWinningTeamCheck == thisCheck)
                {
                    if (!data.Executed && data.CheckTime.ElapsedMilliseconds >= GameOverScan.CheckLength)
                    {
                        OnGameOver(this, new GameOverArgs((Team)thisCheck));
                        data.Executed = true;
                    }
                }
            }
            else
            {
                data.Executed = false;
                data.CurrentWinningTeamCheck = null;
                data.CheckTime.Reset();
            }
        }

        /// <summary>
        /// Events that are executed when the game is over.
        /// To get the winning team, blue team must have "\" on the start of their name, and red needs "*" on the start of their name.
        /// </summary>
        /// <include file='docs.xml' path='doc/OnGameOver/example'></include>
        /// <seealso cref="GameOverArgs.GetWinningTeam"/>
        public event EventHandler<GameOverArgs> OnGameOver;

        private class GameOverScan
        {
            public const int CheckLength = (int)(1.5 * 1000); // 1.5 seconds in milliseconds

            public Team? CurrentWinningTeamCheck = null;
            public Stopwatch CheckTime = new Stopwatch();
            public bool Executed = false;
        }
        #endregion

        #region On Round Over
        private void ScanRoundOver(RoundOverScan roundOverScan)
        {
            if (OnRoundOver != null)
            {
                const int startX = 464;
                const int length = 100;
                const int y = 105;

                bool isOver = false;

                // Check for the KOTH round over animation.
                Parallel.For(startX, startX + length, (x, loop) =>
                {
                    if (Capture.CompareTo(new Point(x, y), Markups.KOTH_ROUND_OVER, new int[] { 190, 185, 188 }, 70, 90))
                    {
                        isOver = true;
                        loop.Break();
                    }
                });

                // Check for the elimination round over animation.
                if (!isOver)
                    isOver = Capture.CompareTo(Points.LOBBY_ELIMINATION_ROUND_OVER, Markups.ELIM_ROUND_OVER, 30, 95, DBCompareFlags.IgnoreBlack);

                if (isOver && !roundOverScan.Executed)
                {
                    OnRoundOver.Invoke(this, new EventArgs());
                    roundOverScan.Executed = true;
                }
                else if (!isOver && roundOverScan.Executed)
                {
                    roundOverScan.Executed = false;
                }
            }
        }

        /// <summary>
        /// Events that are executed when the round ends.
        /// </summary>
        public event EventHandler<EventArgs> OnRoundOver;

        private class RoundOverScan
        {
            public bool Executed = false;
        }
        #endregion
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
}
