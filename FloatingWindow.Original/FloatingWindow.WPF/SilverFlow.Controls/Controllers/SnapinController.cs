using System.Collections.Generic;
using System.Windows;
using SilverFlow.Controls.Extensions;
using SilverFlow.Geometry;

namespace SilverFlow.Controls.Controllers
{
    /// <summary>
    /// Provides helpers methods for calculating window position 
    /// during movement and resizing.
    /// </summary>
    public class SnapinController : ISnapinController
    {
        /// <summary>
        /// Distance on which snapping works.
        /// </summary>
        private const double SnapinDistanceDefaultValue = 5;

        /// <summary>
        /// Gets or sets a value indicating whether snap in is enabled.
        /// </summary>
        /// <value><c>true</c> if snap in is enabled; otherwise, <c>false</c>.</value>
        public bool SnapinEnabled { get; set; }

        /// <summary>
        /// Gets or sets snap in bounds.
        /// </summary>
        /// <value>Snap in bounds.</value>
        public IEnumerable<Rect> SnapinBounds { get; set; }

        /// <summary>
        /// Gets or sets snap in distance.
        /// </summary>
        /// <value>Snap in distance.</value>
        public double SnapinDistance { get; set; }

        /// <summary>
        /// Gets or sets a value snap in margin - distance between adjacent edges.
        /// </summary>
        /// <value>Snap in margin.</value>
        public double SnapinMargin { get; set; }

        /// <summary>
        /// Gets calculations accuracy.
        /// </summary>
        /// <value>Accuracy.</value>
        private double Accuracy 
        {
            get { return SnapinDistance + SnapinMargin;  }
        }

        /// <summary>
        /// Returns new position of the specified rectangle
        /// taking into account bounds the rectangle can be attracted to.
        /// </summary>
        /// <param name="rect">The rectangle.</param>
        /// <returns>New position.</returns>
        public Point SnapRectangle(Rect rect)
        {
            Point point = rect.Position();

            if (SnapinEnabled)
            {
                Distance minDistance = new Distance();
                foreach (var bound in SnapinBounds)
                {
                    Distance distance = DistanceBetweenRectangles(rect, bound);
                    minDistance = Distance.Min(distance, minDistance);
                }

                point = point.Add(minDistance);
            }

            return point;
        }

        /// <summary>
        /// Snaps the bottom right corner of the specified rectangle to the nearest bounds.
        /// </summary>
        /// <param name="rect">The rectangle.</param>
        /// <returns>New position.</returns>
        public Point SnapBottomRightCorner(Rect rect)
        {
            Point point = rect.BottomRight();

            if (SnapinEnabled)
            {
                Distance minDistance = new Distance();
                foreach (var bound in SnapinBounds)
                {
                    Distance distance = new Distance()
                    {
                        X = DistanceBetweenRightEdgeAndRectangle(rect, bound),
                        Y = DistanceBetweenBottomEdgeAndRectangle(rect, bound)
                    };

                    minDistance = Distance.Min(distance, minDistance);
                }

                point = point.Add(minDistance);
            }

            return point;
        }

        /// <summary>
        /// Snaps the upper left corner of the specified rectangle to the nearest bounds.
        /// </summary>
        /// <param name="rect">The rectangle.</param>
        /// <returns>New position.</returns>
        public Point SnapTopLeftCorner(Rect rect)
        {
            Point point = rect.Position();

            if (SnapinEnabled)
            {
                Distance minDistance = new Distance();
                foreach (var bound in SnapinBounds)
                {
                    Distance distance = new Distance()
                    {
                        X = DistanceBetweenLeftEdgeAndRectangle(rect, bound),
                        Y = DistanceBetweenTopEdgeAndRectangle(rect, bound)
                    };

                    minDistance = Distance.Min(distance, minDistance);
                }

                point = point.Add(minDistance);
            }

            return point;
        }

        /// <summary>
        /// Snaps the lower left corner of the specified rectangle to the nearest bounds.
        /// </summary>
        /// <param name="rect">The rectangle.</param>
        /// <returns>New position.</returns>
        public Point SnapBottomLeftCorner(Rect rect)
        {
            Point point = rect.BottomLeft();

            if (SnapinEnabled)
            {
                Distance minDistance = new Distance();
                foreach (var bound in SnapinBounds)
                {
                    Distance distance = new Distance()
                    {
                        X = DistanceBetweenLeftEdgeAndRectangle(rect, bound),
                        Y = DistanceBetweenBottomEdgeAndRectangle(rect, bound)
                    };

                    minDistance = Distance.Min(distance, minDistance);
                }

                point = point.Add(minDistance);
            }

            return point;
        }

