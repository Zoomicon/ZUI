using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.ComponentModel;

namespace TestZoom.Controls
{

    /// <summary>
    /// This is a Window that uses ZoomAndPanControl to zoom and pan around some content.
    /// This demonstrates how to use application specific mouse handling logic with ZoomAndPanControl.
    /// 
    /// also display overview pop up in bottom right of page to view image with thumb to panning.
    /// </summary>
    public partial class HAEZoomingControl : UserControl
    {
        #region Private Variables

        /// <summary>
        /// Specifies the current state of the mouse handling logic.
        /// </summary>
        private MouseHandlingMode mouseHandlingMode = MouseHandlingMode.None;
       
        /// <summary>
        /// The point that was clicked relative to the ZoomAndPanControl.
        /// </summary>
        private Point origZoomAndPanControlMouseDownPoint;

        /// <summary>
        /// The point that was clicked relative to the content that is contained within the ZoomAndPanControl.
        /// </summary>
        private Point origContentMouseDownPoint;

        /// <summary>
        /// Saves the previous zoom rectangle, pressing the backspace key jumps back to this zoom rectangle.
        /// </summary>
        private Rect prevZoomRect;

        /// <summary>
        /// Save the previous content scale, pressing the backspace key jumps back to this scale.
        /// </summary>
        private double prevZoomScale;
        
        /// <summary>
        /// To Detect Double Click
        /// </summary>
        DispatcherTimer _timer;

        /// <summary>
        /// Double Click Detect Interval
        /// </summary>
        private static int INTERVAL = 200;

        /// <summary>
        /// Aminate content image while zooming.
        /// </summary>
        private Storyboard sb;
        #endregion

        #region Properties

        /// <summary>
        /// set source of the content image of zoomandpancontrol.
        /// </summary>
        public ImageSource Source { get; set; }

        /// <summary>
        /// Set slider control Minimum value.
        /// </summary>
        public double MinPercentage { get; set; }

        /// <summary>
        /// Set slider control Maximum value.
        /// </summary>
        public double MaxPercentage { get; set; }
        #endregion

        #region Constructor

        public HAEZoomingControl()
        {
            InitializeComponent();
            _timer = new DispatcherTimer();
            zoomAndPanControl.ContentOffsetY = 200;
            _timer.Interval = new TimeSpan(0, 0, 0, 0, INTERVAL);
            _timer.Tick += new EventHandler(_timer_Tick);

        }
        #endregion

        #region Methods

        /// <summary>
        /// Expand the content area to fit the rectangles.
        /// </summary>
        private void ExpandContent()
        {
            double xOffset = 0;
            double yOffset = 0;
            Rect contentRect = new Rect(0, 0, 0, 0);
            foreach (RectangleData rectangleData in DataModel.Instance.Rectangles)
            {
                if (rectangleData.X < xOffset)
                {
                    xOffset = rectangleData.X;
                }

                if (rectangleData.Y < yOffset)
                {
                    yOffset = rectangleData.Y;
                }

                contentRect.Union(new Rect(rectangleData.X, rectangleData.Y, rectangleData.Width, rectangleData.Height));
            }
            xOffset = Math.Abs(xOffset);
            yOffset = Math.Abs(yOffset);

            foreach (RectangleData rectangleData in DataModel.Instance.Rectangles)
            {
                rectangleData.X += xOffset;
                rectangleData.Y += yOffset;
            }

            DataModel.Instance.ContentWidth = contentRect.Width;
            DataModel.Instance.ContentHeight = contentRect.Height;
        }

        /// <summary>
        /// The 'ZoomOut' command (bound to the minus key) was executed.
        /// </summary>
        private void ZoomOut(Point contentZoomCenter)
        {
            zoomAndPanControl.ZoomAboutPoint(zoomAndPanControl.ContentScale - 0.2, contentZoomCenter);
        }

        /// <summary>
        /// The 'ZoomIn' command (bound to the plus key) was executed.
        /// </summary>
        private void ZoomIn(Point contentZoomCenter)
        {
            zoomAndPanControl.ZoomAboutPoint(zoomAndPanControl.ContentScale + 0.2, contentZoomCenter);
        }
       
        private void InitDragZoomRect(Point pt1, Point pt2)
        {
            SetDragZoomRect(pt1, pt2);

            dragZoomCanvas.Visibility = Visibility.Visible;
            dragZoomBorder.Opacity = 0.5;
        }
        private void SetDragZoomRect(Point pt1, Point pt2)
        {
            double x, y, width, height;
            if (pt2.X < pt1.X)
            {
                x = pt2.X;
                width = pt1.X - pt2.X;
            }
            else
            {
                x = pt1.X;
                width = pt2.X - pt1.X;
            }

            if (pt2.Y < pt1.Y)
            {
                y = pt2.Y;
                height = pt1.Y - pt2.Y;
            }
            else
            {
                y = pt1.Y;
                height = pt2.Y - pt1.Y;
            }
            Canvas.SetLeft(dragZoomBorder, x);
            Canvas.SetTop(dragZoomBorder, y);
            dragZoomBorder.Width = width;
            dragZoomBorder.Height = height;
        }

