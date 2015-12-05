using System;
using System.Windows;
using SilverFlow.Geometry;

namespace SilverFlow.Controls.Extensions
{
    /// <summary>
    /// Geometry extensions
    /// </summary>
    public static class GeometryExtensions
    {
        /// <summary>
        /// Rounds the specified point to the nearest integer coordinates.
        /// </summary>
        /// <param name="point">The point to round coordinates.</param>
        /// <returns>New point with rounded coordinates.</returns>
        public static Point Round(this Point point)
        {
            return new Point(Math.Round(point.X), Math.Round(point.Y));
        }

        /// <summary>
        /// Ensures that coordinates are positive.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns>Point with positive coordinates.</returns>
        public static Point EnsurePositive(this Point point)
        {
            return new Point()
            {
                X = (point.X < 0) ? 0 : point.X,
                Y = (point.Y < 0) ? 0 : point.Y
            };
        }

        /// <summary>
        /// Gets position of the specified rectangle.
        /// </summary>
        /// <param name="rect">Rectangle.</param>
        /// <returns>Upper left corner of the rectangle.</returns>
        public static Point Position(this Rect rect)
        {
            return new Point(rect.X, rect.Y);
        }

        /// <summary>
        /// Gets position the lower right corner of the rectangle.
        /// </summary>
        /// <param name="rect">Rectangle.</param>
        /// <returns>Lower right corner of the rectangle.</returns>
        public static Point BottomRight(this Rect rect)
        {
            return new Point(rect.Right, rect.Bottom);
        }

        /// <summary>
        /// Gets position the lower left corner of the rectangle.
        /// </summary>
        /// <param name="rect">Rectangle.</param>
        /// <returns>Lower left corner of the rectangle.</returns>
        public static Point BottomLeft(this Rect rect)
        {
            return new Point(rect.X, rect.Bottom);
        }

        /// <summary>
        /// Gets position the upper right corner of the rectangle.
        /// </summary>
        /// <param name="rect">Rectangle.</param>
        /// <returns>Upper right corner of the rectangle.</returns>
        public static Point TopRight(this Rect rect)
        {
            return new Point(rect.Right, rect.Y);
        }

        /// <summary>
        /// Ensures that coordinates of the point are in the specified bounds.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <param name="bounds">The bounds.</param>
        /// <returns>Point within the boinds.</returns>
        public static Point EnsureInBounds(this Point point, Rect bounds)
        {
            return new Point()
            {
                X = point.X.EnsureInRange(bounds.X, bounds.Right),
                Y = point.Y.EnsureInRange(bounds.Y, bounds.Bottom)
            };
        }

        /// <summary>
        /// Ensures that the X-coordinate of the point is in the specified horizontal bounds.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <param name="bounds">The bounds.</param>
        /// <returns>Point with the X-coordinate within the boinds.</returns>
        public static Point EnsureInHorizontalBounds(this Point point, Rect bounds)
        {
            return new Point()
            {
                X = point.X.EnsureInRange(bounds.X, bounds.Right),
                Y = point.Y
            };
        }

        /// <summary>
        /// Ensures that the Y-coordinate of the point is in the specified vertical bounds.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <param name="bounds">The bounds.</param>
        /// <returns>Point with the Y-coordinate within the boinds.</returns>
        public static Point EnsureInVerticalBounds(this Point point, Rect bounds)
        {
            return new Point()
            {
                X = point.X,
                Y = point.Y.EnsureInRange(bounds.Y, bounds.Bottom)
            };
        }

        /// <summary>
        /// Determines whether coordinates of the point are Not a Number (NaN).
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns>
        /// <c>true</c> if coordinates are not specified; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNotSet(this Point point)
        {
            return double.IsNaN(point.X) || double.IsNaN(point.Y);
        }

        /// <summary>
        /// Adds an offset to the specified point.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <param name="x">Distance along X-coordinate.</param>
        /// <param name="y">Distance along Y-coordinate.</param>
        /// <returns>New point.</returns>
        public static Point Add(this Point point, double x, double y)
        {
            return new Point(point.X + x, point.Y + y);
        }

        /// <summary>
        /// Adds an offset specified by the Size to the specified point.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <param name="size">Distance to add.</param>
        /// <returns>New point</returns>
        public static Point Add(this Point point, Size size)
        {
            return new Point(point.X + size.Width, point.Y + size.Height);
        }

