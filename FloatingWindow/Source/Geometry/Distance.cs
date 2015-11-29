using SilverFlow.Controls.Extensions;

namespace SilverFlow.Geometry
{
    /// <summary>
    /// Defines displacement used for correction of window position or size.
    /// </summary>
    public class Distance
    {
        /// <summary>
        /// Gets or sets displacement by X coordinate.
        /// </summary>
        /// <value>Displacement by X coordinate.</value>
        public double X { get; set; }

        /// <summary>
        /// Gets or sets displacement by Y coordinate.
        /// </summary>
        /// <value>Displacement by Y coordinate.</value>
        public double Y { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is nonzero.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is nonzero; otherwise, <c>false</c>.
        /// </value>
        public bool IsNonzero
        {
            get
            {
                return !double.IsNaN(X) && !double.IsNaN(Y) && (X != 0 || Y != 0);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Distance"/> class.
        /// </summary>
        public Distance()
        {
            X = double.NaN;
            Y = double.NaN;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Distance"/> class.
        /// </summary>
        /// <param name="x">Displacement by X coordinate.</param>
        /// <param name="y">Displacement by Y coordinate.</param>
        public Distance(double x, double y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Gets the smallest of two distances.
        /// </summary>
        /// <param name="first">First distance.</param>
        /// <param name="second">Second distance.</param>
        /// <returns>Smallest distance.</returns>
        public static Distance Min(Distance first, Distance second)
        {
            Distance distance = new Distance
            {
                X = MathExtensions.AbsMin(first.X, second.X),
                Y = MathExtensions.AbsMin(first.Y, second.Y)
            };

            return distance;
        }
    }
}
