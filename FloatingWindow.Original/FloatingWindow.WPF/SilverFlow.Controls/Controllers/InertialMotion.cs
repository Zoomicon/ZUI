using System.Windows;
using System.Windows.Media.Animation;

namespace SilverFlow.Controls.Controllers
{
    /// <summary>
    /// Represents a vector and duration of the inertial motion, calculated by the InertiaController.
    /// </summary>
    public class InertialMotion
    {
        /// <summary>
        /// Gets or sets ending position of the displaced element.
        /// </summary>
        /// <value>The ending position.</value>
        public Point EndPosition { get; set; }

        /// <summary>
        /// Gets or sets duration of the inertial motion in seconds.
        /// </summary>
        /// <value>Duration of the inertial motion in seconds.</value>
        public double Seconds { get; set; }

        /// <summary>
        /// Gets or sets the Easing Function applied to the animation.
        /// </summary>
        /// <value>The Easing Function.</value>
        public IEasingFunction EasingFunction { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InertialMotion"/> class.
        /// </summary>
        public InertialMotion()
        {
            this.Seconds = 0;
        }
    }
}
