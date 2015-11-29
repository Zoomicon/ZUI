//Filename: FloatingWindowHost.cs
//Version: 20140904

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using System.Collections.Specialized;
using System.Windows.Markup;

using SilverFlow.Controls.Extensions;

#if SILVERLIGHT
using ExitEventArgs = System.EventArgs;
#endif

namespace SilverFlow.Controls
{
  /// <summary>
  /// A Content Control containing floating windows.
  /// </summary>
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
  [TemplateVisualState(Name = VSMSTATE_VisibleOverlay, GroupName = VSMGROUP_Overlay)]
  [TemplateVisualState(Name = VSMSTATE_HiddenOverlay, GroupName = VSMGROUP_Overlay)]
  [StyleTypedProperty(Property = PROPERTY_ToolbarStyle, StyleTargetType = typeof(Border))]
  [StyleTypedProperty(Property = PROPERTY_BootstrapButtonStyle, StyleTargetType = typeof(BootstrapButton))]
  [StyleTypedProperty(Property = PROPERTY_WindowIconStyle, StyleTargetType = typeof(WindowIcon))]
  [ContentProperty("Windows")]
  public class FloatingWindowHost : ContentControl
  {
    #region --- Constants ---

    // Template parts
    protected const string PART_Root = "PART_Root";
    protected const string PART_ContentRoot = "PART_ContentRoot";
    protected const string PART_HostCanvas = "PART_HostCanvas";
    protected const string PART_ModalCanvas = "PART_ModalCanvas";
    protected const string PART_IconbarContainer = "PART_IconbarContainer";
    protected const string PART_Overlay = "PART_Overlay";
    protected const string PART_Iconbar = "PART_Iconbar";
    protected const string PART_Toolbar = "PART_Toolbar";
    protected const string PART_BootstrapButton = "PART_BootstrapButton";
    protected const string PART_BarContent = "PART_BarContent";

    // VSM groups
    protected const string VSMGROUP_Overlay = "OverlayStates";

    // VSM states
    protected const string VSMSTATE_VisibleOverlay = "VisibleOverlay";
    protected const string VSMSTATE_HiddenOverlay = "HiddenOverlay";

    // Style typed properties
    protected const string PROPERTY_ToolbarStyle = "ToolbarStyle";
    protected const string PROPERTY_BootstrapButtonStyle = "BootstrapButtonStyle";
    protected const string PROPERTY_WindowIconStyle = "WindowIconStyle";

    // Thickness of resizing area.
    private const double SnapinDistanceDefaultValue = 5.0;

    // Default icon size
    private const double DefaultIconWidth = 120;
    private const double DefaultIconHeight = 70;

    private const bool DefaultCloseWindowsOnApplicationExit = true;

    #endregion Constants

    #region --- Fields ---

    private Grid root;
    private FrameworkElement contentRoot;
    private Canvas hostCanvas;
    private Canvas modalCanvas;
    private FrameworkElement iconbarContainer;
    private Grid overlay;
    internal Iconbar iconbar;
    internal FrameworkElement toolbar;
    private BootstrapButton bootstrapButton;
    private ContentControl barContent;

    private bool templateIsApplied;
    private bool closeWindowsOnApplicationExit = DefaultCloseWindowsOnApplicationExit;

    #endregion Member Fields

    #region --- Properties ---

    public virtual Rect MaximizedWindowBounds
    {
      get
      {
        return new Rect(0, 0, HostPanel.ActualWidth, HostPanel.ActualHeight);
      }
    }

    public int MaxZIndex
    {
      get
      {
        return FloatingWindows.Aggregate(-1, (maxZIndex, window) =>
        {
          int w = Canvas.GetZIndex(window);
          return (w > maxZIndex) ? w : maxZIndex;
        });
      }
    }

    public bool CloseWindowsOnApplicationExit
    {
      get { return closeWindowsOnApplicationExit; }
      set { closeWindowsOnApplicationExit = value; }
    }

    #region ToolbarStyle

