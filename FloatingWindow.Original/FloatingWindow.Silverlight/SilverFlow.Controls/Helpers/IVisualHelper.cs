using System.Collections.Generic;
using System.Windows;

namespace SilverFlow.Controls.Helpers
{
    /// <summary>
    /// This interface defines methods used to traverse object relationships 
    /// in the visual tree.
    /// </summary>
    public interface IVisualHelper
    {
        /// <summary>
        /// Retrieves a set of objects that are located within a specified point of an object's coordinate space.
        /// </summary>
        /// <param name="intersectingPoint">The point to use as the determination point.</param>
        /// <param name="subtree">The object to search within.</param>
        /// <returns>
        /// An enumerable set of System.Windows.UIElement objects that are determined
        /// to be located in the visual tree composition at the specified point and within
        /// the specified subtee.
        /// </returns>
        IEnumerable<UIElement> FindElementsInCoordinates(Point intersectingPoint, UIElement subtree);
    }
}