        /// <summary>
        /// Determines whether dimensions are Not a Number (NaN).
        /// </summary>
        /// <param name="size">The size.</param>
        /// <returns>
        /// <c>true</c> if dimensions are not specified; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNotSet(this Size size)
        {
            return double.IsNaN(size.Width) || double.IsNaN(size.Height);
        }

        /// <summary>
        /// Increments size to the specified values.
        /// </summary>
        /// <param name="size">The size.</param>
        /// <param name="x">Increment by X-coordinate.</param>
        /// <param name="y">Increment by Y-coordinate.</param>
        /// <returns>New size.</returns>
        public static Size Add(this Size size, double x, double y)
        {
            double width = size.Width + x;
            double height = size.Height + y;

            return new Size()
            {
                Width = width < 0 ? 0 : width,
                Height = height < 0 ? 0 : height
            };
        }

        /// <summary>
        /// Increases size if the rectangle to the specified width and height.
        /// </summary>
        /// <param name="rect">The rectangle.</param>
        /// <param name="width">Value to add to the width of the rectangle.</param>
        /// <param name="height">Value to add to the height of the rectangle.</param>
        /// <returns>New rectangle.</returns>
        public static Rect Add(this Rect rect, double width, double height)
        {
            return new Rect
            {
                X = rect.X,
                Y = rect.Y,
                Width = Math.Max(0, rect.Width + width),
                Height = Math.Max(0, rect.Height + height)
            };
        }

        /// <summary>
        /// Shifts the point to the specified distance.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <param name="distance">The distance.</param>
        /// <returns>Point shifted to the specified distance.</returns>
        public static Point Add(this Point point, Distance distance)
        {
            return new Point()
            {
                X = distance.X.IsNotSet() ? point.X : point.X + distance.X,
                Y = distance.Y.IsNotSet() ? point.Y : point.Y + distance.Y
            };
        }

        /// <summary>
        /// Tests whether the rectangle overlaps with another one vertically.
        /// </summary>
        /// <param name="rect">The rectangle.</param>
        /// <param name="testRect">Test rectangle.</param>
        /// <param name="accuracy">Accuracy.</param>
        /// <returns><c>true</c> if overlaps vertically; otherwise, <c>false</c>.</returns>
        public static bool OverlapsVertically(this Rect rect, Rect testRect, double accuracy = 0)
        {
            double y1 = rect.Y + rect.Height - 1;
            double y2 = testRect.Y + testRect.Height - 1;

            if (rect.Y <= testRect.Y)
            {
                return y1 >= (testRect.Y - accuracy); 
            }
            else if (rect.Y <= (y2 + accuracy))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Tests whether the rectangle overlaps with another one horizontally.
        /// </summary>
        /// <param name="rect">The rectangle.</param>
        /// <param name="testRect">Test rectangle.</param>
        /// <param name="accuracy">Accuracy.</param>
        /// <returns><c>true</c> if overlaps horizontally; otherwise, <c>false</c>.</returns>
        public static bool OverlapsHorizontally(this Rect rect, Rect testRect, double accuracy = 0)
        {
            double x1 = rect.X + rect.Width - 1;
            double x2 = testRect.X + testRect.Width - 1;

            if (rect.X <= testRect.X)
            {
                return x1 >= (testRect.X - accuracy);
            }
            else if (rect.X <= (x2 + accuracy))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Tests whether the point overlaps with the rectangle vertically.
        /// </summary>
        /// <param name="point">Test point.</param>
        /// <param name="rect">Test rectangle.</param>
        /// <param name="accuracy">Accuracy.</param>
        /// <returns><c>true</c> if overlaps vertically; otherwise, <c>false</c>.</returns>
        public static bool OverlapsVertically(this Point point, Rect rect, double accuracy = 0)
        {
            return (point.Y >= (rect.Y - accuracy) && point.Y <= (rect.Y + rect.Height - 1 + accuracy));
        }

        /// <summary>
        /// Tests whether the point overlaps with the rectangle horizontally.
        /// </summary>
        /// <param name="point">Test point.</param>
        /// <param name="rect">Test rectangle.</param>
        /// <param name="accuracy">Accuracy.</param>
        /// <returns><c>true</c> if overlaps horizontally; otherwise, <c>false</c>.</returns>
        public static bool OverlapsHorizontally(this Point point, Rect rect, double accuracy = 0)
        {
            return (point.X >= (rect.X - accuracy) && point.X <= (rect.X + rect.Width - 1 + accuracy));
        }
    }
}