        /// <summary>
        /// Snaps the upper right corner of the specified rectangle to the nearest bounds.
        /// </summary>
        /// <param name="rect">The rectangle.</param>
        /// <returns>New position.</returns>
        public Point SnapTopRightCorner(Rect rect)
        {
            Point point = rect.TopRight();

            if (SnapinEnabled)
            {
                Distance minDistance = new Distance();
                foreach (var bound in SnapinBounds)
                {
                    Distance distance = new Distance()
                    {
                        X = DistanceBetweenRightEdgeAndRectangle(rect, bound),
                        Y = DistanceBetweenTopEdgeAndRectangle(rect, bound)
                    };

                    minDistance = Distance.Min(distance, minDistance);
                }

                point = point.Add(minDistance);
            }

            return point;
        }

        /// <summary>
        /// Returns mininal distance between two rectangles.
        /// </summary>
        /// <param name="first">First rectangle.</param>
        /// <param name="second">Second rectangle.</param>
        /// <returns>Minimal distance.</returns>
        private Distance DistanceBetweenRectangles(Rect first, Rect second)
        {
            double x1 = DistanceBetweenRightEdgeAndRectangle(first, second);
            double x2 = DistanceBetweenLeftEdgeAndRectangle(first, second);
            double y1 = DistanceBetweenBottomEdgeAndRectangle(first, second);
            double y2 = DistanceBetweenTopEdgeAndRectangle(first, second);

            Distance distance = new Distance()
            {
                X = MathExtensions.AbsMin(x1, x2),
                Y = MathExtensions.AbsMin(y1, y2)
            };

            return distance;
        }

        /// <summary>
        /// Returns distance between the right edge of the rectangle and another rectangle.
        /// </summary>
        /// <param name="first">First rectangle.</param>
        /// <param name="second">Second rectangle.</param>
        /// <returns>Minimal distance.</returns>
        private double DistanceBetweenRightEdgeAndRectangle(Rect first, Rect second)
        {
            double snap;
            double distance = double.NaN;
            double x = first.X + first.Width - 1;

            if (first.OverlapsVertically(second, Accuracy))
            {
                snap = second.X - 1 - SnapinMargin;
                if (x.IsNear(snap, SnapinDistance))
                {
                    distance = MathExtensions.AbsMin(snap - x, distance);
                }

                snap = second.X + second.Width - 1;
                if (x.IsNear(snap, SnapinDistance))
                {
                    distance = MathExtensions.AbsMin(snap - x, distance);
                }
            }

            return distance;
        }

        /// <summary>
        /// Returns distance between the left edge of the rectangle and another rectangle.
        /// </summary>
        /// <param name="first">First rectangle.</param>
        /// <param name="second">Second rectangle.</param>
        /// <returns>Minimal distance.</returns>
        private double DistanceBetweenLeftEdgeAndRectangle(Rect first, Rect second)
        {
            double snap;
            double distance = double.NaN;

            if (first.OverlapsVertically(second, Accuracy))
            {
                snap = second.X;
                if (first.X.IsNear(snap, SnapinDistance))
                {
                    distance = MathExtensions.AbsMin(snap - first.X, distance);
                }

                snap = second.X + second.Width + SnapinMargin;
                if (first.X.IsNear(snap, SnapinDistance))
                {
                    distance = MathExtensions.AbsMin(snap - first.X, distance);
                }
            }

            return distance;
        }

        /// <summary>
        /// Returns distance between the bottom edge of the rectangle and another rectangle.
        /// </summary>
        /// <param name="first">First rectangle.</param>
        /// <param name="second">Second rectangle.</param>
        /// <returns>Minimal distance.</returns>
        private double DistanceBetweenBottomEdgeAndRectangle(Rect first, Rect second)
        {
            double snap;
            double distance = double.NaN;
            double y = first.Y + first.Height - 1;

            if (first.OverlapsHorizontally(second, Accuracy))
            {
                snap = second.Y - 1 - SnapinMargin;
                if (y.IsNear(snap, SnapinDistance))
                {
                    distance = MathExtensions.AbsMin(snap - y, distance);
                }

                snap = second.Y + second.Height - 1;
                if (y.IsNear(snap, SnapinDistance))
                {
                    distance = MathExtensions.AbsMin(snap - y, distance);
                }
            }

            return distance;
        }

        /// <summary>
        /// Returns distance between the top edge of the rectangle and another rectangle.
        /// </summary>
        /// <param name="first">First rectangle.</param>
        /// <param name="second">Second rectangle.</param>
        /// <returns>Minimal distance.</returns>
        private double DistanceBetweenTopEdgeAndRectangle(Rect first, Rect second)
        {
            double snap;
            double distance = double.NaN;

            if (first.OverlapsHorizontally(second, Accuracy))
            {
                snap = second.Y;
                if (first.Y.IsNear(snap, SnapinDistance))
                {
                    distance = MathExtensions.AbsMin(snap - first.Y, distance);
                }

                snap = second.Y + second.Height + SnapinMargin;
                if (first.Y.IsNear(snap, SnapinDistance))
                {
                    distance = MathExtensions.AbsMin(snap - first.Y, distance);
                }
            }

            return distance;
        }
    }
}
