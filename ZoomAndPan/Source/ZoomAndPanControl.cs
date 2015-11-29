//Filename: ZoomAndPanControl.cs
//Version: 20140306
//Editor: George Birbilis <zoomicon.com>

//Based on:
//- WPF: http://www.codeproject.com/Articles/85603/A-WPF-custom-control-for-zooming-and-panning
//- Silerlight: http://www.codeproject.com/Articles/167453/A-Silverlight-custom-control-for-zooming-and-panni

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Compatibility; //Birbilis

namespace ZoomAndPan
{
  /// <summary>
  /// A class that wraps up zooming and panning of it's content.
  /// </summary>
  [TemplatePart(Name = "PART_Content", Type = typeof(FrameworkElement))] //Silverlight needs this, WPF controls can also use, but sometimes just detect PART_ prefix in Generic.xaml
  [TemplatePart(Name = "PART_DragZoomCanvas", Type = typeof(Canvas))]
  [TemplatePart(Name = "PART_DragZoomBorder", Type = typeof(Border))]
  public partial class ZoomAndPanControl : ContentControl, IScrollInfo
  {

    #region Internal Data Members

    /// <summary>
    /// Reference to the underlying content, which is named PART_Content in the template.
    /// </summary>
    private FrameworkElement content = null;

    /// <summary>
    /// The transform that is applied to the content to scale it by 'ContentScale'.
    /// </summary>
    private ScaleTransform contentScaleTransform = null;

    /// <summary>
    /// The transform that is applied to the content to offset it by 'ContentOffsetX' and 'ContentOffsetY'.
    /// </summary>
    private TranslateTransform contentOffsetTransform = null;

    /// <summary>
    /// Enable the update of the content offset as the content scale changes.
    /// This enabled for zooming about a point (google-maps style zooming) and zooming to a rect.
    /// </summary>
    private bool enableContentOffsetUpdateFromScale = false;

    /// <summary>
    /// Used to disable syncronization between IScrollInfo interface and ContentOffsetX/ContentOffsetY.
    /// </summary>
    private bool disableScrollOffsetSync = false;

    /// <summary>
    /// Normally when content offsets changes the content focus is automatically updated.
    /// This syncronization is disabled when 'disableContentFocusSync' is set to 'true'.
    /// When we are zooming in or out we 'disableContentFocusSync' is set to 'true' because
    /// we are zooming in or out relative to the content focus we don't want to update the focus.
    /// </summary>
    private bool disableContentFocusSync = false;

    /// <summary>
    /// The width of the viewport in content coordinates, clamped to the width of the content.
    /// </summary>
    private double constrainedContentViewportWidth = 0.0;

    /// <summary>
    /// The height of the viewport in content coordinates, clamped to the height of the content.
    /// </summary>
    private double constrainedContentViewportHeight = 0.0;

    #endregion Internal Data Members

    #region IScrollInfo Data Members

    //
    // These data members are for the implementation of the IScrollInfo interface.
    // This interface works with the ScrollViewer such that when ZoomAndPanControl is
    // wrapped (in XAML) with a ScrollViewer the IScrollInfo interface allows the ZoomAndPanControl to
    // handle the the scrollbar offsets.
    //
    // The IScrollInfo properties and member functions are implemented in ZoomAndPanControl_IScrollInfo.cs.
    //
    // There is a good series of articles showing how to implement IScrollInfo starting here:
    //     http://blogs.msdn.com/bencon/archive/2006/01/05/509991.aspx
    //

    /// <summary>
    /// Set to 'true' when the vertical scrollbar is enabled.
    /// </summary>
    private bool canVerticallyScroll = false;

    /// <summary>
    /// Set to 'true' when the vertical scrollbar is enabled.
    /// </summary>
    private bool canHorizontallyScroll = false;

    /// <summary>
    /// Records the unscaled extent of the content.
    /// This is calculated during the measure and arrange.
    /// </summary>
    private Size unScaledExtent = new Size(0, 0);

    /// <summary>
    /// Records the size of the viewport (in viewport coordinates) onto the content.
    /// This is calculated during the measure and arrange.
    /// </summary>
    private Size viewport = new Size(0, 0);

    /// <summary>
    /// Reference to the ScrollViewer that is wrapped (in XAML) around the ZoomAndPanControl.
    /// Or set to null if there is no ScrollViewer.
    /// </summary>
    private ScrollViewer scrollOwner = null;

    #endregion IScrollInfo Data Members

    #region Dependency Property Definitions

    //
    // Definitions for dependency properties.
    //

    public static readonly DependencyProperty ContentScaleProperty =
            DependencyPropertyExt.Register("ContentScale", typeof(double), typeof(ZoomAndPanControl), new FrameworkPropertyMetadata(1.0, ContentScale_PropertyChanged, ContentScale_Coerce));

