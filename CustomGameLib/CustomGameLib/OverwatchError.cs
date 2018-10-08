using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Deltin.CustomGameAutomation
{
    partial class CustomGame
    {
        /// <summary>
        /// Invoked when the Overwatch process exits.
        /// </summary>
        public event EventHandler OnOverwatchProcessExit;
        /// <summary>
        /// Invoked when Overwatch disconnects.
        /// </summary>
        public event EventHandler OnDisconnect;

        private void InvokeOnOverwatchProcessExit(object sender, EventArgs e)
        {
            if (OnOverwatchProcessExit != null)
                OnOverwatchProcessExit.Invoke(this, new EventArgs());
        }

        private bool OnDisconnectInvoked = false;

        private void InvokeOnDisconnect()
        {
            if (IsDisconnected())
            {
                if (OnDisconnect != null && !OnDisconnectInvoked)
                {
                    OnDisconnect.Invoke(this, new EventArgs());
                    OnDisconnectInvoked = true;
                }
            }
            else
                OnDisconnectInvoked = false;
        }

        /// <summary>
        /// Checks if Overwatch is disconnected.
        /// </summary>
        /// <returns></returns>
        public bool IsDisconnected()
        {
            lock (CustomGameLock)
            {
                updateScreen();
                return CompareColor(Points.EXIT_TO_DESKTOP, Colors.EXIT_TO_DESKTOP, Fades.EXIT_TO_DESKTOP);
            }
        }
    }
}
