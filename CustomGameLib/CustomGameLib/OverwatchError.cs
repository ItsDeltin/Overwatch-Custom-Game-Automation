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
        public event EventHandler OnExit;
        /// <summary>
        /// Invoked when Overwatch disconnects.
        /// </summary>
        public event EventHandler OnDisconnect;

        private void InvokeOnExit(object sender = null, EventArgs e = null)
        {
            if (OnExit != null)
                OnExit.Invoke(this, new EventArgs());
        }

        private bool OnDisconnectInvoked = false;

        private void InvokeOnDisconnect()
        {
            if (OnDisconnect != null && IsDisconnected() && !OnDisconnectInvoked)
            {
                    OnDisconnect.Invoke(this, new EventArgs());
                    OnDisconnectInvoked = true;
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
            using (LockHandler.Passive)
            {
                updateScreen();
                return Capture.CompareColor(Points.EXIT_TO_DESKTOP, Colors.EXIT_TO_DESKTOP, Fades.EXIT_TO_DESKTOP);
            }
        }

        /// <summary>
        /// Checks if Overwatch exited.
        /// </summary>
        /// <returns></returns>
        public bool HasExited()
        {
            return !Validate();
        }

        internal bool Validate()
        {
            return User32.IsWindow(OverwatchHandle);
        }
        static internal bool Validate(IntPtr hwnd)
        {
            return User32.IsWindow(hwnd);
        }
    }
}