    public static readonly DependencyProperty ContentScalableProperty =
           DependencyPropertyExt.Register("ContentScalable", typeof(bool), typeof(ZoomAndPanControl), new FrameworkPropertyMetadata(true));

    public static readonly DependencyProperty MinContentScaleProperty =
            DependencyPropertyExt.Register("MinContentScale", typeof(double), typeof(ZoomAndPanControl), new FrameworkPropertyMetadata(0.01, MinOrMaxContentScale_PropertyChanged));

    public static readonly DependencyProperty MaxContentScaleProperty =
            DependencyPropertyExt.Register("MaxContentScale", typeof(double), typeof(ZoomAndPanControl), new FrameworkPropertyMetadata(10.0, MinOrMaxContentScale_PropertyChanged));

    public static readonly DependencyProperty ContentOffsetXProperty =
            DependencyPropertyExt.Register("ContentOffsetX", typeof(double), typeof(ZoomAndPanControl), new FrameworkPropertyMetadata(0.0, ContentOffsetX_PropertyChanged, ContentOffsetX_Coerce));

    public static readonly DependencyProperty ContentOffsetYProperty =
            DependencyPropertyExt.Register("ContentOffsetY", typeof(double), typeof(ZoomAndPanControl), new FrameworkPropertyMetadata(0.0, ContentOffsetY_PropertyChanged, ContentOffsetY_Coerce));

    public static readonly DependencyProperty AnimationDurationProperty =
            DependencyPropertyExt.Register("AnimationDuration", typeof(double), typeof(ZoomAndPanControl), new FrameworkPropertyMetadata(0.4));

    public static readonly DependencyProperty ContentZoomFocusXProperty =
            DependencyPropertyExt.Register("ContentZoomFocusX", typeof(double), typeof(ZoomAndPanControl), new FrameworkPropertyMetadata(0.0));

    public static readonly DependencyProperty ContentZoomFocusYProperty =
            DependencyPropertyExt.Register("ContentZoomFocusY", typeof(double), typeof(ZoomAndPanControl), new FrameworkPropertyMetadata(0.0));

    public static readonly DependencyProperty ViewportZoomFocusXProperty =
            DependencyPropertyExt.Register("ViewportZoomFocusX", typeof(double), typeof(ZoomAndPanControl), new FrameworkPropertyMetadata(0.0));

    public static readonly DependencyProperty ViewportZoomFocusYProperty =
            DependencyPropertyExt.Register("ViewportZoomFocusY", typeof(double), typeof(ZoomAndPanControl), new FrameworkPropertyMetadata(0.0));

    public static readonly DependencyProperty ContentViewportWidthProperty =
            DependencyPropertyExt.Register("ContentViewportWidth", typeof(double), typeof(ZoomAndPanControl), new FrameworkPropertyMetadata(0.0));

    public static readonly DependencyProperty ContentViewportHeightProperty =
            DependencyPropertyExt.Register("ContentViewportHeight", typeof(double), typeof(ZoomAndPanControl), new FrameworkPropertyMetadata(0.0));

    public static readonly DependencyProperty IsMouseWheelScrollingEnabledProperty =
            DependencyPropertyExt.Register("IsMouseWheelScrollingEnabled", typeof(bool), typeof(ZoomAndPanControl), new FrameworkPropertyMetadata(false));

    #endregion Dependency Property Definitions

    #region Event Handler

    /// <summary>
    /// Event raised when the ContentOffsetX property has changed.
    /// </summary>
    public event EventHandler ContentOffsetXChanged;

    /// <summary>
    /// Event raised when the ContentOffsetY property has changed.
    /// </summary>
    public event EventHandler ContentOffsetYChanged;

    /// <summary>
    /// Event raised when the ContentScale property has changed.
    /// </summary>
    public event EventHandler ContentScaleChanged;

    #endregion Event Handler

    #region Properties

    /// <summary>
    /// Get/set the X offset (in content coordinates) of the view on the content.
    /// </summary>
    public double ContentOffsetX
    {
      get { return (double)GetValue(ContentOffsetXProperty); }
      set { SetValue(ContentOffsetXProperty, value); }
    }

    /// <summary>
    /// Get/set the Y offset (in content coordinates) of the view on the content.
    /// </summary>
    public double ContentOffsetY
    {
      get { return (double)GetValue(ContentOffsetYProperty); }
      set { SetValue(ContentOffsetYProperty, value); }
    }

    /// <summary>
    /// Get/set the current scale (or zoom factor) of the content.
    /// </summary>
    public double ContentScale
    {
      get { return (double)GetValue(ContentScaleProperty); }
      set { SetValue(ContentScaleProperty, value); }
    }

    /// <summary>
    /// Get/set whethe content is scalable via user interaction.
    /// </summary>
    public bool ContentScalable
    {
      get { return (bool)GetValue(ContentScalableProperty); }
      set { SetValue(ContentScalableProperty, value); }
    }

