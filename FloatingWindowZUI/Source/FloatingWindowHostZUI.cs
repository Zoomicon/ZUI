//Filename: FloatingWindowHostZUI.cs
//Version: 20141025

using SilverFlow.Controls;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using Compatibility;
using ZoomAndPan;

namespace FloatingWindowZUI
{

  /// <summary>
  /// A zoom & pan content control containing floating windows.
  /// </summary>
  [TemplatePart(Name = PART_ZoomHost, Type = typeof(ZoomAndPanControl))] //the ZoomAndPan control
  [TemplatePart(Name = PART_Root, Type = typeof(Grid))]
  [TemplatePart(Name = PART_ContentRoot, Type = typeof(FrameworkElement))]
  [TemplatePart(Name = PART_HostCanvas, Type = typeof(Canvas))]
  [TemplatePart(Name = PART_ModalCanvas, Type = typeof(Canvas))]
  [TemplatePart(Name = PART_IconbarContainer, Type = typeof(FrameworkElement))]
  [TemplatePart(Name = PART_Overlay, Type = typeof(Grid))]
  [TemplatePart(Name = PART_Iconbar, Type = typeof(Iconbar))]
  [TemplatePart(Name = PART_Toolbar, Type = typeof(FrameworkElement))]
  [TemplatePart(Name = PART_BootstrapButton, Type = typeof(BootstrapButton))]
  [TemplatePart(Name = PART_BarContent, Type = typeof(ContentControl))]
  [TemplatePart(Name = PART_ZoomToFitButton, Type=typeof(Button))]
  [TemplatePart(Name = PART_ZoomSlider, Type=typeof(Slider))]
  [TemplateVisualState(Name = VSMSTATE_VisibleOverlay, GroupName = VSMGROUP_Overlay)]
  [TemplateVisualState(Name = VSMSTATE_HiddenOverlay, GroupName = VSMGROUP_Overlay)]
  [StyleTypedProperty(Property = PROPERTY_ToolbarStyle, StyleTargetType = typeof(Border))]
  [StyleTypedProperty(Property = PROPERTY_BootstrapButtonStyle, StyleTargetType = typeof(BootstrapButton))]
  [StyleTypedProperty(Property = PROPERTY_WindowIconStyle, StyleTargetType = typeof(WindowIcon))]
  [ContentProperty("Windows")]
  public class FloatingWindowHostZUI : FloatingWindowHost
  {

    #region --- Constants ---

    protected const string PART_ZoomHost = "PART_ZoomHost";
    protected const string PART_ZoomToFitButton = "PART_ZoomToFitButton";
    protected const string PART_ZoomSlider = "PART_ZoomSlider";

    #endregion

    #region --- Properties ---

    #region ZoomHost

    protected ZoomAndPanControl _zoomHost;
    
    public ZoomAndPanControl ZoomHost {
      get
      {
        if (_zoomHost == null) ApplyTemplate();
        return _zoomHost;
      }
      private set
      {
        _zoomHost = value;
      }
    }

    #endregion

    #region ZoomToFitButton

    protected Button _zoomToFitButton;

    public Button ZoomToFitButton
    {
      get
      {
        if (_zoomToFitButton == null) ApplyTemplate();
        return _zoomToFitButton;
      }
      private set
      {
        _zoomToFitButton = value;
      }
    }

    #endregion

    #region ZoomSlider

    protected Slider _zoomSlider;

    public Slider ZoomSlider
    {
      get
      {
        if (_zoomSlider == null) ApplyTemplate();
        return _zoomSlider;
      }
      private set
      {
        _zoomSlider = value;
      }
    }

    #endregion

    #endregion

    #region --- Constructor ---

    #if !SILVERLIGHT
    static FloatingWindowHostZUI()
    {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(FloatingWindowHostZUI), new FrameworkPropertyMetadata(typeof(FloatingWindowHostZUI)));
    }
    #endif

    public FloatingWindowHostZUI()
    {
      ApplyStyle();
    }

    #endregion

    public override void ApplyStyle()
    {
      //don't call base.ApplyStyle() here
      DefaultStyleKey = typeof(FloatingWindowHostZUI);
    }

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate(); //must call this

