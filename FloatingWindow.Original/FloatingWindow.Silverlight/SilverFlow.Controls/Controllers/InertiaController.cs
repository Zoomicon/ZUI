using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Animation;
using SilverFlow.Controls.Extensions;
using SilverFlow.Geometry;

namespace SilverFlow.Controls.Controllers
{
    /// <summary>
    /// Calculates inertial motion of a dragged element.
    /// </summary>
    public class InertiaController
    {
        private const double SCREEN_DPI = 96;
        private const double INCH = 0.0254;
        private const double GRAVITATIONAL_ACCELERATION = 9.81;
        private const double COEFFICIENT_OF_SLIDING_FRICTION = 0.015;
        private const double MIN_DELTA = 0.001;
        private const double DELAY_BEFORE_MOUSE_UP_IN_MILLISECONDS = 100;
        private const int MAX_LAST_POINTS_TO_COUNT = 4;

        private double pixelsPerMeter = SCREEN_DPI / INCH;

        private Queue<Trail> mouseTrails = new Queue<Trail>();

        /// <summary>
        /// An element starts its motion.
        /// </summary>
        /// <param name="point">The starting point of the motion.</param>
        public void StartMotion(Point point)
        {
            mouseTrails.Clear();
            mouseTrails.Enqueue(new Trail(point, DateTime.Now));
        }

        /// <summary>
        /// Saves the last position of the moved element.
        /// </summary>
        /// <param name="point">Current position.</param>
        public void MoveToPoint(Point point)
        {
            while (mouseTrails.Count >= MAX_LAST_POINTS_TO_COUNT)
            {
                // Remove all points except the last ones
                mouseTrails.Dequeue();
            }

            mouseTrails.Enqueue(new Trail(point, DateTime.Now));
        }

        /// <summary>
        /// Gets inertial motion parameters: distance and duration.
        /// </summary>
        /// <param name="hostBounds">The host bounds.</param>
        /// <param name="windowBounds">The window bounds.</param>
        /// <returns>
        /// The inertial motion parameters: distance and duration.
        /// </returns>
        public InertialMotion GetInertialMotionParameters(Rect hostBounds, Rect windowBounds)
        {
            if (mouseTrails.Count < 2) return null;

            var mouseTrailsArray = mouseTrails.ToArray();

            Point startPosition = mouseTrailsArray[0].Position;
            DateTime startTime = mouseTrailsArray[0].Timestamp;
            Point endPosition = mouseTrailsArray[mouseTrails.Count - 1].Position;
            DateTime endTime = mouseTrailsArray[mouseTrails.Count - 1].Timestamp;

            double timeBetweenNowAndLastMove = (DateTime.Now - endTime).TotalMilliseconds;
            Vector2 vector = new Vector2(startPosition, endPosition);

            if (timeBetweenNowAndLastMove < DELAY_BEFORE_MOUSE_UP_IN_MILLISECONDS && !vector.IsZero)
            {
                double time = (endTime - startTime).TotalSeconds;
                time = (time == 0) ? 0.001 : time;

                double distance = vector.Length / pixelsPerMeter;

                double intialVelocity = distance / time;

                double expectedDistance = ((intialVelocity * intialVelocity) / (2 * COEFFICIENT_OF_SLIDING_FRICTION * GRAVITATIONAL_ACCELERATION));
                double expectedTime = (2 * expectedDistance) / intialVelocity;

                double shiftX = Math.Round(vector.LengthX * expectedDistance / distance);
                double shiftY = Math.Round(vector.LengthY * expectedDistance / distance);

                // New Inertial Motion Vector
                Vector2 imVector = new Vector2(endPosition, shiftX, shiftY).Round();
                double expectedLength = imVector.Length;

                Rect bounds = hostBounds.Add(-windowBounds.Width, -windowBounds.Height);

                if (bounds.Contains(endPosition))
                {
                    imVector = EnsureEndPointInBounds(imVector, bounds).Round();
                }
                else if (hostBounds.Contains(endPosition))
                {
                    imVector = EnsureEndPointInBounds(imVector, hostBounds).Round();
                }

                // Reduce expected time if the Inertial Motion Vector was truncated by the bounds
                double realTime = (expectedLength == 0) ? 0 : (expectedTime * imVector.Length / expectedLength);

                var motion = new InertialMotion()
                {
                    Seconds = realTime,
                    EndPosition = imVector.End,
                    EasingFunction = new CubicEase()
                    {
                        EasingMode = EasingMode.EaseOut
                    }
                };

                return motion;
            }

            return null;
        }

        /// <summary>
        /// Ensures the end point is in bounds.
        /// </summary>
        /// <param name="vector">The vector to check its ending point.</param>
        /// <param name="bounds">The bounds.</param>
        /// <returns>A vector with the ending point within specified bounds.</returns>
        private Vector2 EnsureEndPointInBounds(Vector2 vector, Rect bounds)
        {
            if (!vector.IsZero && !bounds.Contains(vector.End))
            {
                if (vector.IsVertical)
                {
                    vector.End = vector.End.EnsureInVerticalBounds(bounds);
                }
                else if (vector.IsHorizontal)
                {
                    vector.End = vector.End.EnsureInHorizontalBounds(bounds);
                }
                else
                {
                    double k = (vector.LengthY) / (vector.LengthX);
                    Point point = vector.End;

                    if (vector.End.X < bounds.Left || vector.End.X > bounds.Right)
                    {
                        point = point.EnsureInHorizontalBounds(bounds);
                        point.Y = (k * (point.X - vector.Start.X)) + vector.Start.Y;
                    }

                    if (point.Y < bounds.Top || point.Y > bounds.Bottom)
                    {
                        point = point.EnsureInVerticalBounds(bounds);
                        point.X = ((point.Y - vector.Start.Y) / k) + vector.Start.X;
                    }

                    vector.End = point.EnsureInBounds(bounds);
                }
            }

            return vector;
        }
    }
}
