using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SilverFlow.Controls.Extensions;

namespace SilverFlow.Controls.Helpers
{
    /// <summary>
    /// Bitmap helper.
    /// </summary>
    public class BitmapHelper : IBitmapHelper
    {
        /// <summary>
        /// Renders the visual element and returns a bitmap, containing bitmap image of the element.
        /// </summary>
        /// <param name="element">The visual element.</param>
        /// <param name="imageWidth">Image width.</param>
        /// <param name="imageHeight">Image height.</param>
        /// <returns>Bitmap image of the element.</returns>
        public ImageSource RenderVisual(FrameworkElement element, double imageWidth, double imageHeight)
        {
            int width = element.Width.IsNotSet() ? (int)element.ActualWidth : (int)element.Width;
            int height = element.Height.IsNotSet() ? (int)element.ActualHeight : (int)element.Height;

            ScaleTransform transform = null;

            // If the element is an image - do not scale it
            if (!(element is Image))
            {
                // Scale down the element to fit it into the window's thumbnail
                double scaleX = imageWidth / width;
                double scaleY = imageHeight / height;
                double minScale = Math.Min(scaleX, scaleY);

                if (minScale < 1)
                {
                    transform = new ScaleTransform
                    {
                        ScaleX = minScale,
                        ScaleY = minScale
                    };

                    width = (int)(width * minScale);
                    height = (int)(height * minScale);
                }
            }

            WriteableBitmap bitmap = new WriteableBitmap(width, height);
            bitmap.Render(element, transform);
            bitmap.Invalidate();

            return bitmap;
        }
    }
}
