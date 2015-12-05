using System;
using System.Windows;
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

            // Scale down the element to fit it into the window's thumbnail
            double scaleX = imageWidth / width;
            double scaleY = imageHeight / height;
            double minScale = Math.Min(scaleX, scaleY);

            if (minScale < 1)
            {
                width = (int)(width * minScale);
                height = (int)(height * minScale);
            }

            // Get current dpi
            PresentationSource presentationSource = PresentationSource.FromVisual(Application.Current.MainWindow);
            Matrix m = presentationSource.CompositionTarget.TransformToDevice;
            double dpiX = m.M11 * 96;
            double dpiY = m.M22 * 96;

            RenderTargetBitmap elementBitmap = new RenderTargetBitmap(width, height, dpiX, dpiY, PixelFormats.Default);

            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                VisualBrush visualBrush = new VisualBrush(element);
                drawingContext.DrawRectangle(visualBrush, null, new Rect(new Point(0, 0), new Size(width, height)));
            }

            // Draw the element
            elementBitmap.Render(drawingVisual);

            return elementBitmap;
        }
    }
}
