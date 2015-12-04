using System;
using System.Windows;
using SilverFlow.Controls.Extensions;

namespace SilverFlow.Geometry
{
    /// <summary>
    /// Defines two-dimensional vector.
    /// </summary>
    public class Vector2
    {
        /// <summary>
        /// Gets or sets the starting point of the vector.
        /// </summary>
        /// <value>The starting point.</value>
        public Point Start { get; set; }

        /// <summary>
        /// Gets or sets the ending point of the vector.
        /// </summary>
        /// <value>The ending point.</value>
        public Point End { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is not set.
        /// </summary>
        /// <value><c>true</c> if this instance is not set; otherwise, <c>false</c>.</value>
        public bool IsNaN
        {
            get
            {
                return this.Start.IsNotSet() || this.End.IsNotSet();
            }
        }

        /// <summary>
        /// Gets a value indicating whether the vector is of zero length.
        /// </summary>
        /// <value><c>true</c> if the vector is of zero length; otherwise, <c>false</c>.</value>
        public bool IsZero
        {
            get
            {
                return this.IsNaN ? true : this.Start == this.End;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the vector is vertical.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the vector is vertical; otherwise, <c>false</c>.
        /// </value>
        public bool IsVertical
        {
            get
            {
                return !this.IsZero && (this.Start.X == this.End.X);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the vector is horizontal.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the vector is horizontal; otherwise, <c>false</c>.
        /// </value>
        public bool IsHorizontal
        {
            get
            {
                return !this.IsZero && (this.Start.Y == this.End.Y);
            }
        }

        /// <summary>
        /// Gets the length of the vector by X-coordinate.
        /// </summary>
        /// <value>The length by X-coordinate.</value>
        public double LengthX
        {
            get
            {
                return this.IsNaN ? 0 : (this.End.X - this.Start.X);
            }
        }

        /// <summary>
        /// Gets the length of the vector by Y-coordinate.
        /// </summary>
        /// <value>The length by Y-coordinate.</value>
        public double LengthY
        {
            get
            {
                return this.IsNaN ? 0 : (this.End.Y - this.Start.Y);
            }
        }

        /// <summary>
        /// Gets the length of the vector.
        /// </summary>
        /// <value>The length of the vector.</value>
        public double Length
        {
            get
            {
                if (this.IsNaN) 
                    return 0;

                return Math.Sqrt(LengthX * LengthX + LengthY * LengthY);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector2"/> class.
        /// </summary>
        public Vector2()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector2"/> class.
        /// </summary>
        /// <param name="start">The starting point.</param>
        /// <param name="end">The ending point.</param>
        public Vector2(Point start, Point end)
        {
            this.Start = start;
            this.End = end;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector2"/> class.
        /// </summary>
        /// <param name="start">The starting point.</param>
        /// <param name="lengthX">The length by X-coordinate.</param>
        /// <param name="lengthY">The length by Y-coordinate.</param>
        public Vector2(Point start, double lengthX, double lengthY)
        {
            this.Start = start;
            this.End = start.Add(lengthX, lengthY);
        }

        /// <summary>
        /// Rounds starting and ending points to the nearest integer coordinates.
        /// </summary>
        /// <returns>Vector with rounded coordinates.</returns>
        public Vector2 Round()
        {
            this.Start = this.Start.Round();
            this.End = this.End.Round();
            return this;
        }
    }
}