    /// <summary>
    /// Gets or sets the style of the Toolbar.
    /// </summary>
    public Style ToolbarStyle
    {
      get { return GetValue(ToolbarStyleProperty) as Style; }
      set { SetValue(ToolbarStyleProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="FloatingWindowHost.ToolbarStyleProperty" /> dependency property.
    /// </summary>
    public static readonly DependencyProperty ToolbarStyleProperty =
        DependencyProperty.Register(
            "ToolbarStyle",
            typeof(Style),
            typeof(FloatingWindowHost),
            new PropertyMetadata(ToolbarStylePropertyChanged));

    /// <summary>
    /// ToolbarStyle PropertyChangedCallback call back static function.
    /// </summary>
    /// <param name="d">FloatingWindowHost object whose ToolbarStyle property is changed.</param>
    /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
    private static void ToolbarStylePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      FloatingWindowHost host = (FloatingWindowHost)d;
      if (host != null && host.toolbar != null)
      {
        host.toolbar.Style = e.NewValue as Style;
      }
    }

    #endregion

    #region BootstrapButtonStyle

    /// <summary>
    /// Gets or sets the style of the BootstrapButton.
    /// </summary>
    public Style BootstrapButtonStyle
    {
      get { return GetValue(BootstrapButtonStyleProperty) as Style; }
      set { SetValue(BootstrapButtonStyleProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="FloatingWindowHost.BootstrapButtonStyleProperty" /> dependency property.
    /// </summary>
    public static readonly DependencyProperty BootstrapButtonStyleProperty =
        DependencyProperty.Register(
            "BootstrapButtonStyle",
            typeof(Style),
            typeof(FloatingWindowHost),
            new PropertyMetadata(BootstrapButtonStylePropertyChanged));

    /// <summary>
    /// BootstrapButtonStyle PropertyChangedCallback call back static function.
    /// </summary>
    /// <param name="d">FloatingWindowHost object whose BootstrapButtonStyle property is changed.</param>
    /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
    private static void BootstrapButtonStylePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      FloatingWindowHost host = (FloatingWindowHost)d;
      if (host != null && host.toolbar != null)
      {
        host.bootstrapButton.Style = e.NewValue as Style;
      }
    }

    #endregion

    #region IsIconbarVisible

    /// <summary>
    /// Gets or sets a value indicating whether icon bar UI is available.
    /// </summary>
    /// <value><c>true</c> if snap in is enabled; otherwise, <c>false</c>.</value>
    public bool IsIconbarVisible
    {
      get { return (bool)GetValue(IsIconbarVisibleProperty); }
      set { SetValue(IsIconbarVisibleProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="FloatingWindowHost.IsIconbarVisible" /> dependency property.
    /// </summary>
    /// <value>
    /// The identifier for the <see cref="FloatingWindowHost.IsIconbarVisible" /> dependency property.
    /// </value>
    public static readonly DependencyProperty IsIconbarVisibleProperty =
        DependencyProperty.Register(
        "IsIconbarVisible",
        typeof(bool),
        typeof(FloatingWindowHost),
        new PropertyMetadata(false, IsIconbarVisiblePropertyChanged));

    /// <summary>
    /// IsIconbarVisible PropertyChangedCallback call back static function.
    /// </summary>
    /// <param name="d">FloatingWindowHost object whose IsIconbarVisible property is changed.</param>
    /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
    private static void IsIconbarVisiblePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      FloatingWindowHost host = (FloatingWindowHost)d;
      if (host != null && e.NewValue != null)
      {
        if (!host.templateIsApplied)
          host.ApplyTemplate();
        host.iconbar.IsOpen = (bool)e.NewValue;
      }
    }

    #endregion

    #region IsToolbarVisible

    /// <summary>
    /// Gets or sets a value indicating whether icon bar UI is available.
    /// </summary>
    /// <value><c>true</c> if snap in is enabled; otherwise, <c>false</c>.</value>
    public bool IsToolbarVisible
    {
      get { return (bool)GetValue(IsToolbarVisibleProperty); }
      set { SetValue(IsToolbarVisibleProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="FloatingWindowHost.IsToolbarVisible" /> dependency property.
    /// </summary>
    /// <value>
    /// The identifier for the <see cref="FloatingWindowHost.IsToolbarVisible" /> dependency property.
    /// </value>
    public static readonly DependencyProperty IsToolbarVisibleProperty =
        DependencyProperty.Register(
        "IsToolbarVisible",
        typeof(bool),
        typeof(FloatingWindowHost),
        new PropertyMetadata(true, IsToolbarVisiblePropertyChanged));

    /// <summary>
    /// IsToolbarVisible PropertyChangedCallback call back static function.
    /// </summary>
    /// <param name="d">FloatingWindowHost object whose IsToolbarVisible property is changed.</param>
    /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
    private static void IsToolbarVisiblePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      FloatingWindowHost host = (FloatingWindowHost)d;
      if (host != null && e.NewValue != null)
      {
        if (!host.templateIsApplied)
          host.ApplyTemplate();
        host.toolbar.Visibility = ((bool)e.NewValue) ? Visibility.Visible : Visibility.Collapsed;
      }
    }

    #endregion

    #region ShowScreenshotButton

    /// <summary>
    /// Gets or sets a value indicating whether window bars show configuration button.
    /// </summary>
    /// <value><c>true</c> if snap in is enabled; otherwise, <c>false</c>.</value>
    public bool ShowScreenshotButton
    {
      get { return (bool)GetValue(ShowScreenshotButtonProperty); }
      set { SetValue(ShowScreenshotButtonProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="FloatingWindowHost.ShowScreenshotButton" /> dependency property.
    /// </summary>
    /// <value>
    /// The identifier for the <see cref="FloatingWindowHost.ShowScreenshot" /> dependency property.
    /// </value>
    public static readonly DependencyProperty ShowScreenshotButtonProperty =
        DependencyProperty.Register(
        "ShowScreenshotButton",
        typeof(bool),
        typeof(FloatingWindowHost),
        new PropertyMetadata(true, ShowScreenshotButtonPropertyChanged));

    /// <summary>
    /// ShowScreenshotButton PropertyChangedCallback call back static function.
    /// </summary>
    /// <param name="d">FloatingWindowHost object whose ShowScreenshotButton property is changed.</param>
    /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
    private static void ShowScreenshotButtonPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      FloatingWindowHost host = (FloatingWindowHost)d;
      if (host != null && host.Windows != null && e.NewValue != null)
        host.Windows.ShowScreenshotButton = (bool)e.NewValue;
    }

    #endregion

    #region ShowHelpButton

    /// <summary>
    /// Gets or sets a value indicating whether window bars show configuration button.
    /// </summary>
    /// <value><c>true</c> if snap in is enabled; otherwise, <c>false</c>.</value>
    public bool ShowHelpButton
    {
      get { return (bool)GetValue(ShowHelpButtonProperty); }
      set { SetValue(ShowHelpButtonProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="FloatingWindowHost.ShowHelpButton" /> dependency property.
    /// </summary>
    /// <value>
    /// The identifier for the <see cref="FloatingWindowHost.ShowHelp" /> dependency property.
    /// </value>
    public static readonly DependencyProperty ShowHelpButtonProperty =
        DependencyProperty.Register(
        "ShowHelpButton",
        typeof(bool),
        typeof(FloatingWindowHost),
        new PropertyMetadata(true, ShowHelpButtonPropertyChanged));

    /// <summary>
    /// ShowHelpButton PropertyChangedCallback call back static function.
    /// </summary>
    /// <param name="d">FloatingWindowHost object whose ShowHelpButton property is changed.</param>
    /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
    private static void ShowHelpButtonPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      FloatingWindowHost host = (FloatingWindowHost)d;
      if (host != null && host.Windows != null && e.NewValue != null)
        host.Windows.ShowHelpButton = (bool)e.NewValue;
    }

    #endregion

    #region ShowOptionsButton

    /// <summary>
    /// Gets or sets a value indicating whether window bars show configuration button.
    /// </summary>
    /// <value><c>true</c> if snap in is enabled; otherwise, <c>false</c>.</value>
    public bool ShowOptionsButton
    {
      get { return (bool)GetValue(ShowOptionsButtonProperty); }
      set { SetValue(ShowOptionsButtonProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="FloatingWindowHost.ShowOptionsButton" /> dependency property.
    /// </summary>
    /// <value>
    /// The identifier for the <see cref="FloatingWindowHost.ShowOptions" /> dependency property.
    /// </value>
    public static readonly DependencyProperty ShowOptionsButtonProperty =
        DependencyProperty.Register(
        "ShowOptionsButton",
        typeof(bool),
        typeof(FloatingWindowHost),
        new PropertyMetadata(true, ShowOptionsButtonPropertyChanged));

    /// <summary>
    /// ShowOptionsButton PropertyChangedCallback call back static function.
    /// </summary>
    /// <param name="d">FloatingWindowHost object whose ShowOptionsButton property is changed.</param>
    /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
    private static void ShowOptionsButtonPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      FloatingWindowHost host = (FloatingWindowHost)d;
      if (host != null && host.Windows != null && e.NewValue != null)
        host.Windows.ShowOptionsButton = (bool)e.NewValue;
    }

    #endregion

    #region WindowIconStyle

    /// <summary>
    /// Gets or sets the style of the WindowIcon.
    /// </summary>
    public Style WindowIconStyle
    {
      get { return GetValue(WindowIconStyleProperty) as Style; }
      set { SetValue(WindowIconStyleProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="FloatingWindowHost.WindowIconStyleProperty" /> dependency property.
    /// </summary>
    public static readonly DependencyProperty WindowIconStyleProperty =
        DependencyProperty.Register(
            "WindowIconStyle",
            typeof(Style),
            typeof(FloatingWindowHost),
            new PropertyMetadata(WindowIconStylePropertyChanged));

    /// <summary>
    /// WindowIconStyle PropertyChangedCallback call back static function.
    /// </summary>
    /// <param name="d">FloatingWindowHost object whose WindowIconStyle property is changed.</param>
    /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
    private static void WindowIconStylePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      FloatingWindowHost host = (FloatingWindowHost)d;
      if (host != null && host.iconbar != null)
      {
        host.iconbar.WindowIconStyle = e.NewValue as Style;
      }
    }

    #endregion

    #region SnapinEnabled

    /// <summary>
    /// Gets or sets a value indicating whether snap in is enabled.
    /// </summary>
    /// <value><c>true</c> if snap in is enabled; otherwise, <c>false</c>.</value>
    public bool SnapinEnabled
    {
      get { return (bool)GetValue(SnapinEnabledProperty); }
      set { SetValue(SnapinEnabledProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="FloatingWindowHost.SnapinEnabled" /> dependency property.
    /// </summary>
    /// <value>
    /// The identifier for the <see cref="FloatingWindowHost.SnapinEnabled" /> dependency property.
    /// </value>
    public static readonly DependencyProperty SnapinEnabledProperty =
        DependencyProperty.Register(
        "SnapinEnabled",
        typeof(bool),
        typeof(FloatingWindowHost),
        new PropertyMetadata(true, null));

    #endregion

    #region SnapinDistance

    /// <summary>
    /// Gets or sets a value of the snap in distance.
    /// </summary>
    /// <value>Snap in distance.</value>
    public double SnapinDistance
    {
      get { return (double)GetValue(SnapinDistanceProperty); }
      set { SetValue(SnapinDistanceProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="FloatingWindowHost.SnapinDistance" /> dependency property.
    /// </summary>
    /// <value>
    /// The identifier for the <see cref="FloatingWindowHost.SnapinDistance" /> dependency property.
    /// </value>
    public static readonly DependencyProperty SnapinDistanceProperty =
        DependencyProperty.Register(
        "SnapinDistance",
        typeof(double),
        typeof(FloatingWindowHost),
        new PropertyMetadata(SnapinDistanceDefaultValue, null));

    #endregion

    #region SnapinMargin

    /// <summary>
    /// Gets or sets a value of the snap in margin - distance between adjacent edges.
    /// </summary>
    /// <value>Snap in margin.</value>
    public double SnapinMargin
    {
      get { return (double)GetValue(SnapinMarginProperty); }
      set { SetValue(SnapinMarginProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="FloatingWindowHost.SnapinMargin" /> dependency property.
    /// </summary>
    /// <value>
    /// The identifier for the <see cref="FloatingWindowHost.SnapinMargin" /> dependency property.
    /// </value>
    public static readonly DependencyProperty SnapinMarginProperty =
        DependencyProperty.Register(
        "SnapinMargin",
        typeof(double),
        typeof(FloatingWindowHost),
        new PropertyMetadata(0.0, null));

    #endregion

    #region ShowMinimizedOnlyInIconbar

    /// <summary>
    /// Gets or sets a value indicating whether to show only minimized windows in the iconbar.
    /// </summary>
    /// <value><c>true</c> if to show only minimized windows in the iconbar; otherwise, <c>false</c>.</value>
    public bool ShowMinimizedOnlyInIconbar
    {
      get { return (bool)GetValue(ShowMinimizedOnlyInIconbarProperty); }
      set { SetValue(ShowMinimizedOnlyInIconbarProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="FloatingWindowHost.ShowMinimizedOnlyInIconbarProperty" /> dependency property.
    /// </summary>
    /// <value>
    /// The identifier for the <see cref="FloatingWindowHost.ShowMinimizedOnlyInIconbarProperty" /> dependency property.
    /// </value>
    public static readonly DependencyProperty ShowMinimizedOnlyInIconbarProperty =
        DependencyProperty.Register(
        "ShowMinimizedOnlyInIconbar",
        typeof(bool),
        typeof(FloatingWindowHost),
        new PropertyMetadata(false, null));

    #endregion

    #region OverlayBrush

    /// <summary>
    /// Gets or sets the overlay color.
    /// </summary>
    /// <value>The overlay color.</value>
    public Brush OverlayBrush
    {
      get { return (Brush)GetValue(OverlayBrushProperty); }
      set { SetValue(OverlayBrushProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="FloatingWindowHost.OverlayBrush" /> dependency property.
    /// </summary>
    /// <value>
    /// The identifier for the <see cref="FloatingWindowHost.OverlayBrush" /> dependency property.
    /// </value>
    public static readonly DependencyProperty OverlayBrushProperty =
        DependencyProperty.Register(
        "OverlayBrush",
        typeof(Brush),
        typeof(FloatingWindowHost),
        new PropertyMetadata(new SolidColorBrush(Color.FromArgb(0x90, 0x20, 0x20, 0x30)), OnOverlayBrushPropertyChanged));

    /// <summary>
    /// OverlayBrushProperty PropertyChangedCallback call back static function.
    /// </summary>
    /// <param name="d">FloatingWindowHost object whose OverlayBrush property is changed.</param>
    /// <param name="e">DependencyPropertyChangedEventArgs which contains the old and new values.</param>
    private static void OnOverlayBrushPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      FloatingWindowHost host = (FloatingWindowHost)d;

      if (host != null && host.overlay != null)
        host.overlay.Background = (Brush)e.NewValue;
    }

    #endregion

    #region IconWidth

    /// <summary>
    /// Gets or sets the width of the window's icon.
    /// </summary>
    /// <value>The width of the window's icon.</value>
    public double IconWidth
    {
      get { return (double)GetValue(IconWidthProperty); }
      set { SetValue(IconWidthProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="FloatingWindowHost.IconWidth" /> dependency property.
    /// </summary>
    /// <value>
    /// The identifier for the <see cref="FloatingWindowHost.IconWidth" /> dependency property.
    /// </value>
    public static readonly DependencyProperty IconWidthProperty =
        DependencyProperty.Register(
        "IconWidth",
        typeof(double),
        typeof(FloatingWindowHost),
        new PropertyMetadata(DefaultIconWidth, null));

    #endregion

    #region IconHeight

    /// <summary>
    /// Gets or sets the height of the window's icon.
    /// </summary>
    /// <value>The height of the window's icon.</value>
    public double IconHeight
    {
      get { return (double)GetValue(IconHeightProperty); }
      set { SetValue(IconHeightProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="FloatingWindowHost.IconHeight" /> dependency property.
    /// </summary>
    /// <value>
    /// The identifier for the <see cref="FloatingWindowHost.IconHeight" /> dependency property.
    /// </value>
    public static readonly DependencyProperty IconHeightProperty =
        DependencyProperty.Register(
        "IconHeight",
        typeof(double),
        typeof(FloatingWindowHost),
        new PropertyMetadata(DefaultIconHeight, null));

    #endregion

    #region Bar

    /// <summary>
    /// Gets or sets a control displayed in the Toolbar.
    /// </summary>
    /// <value>The control displayed in the Toolbar. The default is null.</value>
    public object Bar
    {
      get { return GetValue(BarProperty); }
      set { SetValue(BarProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="FloatingWindowHost.Bar" /> dependency property.
    /// </summary>
    /// <value>
    /// The identifier for the <see cref="FloatingWindowHost.Bar" /> dependency property.
    /// </value>
    public static readonly DependencyProperty BarProperty =
        DependencyProperty.Register(
        "Bar",
        typeof(object),
        typeof(FloatingWindowHost),
        null);

    #endregion

    /// <summary>
    /// Gets the host panel, containing the floating windows.
    /// </summary>
    /// <value>The host panel.</value>
    public Canvas HostPanel
    {
      get { return hostCanvas; }
    }

    /// <summary>
    /// Gets the floating windows collection.
    /// </summary>
    /// <value>The floating windows collection.</value>
    public IEnumerable<FloatingWindow> FloatingWindows
    {
      get { return /* Windows.AsEnumerable(); */ hostCanvas.Children.OfType<FloatingWindow>(); }
    }

    #region Windows

    /// <summary>
    /// Gets or Sets the floating windows collection.
    /// </summary>
    /// <value>The floating windows collection.</value>
    public FloatingWindowCollection Windows
    {
      get { return (FloatingWindowCollection)GetValue(WindowsProperty); }
      set { SetValue(WindowsProperty, value); }
    }

    public static readonly DependencyProperty WindowsProperty = DependencyProperty.Register("Windows", typeof(FloatingWindowCollection), typeof(FloatingWindowHost), new PropertyMetadata(WindowsChanged)); //must not pass a default value to PropertyMetadata here, it would be a singleton collection (setting the property in the constructor instead)

    private static void WindowsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      FloatingWindowHost host = d as FloatingWindowHost;
      if (host != null)
      {
        FloatingWindowCollection oldCollection = (FloatingWindowCollection)e.OldValue;
        FloatingWindowCollection newCollection = (FloatingWindowCollection)e.NewValue;

        if (oldCollection != null)
        {
          //stop listening for changes to old collection before removing windows (although _Remove shouldn't trigger any event)
          oldCollection.CollectionChanged -= new NotifyCollectionChangedEventHandler(host.Windows_CollectionChanged); // collection changed event handler first, since we remove old windows using _Remove

          //remove all items existing in oldItems but not in newItems
          foreach (FloatingWindow v in oldCollection)
            if (newCollection == null || !newCollection.Contains(v)) { host._Remove(v); }   //must call _Remove, not Remove
        }

        if (newCollection != null)
        {
          //add all items existing in newItems but not in oldItems
          foreach (FloatingWindow v in newCollection)
            if (oldCollection == null || !oldCollection.Contains(v)) { host._Add(v); } //must call _Add, not Add

          //listen for changes to new collection after adding windows (although _Add shouldn't trigger any event)
          newCollection.CollectionChanged += new NotifyCollectionChangedEventHandler(host.Windows_CollectionChanged);
        }
      } //TODO: blog about "host=d; ...(host.Window_CollectionChanged)" trick (used from static handler for dependency property change)
    } //TODO: THE IMPLEMENTATION ABOVE WILL CHANGE ORDER OF CONTROLS, SEE IF THIS PLAYS ROLE IN Z-ORDERING, IF NOT KEEP AS IS, ELSE DO REMOVE/ADD ALL WITHOUT THE EXTRA OPTIMIZATION CHECKS

    private void Windows_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
    {
      switch (args.Action)
      {
        case NotifyCollectionChangedAction.Add:
          foreach (object item in args.NewItems)
            _Add((FloatingWindow)item);
          break;

        case NotifyCollectionChangedAction.Remove:
          foreach (object item in args.OldItems)
            _Remove((FloatingWindow)item);
          break;
      }
    }

    #endregion

    /// <summary>
    /// Gets or sets a value indicating whether the layout of the FloatingWindowHost is updated.
    /// </summary>
    /// <value>
    /// <c>true</c> if the layout of the FloatingWindowHost is updated; otherwise, <c>false</c>.
    /// </value>
    internal bool IsLayoutUpdated { get; private set; }

    /// <summary>
    /// Gets current modal window.
    /// </summary>
    /// <value>The modal window.</value>
    private FloatingWindow ModalWindow
    {
      get { return modalCanvas.Children.OfType<FloatingWindow>().FirstOrDefault(); }
    }

    #endregion

    #region --- Constructor ---

    #if !SILVERLIGHT
    static FloatingWindowHost()
    {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(FloatingWindowHost), new FrameworkPropertyMetadata(typeof(FloatingWindowHost)));
    }
    #endif

    /// <summary>
    /// Initializes a new instance of the <see cref="FloatingWindowHost"/> class.
    /// </summary>
    public FloatingWindowHost()
    {
      Windows = new FloatingWindowCollection(); //must set this here and not in the DependencyProperty definition's default value (that would be a singleton collection!)
      ApplyStyle();
    }

    #endregion

    public virtual void ApplyStyle()
    {
      DefaultStyleKey = typeof(FloatingWindowHost);
    }

    /// <summary>
    /// Builds the visual tree for the <see cref="FloatingWindowHost" /> control 
    /// when a new template is applied.
    /// </summary>
    public override void OnApplyTemplate()
    {
      UnsubscribeFromEvents();
      UnsubscribeFromTemplatePartEvents();

      base.OnApplyTemplate();

      root = GetTemplatePart<Grid>(PART_Root);
      contentRoot = GetTemplatePart<FrameworkElement>(PART_ContentRoot);
      hostCanvas = GetTemplatePart<Canvas>(PART_HostCanvas);
      modalCanvas = GetTemplatePart<Canvas>(PART_ModalCanvas);
      iconbarContainer = GetTemplatePart<FrameworkElement>(PART_IconbarContainer);
      overlay = GetTemplatePart<Grid>(PART_Overlay);
      iconbar = GetTemplatePart<Iconbar>(PART_Iconbar);
      toolbar = GetTemplatePart<FrameworkElement>(PART_Toolbar);
      bootstrapButton = GetTemplatePart<BootstrapButton>(PART_BootstrapButton);
      barContent = GetTemplatePart<ContentControl>(PART_BarContent);

      iconbar.FloatingWindowHost = this;

      SetStyles();
      SubscribeToTemplatePartEvents();
      SubscribeToEvents();

      templateIsApplied = true;

      if (Windows != null)
        foreach (FloatingWindow w in Windows)
        {
          _Add(w);
          //w.ApplyTemplate();
          if (w.WindowState != WindowState.Minimized) { w.Show(); }
        }
    }

    #region Events

    /// <summary>
    /// Occurs when the <see cref="FloatingWindowHost" /> is loaded and its template is applied.
    /// </summary>
    public event EventHandler Rendered;

    /// <summary>
    /// Occurs when the active <see cref="FloatingWindow" /> is changed.
    /// </summary>
    public event EventHandler<ActiveWindowChangedEventArgs> ActiveWindowChanged;

    #endregion Events

    /// <summary>
    /// Gets the current view center
    /// </summary>
    /// <returns>The view center</returns>
    public virtual Point ViewCenter
    {
      get { return new Point(HostPanel.ActualWidth / 2, HostPanel.ActualHeight / 2); }
    }

    /// <summary>
    /// Adds the specified floating window to the collection of child elements of the host.
    /// </summary>
    /// <param name="window">The floating window.</param>
    /// <exception cref="ArgumentNullException">Floating window is null.</exception>
    public FloatingWindow Add(FloatingWindow window)
    {
      Windows.Add(window);
      return window;
    }

    private void _Add(FloatingWindow window)
    {
      if (window == null)
        throw new ArgumentNullException("window");

      // Guarantee that the visual tree of the control is complete (NOT GUARANTEED IF ADD IS CALLED FROM XAML LOADING, ADDING ITEMS TO CANVAS LATER AT APPLYTEMPLATE FOR THAT CASE)
      if (!templateIsApplied)
        ApplyTemplate(); //don't use "templateIsApplied =" here, since ApplyTemplate returns false if the visual tree didn't change. Our OnApplyTemplate will set the templateIsApplied to true if called

      if (hostCanvas != null && !hostCanvas.Children.Contains(window))
      {
        //tell window to detach from its current FloatingWindowHost here (if any)
        window.RemoveFromContainer();
        window.FloatingWindowHost = null; //could change the implementation of that property to call RemoveFromContainer if needed

        hostCanvas.Children.Add(window);
        window.FloatingWindowHost = this;
      }
    }

    /// <summary>
    /// Removes the specified floating window from the collection of child elements of the host.
    /// </summary>
    /// <param name="window">The floating window.</param>
    public FloatingWindow Remove(FloatingWindow window)
    {
      Windows.Remove(window);
      return window;
    }

    private void _Remove(FloatingWindow window)
    {
      if (window != null)
      {
        hostCanvas.Children.Remove(window);
        modalCanvas.Children.Remove(window);
        iconbar.Remove(window);
      }
    }

    /// <summary>
    /// Closes all floating windows.
    /// </summary>
    public void CloseAllWindows()
    {
      //IsIconbarVisible = false;
      FloatingWindows.ToList().ForEach(x => x.Close());
    }

    #region Iconbar

    /// <summary>
    /// Gets a collection of windows shown in the Iconbar.
    /// </summary>
    /// <value>Collection of windows shown in the Iconbar.</value>
    public IOrderedEnumerable<FloatingWindow> WindowsInIconbar
    {
      get
      {
        var windows = from window in this.FloatingWindows
                      where window.IsOpen && window.ShowInIconbar &&
                      !(ShowMinimizedOnlyInIconbar && window.WindowState != WindowState.Minimized)
                      orderby window.IconText
                      select window;

        return windows;
      }
    }
    
    /// <summary>
    /// Toggles the Iconbar.
    /// </summary>
    public void ToggleIconbar()
    {
      IsIconbarVisible = !IsIconbarVisible;
    }
    
    /// <summary>
    /// Updates the Iconbar if it is open.
    /// </summary>
    public void UpdateIconbar()
    {
      iconbar.Update();
    }

    /// <summary>
    /// Handles the Click event of the BootstrapButton.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    private void BootstrapButton_Click(object sender, RoutedEventArgs e)
    {
      IsIconbarVisible = bootstrapButton.IsOpen;
    }

    /// <summary>
    /// Handles the Iconbar Visibility Changed event.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    private void Iconbar_VisibilityChanged(object sender, EventArgs e)
    {
      bootstrapButton.IsOpen = iconbar.IsOpen;
    }

    /// <summary>
    /// Handles the MouseLeftButtonDown event of the Toolbar control. 
    /// Toggles the Iconbar on mouse left click.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
    private void Toolbar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
      ToggleIconbar();
    }

    #endregion

    #region Topmost window

    /// <summary>
    /// Sets the specified floating window topmost, and set Focus on it.
    /// </summary>
    /// <param name="window">FloatingWindow to set topmost.</param>
    /// <exception cref="ArgumentNullException">FloatingWindow is null.</exception>
    public void SetTopmostWindow(FloatingWindow window)
    {
      if (window == null)
        throw new ArgumentNullException("window");

      FloatingWindow topmostWindow = GetTopmostWindow();
      if (topmostWindow == null || window != topmostWindow)
      {
        SetTopmost(window);
        SetFocusToActiveWindow(window);

        if (!window.TopMost && !window.IsModal)
          ShowTopmostWindows();

        ActiveWindowChangedEventArgs e = new ActiveWindowChangedEventArgs(topmostWindow, window);
        OnActiveWindowChanged(e);
      }
    }

    /// <summary>
    /// Activates the topmost window and sets focus on it.
    /// </summary>
    public void ActivateTopmostWindow()
    {
      // First, try to activate a modal window, if exists
      var topmostWindow = this.ModalWindow;

      if (topmostWindow == null)
        topmostWindow = GetTopmostWindow();

      SetFocusToActiveWindow(topmostWindow);

      ActiveWindowChangedEventArgs e = new ActiveWindowChangedEventArgs(null, topmostWindow);
      OnActiveWindowChanged(e);
    }

    #endregion

    /// <summary>
    /// Gets Snap In bounds as bounds of the host and all open windows except the specified one.
    /// </summary>
    /// <param name="windowToExclude">The window to exclude from the list.</param>
    /// <returns>List of bounding rectangles.</returns>
    internal IEnumerable<Rect> GetSnapinBounds(FloatingWindow windowToExclude)
    {
      List<Rect> bounds = new List<Rect>();

      // Add host bounds
      bounds.Add(hostCanvas.GetActualBoundingRectangle());

      if (!overlay.IsVisible())
      {
        foreach (var window in FloatingWindows)
        {
          if (window != windowToExclude && window.IsOpen)
            bounds.Add(window.BoundingRectangle);
        }
      }

      return bounds;
    }

    /// <summary>
    /// Shows the overlay, moves the window to the "modal" layer.
    /// </summary>
    /// <param name="modalWindow">The modal window.</param>
    internal void ShowWindowAsModal(FloatingWindow modalWindow)
    {
      //IsIconbarVisible = false;
      VisualStateManager.GoToState(this, VSMSTATE_VisibleOverlay, true);

      MoveWindowToModalLayer(modalWindow);
    }

    /// <summary>
    /// Removes the overlay under the modal window.
    /// </summary>
    internal void RemoveOverlay()
    {
      if (overlay.IsVisible())
      {
        FloatingWindow topmostWindow = null;

        if (FloatingWindows.Count(x => x.IsOpen && x.IsModal) == 0)
        {
          // If there are no more modal windows - remove the overlay
          VisualStateManager.GoToState(this, VSMSTATE_HiddenOverlay, true);

          topmostWindow = GetTopmostWindow();
        }
        else
        {
          topmostWindow = GetTopmostModalWindow();

          if (topmostWindow != null)
            topmostWindow.MoveToContainer(modalCanvas);
        }

        SetFocusToActiveWindow(topmostWindow);
      }
    }

    /// <summary>
    /// Moves the window to the "modal" layer.
    /// </summary>
    /// <param name="modalWindow">The modal window.</param>
    private void MoveWindowToModalLayer(FloatingWindow modalWindow)
    {
      FloatingWindow window = this.ModalWindow;
      if (window != null && window != modalWindow)
      {
        // If there is already a modal window - move it to the HostCanvas
        window.MoveToContainer(hostCanvas);
        SetTopmost(window);
      }

      modalWindow.MoveToContainer(modalCanvas);
    }

    /// <summary>
    /// Raises the <see cref="FloatingWindowHost.ActiveWindowChanged" /> event.
    /// </summary>
    /// <param name="e">The event data.</param>
    protected virtual void OnActiveWindowChanged(ActiveWindowChangedEventArgs e)
    {
      EventHandler<ActiveWindowChangedEventArgs> handler = ActiveWindowChanged;

      if (handler != null)
        handler(this, e);
    }

    /// <summary>
    /// Shows a floating window when the layout is updated.
    /// </summary>
    /// <param name="action">A method that displays a window in the specified coordinates.</param>
    /// <param name="point">Coordinates of the upper-left corner of the window.</param>
    internal void ShowWindow(Action<Point, bool> action, Point point, bool bringToFront = true)
    {
      if (IsLayoutUpdated)
      {
        action(point, bringToFront);
      }
      else
      {
        this.Rendered += (s, e) => { action(point, bringToFront); };
      }
    }

    /// <summary>
    /// Shows a floating window when the layout is updated.
    /// </summary>
    /// <param name="action">A method that displays a window taking into account specified margins.</param>
    /// <param name="margins">Window margins.</param>
    internal void ShowWindow(Action<Thickness> action, Thickness margins)
    {
      if (IsLayoutUpdated)
      {
        action(margins);
      }
      else
      {
        this.Rendered += (s, e) => { action(margins); };

      }
    }

    /// <summary>
    /// Subscribes to the events on the template parts.
    /// </summary>
    private void SubscribeToTemplatePartEvents()
    {
      bootstrapButton.Click += BootstrapButton_Click;
      iconbar.Opened += Iconbar_VisibilityChanged;
      iconbar.Closed += Iconbar_VisibilityChanged;
      toolbar.MouseLeftButtonDown += Toolbar_MouseLeftButtonDown;
      hostCanvas.SizeChanged += HostCanvas_SizeChanged;
      modalCanvas.SizeChanged += ModalCanvas_SizeChanged;
      iconbarContainer.SizeChanged += IconbarContainer_SizeChanged;
    }

    /// <summary>
    /// Unsubscribe from the events that are subscribed on the template part elements.
    /// </summary>
    private void UnsubscribeFromTemplatePartEvents()
    {
      if (bootstrapButton != null)
        bootstrapButton.Click -= new RoutedEventHandler(BootstrapButton_Click);

      if (iconbar != null)
      {
        iconbar.Opened -= Iconbar_VisibilityChanged;
        iconbar.Closed -= Iconbar_VisibilityChanged;
      }

      if (toolbar != null)
        toolbar.MouseLeftButtonDown -= Toolbar_MouseLeftButtonDown;

      if (hostCanvas != null)
        hostCanvas.SizeChanged -= HostCanvas_SizeChanged;

      if (modalCanvas != null)
        modalCanvas.SizeChanged -= ModalCanvas_SizeChanged;

      if (iconbarContainer != null)
        iconbarContainer.SizeChanged -= IconbarContainer_SizeChanged;
    }

    /// <summary>
    /// Subscribes to the events the control shall handle.
    /// </summary>
    private void SubscribeToEvents()
    {
      if (Application.Current != null)
        Application.Current.Exit += Application_Exit;

      this.LayoutUpdated += FloatingWindowHost_LayoutUpdated;
    }

    /// <summary>
    /// Unsubscribes from the subscribed events.
    /// </summary>
    private void UnsubscribeFromEvents()
    {
      if (Application.Current != null)
        Application.Current.Exit -= Application_Exit; 
      
      this.LayoutUpdated -= FloatingWindowHost_LayoutUpdated;
    }

    /// <summary>
    /// Handles the first LayoutUpdated event of the FloatingWindowHost control to raise the OnRendered event.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    private void FloatingWindowHost_LayoutUpdated(object sender, EventArgs e)
    {
      if (!IsLayoutUpdated)
      {
        this.LayoutUpdated -= FloatingWindowHost_LayoutUpdated; //TODO: should this be removed here? now OnRendered will only fire once (is it correct?)
        IsLayoutUpdated = true;
        OnRendered(EventArgs.Empty);
      }
    }

    /// <summary>
    /// Raises the <see cref="E:Rendered"/> event. 
    /// Occures when the control template is applied and the control can be rendered.
    /// </summary>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    protected virtual void OnRendered(EventArgs e)
    {
      EventHandler handler = Rendered;

      if (handler != null)
        handler(this, e);
    }

    /// <summary>
    /// Gets the topmost open FloatingWindow.
    /// </summary>
    /// <returns>The topmost open FloatingWindow.</returns>
    private FloatingWindow GetTopmostWindow()
    {
      var topmost = (from window in FloatingWindows
                     where !window.TopMost && window.IsOpen && window.WindowState != WindowState.Minimized
                     select new
                     {
                       Window = window,
                       ZIndex = Canvas.GetZIndex(window)
                     }
                     ).ToList().OrderBy(x => x.ZIndex).LastOrDefault();

      return topmost != null ? topmost.Window : null;
    }

    /// <summary>
    /// Gets the topmost modal FloatingWindow on the HostCanvas.
    /// </summary>
    /// <returns>The topmost modal FloatingWindow.</returns>
    private FloatingWindow GetTopmostModalWindow()
    {
      var topmost = (from window in FloatingWindows
                     where window.IsModal && window.IsOpen
                     select new
                     {
                       Window = window,
                       ZIndex = Canvas.GetZIndex(window)
                     }
                     ).ToList().OrderBy(x => x.ZIndex).LastOrDefault();

      return topmost != null ? topmost.Window : null;
    }

    /// <summary>
    /// Shows the topmost windows in front of other windows.
    /// </summary>
    private void ShowTopmostWindows()
    {
      FloatingWindows
          .Where(x => x.IsOpen && x.TopMost).ToList()
          .ForEach(x => SetTopmost(x));
    }

    /// <summary>
    /// Sets the specified UIElement topmost.
    /// </summary>
    /// <param name="element">UIElement to set topmost.</param>
    /// <exception cref="ArgumentNullException">UIElement is null.</exception>
    private void SetTopmost(UIElement element)
    {
      if (element == null)
        throw new ArgumentNullException("element");

      Canvas.SetZIndex(element, MaxZIndex + 1);
    }

    /// <summary>
    /// Attempts to set the focus on the FloatingWindow.
    /// </summary>
    /// <param name="window">The window.</param>
    private void SetFocusToActiveWindow(FloatingWindow window)
    {
      try
      {
        if (window != null && !window.TopMost
            && !window.IsVisualAncestorOf(
                    #if SILVERLIGHT
                    (DependencyObject)FocusManager.GetFocusedElement()
                    #else
                   (DependencyObject)Keyboard.FocusedElement
                    #endif
           )) //checking if child control has the focus and not steal it from it
          window.Focus();
      }
      catch
      {
        //NOP ("IsVisualAncestorOf" can throw exception "Reference is not a valid visual Dependency Object" (e.g. if a link in a RichText control hosted at a non-focused FloatingWindow is clicked), it is safe to ignore it here
      }
    }

    /// <summary>
    /// Handles the SizeChanged event of the HostCanvas control to set its clipping region.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.Windows.SizeChangedEventArgs"/> instance containing the event data.</param>
    private void HostCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      hostCanvas.Clip = new RectangleGeometry()
      {
        Rect = hostCanvas.GetActualBoundingRectangle()
      };
    }

    /// <summary>
    /// Handles the SizeChanged event of the ModalCanvas control to set its clipping region.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.Windows.SizeChangedEventArgs"/> instance containing the event data.</param>
    private void ModalCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      modalCanvas.Clip = new RectangleGeometry()
      {
        Rect = modalCanvas.GetActualBoundingRectangle()
      };
    }

    /// <summary>
    /// Handles the SizeChanged event of the IconbarContainer control to set its clipping region.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.Windows.SizeChangedEventArgs"/> instance containing the event data.</param>
    private void IconbarContainer_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      iconbarContainer.Clip = new RectangleGeometry()
      {
        Rect = iconbarContainer.GetActualBoundingRectangle()
      };
    }

    /// <summary>
    /// Executed when the application is exited.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">Event args.</param>
    private void Application_Exit(object sender, ExitEventArgs e)
    {
      if (CloseWindowsOnApplicationExit)
        CloseAllWindows(); //FloatingWindows expect FloatingWindowHost to close them at App exit
    }

    /// <summary>
    /// Sets styles that are applied for different template parts.
    /// </summary>
    private void SetStyles()
    {
      if (toolbar != null && this.ToolbarStyle != null)
        toolbar.Style = this.ToolbarStyle;

      if (bootstrapButton != null && this.BootstrapButtonStyle != null)
        bootstrapButton.Style = this.BootstrapButtonStyle;

      if (iconbar != null && this.WindowIconStyle != null)
        iconbar.WindowIconStyle = this.WindowIconStyle;
    }

    /// <summary>
    /// Gets the FrameworkElement template part with the specified name.
    /// </summary>
    /// <typeparam name="T">The template part type.</typeparam>
    /// <param name="partName">The template part name.</param>
    /// <returns>The requested element.</returns>
    /// <exception cref="NotImplementedException">The template part not found.</exception>
    private T GetTemplatePart<T>(string partName) where T : class
    {
      T part = this.GetTemplateChild(partName) as T;

      if (part == null)
        throw new NotImplementedException(string.Format(CultureInfo.InvariantCulture, "Template Part {0} is required.", partName));

      return part;
    }

  }
}
