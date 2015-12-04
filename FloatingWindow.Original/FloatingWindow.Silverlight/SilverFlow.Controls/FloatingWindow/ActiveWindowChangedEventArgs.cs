using System;

namespace SilverFlow.Controls
{
    /// <summary>
    /// ActiveWindowChanged event arguments class.
    /// </summary>
    public class ActiveWindowChangedEventArgs : EventArgs
    {
        public FloatingWindow Old { get; set; }
        public FloatingWindow New { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveWindowChangedEventArgs"/> class.
        /// </summary>
        public ActiveWindowChangedEventArgs()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveWindowChangedEventArgs"/> class.
        /// </summary>
        /// <param name="oldWindow">The old active FloatingWindow.</param>
        /// <param name="newWindow">The new active FloatingWindow.</param>
        public ActiveWindowChangedEventArgs(FloatingWindow oldWindow, FloatingWindow newWindow)
        {
            this.Old = oldWindow;
            this.New = newWindow;
        }
    }
}
