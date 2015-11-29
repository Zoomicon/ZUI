//Project: ZUI (http://github.com/zoomicon/ZUI)
//Filename: ZoomAndPanControl_Mouse.cs
//Version: 20150104

using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace ZoomAndPan
{
    /// <summary>
    /// This is an extension to the ZoomAndPanControl class that implements
    /// default mouse handling properties and functions.
    ///
    /// </summary>
    public partial class ZoomAndPanControl
    {

      #region Mouse-related Fields

      private bool isDefaultMouseHandling = false;

      /// <summary>
      /// Parts needed for drag zoom rectangle, initialized at "OnApplyTemplate" method
      /// </summary>
      private Canvas dragZoomCanvas;
      private Border dragZoomBorder;

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
      /// Records which mouse button clicked during mouse dragging.
      /// </summary>
      private MouseButton mouseButtonDown; //update WPF Compatibility with System.Windows.Input.MouseButton for SL and System.Windows.Browser.MouseButtons for WPF

      /// <summary>
      /// Saves the previous zoom rectangle, pressing the backspace key jumps back to this zoom rectangle.
      /// </summary>
      private Rect prevZoomRect;

      /// <summary>
      /// Save the previous content scale, pressing the backspace key jumps back to this scale.
      /// </summary>
      private double prevZoomScale;

      /// <summary>
      /// Set to 'true' when the previous zoom rect is saved.
      /// </summary>
      private bool prevZoomRectSet = false; //TODO: check where that should be used

      #endregion

      #region Mouse-related Properties

      public bool IsDefaultMouseHandling {
        get { return isDefaultMouseHandling; }
        set {
          isDefaultMouseHandling = value;
          IsMouseWheelScrollingEnabled = value; //mousewheel scrolls up/down (with SHIFT we also force scroll left/right, with CTRL zoom in/out)
        }
      }

      #endregion

      #region Mouse-related Methods

      protected
#if !SILVERLIGHT
        override
#else
        virtual
#endif
        void OnMouseDoubleClick(MouseButtonEventArgs e)
      {
#if !SILVERLIGHT
        base.OnMouseDoubleClick(e);
#endif
        if (!isDefaultMouseHandling) return;
        if (e.Handled) return;

        if ((Keyboard.Modifiers & ModifierKeys.Control) == 0)
        {
          Point doubleClickPoint = e.GetPosition(content);
          AnimatedSnapTo(doubleClickPoint);
          e.Handled = true;
        }

      }

      protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
      {
        //base.OnMouseLeftButtonDown(e);
        if (!isDefaultMouseHandling) return;
        //if (e.Handled) return;

        OnMouseDown(e, MouseButton.Left);
      }

      protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
      {
        //base.OnMouseRightButtonDown(e);
        if (!isDefaultMouseHandling) return;
        //if (e.Handled) return;

        OnMouseDown(e, MouseButton.Right);
      }

      protected virtual void OnMouseDown(MouseButtonEventArgs e, MouseButton changedButton)
      {
        Focus(); //had content.Focus
#if !SILVERLIGHT
        Keyboard.Focus(content);
#endif

        mouseButtonDown = changedButton; //at WPF one could also use "e.ChangedButton"

        origZoomAndPanControlMouseDownPoint = e.GetPosition(this);
        origContentMouseDownPoint = e.GetPosition(content);

        // Control + left/right-down initiates zooming mode.
        if (ContentScalable && ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
            &&
            (mouseButtonDown == MouseButton.Left ||
             mouseButtonDown == MouseButton.Right))
        {
          Cursor = Cursors.SizeNESW;
          mouseHandlingMode = MouseHandlingMode.Zooming;
        }
        else if (mouseButtonDown == MouseButton.Left)
        {
          Cursor = Cursors.Hand;
          mouseHandlingMode = MouseHandlingMode.Panning;
        }

        if (mouseHandlingMode != MouseHandlingMode.None) //if we will be doing something with the mouse
        {
          //this.CaptureMouse(); // Capture the mouse so that we keep receiving the mouse up event
          e.Handled = true;
        }
        else
          e.Handled = false;
      }

      protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
      {
        //base.OnMouseLeftButtonUp(e);
        if (!isDefaultMouseHandling) return;
        //if (e.Handled) return;

#if SILVERLIGHT
        if (e.ClickCount == 2) { OnMouseDoubleClick(e); return; }
#endif
        OnMouseUp(e, MouseButton.Left);
      }

      protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
      {
        //base.OnMouseRightButtonUp(e);
        if (!isDefaultMouseHandling) return;
        //if (e.Handled) return; //don't do this, else will show Silverlight popup

        OnMouseUp(e, MouseButton.Right);
      }

      protected virtual void OnMouseUp(MouseButtonEventArgs e, MouseButton changedButton)
      {
        if (mouseHandlingMode == MouseHandlingMode.None) return;

        //ReleaseMouseCapture();

        switch (mouseHandlingMode)
        {

          case MouseHandlingMode.Zooming:
            if (ContentScalable && (mouseButtonDown == MouseButton.Left) && (changedButton == MouseButton.Left)) //at WPF one could also use "e.ChangedButton"
            {
              ZoomIn(origContentMouseDownPoint); // Control + left-click zooms in on the content
            }
            else if (ContentScalable && (mouseButtonDown == MouseButton.Right) && (changedButton == MouseButton.Right)) //at WPF one could also use "e.ChangedButton"
            {
              ZoomOut(origContentMouseDownPoint); // Control + left-click zooms out from the content
            }
            e.Handled = ContentScalable;
            break;

          case MouseHandlingMode.DragZooming: //drag-zooming has finished
            ApplyDragZoomRect(); //zoom in on the rectangle that was highlighted by the user.
            e.Handled = true;
            break;

          default:
            e.Handled = false;
            break;
        }

        Cursor = Cursors.Arrow;
        mouseHandlingMode = MouseHandlingMode.None;
      }

      protected override void OnMouseMove(MouseEventArgs e)
      {
        base.OnMouseMove(e);
        if (!isDefaultMouseHandling) return;
#if !SILVERLIGHT
        if (e.Handled) return;
#endif

        if (mouseHandlingMode == MouseHandlingMode.None) return;

        Point curContentMousePoint = e.GetPosition(content);

        switch (mouseHandlingMode)
        {
          case MouseHandlingMode.Panning: // The user is left-dragging the mouse. Pan the viewport by the appropriate amount.

            //Vector dragOffset = curContentMousePoint - origContentMouseDownPoint; //TODO: add Mono's System.Windows.Vector to WPF compatibility and add Point class with extension method for add and subtract to return Vector (get implementation from Mono)
            ContentOffsetX -= curContentMousePoint.X - origContentMouseDownPoint.X; //dragOffset.X
            ContentOffsetY -= curContentMousePoint.Y - origContentMouseDownPoint.Y; //dragOffset.Y

#if !SILVERLIGHT
          e.Handled = true;
#endif
            break;

          case MouseHandlingMode.Zooming:

            Point curZoomAndPanControlMousePoint = e.GetPosition(this);
            //Vector dragOffset = curZoomAndPanControlMousePoint - origZoomAndPanControlMouseDownPoint;
            double dragThreshold = 10;

#if !DRAGZOOMRECT
            if (ContentScalable && (mouseButtonDown == MouseButton.Left))
            {
              if ((curZoomAndPanControlMousePoint.Y - origZoomAndPanControlMouseDownPoint.Y /*dragOffset.X*/) < -dragThreshold) ZoomAboutPoint(ContentScale + 0.02, origContentMouseDownPoint); //CTRL + drag up to zoom in
              else if ((curZoomAndPanControlMousePoint.Y - origZoomAndPanControlMouseDownPoint.Y /*dragOffset.Y*/) > dragThreshold) ZoomAboutPoint(ContentScale - 0.02, origContentMouseDownPoint); //CTRL + drag down to zoom out
            }
#else //DRAGZOOMRECT
            if (ContentScalable && mouseButtonDown == MouseButton.Left &&
                (Math.Abs(curZoomAndPanControlMousePoint.X - origZoomAndPanControlMouseDownPoint.X /*dragOffset.X*/) > dragThreshold ||
                 Math.Abs(curZoomAndPanControlMousePoint.Y - origZoomAndPanControlMouseDownPoint.Y /*dragOffset.Y*/) > dragThreshold))
            {
              //
              // When Control + left-down zooming mode and the user drags beyond the drag threshold,
              // initiate drag zooming mode where the user can drag out a rectangle to select the area
              // to zoom in on.
              //
              mouseHandlingMode = MouseHandlingMode.DragZooming;
              InitDragZoomRect(origContentMouseDownPoint, curContentMousePoint);
            }

#if !SILVERLIGHT
          e.Handled = ContentScalable;
#endif
            break;

          case MouseHandlingMode.DragZooming: // When in drag zooming mode continously update the position of the rectangle that the user is dragging out
            SetDragZoomRect(origContentMouseDownPoint, curContentMousePoint);

#if !SILVERLIGHT
          e.Handled = ContentScalable;
#endif

#endif //DRAGZOOMRECT

            break;

#if !SILVERLIGHT
          default:
            e.Handled = false;
            break;
#endif
        }
      }

      protected override void OnMouseWheel(MouseWheelEventArgs e)
      {
        base.OnMouseWheel(e);
        if (!isDefaultMouseHandling) return;
        if (e.Handled) return;

        if (ContentScalable && ((Keyboard.Modifiers & ModifierKeys.Control) != 0))
        {
          Point mousePosition = e.GetPosition(content); //delta should be either >0 or <0
          if (e.Delta > 0)
             ZoomIn(mousePosition);
          else if (e.Delta < 0)
             ZoomOut(mousePosition);
        }

        else if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0) {
          if (e.Delta > 0) MouseWheelLeft();
          else if (e.Delta < 0) MouseWheelRight();
        }

        else
        {
          if (e.Delta > 0) MouseWheelUp();
          else if (e.Delta < 0) MouseWheelDown();
        }

        e.Handled = true;
      }

      #endregion

      #region Mouse Helper methods

      /// <summary>
      /// Initialise the rectangle that the use is dragging out.
      /// </summary>
      private void InitDragZoomRect(Point pt1, Point pt2)
      {
        SetDragZoomRect(pt1, pt2);

        dragZoomCanvas.Visibility = Visibility.Visible;
        dragZoomBorder.Opacity = 0.5;
      }

      /// <summary>
      /// Update the position and size of the rectangle that user is dragging out.
      /// </summary>
      private void SetDragZoomRect(Point pt1, Point pt2)
      {
        double x, y, width, height;

        //
        // Deterine x,y,width and height of the rect inverting the points if necessary.
        //

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

        //
        // Update the coordinates of the rectangle that is being dragged out by the user.
        // The we offset and rescale to convert from content coordinates.
        //
        Canvas.SetLeft(dragZoomBorder, x); //assuming dragZoomBorder is inside a Canvas
        Canvas.SetTop(dragZoomBorder, y);
        dragZoomBorder.Width = width;
        dragZoomBorder.Height = height;
      }

      /// <summary>
      /// When the user has finished dragging out the rectangle the zoom operation is applied.
      /// </summary>
      private void ApplyDragZoomRect()
      {
        //
        // Record the previous zoom level, so that we can jump back to it when the backspace key is pressed.
        //
        SavePrevZoomRect();

        //
        // Retreive the rectangle that the user draggged out and zoom in on it.
        //
        double contentX = Canvas.GetLeft(dragZoomBorder);
        double contentY = Canvas.GetTop(dragZoomBorder);
        double contentWidth = dragZoomBorder.Width;
        double contentHeight = dragZoomBorder.Height;

        AnimatedZoomTo(new Rect(contentX, contentY, contentWidth, contentHeight));
        FadeOutDragZoomRect();
      }

      //
      // Fade out the drag zoom rectangle.
      //
      private void FadeOutDragZoomRect()
      {
#if !SILVERLIGHT
        AnimationHelper.StartAnimation(dragZoomBorder, Border.OpacityProperty, 0.0, 0.1,
            delegate(object sender, EventArgs e)
            {
              dragZoomCanvas.Visibility = Visibility.Collapsed;
            });
#else
        dragZoomCanvas.Visibility = Visibility.Collapsed;
#endif
      }

      //
      // Record the previous zoom level, so that we can jump back to it when the backspace key is pressed.
      //
      private void SavePrevZoomRect()
      {
        prevZoomRect = new Rect(ContentOffsetX, ContentOffsetY, ContentViewportWidth, ContentViewportHeight);
        prevZoomScale = ContentScale;
        prevZoomRectSet = true;
      }

      /// <summary>
      /// Clear the memory of the previous zoom level.
      /// </summary>
      private void ClearPrevZoomRect()
      {
        prevZoomRectSet = false;
      }

      #endregion

    }
}