    /// <summary>
    /// Get/set the minimum value for 'ContentScale'.
    /// </summary>
    public double MinContentScale
    {
      get { return (double)GetValue(MinContentScaleProperty); }
      set { SetValue(MinContentScaleProperty, value); }
    }

    /// <summary>
    /// Get/set the maximum value for 'ContentScale'.
    /// </summary>
    public double MaxContentScale
    {
      get { return (double)GetValue(MaxContentScaleProperty); }
      set { SetValue(MaxContentScaleProperty, value); }
    }

    /// <summary>
    /// The X coordinate of the content focus, this is the point that we are focusing on when zooming.
    /// </summary>
    public double ContentZoomFocusX
    {
      get { return (double)GetValue(ContentZoomFocusXProperty); }
      set { SetValue(ContentZoomFocusXProperty, value); }
    }

    /// <summary>
    /// The Y coordinate of the content focus, this is the point that we are focusing on when zooming.
    /// </summary>
    public double ContentZoomFocusY
    {
      get { return (double)GetValue(ContentZoomFocusYProperty); }
      set { SetValue(ContentZoomFocusYProperty, value); }
    }

    /// <summary>
    /// The X coordinate of the viewport focus, this is the point in the viewport (in viewport coordinates)
    /// that the content focus point is locked to while zooming in.
    /// </summary>
    public double ViewportZoomFocusX
    {
      get { return (double)GetValue(ViewportZoomFocusXProperty); }
      set { SetValue(ViewportZoomFocusXProperty, value); }
    }

    /// <summary>
    /// The Y coordinate of the viewport focus, this is the point in the viewport (in viewport coordinates)
    /// that the content focus point is locked to while zooming in.
    /// </summary>
    public double ViewportZoomFocusY
    {
      get { return (double)GetValue(ViewportZoomFocusYProperty); }
      set { SetValue(ViewportZoomFocusYProperty, value); }
    }

    /// <summary>
    /// The duration of the animations (in seconds) started by calling AnimatedZoomTo and the other animation methods.
    /// </summary>
    public double AnimationDuration
    {
      get { return (double)GetValue(AnimationDurationProperty); }
      set { SetValue(AnimationDurationProperty, value); }
    }

    /// <summary>
    /// Get the viewport width, in content coordinates.
    /// </summary>
    public double ContentViewportWidth
    {
      get { return (double)GetValue(ContentViewportWidthProperty); }
      set { SetValue(ContentViewportWidthProperty, value); }
    }

    /// <summary>
    /// Get the viewport height, in content coordinates.
    /// </summary>
    public double ContentViewportHeight
    {
      get { return (double)GetValue(ContentViewportHeightProperty); }
      set { SetValue(ContentViewportHeightProperty, value); }
    }

    /// <summary>
    /// Get the viewport bounds, in content coordinates.
    /// </summary>
    public Rect ContentViewportBounds
    {
      get { return new Rect(ContentOffsetX, ContentOffsetY, ContentViewportWidth, ContentViewportHeight); }
    }

    /// <summary>
    /// Set to 'true' to enable the mouse wheel to scroll the zoom and pan control.
    /// This is set to 'false' by default.
    /// </summary>
    public bool IsMouseWheelScrollingEnabled
    {
      get { return (bool)GetValue(IsMouseWheelScrollingEnabledProperty); }
      set { SetValue(IsMouseWheelScrollingEnabledProperty, value); }
    }

    #endregion Properties

    #region Methods

    /// <summary>
    /// Do an animated zoom to view a specific scale and rectangle (in content coordinates).
    /// </summary>
    public void AnimatedZoomTo(double newScale, Rect contentRect)
    {
      AnimatedZoomPointToViewportCenter(newScale, new Point(contentRect.X + (contentRect.Width / 2), contentRect.Y + (contentRect.Height / 2)),
          delegate(object sender, EventArgs e)
          {
            //
            // At the end of the animation, ensure that we are snapped to the specified content offset.
            // Due to zooming in on the content focus point and rounding errors, the content offset may
            // be slightly off what we want at the end of the animation and this bit of code corrects it.
            //
            this.ContentOffsetX = contentRect.X;
            this.ContentOffsetY = contentRect.Y;
          });
    }

    /// <summary>
    /// Do an animated zoom to the specified rectangle (in content coordinates).
    /// </summary>
    public void AnimatedZoomTo(Rect contentRect)
    {
#if !SILVERLIGHT
            double scaleX = this.ContentViewportWidth / contentRect.Width;
            double scaleY = this.ContentViewportHeight / contentRect.Height;
            double newScale = this.ContentScale * Math.Min(scaleX, scaleY);

            AnimatedZoomPointToViewportCenter(newScale, new Point(contentRect.X + (contentRect.Width / 2), contentRect.Y + (contentRect.Height / 2)), null);
#else
      ZoomTo(contentRect);
#endif
    }

