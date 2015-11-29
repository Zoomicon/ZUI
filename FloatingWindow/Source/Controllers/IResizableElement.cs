using System.Windows;
using System.Windows.Input;

namespace SilverFlow.Controls.Controllers
{
    /// <summary>
    /// This interface shall be implemented by an element to be resized.
    /// </summary>
    public interface IResizableElement
    {
        /// <summary>
        /// Gets or sets the width of the element.
        /// </summary>
        /// <value>The width.</value>
        double Width { get; set; }

        /// <summary>
        /// Gets or sets the height of the element.
        /// </summary>
        /// <value>The height.</value>
        double Height { get; set; }

        /// <summary>
        /// Gets or sets minimal width of the element.
        /// </summary>
        /// <value>Minimal width.</value>
        double MinWidth { get; set; }

        /// <summary>
        /// Gets or sets minimal height of the element.
        /// </summary>
        /// <value>Minimal height.</value>
        double MinHeight { get; set; }

        /// <summary>
        /// Gets or sets maximal width of the element.
        /// </summary>
        /// <value>Maximal width.</value>
        double MaxWidth { get; set; }

        /// <summary>
        /// Gets or sets maximal height of the element.
        /// </summary>
        /// <value>Maximal height.</value>
        double MaxHeight { get; set; }

        /// <summary>
        /// Gets the actual width.
        /// </summary>
        /// <value>The actual width.</value>
        double ActualWidth { get; }

        /// <summary>
        /// Gets the actual height.
        /// </summary>
        /// <value>The actual height.</value>
        double ActualHeight { get; }

        /// <summary>
        /// Gets or sets the position of the element.
        /// </summary>
        /// <value>The position.</value>
        Point Position { get; set; }

        /// <summary>
        /// Gets the parent of the element.
        /// </summary>
        /// <value>The parent object.</value>
        DependencyObject Parent { get; }

        /// <summary>
        /// Gets or sets the cursor of the element.
        /// </summary>
        /// <value>The cursor of the element.</value>
        Cursor Cursor { get; set; }

        /// <summary>
        /// Snapin controller.
        /// </summary>
        ISnapinController SnapinController { get; }
    }
}
