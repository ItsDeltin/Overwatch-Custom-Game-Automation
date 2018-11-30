using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deltin.CustomGameAutomation
{
    /// <summary>
    /// Thrown when slot is invalid or out of range.
    /// </summary>
    public class InvalidSlotException : Exception
    {
        /// <summary>
        /// Throws invalid slot exception.
        /// </summary>
        /// <param name="message">Message of exception.</param>
        public InvalidSlotException(string message) : base(message) { }

        /// <summary>
        /// Throws invalid slot exception.
        /// </summary>
        /// <param name="slot">Invalid slot.</param>
        public InvalidSlotException(int slot) : base(string.Format("Slot {0} is not a valid slot.", slot.ToString())) { }
    }

    /// <summary>
    /// Thrown if an Overwatch process is not found.
    /// </summary>
    public class MissingOverwatchProcessException : Exception
    {
        /// <summary>
        /// Throws missing Overwatch process exception.
        /// </summary>
        /// <param name="message">Message to display.</param>
        public MissingOverwatchProcessException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Exception for invalid SetHero values.
    /// </summary>
    public class InvalidSetheroException : Exception
    {
        /// <summary>
        /// Throws invalid SetHero exception.
        /// </summary>
        /// <param name="message">Message to display.</param>
        public InvalidSetheroException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Thrown when creating an Overwatch process using <see cref="CustomGame.StartOverwatch(OverwatchInfoAuto)"/> or <see cref="CustomGame.StartOverwatch(OverwatchInfoManual)"/> fails.
    /// </summary>
    public class OverwatchStartFailedException : Exception
    {
        /// <summary>
        /// Throws Login Failed Exception.
        /// </summary>
        /// <param name="message">Message to display.</param>
        public OverwatchStartFailedException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Thrown if the Overwatch window is closed.
    /// </summary>
    public class OverwatchClosedException : Exception
    {
        /// <summary>
        /// Throws Overwatch closed exception.
        /// </summary>
        /// <param name="message">Message to display.</param>
        public OverwatchClosedException(string message) : base(message) { }
    }
}