        /// <summary>
        /// When the user has finished dragging out the rectangle the zoom operation is applied.
        /// </summary>
        private void ApplyDragZoomRect()
        {

            SavePrevZoomRect();
            double contentX = Canvas.GetLeft(dragZoomBorder);
            double contentY = Canvas.GetTop(dragZoomBorder);
            double contentWidth = dragZoomBorder.Width;
            double contentHeight = dragZoomBorder.Height;
            zoomAndPanControl.AnimatedZoomTo(new Rect(contentX, contentY, contentWidth, contentHeight));
        }

        /// <summary>
        /// Record the previous zoom level, so that we can jump back to it when the backspace key is pressed.
        /// </summary>
        private void SavePrevZoomRect()
        {
            prevZoomRect = new Rect(zoomAndPanControl.ContentOffsetX, zoomAndPanControl.ContentOffsetY, zoomAndPanControl.ContentViewportWidth, zoomAndPanControl.ContentViewportHeight);
            prevZoomScale = zoomAndPanControl.ContentScale;
        }
        private void ClearPrevZoomRect()
        {

        }

        #endregion

        #region Events


        void _timer_Tick(object sender, EventArgs e)
        {
            _timer.Stop();
        }

        /// <summary>
        /// Event raised when the UserControl has loaded.
        /// </summary>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            changeSlider.Minimum = MinPercentage;
            changeSlider.Maximum = MaxPercentage;
            content.Source = Source;
            content2.Source = Source;
            MyPOP.IsOpen = true;
            MyPOP.HorizontalOffset = (this.Width - 200);
            MyPOP.VerticalOffset = (this.Height - 230);
            ExpandContent();
        }

        private void btnFill_Click(object sender, RoutedEventArgs e)
        {
            SavePrevZoomRect();

            zoomAndPanControl.AnimatedZoomToFit();
        }

        private void btn100_Click(object sender, RoutedEventArgs e)
        {
            SavePrevZoomRect();

            zoomAndPanControl.AnimatedZoomTo(1.0);
        }

        private void btnZoomOut_Click(object sender, RoutedEventArgs e)
        {
            if (zoomAndPanControl.ContentScale <= (MaxPercentage / 100))
                btnZoomIn.IsEnabled = true;
            if (zoomAndPanControl.ContentScale >= (MinPercentage / 100))
            {
                sb = new Storyboard();
                DoubleAnimation db = new DoubleAnimation();
                sb.Children.Add(db);
                sb.Completed += new EventHandler(sb_Completed);
                db.To = zoomAndPanControl.ContentScale - 0.2;
                db.Duration = new Duration(TimeSpan.FromSeconds(0.3));
                Storyboard.SetTarget(db, zoomAndPanControl);
                Storyboard.SetTargetProperty(db, new PropertyPath("ContentScale"));
                sb.Begin();
                //ZoomOut(new Point(zoomAndPanControl.ContentZoomFocusX, zoomAndPanControl.ContentZoomFocusY));
            }
            else
            {
                btnZoomOut.IsEnabled = false;
                //zoomAndPanControl.ContentScale = zoomAndPanControl.ContentScale + 0.2;
            }
        }

        private void btnZoomIn_Click(object sender, RoutedEventArgs e)
        {
            if (zoomAndPanControl.ContentScale >= (MinPercentage / 100))
                btnZoomOut.IsEnabled = true;
            if (zoomAndPanControl.ContentScale <= (MaxPercentage / 100))
            {
                sb = new Storyboard();
                DoubleAnimation db = new DoubleAnimation();
                sb.Children.Add(db);
                sb.Completed += new EventHandler(sb_Completed);
                db.To = zoomAndPanControl.ContentScale + 0.2;
                db.Duration = new Duration(TimeSpan.FromSeconds(0.3));
                Storyboard.SetTarget(db, zoomAndPanControl);
                Storyboard.SetTargetProperty(db, new PropertyPath("ContentScale"));
                sb.Begin();
                //ZoomIn(new Point(zoomAndPanControl.ContentZoomFocusX, zoomAndPanControl.ContentZoomFocusY));
            }
            else
            {
                btnZoomIn.IsEnabled = false;
                //zoomAndPanControl.ContentScale = zoomAndPanControl.ContentScale - 0.2;
            }
        }

        void sb_Completed(object sender, EventArgs e)
        {
            sb.Stop();
        }

