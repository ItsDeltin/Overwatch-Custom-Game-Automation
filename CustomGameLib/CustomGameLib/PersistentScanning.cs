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
        GameOverScan GameOverData = new GameOverScan();
        //List<InviteScanData> InviteData = new List<InviteScanData>();

        private void StartPersistentScanning()
        {
            PersistentScanningTask = new Task(() =>
            {
                while (PersistentScan)
                {
                    lock (CustomGameLock)
                    {
                        ScanGameOver(GameOverData);
                        InvokeOnDisconnect();
                    }

                    Thread.Sleep(10); // End
                }
            });
            PersistentScanningTask.Start();
        }
        private void DisposePersistentScanningThread()
        {
            PersistentScan = false;
        }
        Task PersistentScanningTask = null;
        bool PersistentScan = true;

        private void ScanGameOver(GameOverScan data)
        {
            // The blue team must have "\" on the start of their name.
            // The red team must have "*" on the start of their name.
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

    internal class GameOverScan
    {
        public const int CheckLength = (int)(1.5 * 1000); // 1.5 seconds in milliseconds

        public Team? CurrentWinningTeamCheck = null;
        public Stopwatch CheckTime = new Stopwatch();
        public bool Executed = false;
    }
}