    /// <summary>
    /// Instantly zoom to the specified rectangle (in content coordinates).
    /// </summary>
    public void ZoomTo(Rect contentRect)
    {
      if (contentRect.IsEmpty) return;

      double scaleX = this.ContentViewportWidth / contentRect.Width;
      double scaleY = this.ContentViewportHeight / contentRect.Height;
      double newScale = this.ContentScale * Math.Min(scaleX, scaleY);

      ZoomPointToViewportCenter(newScale, new Point(contentRect.X + (contentRect.Width / 2), contentRect.Y + (contentRect.Height / 2)));
    }

    public void ScrollToCenter(Rect contentRect)
    {
      if (contentRect.IsEmpty) return;

      double scaleX = this.ContentViewportWidth / contentRect.Width;
      double scaleY = this.ContentViewportHeight / contentRect.Height;

      ZoomPointToViewportCenter(ContentScale, new Point(contentRect.X + (contentRect.Width / 2), contentRect.Y + (contentRect.Height / 2)));
    }

    /// <summary>
    /// Instantly center the view on the specified point (in content coordinates).
    /// </summary>
    public void SnapContentOffsetTo(Point contentOffset)
    {
      AnimationHelper.CancelAnimation(this, ContentOffsetXProperty);
      AnimationHelper.CancelAnimation(this, ContentOffsetYProperty);

      this.ContentOffsetX = contentOffset.X;
      this.ContentOffsetY = contentOffset.Y;
    }

    /// <summary>
    /// Instantly center the view on the specified point (in content coordinates).
    /// </summary>
    public void SnapTo(Point contentPoint)
    {
      AnimationHelper.CancelAnimation(this, ContentOffsetXProperty);
      AnimationHelper.CancelAnimation(this, ContentOffsetYProperty);

      this.ContentOffsetX = contentPoint.X - (this.ContentViewportWidth / 2);
      this.ContentOffsetY = contentPoint.Y - (this.ContentViewportHeight / 2);
    }

    /// <summary>
    /// Use animation to center the view on the specified point (in content coordinates).
    /// </summary>
    public void AnimatedSnapTo(Point contentPoint)
    {
      double newX = contentPoint.X - (this.ContentViewportWidth / 2);
      double newY = contentPoint.Y - (this.ContentViewportHeight / 2);
      AnimationHelper.StartAnimation(this, ContentOffsetXProperty, newX, AnimationDuration);
      AnimationHelper.StartAnimation(this, ContentOffsetYProperty, newY, AnimationDuration);
    }

    /// <summary>
    /// Zoom in/out centered on the specified point (in content coordinates).
    /// The focus point is kept locked to it's on screen position (ala google maps).
    /// </summary>
    public void AnimatedZoomAboutPoint(double newContentScale, Point contentZoomFocus)
    {
      newContentScale = Math.Min(Math.Max(newContentScale, MinContentScale), MaxContentScale);
      AnimationHelper.CancelAnimation(this, ContentZoomFocusXProperty);
      AnimationHelper.CancelAnimation(this, ContentZoomFocusYProperty);
      AnimationHelper.CancelAnimation(this, ViewportZoomFocusXProperty);
      AnimationHelper.CancelAnimation(this, ViewportZoomFocusYProperty);

      ContentZoomFocusX = contentZoomFocus.X;
      ContentZoomFocusY = contentZoomFocus.Y;
      ViewportZoomFocusX = (ContentZoomFocusX - ContentOffsetX) * ContentScale;
      ViewportZoomFocusY = (ContentZoomFocusY - ContentOffsetY) * ContentScale;
      //
      // When zooming about a point make updates to ContentScale also update content offset.
      //
      enableContentOffsetUpdateFromScale = true;
      AnimationHelper.StartAnimation(this, ContentScaleProperty, newContentScale, AnimationDuration,
          delegate(object sender, EventArgs e)
          {
            enableContentOffsetUpdateFromScale = false;
            ResetViewportZoomFocus();
          });
    }

    /// <summary>
    /// Zoom in/out centered on the specified point (in content coordinates).
    /// The focus point is kept locked to it's on screen position (ala google maps).
    /// </summary>
    public void ZoomAboutPoint(double newContentScale, Point contentZoomFocus)
    {
      newContentScale = Math.Min(Math.Max(newContentScale, MinContentScale), MaxContentScale);

      double screenSpaceZoomOffsetX = (contentZoomFocus.X - ContentOffsetX) * ContentScale;
      double screenSpaceZoomOffsetY = (contentZoomFocus.Y - ContentOffsetY) * ContentScale;
      double contentSpaceZoomOffsetX = screenSpaceZoomOffsetX / newContentScale;
      double contentSpaceZoomOffsetY = screenSpaceZoomOffsetY / newContentScale;
      double newContentOffsetX = contentZoomFocus.X - contentSpaceZoomOffsetX;
      double newContentOffsetY = contentZoomFocus.Y - contentSpaceZoomOffsetY;
      AnimationHelper.CancelAnimation(this, ContentScaleProperty);
      AnimationHelper.CancelAnimation(this, ContentOffsetXProperty);
      AnimationHelper.CancelAnimation(this, ContentOffsetYProperty);

      this.ContentScale = newContentScale;
      this.ContentOffsetX = newContentOffsetX;
      this.ContentOffsetY = newContentOffsetY;
    }

