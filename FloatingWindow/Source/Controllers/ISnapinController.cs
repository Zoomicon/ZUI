using System.Collections.Generic;
using System.Windows;

namespace SilverFlow.Controls.Controllers
{
    public interface ISnapinController
    {
        /// <summary>
        /// Gets or sets a value indicating whether snap in is enabled.
        /// </summary>
        /// <value><c>true</c> if snap in is enabled; otherwise, <c>false</c>.</value>
        bool SnapinEnabled { get; set; }

        /// <summary>
        /// Gets or sets snap in bounds.
        /// </summary>
        /// <value>Snap in bounds.</value>
        IEnumerable<Rect> SnapinBounds { get; set; }

        /// <summary>
        /// Gets or sets snap in distance.
        /// </summary>
        /// <value>Snap in distance.</value>
        double SnapinDistance { get; set; }

        /// <summary>
        /// Gets or sets a value snap in margin - distance between adjacent edges.
        /// </summary>
        /// <value>Snap in margin.</value>
        double SnapinMargin { get; set; }

        /// <summary>
        /// Returns new position of the specified rectangle
        /// taking into account bounds the rectangle can be attracted to.
        /// </summary>
        /// <param name="rect">The rectangle.</param>
        /// <returns>New position.</returns>
        Point SnapRectangle(Rect rect);

        /// <summary>
        /// Snaps the bottom right corner of the specified rectangle to the nearest bounds.
        /// </summary>
        /// <param name="rect">The rectangle.</param>
        /// <returns>New position.</returns>
        Point SnapBottomRightCorner(Rect rect);

        /// <summary>
        /// Snaps the upper left corner of the specified rectangle to the nearest bounds.
        /// </summary>
        /// <param name="rect">The rectangle.</param>
        /// <returns>New position.</returns>
        Point SnapTopLeftCorner(Rect rect);

        /// <summary>
        /// Snaps the lower left corner of the specified rectangle to the nearest bounds.
        /// </summary>
        /// <param name="rect">The rectangle.</param>
        /// <returns>New position.</returns>
        Point SnapBottomLeftCorner(Rect rect);

        /// <summary>
        /// Snaps the upper right corner of the specified rectangle to the nearest bounds.
        /// </summary>
        /// <param name="rect">The rectangle.</param>
        /// <returns>New position.</returns>
        Point SnapTopRightCorner(Rect rect);
    }
}