        /// <summary>
        /// Event raised on mouse move in the ZoomAndPanControl.
        /// </summary>
        private void zoomAndPanControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseHandlingMode == MouseHandlingMode.Panning)
            {
                Point curContentMousePoint = e.GetPosition(content);
                zoomAndPanControl.ContentOffsetX -= curContentMousePoint.X - origContentMouseDownPoint.X;
                zoomAndPanControl.ContentOffsetY -= curContentMousePoint.Y - origContentMouseDownPoint.Y;
            }
        }

        /// <summary>
        /// Event raised by rotating the mouse wheel
        /// </summary>
        private void zoomAndPanControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;

            if (e.Delta > 0)
            {
                Point curContentMousePoint = e.GetPosition(content);
                ZoomIn(curContentMousePoint);
            }
            else if (e.Delta < 0)
            {
                Point curContentMousePoint = e.GetPosition(content);
                ZoomOut(curContentMousePoint);
            }
        }

        /// <summary>
        /// Event raised on mouse down in the ZoomAndPanControl.
        /// </summary>
        private void zoomAndPanControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (zoomAndPanControl.ContentScale <= (MaxPercentage / 100))
                btnZoomIn.IsEnabled = true;
            if (zoomAndPanControl.ContentScale >= (MinPercentage / 100))
                btnZoomOut.IsEnabled = true;
            if (_timer.IsEnabled)
            {
                if (zoomAndPanControl.ContentScale <= (MaxPercentage / 100))
                {
                    _timer.Stop();
                    sb = new Storyboard();
                    DoubleAnimation db = new DoubleAnimation();
                    sb.Children.Add(db);
                    sb.Completed += new EventHandler(sb_Completed);
                    db.To = zoomAndPanControl.ContentScale + 0.2;
                    db.Duration = new Duration(TimeSpan.FromSeconds(0.3));
                    Storyboard.SetTarget(db, zoomAndPanControl);
                    Storyboard.SetTargetProperty(db, new PropertyPath("ContentScale"));
                    sb.Begin();

                    /*Point curContentMousePoint = e.GetPosition(content);
                    //zoomAndPanControl.ContentOffsetX = curContentMousePoint.X ; //dragOffset.X;
                    //zoomAndPanControl.ContentOffsetY = curContentMousePoint.Y ; //dragOffset.Y;               
                    zoomAndPanControl.ZoomAboutPoint(zoomAndPanControl.ContentScale + 0.2, curContentMousePoint);*/
                }
                else
                {
                    btnZoomIn.IsEnabled = false;
                    //zoomAndPanControl.ContentScale = zoomAndPanControl.ContentScale - 0.2;
                }
            }
            else
            {
                _timer.Start();
                if (zoomAndPanControl.ContentScale >= (MinPercentage / 100))
                {

                    origZoomAndPanControlMouseDownPoint = e.GetPosition(zoomAndPanControl);
                    origContentMouseDownPoint = e.GetPosition(content);
                    mouseHandlingMode = MouseHandlingMode.Panning;
                    if (mouseHandlingMode != MouseHandlingMode.None)
                    {
                        zoomAndPanControl.CaptureMouse();
                        e.Handled = true;
                    }
                }

            }

        }

        /// <summary>
        /// Event raised on mouse up in the ZoomAndPanControl.
        /// </summary>
        private void zoomAndPanControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (zoomAndPanControl.ContentScale >= (MinPercentage / 100))
            {
                zoomAndPanControl.ReleaseMouseCapture();
                mouseHandlingMode = MouseHandlingMode.None;
                e.Handled = true;
            }
        }

        private void overviewZoomRectThumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
          double newContentOffsetX = Math.Min(Math.Max(0.0, Canvas.GetLeft(overviewZoomRectThumb) + e.HorizontalChange), DataModel.Instance.ContentWidth - DataModel.Instance.ContentViewportWidth);
          Canvas.SetLeft(overviewZoomRectThumb, newContentOffsetX);

          double newContentOffsetY = Math.Min(Math.Max(0.0, Canvas.GetTop(overviewZoomRectThumb) + e.VerticalChange), DataModel.Instance.ContentHeight - DataModel.Instance.ContentViewportHeight);
          Canvas.SetTop(overviewZoomRectThumb, newContentOffsetY);
        }

        private void overview_SizeChanged(object sender, SizeChangedEventArgs e)
        {
          overview.ZoomToFit();
        }

        private void MyPOP_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

          Point clickedPoint = e.GetPosition(content);
          double newX = clickedPoint.X - (overviewZoomRectThumb.Width / 2);
          double newY = clickedPoint.Y - (overviewZoomRectThumb.Height / 2);
          Canvas.SetLeft(overviewZoomRectThumb, newX);
          Canvas.SetTop(overviewZoomRectThumb, newY);
        }

        #endregion

    }
}
