using System;
using System.Windows;
using System.Windows.Input;
using SilverFlow.Controls.Enums;
using SilverFlow.Controls.Extensions;

namespace SilverFlow.Controls.Controllers
{
    /// <summary>
    /// Calculates resizing element position and size.
    /// </summary>
    public class ResizeController
    {
        /// <summary>
        /// Thickness of resizing area.
        /// </summary>
        private const double ResizingAreaDefaultValue = 6;

        /// <summary>
        /// Minimal width of the resized window if MinWidth is set to 0.
        /// </summary>
        private const double MinResizedWidth = 20;

        /// <summary>
        /// Resizing bounds, i.e. an area within which the resizing cursor can move.
        /// </summary>
        private Rect bounds;

        /// <summary>
        /// Initial element size when resizing started.
        /// </summary>
        private Size initialSize;

        /// <summary>
        /// Initial position of the element when resizing started.
        /// </summary>
        private Point initialPosition;

        /// <summary>
        /// A corner or edge of the element used for resizing.
        /// </summary>
        private ResizeAnchor anchor;

        /// <summary>
        /// An element implementing IResizableElement interface.
        /// </summary>
        private IResizableElement element;

        /// <summary>
        /// Gets a framework element hosting resizing element.
        /// </summary>
        /// <value>The host.</value>
        private FrameworkElement Host
        {
            get
            {
                return element.Parent as FrameworkElement;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the resizing can be started.
        /// </summary>
        /// <value>
        /// <c>true</c> if resizing can be started; otherwise, <c>false</c>.
        /// </value>
        public bool CanResize
        {
            get
            {
                return anchor != ResizeAnchor.None;
            }
        }

        /// <summary>
        /// Gets or sets the width of the resizing area.
        /// </summary>
        /// <value>The width of the resizing area. Default value is 6.</value>
        public double ResizingArea { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance can be resized horizontally.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance can be resized horizontally; otherwise, <c>false</c>.
        /// </value>
        private bool CanResizeHorizontally
        {
            get
            {
                return !(element.MinWidth == element.MaxWidth && element.MinWidth != 0);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance can be resized vertically.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance can be resized vertically; otherwise, <c>false</c>.
        /// </value>
        private bool CanResizeVertically
        {
            get
            {
                return !(element.MinHeight == element.MaxHeight && element.MinHeight != 0);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResizeController"/> class.
        /// </summary>
        /// <param name="resizableElement">The resizable element.</param>
        public ResizeController(IResizableElement resizableElement)
        {
            if (resizableElement == null)
                throw new ArgumentNullException("resizableElement");

            element = resizableElement;
            anchor = ResizeAnchor.None;
            ResizingArea = ResizingAreaDefaultValue;
        }

        /// <summary>
        /// Starts resizing of the element.
        /// </summary>
        public void StartResizing()
        {
            initialSize = new Size(element.ActualWidth, element.ActualHeight);
            initialPosition = element.Position;

            CalculateBoundsForResizing();
        }

        /// <summary>
        /// Resizes the element.
        /// </summary>
        /// <param name="dx">Displacement by X-coordinate.</param>
        /// <param name="dy">Displacement by Y-coordinate.</param>
        public void Resize(double dx, double dy)
        {
            switch (anchor)
            {
                case ResizeAnchor.BottomRight:
                    ResizeBottomRight(dx, dy);
                    break;

                case ResizeAnchor.Right:
                    ResizeBottomRight(dx, 0);
                    break;

                case ResizeAnchor.Bottom:
                    ResizeBottomRight(0, dy);
                    break;

                case ResizeAnchor.TopLeft:
                    ResizeTopLeft(dx, dy);
                    break;

                case ResizeAnchor.Top:
                    ResizeTopLeft(0, dy);
                    break;

                case ResizeAnchor.Left:
                    ResizeTopLeft(dx, 0);
                    break;

                case ResizeAnchor.BottomLeft:
                    ResizeBottomLeft(dx, dy);
                    break;

                case ResizeAnchor.TopRight:
                    ResizeTopRight(dx, dy);
                    break;
            }
        }

        /// <summary>
        /// Determines resizing anchor and sets appropriate Cursor.
        /// </summary>
        /// <param name="p">Coordinates of the mouse pointer.</param>
        public void SetCursor(Point p)
        {
            if (p.Y < ResizingArea && p.X < ResizingArea)
            {
                anchor = ResizeAnchor.TopLeft;
                element.Cursor = Cursors.SizeNWSE;
            }
            else if (p.Y < ResizingArea && p.X >= (element.ActualWidth - ResizingArea))
            {
                anchor = ResizeAnchor.TopRight;
                element.Cursor = Cursors.SizeNESW;
            }
            else if (p.Y < ResizingArea)
            {
                if (CanResizeVertically)
                {
                    anchor = ResizeAnchor.Top;
                    element.Cursor = Cursors.SizeNS;
                }
            }
            else if (p.X < ResizingArea && p.Y >= (element.ActualHeight - ResizingArea))
            {
                anchor = ResizeAnchor.BottomLeft;
                element.Cursor = Cursors.SizeNESW;
            }
            else if (p.X < ResizingArea)
            {
                if (CanResizeHorizontally)
                {
                    anchor = ResizeAnchor.Left;
                    element.Cursor = Cursors.SizeWE;
                }
            }
            else if (p.X >= (element.ActualWidth - ResizingArea) && p.Y >= (element.ActualHeight - ResizingArea))
            {
                anchor = ResizeAnchor.BottomRight;
                element.Cursor = Cursors.SizeNWSE;
            }
            else if (p.X >= (element.ActualWidth - ResizingArea))
            {
                if (CanResizeHorizontally)
                {
                    anchor = ResizeAnchor.Right;
                    element.Cursor = Cursors.SizeWE;
                }
            }
            else if (p.Y >= (element.ActualHeight - ResizingArea))
            {
                if (CanResizeVertically)
                {
                    anchor = ResizeAnchor.Bottom;
                    element.Cursor = Cursors.SizeNS;
                }
            }
            else
            {
                anchor = ResizeAnchor.None;
                element.Cursor = null;
            }
        }

        /// <summary>
        /// Calculates bounds for resizing.
        /// </summary>
        private void CalculateBoundsForResizing()
        {
            switch (anchor)
            {
                case ResizeAnchor.BottomRight:
                case ResizeAnchor.Right:
                case ResizeAnchor.Bottom:
                    bounds = GetBoundsForLowerRightCorner();
                    break;

                case ResizeAnchor.TopLeft:
                case ResizeAnchor.Top:
                case ResizeAnchor.Left:
                    bounds = GetBoundsForUpperLeftCorner();
                    break;

                case ResizeAnchor.BottomLeft:
                    bounds = GetBoundsForLowerLeftCorner();
                    break;

                case ResizeAnchor.TopRight:
                    bounds = GetBoundsForUpperRightCorner();
                    break;
            }
        }

        /// <summary>
        /// Calculates bounds for resizing by the lower right corner.
        /// </summary>
        /// <returns>Bounding rectangle.</returns>
        private Rect GetBoundsForLowerRightCorner()
        {
            double minWidth = (element.MinWidth != 0) ? element.MinWidth : MinResizedWidth;
            double minHeight = element.MinHeight;

            double maxWidth = Host.ActualWidth - initialPosition.X;

            if (!element.MaxWidth.IsNotSet() && element.MaxWidth > 0)
                maxWidth = Math.Min(maxWidth, element.MaxWidth);

            double maxHeight = Host.ActualHeight - initialPosition.Y;
            if (!element.MaxHeight.IsNotSet() && element.MaxHeight > 0)
                maxHeight = Math.Min(maxHeight, element.MaxHeight);

            Point p1 = initialPosition.Add(minWidth, minHeight);
            Point p2 = initialPosition.Add(maxWidth, maxHeight);

            return new Rect(p1, p2);
        }

        /// <summary>
        /// Calculates bounds for resizing by the upper left corner.
        /// </summary>
        /// <returns>Bounding rectangle.</returns>
        private Rect GetBoundsForUpperLeftCorner()
        {
            double minWidth = (element.MinWidth != 0) ? element.MinWidth : MinResizedWidth;
            double minHeight = element.MinHeight;

            Point point = initialPosition.Add(initialSize);
            double maxWidth = point.X;
            double maxHeight = point.Y;

            if (!element.MaxWidth.IsNotSet() && element.MaxWidth > 0)
                maxWidth = Math.Min(maxWidth, element.MaxWidth);

            if (!element.MaxHeight.IsNotSet() && element.MaxHeight > 0)
                maxHeight = Math.Min(maxHeight, element.MaxHeight);

            Point p1 = point.Add(-maxWidth, -maxHeight);
            Point p2 = point.Add(-minWidth, -minHeight);

            return new Rect(p1, p2);
        }

        /// <summary>
        /// Calculates bounds for resizing by the lower left corner.
        /// </summary>
        /// <returns>Bounding rectangle.</returns>
        private Rect GetBoundsForLowerLeftCorner()
        {
            double minWidth = (element.MinWidth != 0) ? element.MinWidth : MinResizedWidth;
            double minHeight = element.MinHeight;

            Point point = initialPosition.Add(initialSize.Width, 0);
            double maxWidth = point.X;

            if (!element.MaxWidth.IsNotSet() && element.MaxWidth > 0)
                maxWidth = Math.Min(maxWidth, element.MaxWidth);

            double maxHeight = Host.ActualHeight - initialPosition.Y;
            if (!element.MaxHeight.IsNotSet() && element.MaxHeight > 0)
                maxHeight = Math.Min(maxHeight, element.MaxHeight);

            Point p1 = point.Add(-maxWidth, minHeight);
            Point p2 = point.Add(-minWidth, maxHeight);

            return new Rect(p1, p2);
        }

        /// <summary>
        /// Calculates bounds for resizing by the upper right corner.
        /// </summary>
        /// <returns>Bounding rectangle.</returns>
        private Rect GetBoundsForUpperRightCorner()
        {
            double minWidth = (element.MinWidth != 0) ? element.MinWidth : MinResizedWidth;
            double minHeight = element.MinHeight;

            Point point = initialPosition.Add(0, initialSize.Height);
            double maxWidth = Host.ActualWidth - initialPosition.X;

            if (!element.MaxWidth.IsNotSet() && element.MaxWidth > 0)
                maxWidth = Math.Min(maxWidth, element.MaxWidth);

            double maxHeight = point.Y;
            if (!element.MaxHeight.IsNotSet() && element.MaxHeight > 0)
                maxHeight = Math.Min(maxHeight, element.MaxHeight);

            Point p1 = point.Add(minWidth, -maxHeight);
            Point p2 = point.Add(maxWidth, -minHeight);

            return new Rect(p1, p2);
        }

        /// <summary>
        /// Resizes the window by the bottom right corner of the window.
        /// </summary>
        /// <param name="dx">Increment by X-coordinate.</param>
        /// <param name="dy">Increment by Y-coordinate.</param>
        private void ResizeBottomRight(double dx, double dy)
        {
            Rect newBounds = new Rect(initialPosition, initialSize.Add(dx, dy));
            Point point = element.SnapinController.SnapBottomRightCorner(newBounds);

            // If only one coordinate was changed - restore the other after snap in
            if (dx == 0)
                point.X = newBounds.BottomRight().X;

            if (dy == 0)
                point.Y = newBounds.BottomRight().Y;

            point = point.EnsureInBounds(bounds);

            element.Width = point.X - initialPosition.X;
            element.Height = point.Y - initialPosition.Y;
        }

        /// <summary>
        /// Resizes the window by the top left corner of the window.
        /// </summary>
        /// <param name="dx">Increment by X-coordinate.</param>
        /// <param name="dy">Increment by Y-coordinate.</param>
        private void ResizeTopLeft(double dx, double dy)
        {
            Rect newBounds = new Rect(initialPosition.Add(dx, dy), initialSize);
            Point point = element.SnapinController.SnapTopLeftCorner(newBounds).EnsureInBounds(bounds);

            // If only one coordinate was changed - restore the other after snap in
            if (dx == 0)
                point.X = newBounds.Position().X;

            if (dy == 0)
                point.Y = newBounds.Position().Y;

            element.Position = point;

            Point lowerRight = initialPosition.Add(initialSize);
            element.Width = lowerRight.X - point.X;
            element.Height = lowerRight.Y - point.Y;
        }

        /// <summary>
        /// Resizes the window by the lower left corner of the window.
        /// </summary>
        /// <param name="dx">Increment by X-coordinate.</param>
        /// <param name="dy">Increment by Y-coordinate.</param>
        private void ResizeBottomLeft(double dx, double dy)
        {
            Rect newBounds = new Rect(initialPosition.Add(dx, 0), initialSize.Add(-dx, dy));

            Point lowerLeft = element.SnapinController.SnapBottomLeftCorner(newBounds).EnsureInBounds(bounds);
            Point topLeft = new Point(lowerLeft.X, initialPosition.Y);

            element.Position = topLeft;

            element.Width = initialSize.Width + initialPosition.X - topLeft.X;
            element.Height = lowerLeft.Y - initialPosition.Y;
        }

        /// <summary>
        /// Resizes the window by the top right corner of the window.
        /// </summary>
        /// <param name="dx">Increment by X-coordinate.</param>
        /// <param name="dy">Increment by Y-coordinate.</param>
        private void ResizeTopRight(double dx, double dy)
        {
            Rect newBounds = new Rect(initialPosition.Add(0, dy), initialSize.Add(dx, -dy));

            Point topRight = element.SnapinController.SnapTopRightCorner(newBounds).EnsureInBounds(bounds);
            Point topLeft = new Point(initialPosition.X, topRight.Y);

            element.Position = topLeft;

            element.Width = topRight.X - initialPosition.X;
            element.Height = initialSize.Height + initialPosition.Y - topRight.Y;
        }
    }
}
