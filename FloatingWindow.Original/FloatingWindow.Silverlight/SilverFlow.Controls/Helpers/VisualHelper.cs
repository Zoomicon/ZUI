using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace SilverFlow.Controls.Helpers
{
    public class VisualHelper : IVisualHelper
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
        public IEnumerable<UIElement> FindElementsInCoordinates(Point intersectingPoint, UIElement subtree)
        {
            return VisualTreeHelper.FindElementsInHostCoordinates(intersectingPoint, subtree);
        }
    }
}