      ZoomHost = base.GetTemplateChild(PART_ZoomHost) as ZoomAndPanControl;
      ZoomHost.ContentScale = ContentScale; //TODO: also need event handler for the property to apply content scale to ZoomHost
      ZoomHost.IsDefaultMouseHandling = true; //use default mouse handling

      ZoomToFitButton = base.GetTemplateChild(PART_ZoomToFitButton) as Button;
      ZoomToFitButton.Click += ZoomToFitButton_Click;

      ZoomSlider = base.GetTemplateChild(PART_ZoomSlider) as Slider;

      SubscribeToFloatingWindowEvents();
    }

    void ZoomToFitButton_Click(object sender, RoutedEventArgs e)
    {
      ZoomToFit();
    }

    public void ZoomToFit()
    {
      ZoomHost.ZoomTo(Windows.GetBoundingRectangle(notMinimized:true));
    }

    #region ContentScalable

    /// <summary>
    /// ContentScalable Dependency Property
    /// </summary>
    public static readonly DependencyProperty ContentScalableProperty =
        DependencyProperty.Register("ContentScalable", typeof(bool), typeof(FloatingWindowHostZUI), new FrameworkPropertyMetadata(true));

    /// <summary>
    /// Gets or sets the ContentScalable property.
    /// </summary>
    public bool ContentScalable
    {
      get { return (bool)GetValue(ContentScalableProperty); }
      set { SetValue(ContentScalableProperty, value); }
    }

    #endregion

    #region ContentScale

    /// <summary>
    /// ContentScale Dependency Property
    /// </summary>
    public static readonly DependencyProperty ContentScaleProperty =
      DependencyPropertyExt.Register("ContentScale", typeof(double), typeof(FloatingWindowHostZUI), new FrameworkPropertyMetadata(1.0)); //no need to do value Coercion, we bind to ZoomHost which will do it

    /// <summary>
    /// Gets or sets the ContentScale property.
    /// </summary>
    public double ContentScale
    {
      get { return (double)GetValue(ContentScaleProperty); }
      set { SetValue(ContentScaleProperty, value); }
    }

    #endregion

    #region MinContentScale

    /// <summary>
    /// MinContentScale Dependency Property
    /// </summary>
    public static readonly DependencyProperty MinContentScaleProperty =
      DependencyPropertyExt.Register("MinContentScale", typeof(double), typeof(FloatingWindowHostZUI), new FrameworkPropertyMetadata(0.01)); //no need to do value Coercion, we bind to ZoomHost which will do it

    /// <summary>
    /// Gets or sets the MinContentScale property.
    /// </summary>
    public double MinContentScale
    {
      get { return (double)GetValue(MinContentScaleProperty); }
      set { SetValue(MinContentScaleProperty, value); }
    }

    #endregion

    #region MaxContentScale

    /// <summary>
    /// MaxContentScale Dependency Property
    /// </summary>
    public static readonly DependencyProperty MaxContentScaleProperty =
      DependencyPropertyExt.Register("MaxContentScale", typeof(double), typeof(FloatingWindowHostZUI), new FrameworkPropertyMetadata(10.0)); //no need to do value Coercion, we bind to ZoomHost which will do it

    /// <summary>
    /// Gets or sets the MaxContentScale property.
    /// </summary>
    public double MaxContentScale
    {
      get { return (double)GetValue(MaxContentScaleProperty); }
      set { SetValue(MaxContentScaleProperty, value); }
    }

    #endregion
 
    #region FloatingWindowEvents

    private void SubscribeToFloatingWindowEvents()
    {
      //subscribing to current windows
      foreach (FloatingWindow w in Windows)
        SubscribeToFloatingWindowEvents(w);

      //subscribing to added windows and unsubscribing from removed windows
      Windows.CollectionChanged += (s, e) =>
      {
        switch (e.Action)
        {
          case NotifyCollectionChangedAction.Add:
            foreach (FloatingWindow w in e.NewItems)
              SubscribeToFloatingWindowEvents(w);
            break;
          case NotifyCollectionChangedAction.Remove:
            foreach (FloatingWindow w in e.OldItems)
              UnsubscribeFromFloatingWindowEvents(w);
            break;
        }
      };
    }

    private void SubscribeToFloatingWindowEvents(FloatingWindow w)
    {
      //w.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(FloatingWindow_MouseLeftButtonDown), true); //passing true to get handled events too
      //w.AddHandler(MouseRightButtonDownEvent, new MouseButtonEventHandler(FloatingWindow_MouseRightButtonDown), true);
      w.AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(FloatingWindow_MouseLeftButtonUp), true);
      w.AddHandler(MouseRightButtonUpEvent, new MouseButtonEventHandler(FloatingWindow_MouseRightButtonUp), true);
      //w.MouseMove += FloatingWindow_MouseMove;
      w.MouseWheel += FloatingWindow_MouseWheel;
    }

    private void UnsubscribeFromFloatingWindowEvents(FloatingWindow w)
    {
      //w.RemoveHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(FloatingWindow_MouseLeftButtonDown));
      //w.RemoveHandler(MouseRightButtonDownEvent, new MouseButtonEventHandler(FloatingWindow_MouseRightButtonDown));
      w.RemoveHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(FloatingWindow_MouseLeftButtonUp));
      w.RemoveHandler(MouseRightButtonUpEvent, new MouseButtonEventHandler(FloatingWindow_MouseRightButtonUp));
      //w.MouseMove -= FloatingWindow_MouseMove;
      w.MouseWheel -= FloatingWindow_MouseWheel;
    }

    private void FloatingWindow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      FloatingWindow_MouseUp(sender, e, MouseButton.Left);
    }

    private void FloatingWindow_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
      FloatingWindow_MouseUp(sender, e, MouseButton.Right);
    }

    private void FloatingWindow_MouseUp(object sender, MouseButtonEventArgs e, MouseButton changedButton)
    {
      FloatingWindow window = (FloatingWindow)sender;
      if (window.ScaleEnabled && (Keyboard.Modifiers & ModifierKeys.Control) != 0)
      {
        if (changedButton == MouseButton.Left)
          window.Scale += 0.05; //zoom in
        else if (changedButton == MouseButton.Right)
          window.Scale -= 0.05; //zoom out

        e.Handled = true;
      }
      else
        e.Handled = false;
    }

    private void FloatingWindow_MouseWheel(object sender, MouseWheelEventArgs e)
    {
      FloatingWindow window = (FloatingWindow)sender;
      if (window.ScaleEnabled && (Keyboard.Modifiers & ModifierKeys.Control) != 0)
      {
        //delta should be either >0 or <0
        //Point mousePosition = args.GetPosition(HostPanel); //could use mousePosition here to center the window to the mouse point or something, but preferred to have the logic at FloatingWindow.Scale itself, to recenter arround its previous center point after scaling
        if (e.Delta > 0)
          window.Scale += 0.05; //zoom in
        else if (e.Delta < 0)
          window.Scale -= 0.05; //zoom out

        e.Handled = true;
      }
      else
        e.Handled = false;
    }

    #endregion

    //---------------------------------------------------------------------//

    /// <summary>
    /// Gets the current view center
    /// </summary>
    /// <returns>The view center</returns>
    public override Point ViewCenter
    {
      get { return new Point(ZoomHost.ContentOffsetX + ZoomHost.ContentViewportWidth / 2, ZoomHost.ContentOffsetY + ZoomHost.ContentViewportHeight / 2); }
    }

    public override Rect MaximizedWindowBounds
    {
      get
      {
        return ZoomHost.ContentViewportBounds;
      }
    }

    /// <summary>
    /// The 'ZoomOut' command (bound to the minus key) was executed.
    /// </summary>
    private void ZoomOut(Point contentZoomCenter)
    {
      ZoomHost.ZoomAboutPoint(ZoomHost.ContentScale - 0.2, contentZoomCenter);
    }

    /// <summary>
    /// The 'ZoomIn' command (bound to the plus key) was executed.
    /// </summary>
    private void ZoomIn(Point contentZoomCenter)
    {
      ZoomHost.ZoomAboutPoint(ZoomHost.ContentScale + 0.2, contentZoomCenter);
    }

  }

}