    /// <summary>
    /// Zoom in/out centered on the viewport center.
    /// </summary>
    public void AnimatedZoomTo(double contentScale)
    {
      Point zoomCenter = new Point(ContentOffsetX + (ContentViewportWidth / 2), ContentOffsetY + (ContentViewportHeight / 2));
      AnimatedZoomAboutPoint(contentScale, zoomCenter);
    }

    /// <summary>
    /// Zoom in/out centered on the viewport center.
    /// </summary>
    public void ZoomTo(double contentScale)
    {
      Point zoomCenter = new Point(ContentOffsetX + (ContentViewportWidth / 2), ContentOffsetY + (ContentViewportHeight / 2));
      ZoomAboutPoint(contentScale, zoomCenter);
    }

    /// <summary>
    /// Do animation that scales the content so that it fits completely in the control.
    /// </summary>
    public void AnimatedZoomToFit()
    {
      if (content == null)
        throw new ApplicationException("PART_Content was not found in the ZoomAndPanControl visual template!");

      AnimatedZoomTo(new Rect(0, 0, content.ActualWidth, content.ActualHeight));
    }

    /// <summary>
    /// Instantly scale the content so that it fits completely in the control.
    /// </summary>
    public void ZoomToFit()
    {
      if (content == null)
        throw new ApplicationException("PART_Content was not found in the ZoomAndPanControl visual template!");

      ZoomTo(new Rect(0, 0, content.ActualWidth, content.ActualHeight));
    }

    /// <summary>
    /// Zoom out at logical point
    /// </summary>
    public void ZoomOut(Point contentZoomCenter, double ZoomFactorStep = 0.2) //Birbilis
    {
      ZoomAboutPoint(ContentScale - ZoomFactorStep, contentZoomCenter);
    }

    /// <summary>
    /// Zoom out at view center
    /// </summary>
    public void ZoomOut(double ZoomFactorStep = 0.2) //Birbilis
    {
      Point contentViewCenter = new Point(ContentOffsetX + ContentViewportWidth / 2, ContentOffsetY + ContentViewportHeight / 2);
      ZoomOut(contentViewCenter, ZoomFactorStep);
    }

    /// <summary>
    /// Zoom in at logical point
    /// </summary>
    public void ZoomIn(Point contentZoomCenter, double ZoomFactorStep = 0.2) //Birbilis
    {
      ZoomAboutPoint(ContentScale + ZoomFactorStep, contentZoomCenter);
    }

    /// <summary>
    /// Zoom in at view center
    /// </summary>
    public void ZoomIn(double ZoomFactorStep = 0.2) //Birbilis
    {
      Point contentViewCenter = new Point(ContentOffsetX + ContentViewportWidth / 2, ContentOffsetY + ContentViewportHeight / 2);
      ZoomIn(contentViewCenter, ZoomFactorStep);
    }

    //Modelled after respective method in MultiScaleImage
    public Point ElementToLogicalPoint(Point elementPoint) //Birbilis
    {
      return new Point(ContentOffsetX + (elementPoint.X * ContentScale),
                        ContentOffsetY + (elementPoint.Y * ContentScale));
    }

    //Modelled after respective method in MultiScaleImage
    public Point LogicalToElementPoint(Point logicalPoint) //Birbilis
    {
      return new Point((logicalPoint.X - ContentOffsetX) / ContentScale,
                        (logicalPoint.Y - ContentOffsetY) / ContentScale);
    }

    #endregion Methods

    #region Internal Methods

    public void UpdateScrollOffsets()
    {
      // Notify the owning ScrollViewer that the scrollbar offsets should be updated.
      if (!disableScrollOffsetSync && scrollOwner != null)
        scrollOwner.InvalidateScrollInfo();
    }

    //#if SILVERLIGHT
    /// <summary>
    /// Constructor to define metadata for the control (and link it to the style in Generic.xaml).
    /// </summary>
    public ZoomAndPanControl()
    {
      this.DefaultStyleKey = typeof(ZoomAndPanControl); //this should be enough for both WPF and Silerlight, if not uncomment the #if/#else/#endif
    }
    /*//#else
            /// <summary>
            /// Static constructor to define metadata for the control (and link it to the style in Generic.xaml).
            /// </summary>
            static ZoomAndPanControl()
            {
                DefaultStyleKeyProperty.OverrideMetadata(typeof(ZoomAndPanControl), new FrameworkPropertyMetadata(typeof(ZoomAndPanControl)));
            }
    //#endif
    */

