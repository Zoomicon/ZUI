﻿using System.Windows;
using System.Windows.Media;

namespace SilverFlow.Controls.Helpers
{
    /// <summary>
    /// This interface defines methods used the create bitmap images.
    /// </summary>
    public interface IBitmapHelper
    {
        /// <summary>
        /// Renders the visual element and returns a bitmap, containing bitmap image of the element.
        /// </summary>
        /// <param name="element">The visual element.</param>
        /// <param name="imageWidth">Image width.</param>
        /// <param name="imageHeight">Image height.</param>
        /// <returns>Bitmap image of the element.</returns>
        ImageSource RenderVisual(FrameworkElement element, double imageWidth, double imageHeight);
    }
}
