/* Lock Guide
Any public method in the custom game class should use a lock.

Passive locks is for functions that scan the Overwatch window but do not interact with it. 
Any amount of passive locks will run side by side.
- Usage in CustomGame class:
using (LockHandler.Passive)
- Usage in CustomGameBase class:
using (cg.LockHandler.Passive)

Semi-Interactive locks is for functions that interact with the Overwatch window but do not go into any other menues allowing scanning to continue as normal. 
Only 1 semi-interactive lock will run at a time.
- Usage in CustomGame class:
using (LockHandler.SemiInteractive)
- Usage in CustomGameBase class:
using (cg.LockHandler.SemiInteractive)

Interactive locks is for functions that interact with the Overwatch window and go into new menus. This will block passive locks from running during the interactive lock.
Only 1 interactive lock will run at a time.
- Usage in CustomGame class:
using (LockHandler.Interactive)
- Usage in CustomGameBase class:
using (cg.LockHandler.Interactive)

A deadlock will occur if LockHandler.Passive, LockHandler.Interactive, or LockHandler.SemiInteractive are accessed outside of a using() statement.

The following will also cause a deadlock:
using (LockHandler.Interactive)
{
    Parallel.For(0, 10, (i) => 
    {
        using (LockHandler...)
        {
            // ...
        }
    });
}
Because the first using statement is waiting for Parallel.For to complete, and the second using statement is waiting for the first using statement to release the lock.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Deltin.CustomGameAutomation
{
    partial class CustomGame
    {
        /// <include file='docs.xml' path='doc/lockHandler/summary'/>
        /// <include file='docs.xml' path='doc/lockHandler/remarks'/>
        public LockHandler LockHandler { get; private set; }
    }

    /// <include file='docs.xml' path='doc/lockHandler/summary'/>
    /// <include file='docs.xml' path='doc/lockHandler/remarks'/>
    /// <seealso cref="CustomGame.LockHandler"/>
    public class LockHandler
    {
        private const int PassiveNum = 0;
        private const int InteractiveNum = 1;
        private const int SemiInteractiveNum = 2;

        internal LockHandler(CustomGame cg)
        {
            this.cg = cg;
        }

        private CustomGame cg;

        /// <summary>
        /// Lock for functions that only scan the Overwatch window.
        /// </summary>
        public Locker Passive         { get { return new Locker(PassiveNum,         this); } }
        /// <summary>
        /// Lock for functions that interact with the Overwatch window that go into different menus.
        /// </summary>
        public Locker Interactive     { get { return new Locker(InteractiveNum,     this); } }
        /// <summary>
        /// Lock for functions that interact with the Overwatch window that do not go into different menus.
        /// </summary>
        public Locker SemiInteractive { get { return new Locker(SemiInteractiveNum, this); } }

        readonly ReaderWriterLockSlim RWLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        readonly object InteractiveLock = new object();

        private void SetLock(Locker locker)
        {
            switch (locker.LockType)
            {
                // Passive:
                case PassiveNum:

                    //RWLock.EnterUpgradeableReadLock();
                    Monitor.Enter(InteractiveLock);

                    break;

                // Interactive:
                case SemiInteractiveNum:

                    Monitor.Enter(InteractiveLock);

                    break;

                // Semi-Interactive:
                case InteractiveNum:

                    //RWLock.EnterWriteLock();
                    Monitor.Enter(InteractiveLock);

                    break;
            }
        }
        private void Unlock(Locker locker)
        {
            switch (locker.LockType)
            {
                // Passive:
                case PassiveNum:

                    //RWLock.ExitUpgradeableReadLock();
                    Monitor.Exit(InteractiveLock);

                    break;

                // Semi-Interactive:
                case SemiInteractiveNum:

                    Monitor.Exit(InteractiveLock);

                    break;

                // Interactive:
                case InteractiveNum:

                    Monitor.Exit(InteractiveLock);
                    //RWLock.ExitWriteLock();

                    break;
            }
        }

        /// <summary>
        /// Lock for a <see cref="LockHandler"/>.
        /// </summary>
        public class Locker : IDisposable
        {
            internal Locker(int lockType, LockHandler lockHandler)
            {
                LockType = lockType;
                LockHandler = lockHandler;
                LockHandler.SetLock(this);
            }
            internal int LockType { get; private set; }
            private LockHandler LockHandler;

            /// <summary>
            /// Releases the lock.
            /// </summary>
            public void Dispose()
            {
                LockHandler.Unlock(this);
            }
        }
    }
}