    /// <summary>
    /// Called when a template has been applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();
      //WPF// content = this.Template.FindName("PART_Content", this) as FrameworkElement;
      this.content = this.GetTemplateChild("PART_Content") as FrameworkElement;
      this.dragZoomCanvas = this.GetTemplateChild("PART_DragZoomCanvas") as Canvas;
      this.dragZoomBorder = this.GetTemplateChild("PART_DragZoomBorder") as Border;

      if (content == null)
        return;

      // Setup the transform on the content so that we can scale it by 'ContentScale'.
      this.contentScaleTransform = new ScaleTransform()
      {
        ScaleX = this.ContentScale,
        ScaleY = this.ContentScale
      };

      // Setup the transform on the content so that we can translate it by 'ContentOffsetX' and 'ContentOffsetY'.
      this.contentOffsetTransform = new TranslateTransform();
      UpdateTranslationX();
      UpdateTranslationY();

      // Setup a transform group to contain the translation and scale transforms, and then
      // assign this to the content's 'RenderTransform'.
      TransformGroup transformGroup = new TransformGroup();
      transformGroup.Children.Add(this.contentOffsetTransform);
      transformGroup.Children.Add(this.contentScaleTransform);
      content.RenderTransform = transformGroup;
    }

    /// <summary>
    /// Zoom to the specified scale and move the specified focus point to the center of the viewport.
    /// </summary>
    private void AnimatedZoomPointToViewportCenter(double newContentScale, Point contentZoomFocus, EventHandler callback)
    {
      newContentScale = Math.Min(Math.Max(newContentScale, MinContentScale), MaxContentScale);
      AnimationHelper.CancelAnimation(this, ContentZoomFocusXProperty);
      AnimationHelper.CancelAnimation(this, ContentZoomFocusYProperty);
      AnimationHelper.CancelAnimation(this, ViewportZoomFocusXProperty);
      AnimationHelper.CancelAnimation(this, ViewportZoomFocusYProperty);

      ContentZoomFocusX = contentZoomFocus.X;
      ContentZoomFocusY = contentZoomFocus.Y;
      ViewportZoomFocusX = (ContentZoomFocusX - ContentOffsetX) * ContentScale;
      ViewportZoomFocusY = (ContentZoomFocusY - ContentOffsetY) * ContentScale;
      //
      // When zooming about a point make updates to ContentScale also update content offset.
      //
      enableContentOffsetUpdateFromScale = true;

      AnimationHelper.StartAnimation(this, ContentScaleProperty, newContentScale, AnimationDuration,
          delegate(object sender, EventArgs e)
          {
            enableContentOffsetUpdateFromScale = false;

            if (callback != null)
              callback(this, EventArgs.Empty);
          });

      AnimationHelper.StartAnimation(this, ViewportZoomFocusXProperty, ViewportWidth / 2, AnimationDuration);
      AnimationHelper.StartAnimation(this, ViewportZoomFocusYProperty, ViewportHeight / 2, AnimationDuration);
    }

    /// <summary>
    /// Zoom to the specified scale and move the specified focus point to the center of the viewport.
    /// </summary>
    private void ZoomPointToViewportCenter(double newContentScale, Point contentZoomFocus)
    {
      newContentScale = Math.Min(Math.Max(newContentScale, MinContentScale), MaxContentScale);
      AnimationHelper.CancelAnimation(this, ContentScaleProperty);
      AnimationHelper.CancelAnimation(this, ContentOffsetXProperty);
      AnimationHelper.CancelAnimation(this, ContentOffsetYProperty);

      this.ContentScale = newContentScale;
      this.ContentOffsetX = contentZoomFocus.X - (ContentViewportWidth / 2);
      this.ContentOffsetY = contentZoomFocus.Y - (ContentViewportHeight / 2);
    }

    #region ContentScale

    /// <summary>
    /// Event raised when the 'ContentScale' property has changed value.
    /// </summary>
    public static void ContentScale_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
      ZoomAndPanControl c = (ZoomAndPanControl)o;

      // Update the content scale transform whenever 'ContentScale' changes.
      if (c.contentScaleTransform != null)
      {
        c.contentScaleTransform.ScaleX = c.ContentScale;
        c.contentScaleTransform.ScaleY = c.ContentScale;
      }

      // Update the size of the viewport in content coordinates.
      c.UpdateContentViewportSize();

