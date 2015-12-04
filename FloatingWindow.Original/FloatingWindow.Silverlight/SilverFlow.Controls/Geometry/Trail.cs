using System;
using System.Windows;

namespace SilverFlow.Geometry
{
    /// <summary>
    /// Mouse pointer trail.
    /// </summary>
    public class Trail
    {
        /// <summary>
        /// Gets or sets mouse position.
        /// </summary>
        /// <value>The position.</value>
        public Point Position { get; set; }

        /// <summary>
        /// Gets or sets the timestamp.
        /// </summary>
        /// <value>The timestamp.</value>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Trail"/> class.
        /// </summary>
        public Trail()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Trail"/> class.
        /// </summary>
        /// <param name="point">Point.</param>
        /// <param name="time">Time.</param>
        public Trail(Point point, DateTime time)
        {
            this.Position = point;
            this.Timestamp = time;
        }
    }
}