      if (c.enableContentOffsetUpdateFromScale)
      {
        try
        {
          // Disable content focus syncronization.  We are about to update content offset whilst zooming
          // to ensure that the viewport is focused on our desired content focus point.  Setting this
          // to 'true' stops the automatic update of the content focus when content offset changes.
          c.disableContentFocusSync = true;

          // Whilst zooming in or out keep the content offset up-to-date so that the viewport is always
          // focused on the content focus point (and also so that the content focus is locked to the
          // viewport focus point - this is how the google maps style zooming works).
          double viewportOffsetX = c.ViewportZoomFocusX - (c.ViewportWidth / 2);
          double viewportOffsetY = c.ViewportZoomFocusY - (c.ViewportHeight / 2);
          double contentOffsetX = viewportOffsetX / c.ContentScale;
          double contentOffsetY = viewportOffsetY / c.ContentScale;
          c.ContentOffsetX = (c.ContentZoomFocusX - (c.ContentViewportWidth / 2)) - contentOffsetX;
          c.ContentOffsetY = (c.ContentZoomFocusY - (c.ContentViewportHeight / 2)) - contentOffsetY;
        }
        finally
        {
          c.disableContentFocusSync = false;
        }
      }

      if (c.ContentScaleChanged != null)
        c.ContentScaleChanged(c, EventArgs.Empty);

      if (c.scrollOwner != null)
        c.scrollOwner.InvalidateScrollInfo();
    }

    /// <summary>
    /// Method called to clamp the 'ContentScale' value to its valid range.
    /// </summary>
    public static object ContentScale_Coerce(DependencyObject d, object baseValue)
    {
      ZoomAndPanControl c = (ZoomAndPanControl)d;
      double value = (double)baseValue;
      value = Math.Min(Math.Max(value, c.MinContentScale), c.MaxContentScale);
      return value;
    }

    #endregion

    /// <summary>
    /// Event raised 'MinContentScale' or 'MaxContentScale' has changed.
    /// </summary>
    public static void MinOrMaxContentScale_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
      ZoomAndPanControl c = (ZoomAndPanControl)o;
      c.ContentScale = Math.Min(Math.Max(c.ContentScale, c.MinContentScale), c.MaxContentScale);
    }

    #region ContentOffsetX

    /// <summary>
    /// Event raised when the 'ContentOffsetX' property has changed value.
    /// </summary>
    private static void ContentOffsetX_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
      ZoomAndPanControl c = (ZoomAndPanControl)o;

      c.UpdateTranslationX();

      // Normally want to automatically update content focus when content offset changes.
      // Although this is disabled using 'disableContentFocusSync' when content offset changes due to in-progress zooming.
      if (!c.disableContentFocusSync)
        c.UpdateContentZoomFocusX();

      // Raise an event to let users of the control know that the content offset has changed.
      if (c.ContentOffsetXChanged != null)
        c.ContentOffsetXChanged(c, EventArgs.Empty);

      // Notify the owning ScrollViewer that the scrollbar offsets should be updated.
      c.UpdateScrollOffsets();
    }

    /// <summary>
    /// Method called to clamp the 'ContentOffsetX' value to its valid range.
    /// </summary>
    private static object ContentOffsetX_Coerce(DependencyObject d, object baseValue)
    {
      ZoomAndPanControl c = (ZoomAndPanControl)d;
      double value = (double)baseValue;
      double minOffsetX = 0.0;
      double maxOffsetX = Math.Max(0.0, c.unScaledExtent.Width - c.constrainedContentViewportWidth);
      value = Math.Min(Math.Max(value, minOffsetX), maxOffsetX);
      return value;
    }

#endregion

    #region ContentOffsetY

    /// <summary>
    /// Event raised when the 'ContentOffsetY' property has changed value.
    /// </summary>
    private static void ContentOffsetY_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
      ZoomAndPanControl c = (ZoomAndPanControl)o;

      c.UpdateTranslationY();

      // Normally want to automatically update content focus when content offset changes.
      // Although this is disabled using 'disableContentFocusSync' when content offset changes due to in-progress zooming.
      if (!c.disableContentFocusSync)
        c.UpdateContentZoomFocusY();

      // Raise an event to let users of the control know that the content offset has changed.
      if (c.ContentOffsetYChanged != null)
        c.ContentOffsetYChanged(c, EventArgs.Empty);

      // Notify the owning ScrollViewer that the scrollbar offsets should be updated.
      c.UpdateScrollOffsets();
    }

    /// <summary>
    /// Method called to clamp the 'ContentOffsetY' value to its valid range.
    /// </summary>
    private static object ContentOffsetY_Coerce(DependencyObject d, object baseValue)
    {
      ZoomAndPanControl c = (ZoomAndPanControl)d;
      double value = (double)baseValue;
      double minOffsetY = 0.0;
      double maxOffsetY = Math.Max(0.0, c.unScaledExtent.Height - c.constrainedContentViewportHeight);
      value = Math.Min(Math.Max(value, minOffsetY), maxOffsetY);
      return value;
    }

    #endregion

    /// <summary>
    /// Reset the viewport zoom focus to the center of the viewport.
    /// </summary>
    private void ResetViewportZoomFocus()
    {
      ViewportZoomFocusX = ViewportWidth / 2;
      ViewportZoomFocusY = ViewportHeight / 2;
    }

    /// <summary>
    /// Update the viewport size from the specified size.
    /// </summary>
    private void UpdateViewportSize(Size newSize)
    {
      // Check if the viewport is already the specified size.
      if (viewport == newSize)
        return;

      viewport = newSize;

      // Update the viewport size in content coordiates.
      UpdateContentViewportSize();

      // Initialise the content zoom focus point.
      UpdateContentZoomFocusX();
      UpdateContentZoomFocusY();

      // Reset the viewport zoom focus to the center of the viewport.
      ResetViewportZoomFocus();

      // Update content offset from itself when the size of the viewport changes.
      // This ensures that the content offset remains properly clamped to its valid range.
      this.ContentOffsetX = this.ContentOffsetX;
      this.ContentOffsetY = this.ContentOffsetY;

      // Tell owning ScrollViewer that scrollbar data has changed.
      if (scrollOwner != null)
        scrollOwner.InvalidateScrollInfo();
    }

    /// <summary>
    /// Update the size of the viewport in content coordinates after the viewport size or 'ContentScale' has changed.
    /// </summary>
    private void UpdateContentViewportSize()
    {
      ContentViewportWidth = ViewportWidth / ContentScale;
      ContentViewportHeight = ViewportHeight / ContentScale;

      constrainedContentViewportWidth = Math.Min(ContentViewportWidth, unScaledExtent.Width);
      constrainedContentViewportHeight = Math.Min(ContentViewportHeight, unScaledExtent.Height);

      UpdateTranslationX();
      UpdateTranslationY();
    }

    /// <summary>
    /// Update the X coordinate of the translation transformation.
    /// </summary>
    private void UpdateTranslationX()
    {
      if (contentOffsetTransform != null)
      {
        double scaledContentWidth = unScaledExtent.Width * ContentScale;
        if (scaledContentWidth < ViewportWidth && !double.IsPositiveInfinity(ContentViewportWidth)) //!!! checking for infinity
          // When the content can fit entirely within the viewport, center it.
          contentOffsetTransform.X = (ContentViewportWidth - unScaledExtent.Width) / 2;
        else
          contentOffsetTransform.X = -ContentOffsetX;
      }
    }

    /// <summary>
    /// Update the Y coordinate of the translation transformation.
    /// </summary>
    private void UpdateTranslationY()
    {
      if (contentOffsetTransform != null)
      {
        double scaledContentHeight = unScaledExtent.Height * ContentScale;
        if (scaledContentHeight < ViewportHeight && !double.IsPositiveInfinity(ContentViewportHeight)) //!!! checking for infinity
          // When the content can fit entirely within the viewport, center it.
          contentOffsetTransform.Y = (ContentViewportHeight - unScaledExtent.Height) / 2;
          else
          contentOffsetTransform.Y = -ContentOffsetY;
      }
    }

    /// <summary>
    /// Update the X coordinate of the zoom focus point in content coordinates.
    /// </summary>
    private void UpdateContentZoomFocusX()
    {
      ContentZoomFocusX = ContentOffsetX + (constrainedContentViewportWidth / 2);
    }

    /// <summary>
    /// Update the Y coordinate of the zoom focus point in content coordinates.
    /// </summary>
    private void UpdateContentZoomFocusY()
    {
      ContentZoomFocusY = ContentOffsetY + (constrainedContentViewportHeight / 2);
    }

    /// <summary>
    /// Measure the control and it's children.
    /// </summary>
    protected override Size MeasureOverride(Size constraint)
    {
      Size infiniteSize = new Size(double.PositiveInfinity, double.PositiveInfinity);
      Size childSize = base.MeasureOverride(infiniteSize);

      if (childSize != unScaledExtent)
      {
        // Use the size of the child as the un-scaled extent content.
        unScaledExtent = childSize;

        if (scrollOwner != null)
          scrollOwner.InvalidateScrollInfo();
      }

      // Update the size of the viewport onto the content based on the passed in 'constraint'.
      UpdateViewportSize(constraint);

      double width = constraint.Width;
      double height = constraint.Height;

      // Make sure we don't return infinity!
      if (double.IsInfinity(width))
        width = childSize.Width;

      // Make sure we don't return infinity!
      if (double.IsInfinity(height))
        height = childSize.Height;

      UpdateTranslationX();
      UpdateTranslationY();

      return new Size(width, height);
    }

    /// <summary>
    /// Arrange the control and it's children.
    /// </summary>
    protected override Size ArrangeOverride(Size arrangeBounds)
    {
      Size size = base.ArrangeOverride(this.DesiredSize);

      if (content.DesiredSize != unScaledExtent)
      {
        // Use the size of the child as the un-scaled extent content.
        unScaledExtent = content.DesiredSize;

        if (scrollOwner != null)
          scrollOwner.InvalidateScrollInfo();
      }

      //
      // Update the size of the viewport onto the content based on the passed in 'arrangeBounds'.
      //
      UpdateViewportSize(arrangeBounds);

      return size;
    }

    #endregion Internal Methods

  }
}
