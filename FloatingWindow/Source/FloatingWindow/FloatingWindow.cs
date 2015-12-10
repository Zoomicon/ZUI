//Filename: FloatingWindow.cs
//Version: 20151210

//#define BORDER_ONLY_AT_RESIZABLE //using BorderThickness instead to allow user to define when they want the border to be visible themselves

using System;
using System.IO;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

using SilverFlow.Controls.Controllers;
using SilverFlow.Controls.Enums;
using SilverFlow.Controls.Extensions;
using SilverFlow.Controls.Helpers;

#if !SILVERLIGHT
using Microsoft.Win32; //for SaveFileDialog
#endif

using Compatibility;

namespace SilverFlow.Controls
{
  /// <summary>
  /// Provides a window that can be displayed over a parent window not blocking
  /// interaction with the parent window.
  /// </summary>
  #region Attributes
  [TemplatePart(Name = PART_Chrome, Type = typeof(FrameworkElement))]
  [TemplatePart(Name = PART_TitleContent, Type = typeof(ContentControl))]
  [TemplatePart(Name = PART_ScreenshotButton, Type = typeof(ButtonBase))]
  [TemplatePart(Name = PART_HelpButton, Type = typeof(ButtonBase))]
  [TemplatePart(Name = PART_OptionsButton, Type = typeof(ButtonBase))]
  [TemplatePart(Name = PART_CloseButton, Type = typeof(ButtonBase))]
  [TemplatePart(Name = PART_ContentPresenter, Type = typeof(FrameworkElement))]
  [TemplatePart(Name = PART_ContentRoot, Type = typeof(FrameworkElement))]
  [TemplatePart(Name = PART_ContentBorder, Type = typeof(FrameworkElement))]
  [TemplatePart(Name = PART_Root, Type = typeof(FrameworkElement))]
  [TemplateVisualState(Name = VSMSTATE_StateClosed, GroupName = VSMGROUP_Window)]
  [TemplateVisualState(Name = VSMSTATE_StateOpen, GroupName = VSMGROUP_Window)]
  [TemplateVisualState(Name = VSMSTATE_StateMinimized, GroupName = VSMGROUP_Window)]
  [TemplateVisualState(Name = VSMSTATE_StateRestored, GroupName = VSMGROUP_Window)]
  [TemplateVisualState(Name = VSMSTATE_StateNormal, GroupName = VSMGROUP_Button)]
  [StyleTypedProperty(Property = PROPERTY_TitleStyle, StyleTargetType = typeof(ContentControl))]
  [StyleTypedProperty(Property = PROPERTY_ScreenshotButtonStyle, StyleTargetType = typeof(Button))]
  [StyleTypedProperty(Property = PROPERTY_HelpButtonStyle, StyleTargetType = typeof(Button))]
  [StyleTypedProperty(Property = PROPERTY_OptionsButtonStyle, StyleTargetType = typeof(Button))]
  [StyleTypedProperty(Property = PROPERTY_CloseButtonStyle, StyleTargetType = typeof(Button))]
  [StyleTypedProperty(Property = PROPERTY_MinimizeButtonStyle, StyleTargetType = typeof(Button))]
  [StyleTypedProperty(Property = PROPERTY_MaximizeButtonStyle, StyleTargetType = typeof(Button))]
  [StyleTypedProperty(Property = PROPERTY_RestoreButtonStyle, StyleTargetType = typeof(Button))]
  #endregion
  public class FloatingWindow : ContentControl, IResizableElement, IDisposable
  {

    #region --- Constants ---

    // Template parts
    private const string PART_Chrome = "Chrome";
    private const string PART_TitleContent = "TitleContent";
    private const string PART_ScreenshotButton = "ScreenshotButton";
    private const string PART_HelpButton = "HelpButton";
    private const string PART_OptionsButton = "OptionsButton";
    private const string PART_CloseButton = "CloseButton";
    private const string PART_MaximizeButton = "MaximizeButton";
    private const string PART_RestoreButton = "RestoreButton";
    private const string PART_MinimizeButton = "MinimizeButton";
    private const string PART_ContentPresenter = "ContentPresenter";
    private const string PART_ContentRoot = "ContentRoot";
    private const string PART_ContentBorder = "ContentBorder";
    private const string PART_Root = "Root";

    // VSM groups
    private const string VSMGROUP_Window = "WindowStates";
    private const string VSMGROUP_Button = "CommonStates";

    // VSM states
    private const string VSMSTATE_StateNormal = "Normal";
    private const string VSMSTATE_StateClosed = "Closed";
    private const string VSMSTATE_StateOpen = "Open";
    private const string VSMSTATE_StateMinimized = "Minimized";
    private const string VSMSTATE_StateRestored = "Restored";

    // Style typed properties
    private const string PROPERTY_TitleStyle = "TitleStyle";
    private const string PROPERTY_ScreenshotButtonStyle = "ScreenshotButtonStyle";
    private const string PROPERTY_HelpButtonStyle = "HelpButtonStyle";
    private const string PROPERTY_OptionsButtonStyle = "OptionsButtonStyle";
    private const string PROPERTY_CloseButtonStyle = "CloseButtonStyle";
    private const string PROPERTY_MinimizeButtonStyle = "MinimizeButtonStyle";
    private const string PROPERTY_MaximizeButtonStyle = "MaximizeButtonStyle";
    private const string PROPERTY_RestoreButtonStyle = "RestoreButtonStyle";

    // Thickness of resizing area
    private const double ResizingAreaDefaultValue = 6;

    // Animation duration in milliseconds
    private const double MaximizingDurationInMilliseconds = 20;
    private const double MinimizingDurationInMilliseconds = 200;
    private const double RestoringDurationInMilliseconds = 20;

    #endregion

    #region --- Fields ---

    // Mouse click point
    private Point clickPoint;

    // Window position when the mouse was clicked
    private Point clickWindowPosition;

    // Window position, size and state when it was maximized or minimized
    private Point previousPosition;
    private Size previousSize;
    private WindowState previousWindowState;

    private Thickness contentBorderThickness;
    private CornerRadius contentBorderCornerRadius;
    private CornerRadius chromeBorderCornerRadius;

    private Storyboard openingStoryboard;
    private Storyboard closingStoryboard;
    private Storyboard maximizingStoryboard;
    private Storyboard restoringStoryboard;
    private Storyboard restoreMaximizedStoryboard;
    private Storyboard inertialMotionStoryboard;

    private FrameworkElement root;
    private FrameworkElement contentPresenter;
    private FrameworkElement contentRoot;
    private FrameworkElement chrome;
    private ContentControl titleContent;
    private Border contentBorder;

    private ButtonBase screenshotButton;
    private ButtonBase helpButton;
    private ButtonBase optionsButton;
    private ButtonBase closeButton;
    private ButtonBase maximizeButton;
    private ButtonBase restoreButton;
    private ButtonBase minimizeButton;

    private bool isAppExit;
    private bool isMouseCaptured;

    private ResizeController resizeController;
    private ISnapinController snapinController;
    private InertiaController inertiaController;
    private ILocalStorage localStorage;
    private IBitmapHelper bitmapHelper;

    // Current window state
    private WindowState windowState = WindowState.Normal;

    // Specifies whether the window is moving or resizing
    private WindowAction windowAction;

    // Bitmap containing thumbnail image
    private ImageSource minimizedWindowThumbnail;

    private bool templateIsApplied; //=false

    #endregion

    #region --- Constructor ---

    #if !SILVERLIGHT
    static FloatingWindow()
    {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(FloatingWindow), new FrameworkPropertyMetadata(typeof(FloatingWindow)));
    }
    #endif

    /// <summary>
    /// Initializes a new instance of the <see cref="FloatingWindow" /> class.
    /// </summary>
    public FloatingWindow()
    {
      DefaultStyleKey = typeof(FloatingWindow);

      resizeController = new ResizeController(this);
      resizeController.ResizingArea = ResizingAreaThickness;
      snapinController = new SnapinController();
      inertiaController = new InertiaController();
      localStorage = new LocalStorage();
      bitmapHelper = new BitmapHelper();

      this.SetVisible(false);
    }

    #endregion

    #region --- Destructor ---

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
      GC.Collect();
      GC.WaitForPendingFinalizers();
      GC.Collect();
    }

    #endregion

    #region --- Properties ---

    #region DialogResult

    /// <summary>
    /// Gets or sets a value that indicates whether the FloatingWindow was accepted or canceled.
    /// </summary>
    /// <value>true if the FloatingWindow was accepted; false - if canceled. The default is null.</value>
    [TypeConverter(typeof(NullableBoolConverter))]
    public bool? DialogResult { get; set; }

    #endregion

    #region Objects

    /// <summary>
    /// Gets or sets a reference to the FloatingWindowHost, containing the window.
    /// </summary>
    /// <value>The floating window host.</value>
    public FloatingWindowHost FloatingWindowHost { get; set; }

    /// <summary>
    /// Gets the host panel, containing the floating windows.
    /// </summary>
    /// <value>The host panel.</value>
    public Panel HostPanel
    {
      get { return this.FloatingWindowHost == null ? null : this.FloatingWindowHost.HostPanel; }
    }

    /// <summary>
    /// Gets a Snapin controller.
    /// </summary>
    /// <value>Snapin controller.</value>
    public ISnapinController SnapinController
    {
      get { return snapinController; }
    }

    /// <summary>
    /// Gets the window thumbnail.
    /// </summary>
    /// <value>The window thumbnail.</value>
    public ImageSource WindowThumbnail
    {
      get
      {
        return (windowState == WindowState.Minimized) ? minimizedWindowThumbnail : GetThumbnailImage();
      }
    }

    #endregion

    #region State

    /// <summary>
    /// Gets or sets a value indicating whether the window is always displayed in front of other windows.
    /// </summary>
    /// <value><c>true</c> if top most; otherwise, <c>false</c>.</value>
    public bool TopMost { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this window is open.
    /// </summary>
    /// <value><c>true</c> if this window is open; otherwise, <c>false</c>.</value>
    public bool IsOpen { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether this window is modal.
    /// </summary>
    /// <value><c>true</c> if this window is modal; otherwise, <c>false</c>.</value>
    public bool IsModal { get; private set; }

    /// <summary>
    /// Gets the state of the window.
    /// </summary>
    /// <value>Current state of the window.</value>
    public WindowState WindowState
    {
      get { return windowState; }
      set
      {
        switch (value)
        {
          case WindowState.Normal: RestoreWindow(); break;
          case WindowState.Maximized: MaximizeWindow(); break;
          case WindowState.Minimized: MinimizeWindow(); break;
        }
      }
    }

    /// <summary>
    /// Gets a value indicating whether window size is explicitly set.
    /// </summary>
    /// <value><c>true</c> if window size is set; otherwise, <c>false</c>.</value>
    private bool IsWindowSizeSet
    {
      get { return !Width.IsNotSet() && !Height.IsNotSet(); }
    }

    /// <summary>
    /// Gets a value indicating whether the window Tag property is set.
    /// </summary>
    /// <value><c>true</c> if the Tag set; otherwise, <c>false</c>.</value>
    private bool IsWindowTagSet
    {
      get
      {
        string tag = this.Tag as string;
        return !string.IsNullOrWhiteSpace(tag);
      }
    }

    #endregion

    #region Constraints

    /// <summary>
    /// Gets or sets the minimum height constraint of a <see cref="T:System.Windows.FrameworkElement"/>.
    /// </summary>
    /// <value>he minimum height of the window.</value>
    /// <returns>The minimum height of the window, in pixels. The default is 0.
    /// This value can be any value equal to or greater than 0.
    /// However, <see cref="F:System.Double.PositiveInfinity"/> is not valid.</returns>
    public new double MinHeight
    {
      get
      {
        double minHeight = base.MinHeight;

        if ((base.MinHeight.IsNotSet() || base.MinHeight == 0) &&
            (chrome != null && contentRoot != null))
        {
          // Set minimal height to the height of the chrome element of the window
          minHeight = chrome.GetRelativePosition(contentRoot).Y + chrome.ActualHeight;
        }

        return minHeight;
      }

      set
      {
        base.MinHeight = value;
      }
    }

    #endregion

    #region Bounds

    /// <summary>
    /// Gets a bounding rectangle of the window.
    /// </summary>
    /// <value>Bounding rectangle.</value>
    public Rect BoundingRectangle
    {
      get { return new Rect(Position.X, Position.Y, ActualWidth, ActualHeight); }
    }

    /// <summary>
    /// Gets the bounds of the maximized window.
    /// </summary>
    /// <value>Bounding rectangle of maximized window.</value>
    public Rect BoundingRectangleMaximized
    {
      get {
        Rect bounds = FloatingWindowHost.MaximizedWindowBounds;
        return bounds.Scaled(1 / Scale); //TODO: this doesn't work very well when Scale isn't 1, seems to do wrong positioning
      }
    }

    #endregion

    #region ShowChrome

    /// <summary>
    /// Gets or sets a value indicating whether to show Chrome (titlebar).
    /// </summary>
    /// <value><c>true</c> if to show Chrome; otherwise, <c>false</c>.</value>
    public bool ShowChrome
    {
      get { return (bool)GetValue(ShowChromeProperty); }
      set { SetValue(ShowChromeProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="FloatingWindow.ShowChrome" /> dependency property.
    /// </summary>
    /// <value>
    /// The identifier for the <see cref="FloatingWindow.ShowChrome" /> dependency property.
    /// </value>
    public static readonly DependencyProperty ShowChromeProperty =
        DependencyProperty.Register(
        "ShowChrome",
        typeof(bool),
        typeof(FloatingWindow),
        new PropertyMetadata(true, OnShowChromePropertyChanged));

    /// <summary>
    /// ShowChromeProperty PropertyChangedCallback call back static function.
    /// </summary>
    /// <param name="d">FloatingWindow object whose ShowChrome property is changed.</param>
    /// <param name="e">DependencyPropertyChangedEventArgs which contains the old and new values.</param>
    private static void OnShowChromePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      FloatingWindow window = (FloatingWindow)d;

      if (window.chrome != null)
        window.chrome.SetVisible((bool)e.NewValue); //TODO: could also use a VisibilityToBooleanConverter at the Template
    }

    #endregion

    #region ShowScreenshotButton

    /// <summary>
    /// Gets or sets a value indicating whether to show Screenshot button.
    /// </summary>
    /// <value><c>true</c> if to show Screenshot button; otherwise, <c>false</c>.</value>
    public bool ShowScreenshotButton
    {
      get { return (bool)GetValue(ShowScreenshotButtonProperty); }
      set { SetValue(ShowScreenshotButtonProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="FloatingWindow.ShowScreenshotButton" /> dependency property.
    /// </summary>
    /// <value>
    /// The identifier for the <see cref="FloatingWindow.ShowScreenshotButton" /> dependency property.
    /// </value>
    public static readonly DependencyProperty ShowScreenshotButtonProperty =
        DependencyProperty.Register(
        "ShowScreenshotButton",
        typeof(bool),
        typeof(FloatingWindow),
        new PropertyMetadata(true, OnShowScreenshotButtonPropertyChanged));

    /// <summary>
    /// ShowScreenshotButtonProperty PropertyChangedCallback call back static function.
    /// </summary>
    /// <param name="d">FloatingWindow object whose ShowScreenshotButton property is changed.</param>
    /// <param name="e">DependencyPropertyChangedEventArgs which contains the old and new values.</param>
    private static void OnShowScreenshotButtonPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      FloatingWindow window = (FloatingWindow)d;

      if (window.screenshotButton != null)
        window.screenshotButton.SetVisible((bool)e.NewValue); //TODO: could also use a VisibilityToBooleanConverter at the Template
    }

    #endregion

    #region ShowHelpButton

    /// <summary>
    /// Gets or sets a value indicating whether to show Help button.
    /// </summary>
    /// <value><c>true</c> if to show Help button; otherwise, <c>false</c>.</value>
    public bool ShowHelpButton
    {
      get { return (bool)GetValue(ShowHelpButtonProperty); }
      set { SetValue(ShowHelpButtonProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="FloatingWindow.ShowHelpButton" /> dependency property.
    /// </summary>
    /// <value>
    /// The identifier for the <see cref="FloatingWindow.ShowHelpButton" /> dependency property.
    /// </value>
    public static readonly DependencyProperty ShowHelpButtonProperty =
        DependencyProperty.Register(
        "ShowHelpButton",
        typeof(bool),
        typeof(FloatingWindow),
        new PropertyMetadata(true, OnShowHelpButtonPropertyChanged));

    /// <summary>
    /// ShowHelpButtonProperty PropertyChangedCallback call back static function.
    /// </summary>
    /// <param name="d">FloatingWindow object whose ShowHelpButton property is changed.</param>
    /// <param name="e">DependencyPropertyChangedEventArgs which contains the old and new values.</param>
    private static void OnShowHelpButtonPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      FloatingWindow window = (FloatingWindow)d;

      if (window.helpButton != null)
        window.helpButton.SetVisible((bool)e.NewValue); //TODO: could also use a VisibilityToBooleanConverter at the Template
    }

    #endregion

    #region ShowOptionsButton

    /// <summary>
    /// Gets or sets a value indicating whether to show Options button.
    /// </summary>
    /// <value><c>true</c> if to show Options button; otherwise, <c>false</c>.</value>
    public bool ShowOptionsButton
    {
      get { return (bool)GetValue(ShowOptionsButtonProperty); }
      set { SetValue(ShowOptionsButtonProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="FloatingWindow.ShowOptionsButton" /> dependency property.
    /// </summary>
    /// <value>
    /// The identifier for the <see cref="FloatingWindow.ShowOptionsButton" /> dependency property.
    /// </value>
    public static readonly DependencyProperty ShowOptionsButtonProperty =
        DependencyProperty.Register(
        "ShowOptionsButton",
        typeof(bool),
        typeof(FloatingWindow),
        new PropertyMetadata(true, OnShowOptionsButtonPropertyChanged));

    /// <summary>
    /// ShowOptionsButtonProperty PropertyChangedCallback call back static function.
    /// </summary>
    /// <param name="d">FloatingWindow object whose ShowOptionsButton property is changed.</param>
    /// <param name="e">DependencyPropertyChangedEventArgs which contains the old and new values.</param>
    private static void OnShowOptionsButtonPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      FloatingWindow window = (FloatingWindow)d;

      if (window.optionsButton != null)
        window.optionsButton.SetVisible((bool)e.NewValue); //TODO: could also use a VisibilityToBooleanConverter at the Template
    }

    #endregion

    #region ShowCloseButton

    /// <summary>
    /// Gets or sets a value indicating whether to show Close button.
    /// </summary>
    /// <value><c>true</c> if to show Close button; otherwise, <c>false</c>.</value>
    public bool ShowCloseButton
    {
      get { return (bool)GetValue(ShowCloseButtonProperty); }
      set { SetValue(ShowCloseButtonProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="FloatingWindow.ShowCloseButton" /> dependency property.
    /// </summary>
    /// <value>
    /// The identifier for the <see cref="FloatingWindow.ShowCloseButton" /> dependency property.
    /// </value>
    public static readonly DependencyProperty ShowCloseButtonProperty =
        DependencyProperty.Register(
        "ShowCloseButton",
        typeof(bool),
        typeof(FloatingWindow),
        new PropertyMetadata(true, OnShowCloseButtonPropertyChanged));

    /// <summary>
    /// ShowCloseButtonProperty PropertyChangedCallback call back static function.
    /// </summary>
    /// <param name="d">FloatingWindow object whose ShowCloseButton property is changed.</param>
    /// <param name="e">DependencyPropertyChangedEventArgs which contains the old and new values.</param>
    private static void OnShowCloseButtonPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      FloatingWindow window = (FloatingWindow)d;

      if (window.closeButton != null)
        window.closeButton.SetVisible((bool)e.NewValue); //TODO: could also use a VisibilityToBooleanConverter at the Template
    }

    #endregion

    #region ShowMaximizeRestoreButton

    /// <summary>
    /// Gets or sets a value indicating whether to show Maximize (and respective Restore) button.
    /// </summary>
    /// <value><c>true</c> if to show Maximize button; otherwise, <c>false</c>.</value>
    public bool ShowMaximizeRestoreButton
    {
      get { return (bool)GetValue(ShowMaximizeRestoreButtonProperty); }
      set { SetValue(ShowMaximizeRestoreButtonProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="FloatingWindow.ShowMaximizeRestoreButton" /> dependency property.
    /// </summary>
    /// <value>
    /// The identifier for the <see cref="FloatingWindow.ShowMaximizeRestoreButton" /> dependency property.
    /// </value>
    public static readonly DependencyProperty ShowMaximizeRestoreButtonProperty =
        DependencyProperty.Register(
        "ShowMaximizeRestoreButton",
        typeof(bool),
        typeof(FloatingWindow),
        new PropertyMetadata(true, ShowMaximizeRestoreButtonPropertyChanged));

    /// <summary>
    /// ShowMaximizeRestoreButtonProperty PropertyChangedCallback call back static function.
    /// </summary>
    /// <param name="d">FloatingWindow object whose ShowMaximizeRestoreButton property is changed.</param>
    /// <param name="e">DependencyPropertyChangedEventArgs which contains the old and new values.</param>
    private static void ShowMaximizeRestoreButtonPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      FloatingWindow window = (FloatingWindow)d;
      bool visible = window.IsModal ? false : (bool)e.NewValue;

      if (window.maximizeButton != null)
        window.maximizeButton.SetVisible(visible); //TODO: could also use a VisibilityToBooleanConverter at the Template

      if (window.restoreButton != null)
        window.restoreButton.SetVisible(false); //TODO: could also use a VisibilityToBooleanConverter at the Template
    }

    #endregion

    #region ShowMinimizeButton

    /// <summary>
    /// Gets or sets a value indicating whether to show Minimize button.
    /// </summary>
    /// <value><c>true</c> if to show Minimize button; otherwise, <c>false</c>.</value>
    public bool ShowMinimizeButton
    {
      get { return (bool)GetValue(ShowMinimizeButtonProperty); }
      set { SetValue(ShowMinimizeButtonProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="FloatingWindow.ShowMinimizeButton" /> dependency property.
    /// </summary>
    /// <value>
    /// The identifier for the <see cref="FloatingWindow.ShowMinimizeButton" /> dependency property.
    /// </value>
    public static readonly DependencyProperty ShowMinimizeButtonProperty =
        DependencyProperty.Register(
        "ShowMinimizeButton",
        typeof(bool),
        typeof(FloatingWindow),
        new PropertyMetadata(true, ShowMinimizeButtonPropertyChanged));

    /// <summary>
    /// ShowMinimizeButtonProperty PropertyChangedCallback call back static function.
    /// </summary>
    /// <param name="d">FloatingWindow object whose ShowMinimizeButton property is changed.</param>
    /// <param name="e">DependencyPropertyChangedEventArgs which contains the old and new values.</param>
    private static void ShowMinimizeButtonPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      FloatingWindow window = (FloatingWindow)d;
      bool visible = window.IsModal ? false : (bool)e.NewValue;

      if (window.minimizeButton != null)
        window.minimizeButton.SetVisible(visible); //TODO: could also use a VisibilityToBooleanConverter at the Template
    }

    #endregion

    #region Title

    /// <summary>
    /// Gets or sets title content that is displayed on the top of the window.
    /// Can contain any UI elements - not only a text.
    /// </summary>
    /// <value>
    /// The title displayed at the top of the window. The default is null.
    /// </value>
    public object Title
    {
      get { return GetValue(TitleProperty); }
      set { SetValue(TitleProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="FloatingWindow.Title" /> dependency property.
    /// </summary>
    /// <value>
    /// The identifier for the <see cref="FloatingWindow.Title" /> dependency property.
    /// </value>
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
        "Title",
        typeof(object),
        typeof(FloatingWindow),
        null);

    #endregion

    #region Icon

    /// <summary>
    /// Gets or sets content that is displayed as an icon of the window on the iconbar.
    /// </summary>
    /// <value>
    /// The content displayed as an icon of the window on the iconbar. The default is null.
    /// </value>
    public object Icon
    {
      get { return GetValue(IconProperty); }
      set { SetValue(IconProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="FloatingWindow.Icon" /> dependency property.
    /// </summary>
    /// <value>
    /// The identifier for the <see cref="FloatingWindow.Icon" /> dependency property.
    /// </value>
    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(
        "Icon",
        typeof(object),
        typeof(FloatingWindow),
        null);

    #endregion

    #region IconText

    /// <summary>
    /// Gets or sets a text displayed on the icon of the minimized window.
    /// </summary>
    /// <value>
    /// The text displayed on the icon.
    /// </value>
    public string IconText
    {
      get { return (string)GetValue(IconTextProperty); }
      set { SetValue(IconTextProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="FloatingWindow.IconText" /> dependency property.
    /// </summary>
    /// <value>
    /// The identifier for the <see cref="FloatingWindow.IconText" /> dependency property.
    /// </value>
    public static readonly DependencyProperty IconTextProperty =
        DependencyProperty.Register(
        "IconText",
        typeof(string),
        typeof(FloatingWindow),
        null);

    #endregion

    #region TitleForeground

    /// <summary>
    /// Gets or sets the title Foreground.
    /// </summary>
    /// <value>The title Foreground.</value>
    public Brush TitleForeground
    {
      get { return (Brush)GetValue(TitleForegroundProperty); }
      set { SetValue(TitleForegroundProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="FloatingWindow.TitleForeground" /> dependency property.
    /// </summary>
    /// <value>
    /// The identifier for the <see cref="FloatingWindow.TitleForeground" /> dependency property.
    /// </value>
    public static readonly DependencyProperty TitleForegroundProperty =
        DependencyProperty.Register(
        "TitleForeground",
        typeof(Brush),
        typeof(FloatingWindow),
        new PropertyMetadata(new SolidColorBrush(Colors.Transparent)));

    #endregion

    #region TitleBackground

    /// <summary>
    /// Gets or sets the title background.
    /// </summary>
    /// <value>The title background.</value>
    public Brush TitleBackground
    {
      get { return (Brush)GetValue(TitleBackgroundProperty); }
      set { SetValue(TitleBackgroundProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="FloatingWindow.TitleBackground" /> dependency property.
    /// </summary>
    /// <value>
    /// The identifier for the <see cref="FloatingWindow.TitleBackground" /> dependency property.
    /// </value>
    public static readonly DependencyProperty TitleBackgroundProperty =
        DependencyProperty.Register(
        "TitleBackground",
        typeof(Brush),
        typeof(FloatingWindow),
        new PropertyMetadata(new SolidColorBrush(Colors.Transparent)));

    #endregion

    #region CornerRadius

    /// <summary>
    /// Gets or sets the corner radius.
    /// </summary>
    /// <value>The corner radius.</value>
    public CornerRadius CornerRadius
    {
      get { return (CornerRadius)GetValue(CornerRadiusProperty); }
      set { SetValue(CornerRadiusProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="FloatingWindow.CornerRadius" /> dependency property.
    /// </summary>
    /// <value>
    /// The identifier for the <see cref="FloatingWindow.CornerRadius" /> dependency property.
    /// </value>
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(
        "CornerRadius",
        typeof(CornerRadius),
        typeof(FloatingWindow),
        new PropertyMetadata(new CornerRadius(3)));

    #endregion

    #region MoveEnabled

    /// <summary>
    /// Gets or sets a value indicating whether moving is enabled.
    /// </summary>
    /// <value><c>true</c> if moving is enabled; otherwise, <c>false</c>.</value>
    public bool MoveEnabled
    {
      get { return (bool)GetValue(MoveEnabledProperty); }
      set { SetValue(MoveEnabledProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="FloatingWindow.MoveEnabled" /> dependency property.
    /// </summary>
    /// <value>
    /// The identifier for the <see cref="FloatingWindow.MoveEnabled" /> dependency property.
    /// </value>
    public static readonly DependencyProperty MoveEnabledProperty =
        DependencyProperty.Register(
        "MoveEnabled",
        typeof(bool),
        typeof(FloatingWindow),
        new PropertyMetadata(true));

    #endregion

    #region MoveMaximizedEnabled

    /// <summary>
    /// Gets or sets a value indicating whether moving is enabled when window is maximized.
    /// </summary>
    /// <value><c>true</c> if moving is enabled when window is maximized; otherwise, <c>false</c>.</value>
    public bool MoveMaximizedEnabled
    {
      get { return (bool)GetValue(MoveMaximizedEnabledProperty); }
      set { SetValue(MoveMaximizedEnabledProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="FloatingWindow.MoveMaximizedEnabled" /> dependency property.
    /// </summary>
    /// <value>
    /// The identifier for the <see cref="FloatingWindow.MoveMaximizedEnabled" /> dependency property.
    /// </value>
    public static readonly DependencyProperty MoveMaximizedEnabledProperty =
        DependencyProperty.Register(
        "MoveMaximizedEnabled",
        typeof(bool),
        typeof(FloatingWindow),
        new PropertyMetadata(false));

    #endregion

    #region ResizeEnabled

    /// <summary>
    /// Gets or sets a value indicating whether resizing is enabled.
    /// </summary>
    /// <value><c>true</c> if resizing is enabled; otherwise, <c>false</c>.</value>
    public bool ResizeEnabled
    {
      get { return (bool)GetValue(ResizeEnabledProperty); }
      set { SetValue(ResizeEnabledProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="FloatingWindow.ResizeEnabled" /> dependency property.
    /// </summary>
    /// <value>
    /// The identifier for the <see cref="FloatingWindow.ResizeEnabled" /> dependency property.
    /// </value>
    public static readonly DependencyProperty ResizeEnabledProperty =
        DependencyProperty.Register(
        "ResizeEnabled",
        typeof(bool),
        typeof(FloatingWindow),
        new PropertyMetadata(true, OnResizeEnabledChanged));

    /// <summary>
    /// ResizeEnabled PropertyChangedCallback call back static function.
    /// </summary>
    /// <param name="d">FloatingWindow object whose ResizeEnabled property is changed.</param>
    /// <param name="e">DependencyPropertyChangedEventArgs which contains the old and new values.</param>
    private static void OnResizeEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      #if BORDER_ONLY_AT_RESIZABLE
      FloatingWindow window = (FloatingWindow)d;
      if ((bool)e.NewValue)
        window.RestoreBorder();
      else
        window.HideBorder();
      #endif
    }

    #endregion

    #region ResizeMaximizedEnabled

    /// <summary>
    /// Gets or sets a value indicating whether resizing is enabled when window is maximized.
    /// </summary>
    /// <value><c>true</c> if resizing is enabled when window is maximized; otherwise, <c>false</c>.</value>
    public bool ResizeMaximizedEnabled
    {
      get { return (bool)GetValue(ResizeMaximizedEnabledProperty); }
      set { SetValue(ResizeMaximizedEnabledProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="FloatingWindow.ResizeMaximizedEnabled" /> dependency property.
    /// </summary>
    /// <value>
    /// The identifier for the <see cref="FloatingWindow.ResizeMaximizedEnabled" /> dependency property.
    /// </value>
    public static readonly DependencyProperty ResizeMaximizedEnabledProperty =
        DependencyProperty.Register(
        "ResizeMaximizedEnabled",
        typeof(bool),
        typeof(FloatingWindow),
        new PropertyMetadata(false));

    #endregion

    #region ResizingAreaThickness

    /// <summary>
    /// Gets or sets the width of the resizing area.
    /// </summary>
    /// <value>The width of the resizing area.</value>
    public double ResizingAreaThickness
    {
      get { return (double)GetValue(ResizingAreaThicknessProperty); }
      set { SetValue(ResizingAreaThicknessProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="FloatingWindow.ResizingAreaThickness" /> dependency property.
    /// </summary>
    /// <value>
    /// The identifier for the <see cref="FloatingWindow.ResizingAreaThickness" /> dependency property.
    /// </value>
    public static readonly DependencyProperty ResizingAreaThicknessProperty =
        DependencyProperty.Register(
        "ResizingAreaThickness",
        typeof(double),
        typeof(FloatingWindow),
        new PropertyMetadata(ResizingAreaDefaultValue, OnResizingAreaThicknessPropertyChanged));

    /// <summary>
    /// ResizingAreaThicknessProperty PropertyChangedCallback call back static function.
    /// </summary>
    /// <param name="d">FloatingWindow object whose ResizingAreaThickness property is changed.</param>
    /// <param name="e">DependencyPropertyChangedEventArgs which contains the old and new values.</param>
    private static void OnResizingAreaThicknessPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      FloatingWindow window = (FloatingWindow)d;
      window.resizeController.ResizingArea = (double)e.NewValue;
    }

    #endregion

    #region Position

    /// <summary>
    /// Gets or sets current window position.
    /// </summary>
    /// <value>Current position.</value>
    public Point Position
    {
      get { return (Point)GetValue(PositionProperty); }
      set { SetValue(PositionProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="FloatingWindow.Position" /> dependency property.
    /// </summary>
    /// <value>
    /// The identifier for the <see cref="FloatingWindow.Position" /> dependency property.
    /// </value>
    public static readonly DependencyProperty PositionProperty =
        DependencyProperty.Register(
        "Position",
        typeof(Point),
        typeof(FloatingWindow),
        new PropertyMetadata(new Point(double.NaN, double.NaN), OnPositionPropertyChanged));

    /// <summary>
    /// PositionProperty PropertyChangedCallback call back static function.
    /// </summary>
    /// <param name="d">FloatingWindow object whose Position property is changed.</param>
    /// <param name="e">DependencyPropertyChangedEventArgs which contains the old and new values.</param>
    private static void OnPositionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      FloatingWindow window = (FloatingWindow)d;

      if (window != null)
      {
        if (window.FloatingWindowHost != null && window.FloatingWindowHost.IsLayoutUpdated)
          window.MoveWindow((Point)e.NewValue);
      }
    }

    #endregion

    #region CenteredWindowPosition (read-only)

    /// <summary>
    /// Gets coordinates of the window placed in the center of its host.
    /// </summary>
    /// <value>The centered window position.</value>
    private Point CenteredWindowPosition
    {
      get
      {
        //return new Point((HostPanel.ActualWidth - ActualWidth.ValueOrZero()) / 2, (HostPanel.ActualHeight - ActualHeight.ValueOrZero()) / 2); //using ActualWidth/ActualHeight to cater for any applied ScaleTransform
        //return new Point((HostPanel.ActualWidth - Width * Scale) / 2, (HostPanel.ActualHeight - Height * Scale) / 2); //using ActualWidth/ActualHeight to cater for any applied ScaleTransform
        Point parentCenter = this.FloatingWindowHost.ViewCenter;
        return new Point(parentCenter.X - Width / 2 * Scale, parentCenter.Y - Height / 2 * Scale);
      } //TODO: need to have some flag on whether Window is allowed to be placed out of container bounds and if not use Max(...,0) to make negative values to 0
    }

    #endregion

    #region Scale

    /// <summary>
    /// Gets or sets current window scale.
    /// </summary>
    /// <value>Current scale.</value>
    public double Scale
    {
      get { return (double)GetValue(ScaleProperty); }
      set { SetValue(ScaleProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="FloatingWindow.Scale" /> dependency property.
    /// </summary>
    /// <value>
    /// The identifier for the <see cref="FloatingWindow.Scale" /> dependency property.
    /// </value>
    public static readonly DependencyProperty ScaleProperty =
        DependencyProperty.Register(
        "Scale",
        typeof(double),
        typeof(FloatingWindow),
        new PropertyMetadata(1d, OnScalePropertyChanged)); //Must use 1d here, not 1 (else will get XAMLParseException at runtime)

    /// <summary>
    /// ScaleProperty PropertyChangedCallback call back static function.
    /// </summary>
    /// <param name="d">FloatingWindow object whose Scale property is changed.</param>
    /// <param name="e">DependencyPropertyChangedEventArgs which contains the old and new values.</param>
    private static void OnScalePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      FloatingWindow window = (FloatingWindow)d;
      if (window != null)
      {
        window.RenderTransformOrigin = new Point(0.5, 0.5); //scale around the window center
        window.RenderTransform = new ScaleTransform().SetScale((double)e.NewValue); //TODO: this may cause the window top-left to get out of container bounds, maybe should coerce scale then, or autoexpand the container (when that is implemented)

        /* //NOTE: causes issue with video acceleration
        if (window.CacheMode != null) //this will work if GPU acceleration has been turned on (at HTML/ASPX page or at OOB settings for OOB apps), assuming we've set CacheMode already
        {
          BitmapCache bitmapCache = window.CacheMode as BitmapCache; //will return null if other type
          if (bitmapCache != null)
          {
            bitmapCache.RenderAtScale = 1; //window.Scale; //TODO: should change this to take in mind the container's scale (need to add ContentScale property to FloatingWindowHost and override at FloatingWindowHostZUI to set ContentScale of nested ZoomHost)
            window.CacheMode = bitmapCache;
          }
        }
        */

      }
    }

    #endregion

    #region ScaleEnabled

    /// <summary>
    /// Gets or sets a value indicating whether to allow the user to scale the window.
    /// </summary>
    /// <value><c>true</c> to allow the user to scale the window; otherwise, <c>false</c>.</value>
    public bool ScaleEnabled
    {
      get { return (bool)GetValue(ScaleEnabledProperty); }
      set { SetValue(ScaleEnabledProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="FloatingWindow.ScaleEnabled" /> dependency property.
    /// </summary>
    /// <value>
    /// The identifier for the <see cref="FloatingWindow.ScaleEnabled" /> dependency property.
    /// </value>
    public static readonly DependencyProperty ScaleEnabledProperty =
        DependencyProperty.Register(
        "ScaleEnabled",
        typeof(bool),
        typeof(FloatingWindow),
        new PropertyMetadata(true));

    #endregion

    #region ShowInIconbar

    /// <summary>
    /// Gets or sets a value indicating whether to show minimized window in the iconbar.
    /// </summary>
    /// <value><c>true</c> if to show minimized window in the iconbar; otherwise, <c>false</c>.</value>
    public bool ShowInIconbar
    {
      get { return (bool)GetValue(ShowInIconbarProperty); }
      set { SetValue(ShowInIconbarProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="FloatingWindow.ShowInIconbar" /> dependency property.
    /// </summary>
    /// <value>
    /// The identifier for the <see cref="FloatingWindow.ShowInIconbar" /> dependency property.
    /// </value>
    public static readonly DependencyProperty ShowInIconbarProperty =
        DependencyProperty.Register(
        "ShowInIconbar",
        typeof(bool),
        typeof(FloatingWindow),
        new PropertyMetadata(true));

    #endregion

    #region RTL

    /// <summary>
    /// RTL Dependency Property
    /// </summary>
    public static readonly DependencyProperty RTLProperty =
        DependencyProperty.Register("RTL", typeof(bool), typeof(FloatingWindow),
            new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnRTLChanged)));

    /// <summary>
    /// Gets or sets the RTL property.
    /// </summary>
    public bool RTL
    {
      get { return (bool)GetValue(RTLProperty); }
      set { SetValue(RTLProperty, value); }
    }

    /// <summary>
    /// Handles changes to the RTL property.
    /// </summary>
    private static void OnRTLChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      FloatingWindow target = (FloatingWindow)d;
      target.OnRTLChanged((bool)e.OldValue, target.RTL);
    }

    /// <summary>
    /// Provides derived classes an opportunity to handle changes to the IsAvailable property.
    /// </summary>
    protected virtual void OnRTLChanged(bool oldRTL, bool newRTL)
    {
      FlowDirection = (newRTL) ? System.Windows.FlowDirection.RightToLeft : System.Windows.FlowDirection.LeftToRight; //this will also flip titlebar and toolbar direction, scrollpanes etc.
    }

    #endregion

    /*  #region FlowDirection

        /// <summary>
        /// Gets or sets the direction that title text flows within window's icon.
        /// </summary>
        /// <value>A constant name from the FlowDirection enumeration, either LeftToRight or RightToLeft.</value>
        public FlowDirection FlowDirection
        {
            get { return (FlowDirection)GetValue(FlowDirectionProperty); }
            set { SetValue(FlowDirectionProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="FloatingWindow.FlowDirection" /> dependency property.
        /// </summary>
        /// <value>
        /// The identifier for the <see cref="FloatingWindow.FlowDirection" /> dependency property.
        /// </value>
        public static readonly DependencyProperty FlowDirectionProperty =
            DependencyProperty.Register(
            "FlowDirection",
            typeof(FlowDirection),
            typeof(FloatingWindow),
            new PropertyMetadata(FlowDirection.LeftToRight));

        #endregion
    */

    #region TitleStyle

    /// <summary>
    /// Gets or sets the style that is used when rendering the Title of the window.
    /// </summary>
    public Style TitleStyle
    {
      get { return GetValue(TitleStyleProperty) as Style; }
      set { SetValue(TitleStyleProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="FloatingWindow.TitleStyleProperty" /> dependency property.
    /// </summary>
    public static readonly DependencyProperty TitleStyleProperty =
        DependencyProperty.Register(
            "TitleStyle",
            typeof(Style),
            typeof(FloatingWindow),
            new PropertyMetadata(OnTitleStylePropertyChanged));

    /// <summary>
    /// TitleStyle PropertyChangedCallback call back static function.
    /// </summary>
    /// <param name="d">FloatingWindow object whose TitleStyle property is changed.</param>
    /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
    private static void OnTitleStylePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      FloatingWindow window = (FloatingWindow)d;
      if (window != null && window.titleContent != null)
      {
        Style style = e.NewValue as Style;
        window.titleContent.Style = style;
      }
    }

    #endregion

    #region ScreenshotButtonStyle

    /// <summary>
    /// Gets or sets the style of the Screenshot button.
    /// </summary>
    public Style ScreenshotButtonStyle
    {
      get { return GetValue(ScreenshotButtonStyleProperty) as Style; }
      set { SetValue(ScreenshotButtonStyleProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="FloatingWindow.ScreenshotButtonStyleProperty" /> dependency property.
    /// </summary>
    public static readonly DependencyProperty ScreenshotButtonStyleProperty =
        DependencyProperty.Register(
            "ScreenshotButtonStyle",
            typeof(Style),
            typeof(FloatingWindow),
            new PropertyMetadata(OnScreenshotButtonStylePropertyChanged));

    /// <summary>
    /// ScreenshotButtonStyle PropertyChangedCallback call back static function.
    /// </summary>
    /// <param name="d">FloatingWindow object whose ScreenshotButtonStyle property is changed.</param>
    /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
    private static void OnScreenshotButtonStylePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var window = (FloatingWindow)d;
      if (window != null)
      {
        Style style = e.NewValue as Style;
        window.ScreenshotButtonStyle = style;

        if (window.screenshotButton != null)
          window.screenshotButton.Style = style;
      }
    }

    #endregion

    #region HelpButtonStyle

    /// <summary>
    /// Gets or sets the style of the Help button.
    /// </summary>
    public Style HelpButtonStyle
    {
      get { return GetValue(HelpButtonStyleProperty) as Style; }
      set { SetValue(HelpButtonStyleProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="FloatingWindow.HelpButtonStyleProperty" /> dependency property.
    /// </summary>
    public static readonly DependencyProperty HelpButtonStyleProperty =
        DependencyProperty.Register(
            "HelpButtonStyle",
            typeof(Style),
            typeof(FloatingWindow),
            new PropertyMetadata(OnHelpButtonStylePropertyChanged));

    /// <summary>
    /// HelpButtonStyle PropertyChangedCallback call back static function.
    /// </summary>
    /// <param name="d">FloatingWindow object whose HelpButtonStyle property is changed.</param>
    /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
    private static void OnHelpButtonStylePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var window = (FloatingWindow)d;
      if (window != null)
      {
        Style style = e.NewValue as Style;
        window.HelpButtonStyle = style;

        if (window.helpButton != null)
          window.helpButton.Style = style;
      }
    }

    #endregion

    #region OptionsButtonStyle

    /// <summary>
    /// Gets or sets the style of the Options button.
    /// </summary>
    public Style OptionsButtonStyle
    {
      get { return GetValue(OptionsButtonStyleProperty) as Style; }
      set { SetValue(OptionsButtonStyleProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="FloatingWindow.OptionsButtonStyleProperty" /> dependency property.
    /// </summary>
    public static readonly DependencyProperty OptionsButtonStyleProperty =
        DependencyProperty.Register(
            "OptionsButtonStyle",
            typeof(Style),
            typeof(FloatingWindow),
            new PropertyMetadata(OnOptionsButtonStylePropertyChanged));

    /// <summary>
    /// OptionsButtonStyle PropertyChangedCallback call back static function.
    /// </summary>
    /// <param name="d">FloatingWindow object whose OptionsButtonStyle property is changed.</param>
    /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
    private static void OnOptionsButtonStylePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var window = (FloatingWindow)d;
      if (window != null)
      {
        Style style = e.NewValue as Style;
        window.OptionsButtonStyle = style;

        if (window.optionsButton != null)
          window.optionsButton.Style = style;
      }
    }

    #endregion

    #region CloseButtonStyle

    /// <summary>
    /// Gets or sets the style of the Close button.
    /// </summary>
    public Style CloseButtonStyle
    {
      get { return GetValue(CloseButtonStyleProperty) as Style; }
      set { SetValue(CloseButtonStyleProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="FloatingWindow.CloseButtonStyleProperty" /> dependency property.
    /// </summary>
    public static readonly DependencyProperty CloseButtonStyleProperty =
        DependencyProperty.Register(
            "CloseButtonStyle",
            typeof(Style),
            typeof(FloatingWindow),
            new PropertyMetadata(OnCloseButtonStylePropertyChanged));

    /// <summary>
    /// CloseButtonStyle PropertyChangedCallback call back static function.
    /// </summary>
    /// <param name="d">FloatingWindow object whose CloseButtonStyle property is changed.</param>
    /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
    private static void OnCloseButtonStylePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var window = (FloatingWindow)d;
      if (window != null)
      {
        Style style = e.NewValue as Style;
        window.CloseButtonStyle = style;

        if (window.closeButton != null)
          window.closeButton.Style = style;
      }
    }

    #endregion

    #region MinimizeButtonStyle

    /// <summary>
    /// Gets or sets the style of the Minimize button.
    /// </summary>
    public Style MinimizeButtonStyle
    {
      get { return GetValue(MinimizeButtonStyleProperty) as Style; }
      set { SetValue(MinimizeButtonStyleProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="FloatingWindow.MinimizeButtonStyleProperty" /> dependency property.
    /// </summary>
    public static readonly DependencyProperty MinimizeButtonStyleProperty =
        DependencyProperty.Register(
            "MinimizeButtonStyle",
            typeof(Style),
            typeof(FloatingWindow),
            new PropertyMetadata(OnMinimizeButtonStylePropertyChanged));

    /// <summary>
    /// MinimizeButtonStyle PropertyChangedCallback call back static function.
    /// </summary>
    /// <param name="d">FloatingWindow object whose MinimizeButtonStyle property is changed.</param>
    /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
    private static void OnMinimizeButtonStylePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var window = (FloatingWindow)d;
      if (window != null)
      {
        Style style = e.NewValue as Style;
        window.MinimizeButtonStyle = style;

        if (window.minimizeButton != null)
          window.minimizeButton.Style = style;
      }
    }

    #endregion

    #region MaximizeButtonStyle

    /// <summary>
    /// Gets or sets the style of the Maximize button.
    /// </summary>
    public Style MaximizeButtonStyle
    {
      get { return GetValue(MaximizeButtonStyleProperty) as Style; }
      set { SetValue(MaximizeButtonStyleProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="FloatingWindow.MaximizeButtonStyleProperty" /> dependency property.
    /// </summary>
    public static readonly DependencyProperty MaximizeButtonStyleProperty =
        DependencyProperty.Register(
            "MaximizeButtonStyle",
            typeof(Style),
            typeof(FloatingWindow),
            new PropertyMetadata(OnMaximizeButtonStylePropertyChanged));

    /// <summary>
    /// MaximizeButtonStyle PropertyChangedCallback call back static function.
    /// </summary>
    /// <param name="d">FloatingWindow object whose MaximizeButtonStyle property is changed.</param>
    /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
    private static void OnMaximizeButtonStylePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var window = (FloatingWindow)d;
      if (window != null)
      {
        Style style = e.NewValue as Style;
        window.MaximizeButtonStyle = style;

        if (window.maximizeButton != null)
          window.maximizeButton.Style = style;
      }
    }

    #endregion

    #region RestoreButtonStyle

    /// <summary>
    /// Gets or sets the style of the Restore button.
    /// </summary>
    public Style RestoreButtonStyle
    {
      get { return GetValue(RestoreButtonStyleProperty) as Style; }
      set { SetValue(RestoreButtonStyleProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="FloatingWindow.RestoreButtonStyleProperty" /> dependency property.
    /// </summary>
    public static readonly DependencyProperty RestoreButtonStyleProperty =
        DependencyProperty.Register(
            "RestoreButtonStyle",
            typeof(Style),
            typeof(FloatingWindow),
            new PropertyMetadata(OnRestoreButtonStylePropertyChanged));

    /// <summary>
    /// RestoreButtonStyle PropertyChangedCallback call back static function.
    /// </summary>
    /// <param name="d">FloatingWindow object whose RestoreButtonStyle property is changed.</param>
    /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
    private static void OnRestoreButtonStylePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var window = (FloatingWindow)d;
      if (window != null)
      {
        Style style = e.NewValue as Style;
        window.RestoreButtonStyle = style;

        if (window.restoreButton != null)
          window.restoreButton.Style = style;
      }
    }

    #endregion

    #endregion

    #region --- Methods ---

    /// <summary>
    /// Creates a thumbnail of the window.
    /// </summary>
    /// <returns>Bitmap containing thumbnail image.</returns>
    public ImageSource GetThumbnailImage()
    {
      // If an Icon is an image - use it as a thumbnail displayed on the iconbar
      if (Icon != null)
        return (Icon is Image) ? (Icon as Image).Source : null;

      // If other kind of Icon is specified - use it as a thumbnail displayed on the iconbar
      // Otherwise, display the window itself
      FrameworkElement icon = (Icon as FrameworkElement) ?? contentRoot;
      ImageSource bitmap = bitmapHelper.RenderVisual(icon, FloatingWindowHost.IconWidth, FloatingWindowHost.IconHeight);

      return bitmap;
    }

    public ImageSource GetImage()
    {
      return bitmapHelper.RenderVisual(contentRoot, contentRoot.Width, contentRoot.Height);
    }

    #region Show

    /// <summary>
    /// Shows the window as a modal one.
    /// </summary>
    public void ShowModal()
    {
      IsModal = true;
      Position = new Point(double.NaN, double.NaN);
      ShowMaximizeRestoreButton = false;
      ShowMinimizeButton = false;

      Show();
    }

    /// <summary>
    /// Opens the <see cref="FloatingWindow" /> in previously saved position or
    /// in the center of the <see cref="FloatingWindowHost" />.
    /// </summary>
    public void Show(bool bringToFront = true)
    {
      Show(Position, centered: false, bringToFront: bringToFront);
    }

    /// <summary>
    /// Shows the window in the specified coordinates, relative to the window's Host.
    /// </summary>
    /// <param name="x">X-coordinate.</param>
    /// <param name="y">Y-coordinate.</param>
    /// <param name="centered">Center around point</param>
    public void Show(double x, double y, bool centered = false)
    {
      Show(new Point(x, y), centered);
    }

    /// <summary>
    /// Shows a <see cref="FloatingWindow"/> at maximal size taking into account
    /// specified margins and current <see cref="FloatingWindowHost"/> size.
    /// </summary>
    /// <param name="margins">Window margins.</param>
    public void Show(Thickness margins)
    {
      CheckHost();
      Action<Thickness> action = new Action<Thickness>(ShowWindow);
      this.FloatingWindowHost.ShowWindow(action, margins);
    }

    /// <summary>
    /// Shows the window in the specified coordinates, relative to the window's Host.
    /// </summary>
    /// <param name="point">Coordinates of the upper-left corner of the window.</param>
    /// <param name="centered">Center around point</param>
    public void Show(Point point, bool centered = false, bool bringToFront = true)
    {
      CheckHost();
      Action<Point, bool> action = new Action<Point, bool>(ShowWindow);
      this.FloatingWindowHost.ShowWindow(action, (centered) ? new Point(point.X - Width / 2 * Scale, point.Y - Height / 2 * Scale) : point, bringToFront);
    }

    /// <summary>
    /// Shows a <see cref="FloatingWindow"/> at maximal size taking into account
    /// specified margins and current <see cref="FloatingWindowHost"/> size.
    /// </summary>
    /// <param name="margins">Window margins.</param>
    private void ShowWindow(Thickness margins)
    {
      Width = Math.Max(MinWidth, HostPanel.ActualWidth - margins.Horizontal());
      Height = Math.Max(MinHeight, HostPanel.ActualHeight - margins.Vertical());

      ShowWindow(new Point(margins.Left, margins.Top));
    }

    /// <summary>
    /// Shows the window in the specified coordinates, relative to the window's Host.
    /// </summary>
    /// <param name="point">Coordinates of the upper-left corner of the window.</param>
    /// <exception cref="System.InvalidOperationException">"The FloatingWindow was not added to the host.</exception>
    private void ShowWindow(Point point, bool bringToFront = true)
    {
      if (IsOpen)
      {
        MoveWindow(point);
        this.SetVisible(true);
        return;
      }

      if (IsModal)
        this.FloatingWindowHost.ShowWindowAsModal(this);

      SubscribeToEvents();

      //SubscribeToTemplatePartEvents(); //this is done at "OnApplyTemplate", called below via ApplyTemplate (and may have been done already if templateIsApplied)
      //SubscribeToStoryBoardEvents();   //this is done at "OnApplyTemplate", called below via ApplyTemplate (and may have been done already if templateIsApplied)

      // Guarantee that the visual tree of an element is complete
      if (!templateIsApplied)
        ApplyTemplate();

      // Brings current window to the front
      if (bringToFront)
        SetTopmost();

      Point position = (point.IsNotSet()) ? CenteredWindowPosition : point;

      MoveWindow(position);
      this.SetVisible(true);

      // If window size is not set explicitly we should wait
      // when the window layout is updated and update its position
      if (!IsWindowSizeSet && point.IsNotSet() & (contentRoot != null))
        contentRoot.SizeChanged += new SizeChangedEventHandler(ContentRoot_SizeChanged);

      VisualStateManager.GoToState(this, VSMSTATE_StateOpen, true);
      IsOpen = true;
    }

    #endregion

    #region Minimize / Restore

    /// <summary>
    /// Minimizes the window.
    /// </summary>
    public void MinimizeWindow()
    {
      if (windowState != WindowState.Minimized)
      {
        if (minimizeButton != null)
          VisualStateManager.GoToState(minimizeButton, VSMSTATE_StateNormal, true);

        if (windowState == WindowState.Normal)
        {
          // Store previous coordinates
          previousPosition = Position;
          previousSize = new Size(ActualWidth, ActualHeight);
        }

        minimizedWindowThumbnail = GetThumbnailImage();

        previousWindowState = windowState;
        VisualStateManager.GoToState(this, VSMSTATE_StateMinimized, true);
        OnMinimized(EventArgs.Empty);
      }

      windowState = WindowState.Minimized;
      OnDeactivated(EventArgs.Empty);

      this.FloatingWindowHost.ActivateTopmostWindow();
    }

    /// <summary>
    /// Restores window state, size and its position.
    /// </summary>
    public void RestoreWindow()
    {
      switch (windowState)
      {
        case WindowState.Minimized:
          if (previousWindowState == WindowState.Maximized)
            ResizeToView();

          SetTopmost();
          windowState = previousWindowState;
          VisualStateManager.GoToState(this, VSMSTATE_StateRestored, true);
          OnRestored(EventArgs.Empty);
          break;

        case WindowState.Normal:
          SetTopmost();
          EnsureVisible();
          break;

        case WindowState.Maximized:
          SetTopmost();
          break;
      }

      Focus();
    }

    #endregion

    #region Maximize

    /// <summary>
    /// Resizing the window to current view bounds without setting it to maximized mode
    /// </summary
    public void ResizeToView()
    {
      Rect r = BoundingRectangleMaximized; //note: was FloatingWindowHost.MaximizedWindowBounds
      Position = r.Position(); //there is no r.Location in Silverlight
      Width = r.Width;
      Height = r.Height;
    }

    /// <summary>
    /// Maximizes the window.
    /// </summary>
    public void MaximizeWindow()
    {
      if (windowState != WindowState.Maximized)
      {
        if (maximizeButton != null && restoreButton != null && HostPanel != null)
        {
          if (this.ShowMaximizeRestoreButton)
          {
            maximizeButton.SetVisible(false);
            restoreButton.SetVisible(true);
          }

          VisualStateManager.GoToState(restoreButton, VSMSTATE_StateNormal, true);

          // Store previous coordinates
          previousPosition = Position;
          previousSize = new Size(ActualWidth, ActualHeight);

          HideBorder();

          SetTopmost(); //!!! this seems to be needed, else after maximize action, windows don't overlap correctly

          StartMaximizingAnimation();
        }

        previousWindowState = windowState;
        windowState = WindowState.Maximized;
      }
    }

    #endregion

    /// <summary>
    /// Makes the window topmost and tries to set focus on it.
    /// </summary>
    public void Activate()
    {
      SetTopmost();
    }

    /// <summary>
    /// Brings current window to the front.
    /// </summary>
    private void SetTopmost()
    {
      if (this.FloatingWindowHost != null)
        this.FloatingWindowHost.SetTopmostWindow(this);
    }

    /// <summary>
    /// Ensures the window is visible.
    /// </summary>
    private void EnsureVisible()
    {
      if (HostPanel != null && (Position.X >= HostPanel.ActualWidth || Position.Y >= HostPanel.ActualHeight))
      {
        Position = CenteredWindowPosition;
      }
    }

    /// <summary>
    /// Closes a <see cref="FloatingWindow" />.
    /// </summary>
    public void Close()
    {
      SaveSizeAndPosition();

      CancelEventArgs e = new CancelEventArgs();
      OnClosing(e);

      // On ApplicationExit, Close() cannot be canceled
      if (IsOpen && (!e.Cancel || isAppExit))
      {
        IsOpen = false;

        if (IsModal)
          this.FloatingWindowHost.RemoveOverlay();

        if (closingStoryboard != null)
        {
          VisualStateManager.GoToState(this, VSMSTATE_StateClosed, true);
        }
        else
        {
          this.Visibility = Visibility.Collapsed;
          OnClosed(EventArgs.Empty);
        }

        UnSubscribeFromEvents();
        UnsubscribeFromTemplatePartEvents();
      }
    }

    /// <summary>
    /// Performs Screenshot action.
    /// </summary>
    public virtual void Screenshot()
    {
      OnScreenshot(EventArgs.Empty);
    }

    /// <summary>
    /// Performs Help action.
    /// </summary>
    public virtual void Help()
    {
      OnHelp(EventArgs.Empty);
    }

    /// <summary>
    /// Performs Options action.
    /// </summary>
    public virtual void Options()
    {
      OnOptions(EventArgs.Empty);
    }

    #endregion

    #region --- Events ---

    /// <summary>
    /// Occurs when the <see cref="FloatingWindow" /> is activated.
    /// </summary>
    /// <remarks>Not visible <see cref="FloatingWindow" /> cannot be the active. </remarks>
    public event EventHandler Activated;

    /// <summary>
    /// Occurs when the <see cref="FloatingWindow" /> is deactivated.
    /// </summary>
    public event EventHandler Deactivated;

    /// <summary>
    /// Occurs when screenshot is requested.
    /// </summary>
    public event EventHandler ScreenshotRequested;

    /// <summary>
    /// Occurs when help is requested.
    /// </summary>
    public event EventHandler HelpRequested;

    /// <summary>
    /// Occurs when options are requested.
    /// </summary>
    public event EventHandler OptionsRequested;

    /// <summary>
    /// Occurs when the <see cref="FloatingWindow" /> is closed.
    /// </summary>
    public event EventHandler Closed;

    /// <summary>
    /// Occurs when the <see cref="FloatingWindow" /> is maximized.
    /// </summary>
    public event EventHandler Maximized;

    /// <summary>
    /// Occurs when the <see cref="FloatingWindow" /> is minimized.
    /// </summary>
    public event EventHandler Minimized;

    /// <summary>
    /// Occurs when the <see cref="FloatingWindow" /> is restored.
    /// </summary>
    public event EventHandler Restored;

    /// <summary>
    /// Occurs when the <see cref="FloatingWindow" /> is closing.
    /// </summary>
    public event EventHandler<CancelEventArgs> Closing;

    #endregion

    #region Event Handlers

    /// <summary>
    /// Handles the SizeChanged event of the ContentRoot control to update window position
    /// only once when the window is opened.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.Windows.SizeChangedEventArgs"/> instance containing the event data.</param>
    private void ContentRoot_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      contentRoot.SizeChanged -= new SizeChangedEventHandler(ContentRoot_SizeChanged);
      double dx = -(e.NewSize.Width.ValueOrZero() - e.PreviousSize.Width.ValueOrZero()) / 2;
      double dy = -(e.NewSize.Height.ValueOrZero() - e.PreviousSize.Height.ValueOrZero()) / 2;
      Point point = Position.Add(dx, dy);
      MoveWindow(point);
    }

    /// <summary>
    /// Executed when the application is exited.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">Event args.</param>
    private void Application_Exit(object sender, EventArgs e)
    {
      isAppExit = true; //expecting FloatingWindowHost to close its hosted FloatingWindows
      //Do not call Close here explicitly, since then Close handlers of FloatingWindows may get called at any order by the App which will cause problems if the FloatingWindowHost is also hosted in a FloatingWindow (nesting)
    }

    /// <summary>
    /// Executed when the Screenshot button is clicked.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="e">Routed event args.</param>
    private void ScreenshotButton_Click(object sender, RoutedEventArgs e)
    {
      Screenshot();
    }

    /// <summary>
    /// Executed when the Help button is clicked.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="e">Routed event args.</param>
    private void HelpButton_Click(object sender, RoutedEventArgs e)
    {
      Help();
    }

    /// <summary>
    /// Executed when the Options button is clicked.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="e">Routed event args.</param>
    private void OptionsButton_Click(object sender, RoutedEventArgs e)
    {
      Options();
    }

    /// <summary>
    /// Executed when the Close button is clicked.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="e">Routed event args.</param>
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
      Close();
    }

    /// <summary>
    /// Executed when the Closing storyboard ends.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="e">Event args.</param>
    private void Closing_Completed(object sender, EventArgs e)
    {
      if (closingStoryboard != null)
        closingStoryboard.Completed -= new EventHandler(Closing_Completed);

      this.Visibility = Visibility.Collapsed;
      OnClosed(EventArgs.Empty);
    }

    /// <summary>
    /// Raises the <see cref="FloatingWindow.Activated" /> event.
    /// </summary>
    /// <param name="e">The event data.</param>
    protected virtual void OnActivated(EventArgs e)
    {
      EventHandler handler = Activated;
      if (handler != null)
        handler(this, e);
    }

    /// <summary>
    /// Raises the <see cref="FloatingWindow.Deactivated" /> event.
    /// </summary>
    /// <param name="e">The event data.</param>
    protected virtual void OnDeactivated(EventArgs e)
    {
      EventHandler handler = Deactivated;
      if (handler != null)
        handler(this, e);
    }

    /// <summary>
    /// Raises the <see cref="FloatingWindow.ScreenshotRequested" /> event.
    /// </summary>
    /// <param name="e">The event data.</param>
    protected virtual void OnScreenshot(EventArgs e)
    {
      EventHandler handler = ScreenshotRequested;
      if (handler != null)
        handler(this, e);

      SaveFileDialog dlg = new SaveFileDialog()
      {
        //DefaultFileName = ( (Title == null) || String.IsNullOrWhiteSpace(Title.ToString()) )? "" : Title + ".jpg", //not using this, because Silverlight would ask "Do you want to save..."
        Filter = "JPEG image (*.jpg)|*.jpg"
      };

      if (dlg.ShowDialog() == true)
        using (Stream stream = dlg.OpenFile())
          bitmapHelper.SaveToJPEG((WriteableBitmap)GetImage(), stream); //assuming ImageSource returned by GetImage is a WriteableBitmap in both WPF and Silverlight (and not just a RenderTargetBitmap in WPF)
    }

    /// <summary>
    /// Raises the <see cref="FloatingWindow.HelpRequested" /> event.
    /// </summary>
    /// <param name="e">The event data.</param>
    protected virtual void OnHelp(EventArgs e)
    {
      EventHandler handler = HelpRequested;
      if (handler != null)
        handler(this, e);
    }

    /// <summary>
    /// Raises the <see cref="FloatingWindow.OptionsRequested" /> event.
    /// </summary>
    /// <param name="e">The event data.</param>
    protected virtual void OnOptions(EventArgs e)
    {
      EventHandler handler = OptionsRequested;
      if (handler != null)
        handler(this, e);
    }

    /// <summary>
    /// Raises the <see cref="FloatingWindow.Closed" /> event.
    /// </summary>
    /// <param name="e">The event data.</param>
    protected virtual void OnClosed(EventArgs e)
    {
      this.FloatingWindowHost.Remove(this);
      this.FloatingWindowHost.ActivateTopmostWindow();
      UnsubscribeFromStoryBoardEvents();

      EventHandler handler = Closed;
      if (handler != null)
        handler(this, e);

      Dispose();
    }

    /// <summary>
    /// Raises the <see cref="FloatingWindow.Closing" /> event.
    /// </summary>
    /// <param name="e">The event data.</param>
    protected virtual void OnClosing(CancelEventArgs e)
    {
      EventHandler<CancelEventArgs> handler = Closing;
      if (handler != null)
        handler(this, e);
    }

    /// <summary>
    /// Raises the <see cref="FloatingWindow.Maximized" /> event.
    /// </summary>
    /// <param name="e">The event data.</param>
    protected virtual void OnMaximized(EventArgs e)
    {
      EventHandler handler = Maximized;
      if (handler != null)
        handler(this, e);
    }

    /// <summary>
    /// Raises the <see cref="FloatingWindow.Minimized" /> event.
    /// </summary>
    /// <param name="e">The event data.</param>
    protected virtual void OnMinimized(EventArgs e)
    {
      EventHandler handler = Minimized;
      if (handler != null)
        handler(this, e);
    }

    /// <summary>
    /// Raises the <see cref="FloatingWindow.Restored" /> event.
    /// </summary>
    /// <param name="e">The event data.</param>
    protected virtual void OnRestored(EventArgs e)
    {
      EventHandler handler = Restored;
      if (handler != null)
        handler(this, e);
    }

    /// <summary>
    /// This method is called every time a <see cref="FloatingWindow" /> is displayed.
    /// </summary>
    protected virtual void OnOpened()
    {
      if (!Focus())
      {
        // If the Focus() fails it means there is no focusable element in the window.
        // In this case we set IsTabStop to true to have the keyboard functionality
        IsTabStop = true;
        Focus();
      }

      #if !SILVERLIGHT
      // Set focus to the first focusable element in the window //TODO: is this necessary? Doesn't it go automatically to the first focusable element since the window itself isn't a tabstop by default?
      TraversalRequest request = new TraversalRequest(FocusNavigationDirection.First);
      this.MoveFocus(request);
      #endif
    }

    /// <summary>
    /// Executed when the opening storyboard finishes.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="e">Event args.</param>
    private void Opening_Completed(object sender, EventArgs e)
    {
      if (openingStoryboard != null)
        openingStoryboard.Completed -= new EventHandler(Opening_Completed);

      this.FloatingWindowHost.UpdateIconbar();
      IsOpen = true;
      OnOpened();
    }

    /// <summary>
    /// Subscribes to events when the window is opened.
    /// </summary>
    private void SubscribeToEvents()
    {
      if (Application.Current != null)
        Application.Current.Exit += Application_Exit;

      if (this.FloatingWindowHost != null)
      {
        this.FloatingWindowHost.SizeChanged += new SizeChangedEventHandler(Host_SizeChanged);
        this.FloatingWindowHost.ActiveWindowChanged += new EventHandler<ActiveWindowChangedEventArgs>(ActiveWindowChanged);
      }

      // Attach Mouse event handler to catch already handled events to bring the window to the front
      this.AddHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(FloatingWindow_MouseLeftButtonDown), true);
    }

    /// <summary>
    /// Handles the MouseLeftButtonDown event to bring the window to the front.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
    private void FloatingWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      // Gets the element with keyboard focus (must do before calling "SetTopmost")
      Control elementWithFocus =
        #if SILVERLIGHT
        System.Windows.Input.FocusManager.GetFocusedElement() as Control;
        #else
        Keyboard.FocusedElement as Control;
        #endif

      // Brings current window to the front
      SetTopmost(); //TODO: add property AutoBringToFront (default true) to select whether we want it to be brought to front automatically

      if (elementWithFocus != null)
      {
        #if SILVERLIGHT
        if (IsControlInVisualTree(elementWithFocus)) //TODO: maybe use WPF code for Silverlight too here?
        #else
        if ((this as DependencyObject).IsVisualAncestorOf(elementWithFocus))
        #endif
          elementWithFocus.Focus();
        else // Try to set focus on the window
          Focus();
      }

      // Stop any inertial motion
      StopInertialMotion();

      //FloatingWindowHost.IsIconbarVisible = false;
    }

    /// <summary>
    /// Unsubscribe from events when the ChildWindow is closed.
    /// </summary>
    private void UnSubscribeFromEvents()
    {
      if (Application.Current != null)
        Application.Current.Exit -= Application_Exit;

      if (this.FloatingWindowHost != null)
      {
        this.FloatingWindowHost.SizeChanged -= new SizeChangedEventHandler(Host_SizeChanged);
        this.FloatingWindowHost.ActiveWindowChanged -= new EventHandler<ActiveWindowChangedEventArgs>(ActiveWindowChanged);
      }

      this.RemoveHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(FloatingWindow_MouseLeftButtonDown));
    }

    /// <summary>
    /// Handles the ActiveWindowChanged event of the Host control.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="SilverFlow.Controls.ActiveWindowChangedEventArgs"/> instance containing the event data.</param>
    private void ActiveWindowChanged(object sender, ActiveWindowChangedEventArgs e)
    {
      if (e.Old == this)
        OnDeactivated(EventArgs.Empty);

      if (e.New == this)
        OnActivated(EventArgs.Empty);
    }

    /// <summary>
    /// Handles the Click event of the MaximizeButton control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
      MaximizeWindow();
    }

    /// <summary>
    /// Handles the Click event of the RestoreButton control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    private void RestoreButton_Click(object sender, RoutedEventArgs e)
    {
      RestoreMaximizedWindow();
    }

    /// <summary>
    /// Handles the Click event of the MinimizeButton control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
      MinimizeWindow();
    }

    /// <summary>
    /// Updates clipping region on host SizeChanged event.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.Windows.SizeChangedEventArgs"/> instance containing the event data.</param>
    private void Host_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      if (windowState == WindowState.Maximized)
        ResizeToView();
    } //TODO: check what was the original implementation here in both WPF and Silverlight - don't see anything related to clipping set here

    /// <summary>
    /// Executed when mouse left button is down.
    /// </summary>
    /// <param name="e">The data for the event.</param>
    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
      base.OnMouseLeftButtonDown(e);

      //if (e.Handled) return;
      e.Handled = true; //always handle events so that container doesn't get confused (e.g. when dragging the window title bar we don't want container to pan)

      windowAction = WindowAction.None;

      // Stop inertial motion before the mouse is captured
      StopInertialMotion();

      clickPoint = e.GetPosition(HostPanel);
      clickWindowPosition = Position;
      snapinController.SnapinDistance = this.FloatingWindowHost.SnapinDistance;
      snapinController.SnapinMargin = this.FloatingWindowHost.SnapinMargin;
      snapinController.SnapinEnabled = this.FloatingWindowHost.SnapinEnabled;

      if (CheckResizable() && resizeController.CanResize)
      {
        snapinController.SnapinBounds = this.FloatingWindowHost.GetSnapinBounds(this);
        resizeController.StartResizing();
        CaptureMouseCursor();
        windowAction = WindowAction.Resize;
      }
      else if (MoveMaximizedEnabled || (WindowState != WindowState.Maximized))
      {
        if (chrome != null)
        {
          // If the mouse was clicked on the chrome - start dragging the window
          Point point = e.GetPosition(chrome);

          if (MoveEnabled && chrome.ContainsPoint(point))
            StartInertialMotion();
        }

        if (contentBorder != null)
        {
          // If the mouse was clicked on the contentBorder - start dragging the window
          Point point = e.GetPosition(contentBorder);

          if (MoveEnabled && contentBorder.ContainsPoint(point))
            StartInertialMotion();
        }

      }
    }

    /// <summary>
    /// Executed when mouse left button is up.
    /// </summary>
    /// <param name="e">The data for the event.</param>
    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
      base.OnMouseLeftButtonUp(e);

      e.Handled = true; //always handle events so that container doesn't get confused (e.g. when dragging the window title bar we don't want container to pan)

      if (windowAction == WindowAction.Move)
      {
        InertialMotion motion = inertiaController.GetInertialMotionParameters(
            this.FloatingWindowHost.HostPanel.GetActualBoundingRectangle(), this.BoundingRectangle);

        if (motion != null)
        {
          windowAction = WindowAction.InertialMotion;
          contentRoot.AnimateTranslateTransform(inertialMotionStoryboard, motion.EndPosition, motion.Seconds, motion.EasingFunction);
        }
      }

      if (isMouseCaptured)
      {
        contentRoot.ReleaseMouseCapture();
        isMouseCaptured = false;
      }

      if (windowAction != WindowAction.InertialMotion)
        windowAction = WindowAction.None;
    }

    /// <summary>
    /// Executed when mouse moves.
    /// </summary>
    /// <param name="e">The data for the event.</param>
    protected override void OnMouseMove(MouseEventArgs e)
    {
      base.OnMouseMove(e);

      if (CheckResizable() && windowAction == WindowAction.None)
      {
        Point mousePosition = e.GetPosition(contentRoot);

        if (IsMouseOverButtons(mousePosition, contentRoot))
          this.Cursor = Cursors.Arrow;
        else
          resizeController.SetCursor(mousePosition);
      }

      Point p = e.GetPosition(HostPanel);
      double dx = p.X - clickPoint.X;
      double dy = p.Y - clickPoint.Y;

      if (windowAction == WindowAction.Resize)
        resizeController.Resize(dx, dy);

      else if (windowAction == WindowAction.Move)
      {
        double flow = (RTL) ? -1 : 1;
        Point point = clickWindowPosition.Add(flow * dx, dy); //RTL only affects horizontal flow
        Rect rect = new Rect(point.X, point.Y, ActualWidth, ActualHeight);

        point = snapinController.SnapRectangle(rect);
        MoveWindow(point);

        //inertiaController.MoveToPoint(point);
      }
    }

    #endregion

    #region Templating

    /// <summary>
    /// Builds the visual tree for the <see cref="FloatingWindow" /> control
    /// when a new template is applied.
    /// </summary>
    public override void OnApplyTemplate()
    {
      UnsubscribeFromTemplatePartEvents();
      UnsubscribeFromStoryBoardEvents();

      base.OnApplyTemplate();

      root = GetTemplateChild(PART_Root) as FrameworkElement;
      contentRoot = GetTemplateChild(PART_ContentRoot) as FrameworkElement;
      contentBorder = GetTemplateChild(PART_ContentBorder) as Border;
      chrome = GetTemplateChild(PART_Chrome) as FrameworkElement;
      titleContent = GetTemplateChild(PART_TitleContent) as ContentControl;
      contentPresenter = GetTemplateChild(PART_ContentPresenter) as FrameworkElement;
      screenshotButton = GetTemplateChild(PART_ScreenshotButton) as ButtonBase;
      helpButton = GetTemplateChild(PART_HelpButton) as ButtonBase;
      optionsButton = GetTemplateChild(PART_OptionsButton) as ButtonBase;
      closeButton = GetTemplateChild(PART_CloseButton) as ButtonBase;
      maximizeButton = GetTemplateChild(PART_MaximizeButton) as ButtonBase;
      minimizeButton = GetTemplateChild(PART_MinimizeButton) as ButtonBase;
      restoreButton = GetTemplateChild(PART_RestoreButton) as ButtonBase;

      if (root == null)
        throw new NotImplementedException("Template Part PART_Root is required to display FloatingWindow.");

      if (contentRoot == null)
        throw new NotImplementedException("Template Part PART_ContentRoot is required to display FloatingWindow.");

      if (contentPresenter == null)
        throw new NotImplementedException("Template Part PART_ContentPresenter is required to display FloatingWindow.");

      SetStyles();
      GetStoryboards();
      SetInitialRootPosition();
      InitializeContentRootTransformGroup();

      //TODO: the following if...SetVisible clauses could be removed if databinding with BooleanToVisibilityConverter in XAML are used
      if (chrome != null)
        chrome.SetVisible(ShowChrome);
      if (screenshotButton != null)
        screenshotButton.SetVisible(ShowScreenshotButton);
      if (helpButton != null)
        helpButton.SetVisible(ShowHelpButton);
      if (optionsButton != null)
        optionsButton.SetVisible(ShowOptionsButton);
      if (closeButton != null)
        closeButton.SetVisible(ShowCloseButton);
      if (minimizeButton != null)
        minimizeButton.SetVisible(ShowMinimizeButton);
      if (maximizeButton != null)
        maximizeButton.SetVisible(ShowMaximizeRestoreButton);

      SubscribeToTemplatePartEvents();
      SubscribeToStoryBoardEvents();

      templateIsApplied = true;
    }

    /// <summary>
    /// Sets styles that are applied for different template parts.
    /// </summary>
    private void SetStyles()
    {
      if (titleContent != null && this.TitleStyle != null)
        titleContent.Style = this.TitleStyle;

      if (minimizeButton != null && this.MinimizeButtonStyle != null)
        minimizeButton.Style = this.MinimizeButtonStyle;

      if (maximizeButton != null && this.MaximizeButtonStyle != null)
        maximizeButton.Style = this.MaximizeButtonStyle;

      if (restoreButton != null && this.RestoreButtonStyle != null)
        restoreButton.Style = this.RestoreButtonStyle;

      if (closeButton != null && this.CloseButtonStyle != null)
        closeButton.Style = this.CloseButtonStyle;

      if (screenshotButton != null && this.ScreenshotButtonStyle != null)
        screenshotButton.Style = this.ScreenshotButtonStyle;

      if (helpButton != null && this.HelpButtonStyle != null)
        helpButton.Style = this.HelpButtonStyle;

      if (optionsButton != null && this.OptionsButtonStyle != null)
        optionsButton.Style = this.OptionsButtonStyle;

    }

    /// <summary>
    /// Subscribes to the events on the template parts.
    /// </summary>
    private void SubscribeToTemplatePartEvents()
    {
      if (screenshotButton != null)
        screenshotButton.Click += new RoutedEventHandler(ScreenshotButton_Click);

      if (helpButton != null)
        helpButton.Click += new RoutedEventHandler(HelpButton_Click);

      if (optionsButton != null)
        optionsButton.Click += new RoutedEventHandler(OptionsButton_Click);

      if (closeButton != null)
        closeButton.Click += new RoutedEventHandler(CloseButton_Click);

      if (maximizeButton != null)
        maximizeButton.Click += new RoutedEventHandler(MaximizeButton_Click);

      if (restoreButton != null)
        restoreButton.Click += new RoutedEventHandler(RestoreButton_Click);

      if (minimizeButton != null)
        minimizeButton.Click += new RoutedEventHandler(MinimizeButton_Click);
    }

    /// <summary>
    /// Unsubscribe from the events that are subscribed on the template part elements.
    /// </summary>
    private void UnsubscribeFromTemplatePartEvents()
    {
      if (screenshotButton != null)
        screenshotButton.Click -= new RoutedEventHandler(ScreenshotButton_Click);

      if (helpButton != null)
        helpButton.Click -= new RoutedEventHandler(HelpButton_Click);

      if (optionsButton != null)
        optionsButton.Click -= new RoutedEventHandler(OptionsButton_Click);

      if (closeButton != null)
        closeButton.Click -= new RoutedEventHandler(CloseButton_Click);

      if (maximizeButton != null)
        maximizeButton.Click -= new RoutedEventHandler(MaximizeButton_Click);

      if (restoreButton != null)
        restoreButton.Click -= new RoutedEventHandler(RestoreButton_Click);

      if (minimizeButton != null)
        minimizeButton.Click -= new RoutedEventHandler(MinimizeButton_Click);
    }

    #endregion

    #region Animation

    /// <summary>
    /// Gets the storyboards defined in the <see cref="FloatingWindow" /> style.
    /// </summary>
    private void GetStoryboards()
    {
      if (root != null)
      {
        var groups = VisualStateManager.GetVisualStateGroups(root) as Collection<VisualStateGroup>;
        if (groups != null)
        {
          var states = (from stategroup in groups
                        where stategroup.Name == FloatingWindow.VSMGROUP_Window
                        select stategroup.States).FirstOrDefault() as
                        #if SILVERLIGHT
                        Collection
                        #else
                        FreezableCollection
                        #endif
                        <VisualState>;

          if (states != null)
          {
            closingStoryboard = (from state in states
                                 where state.Name == FloatingWindow.VSMSTATE_StateClosed
                                 select state.Storyboard).FirstOrDefault();

            openingStoryboard = (from state in states
                                 where state.Name == FloatingWindow.VSMSTATE_StateOpen
                                 select state.Storyboard).FirstOrDefault();

            restoringStoryboard = (from state in states
                                   where state.Name == FloatingWindow.VSMSTATE_StateRestored
                                   select state.Storyboard).FirstOrDefault();
          }
        }

        if (inertialMotionStoryboard == null)
          inertialMotionStoryboard = new Storyboard();

        if (maximizingStoryboard == null)
          maximizingStoryboard = new Storyboard();

        if (restoreMaximizedStoryboard == null)
          restoreMaximizedStoryboard = new Storyboard();
      }
    }

    /// <summary>
    /// Subscribes to the events on the storyboards.
    /// </summary>
    private void SubscribeToStoryBoardEvents()
    {
      if (closingStoryboard != null)
        closingStoryboard.Completed += new EventHandler(Closing_Completed);

      if (openingStoryboard != null)
        openingStoryboard.Completed += new EventHandler(Opening_Completed);

      if (inertialMotionStoryboard != null)
        inertialMotionStoryboard.Completed += new EventHandler(InertialMotion_Completed);

      if (maximizingStoryboard != null)
        maximizingStoryboard.Completed += new EventHandler(Maximizing_Completed);

      if (restoringStoryboard != null)
        restoringStoryboard.Completed += new EventHandler(RestoringMinimized_Completed);

      if (restoreMaximizedStoryboard != null)
        restoreMaximizedStoryboard.Completed += new EventHandler(RestoringMaximized_Completed);
    }

    /// <summary>
    /// Unsubscribe from events that are subscribed on the storyboards.
    /// </summary>
    private void UnsubscribeFromStoryBoardEvents()
    {
      if (closingStoryboard != null)
        closingStoryboard.Completed -= new EventHandler(Closing_Completed);

      if (openingStoryboard != null)
        openingStoryboard.Completed -= new EventHandler(Opening_Completed);

      if (inertialMotionStoryboard != null)
        inertialMotionStoryboard.Completed -= new EventHandler(InertialMotion_Completed);

      if (maximizingStoryboard != null)
        maximizingStoryboard.Completed -= new EventHandler(Maximizing_Completed);

      if (restoringStoryboard != null)
        restoringStoryboard.Completed -= new EventHandler(RestoringMinimized_Completed);

      if (restoreMaximizedStoryboard != null)
        restoreMaximizedStoryboard.Completed -= new EventHandler(RestoringMaximized_Completed);
    }

    /// <summary>
    /// Starts maximizing animation.
    /// </summary>
    private void StartMaximizingAnimation()
    {
      SaveActualSize();

      #if SILVERLIGHT
      this.MoveAndResize(BoundingRectangleMaximized, MaximizingDurationInMilliseconds, Maximizing_Completed); //NOTE: MaximizedBounds takes in mind window's scale when resizing
      #else
      this.MoveAndResize(maximizingStoryboard, new Point(0, 0), HostPanel.ActualWidth, HostPanel.ActualHeight, MaximizingDurationInMilliseconds);
      #endif
    }

    /// <summary>
    /// Starts restoring animation.
    /// </summary>
    private void StartRestoringAnimation()
    {
      #if SILVERLIGHT
      SaveActualSize(); //do we need this here?

      this.MoveAndResize(previousPosition, previousSize.Width, previousSize.Height, RestoringDurationInMilliseconds, RestoringMaximized_Completed);
      #else
      this.MoveAndResize(restoreMaximizedStoryboard, previousPosition, previousSize.Width, previousSize.Height, RestoringDurationInMilliseconds); //the story board has its own completion event handler attached
      #endif
    }

    /// <summary>
    /// Handles the Completed event of the Maximizing animation.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    private void Maximizing_Completed(object sender, EventArgs e)
    {
      #if !SILVERLIGHT
      maximizingStoryboard.Stop();
      maximizingStoryboard.Remove(this);

      this.Width = HostPanel.ActualWidth;
      this.Height = HostPanel.ActualHeight;
      this.Position = new Point(0, 0);
      #endif

      Focus();
      OnMaximized(EventArgs.Empty);
    }

    /// <summary>
    /// Handles the Completed event of the Restoring Maximized window animation.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    private void RestoringMaximized_Completed(object sender, EventArgs e)
    {
      #if !SILVERLIGHT
      restoreMaximizedStoryboard.Stop();
      restoreMaximizedStoryboard.Remove(this);

      this.Width = previousSize.Width;
      this.Height = previousSize.Height;
      this.Position = previousPosition;
      #endif

      Focus();
      OnRestored(EventArgs.Empty);
    }

    /// <summary>
    /// Handles the Completed event of the Restoring Minimized window animation.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    private void RestoringMinimized_Completed(object sender, EventArgs e)
    {
      SetTopmost();
      Focus();
      OnRestored(EventArgs.Empty);
    }

    #endregion

    #region Inertial Motion

    /// <summary>
    /// Starts the inertial motion.
    /// </summary>
    private void StartInertialMotion()
    {
      snapinController.SnapinBounds = this.FloatingWindowHost.GetSnapinBounds(this);
      CaptureMouseCursor();
      windowAction = WindowAction.Move;
      inertiaController.StartMotion(Position); //TODO: take in mind component's zoom to avoid jumpy motion
    }

    /// <summary>
    /// Stops current inertial motion.
    /// </summary>
    private void StopInertialMotion()
    {
      if (inertialMotionStoryboard.Children.Count > 0 &&
          inertialMotionStoryboard.GetCurrentState() != ClockState.Stopped)
      {
        inertialMotionStoryboard.Pause();

        // The Position has rounded coordinates now, but real X and Y coordinates are fractional
        Position = GetCurrentWindowPosition();

        // Move the window to the rounded coordinates
        MoveWindow(Position);

        inertialMotionStoryboard.Stop();
        inertialMotionStoryboard.Children.Clear(); //note: in WPF was inertialMotionStoryboard.Remove(contentRoot as FrameworkElement) - guess it's the same
      }
    }

    /// <summary>
    /// Handles the Completed event of the InertialMotionStoryboard.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    private void InertialMotion_Completed(object sender, EventArgs e)
    {
      // Save current window position reading it from the TranslateTransform object
      Position = GetCurrentWindowPosition();

      #if !SILVERLIGHT
      // Stop the animation affecting its target property
      inertialMotionStoryboard.Remove(contentRoot as FrameworkElement);
      #endif
      windowAction = WindowAction.None;
    }

    #endregion

    #region Helpers

    void HideBorder()
    {
      // Guarantee that the visual tree of an element is complete
      if (!templateIsApplied)
        ApplyTemplate();

      // Hide the outer border
      if (contentBorder != null)
      {
        contentBorderThickness = BorderThickness;
        contentBorderCornerRadius = CornerRadius;
        BorderThickness = new Thickness(0); //must not set contentBorder.BorderThickness directly, since we don't use two-way binding to TemplatedParent, but use simpler TemplateBinding instead
        CornerRadius = new CornerRadius(0); //must not set contentBorder.CornerRadius directly
      }

      Border border = chrome as Border;
      if (border != null)
      {
        chromeBorderCornerRadius = border.CornerRadius;
        border.CornerRadius = new CornerRadius(0);
      }
    }

    void RestoreBorder()
    {
      #if BORDER_ONLY_AT_RESIZABLE
      if (!ResizeEnabled) return; //keep border hidden if window is not resizable
      #endif

      // Guarantee that the visual tree of an element is complete
      if (!templateIsApplied)
        ApplyTemplate();

      // Restore the outer border
      if (contentBorder != null)
      {
        BorderThickness = contentBorderThickness; //must not set contentBorder.BorderThickness directly, since we don't use two-way binding to TemplatedParent, but use simpler TemplateBinding instead
        CornerRadius = contentBorderCornerRadius; //must not set contentBorder.CornerRadius directly
      }

      Border border = chrome as Border;
      if (border != null)
        border.CornerRadius = chromeBorderCornerRadius;
    }

    /// <summary>
    /// Checks if the floating window was added to the FloatingWindowHost.
    /// </summary>
    private void CheckHost()
    {
      if (this.FloatingWindowHost == null)
        throw new InvalidOperationException("The FloatingWindow was not added to the FloatingWindowHost.");
    }

    /// <summary>
    /// Saves the actual size if it was not set explicitly set.
    /// E.g. the Width can be NaN, that means "Auto".
    /// </summary>
    private void SaveActualSize()
    {
      if (Width.IsNotSet())
        Width = ActualWidth;

      if (Height.IsNotSet())
        Height = ActualHeight;
    }

    /// <summary>
    /// Restores maximized window position and size.
    /// </summary>
    private void RestoreMaximizedWindow()
    {
      if (windowState != WindowState.Normal)
      {
        if (maximizeButton != null && restoreButton != null && HostPanel != null)
        {
          if (this.ShowMaximizeRestoreButton)
          {
            maximizeButton.SetVisible(true);
            restoreButton.SetVisible(false);
          }

          VisualStateManager.GoToState(maximizeButton, VSMSTATE_StateNormal, true);
        }

        RestoreBorder();

        StartRestoringAnimation();
        windowState = WindowState.Normal;
      }
      else
        Show(Position);
    }

    /// <summary>
    /// Shift the root of the window to compensate its margins.
    /// </summary>
    private void SetInitialRootPosition()
    {
      double x = Math.Round(-this.Margin.Left);
      double y = Math.Round(-this.Margin.Top);

      var transformGroup = (root.RenderTransform as TransformGroup)
      #if !SILVERLIGHT
        .Clone()
      #endif
      ;
      if (transformGroup == null)
      {
        transformGroup = new TransformGroup();
        transformGroup.Children.Add(root.RenderTransform);
        root.RenderTransform = transformGroup;
      }

      var translateTransform = transformGroup.Children.OfType<TranslateTransform>().FirstOrDefault();
      if (translateTransform == null)
      {
        transformGroup.Children.Add(new TranslateTransform() { X = x, Y = y });
      }
      else
      {
        translateTransform.X = x;
        translateTransform.Y = y;
      }

      #if !SILVERLIGHT
      root.RenderTransform = transformGroup;
      #endif
    }

    /// <summary>
    /// Checks the TransformGroup of the content root or creates it if necessary.
    /// </summary>
    private void InitializeContentRootTransformGroup()
    {
      var transformGroup = contentRoot.RenderTransform as TransformGroup;
      if (transformGroup == null)
      {
        transformGroup = new TransformGroup();
        transformGroup.Children.Add(contentRoot.RenderTransform);
        contentRoot.RenderTransform = transformGroup;
      }

      // Check that ScaleTransform exists in the TransformGroup
      // ScaleTransform is used as a target in Storyboards
      var scaleTransform = transformGroup.Children.OfType<ScaleTransform>().FirstOrDefault();

      if (scaleTransform == null)
        transformGroup.Children.Insert(0, new ScaleTransform());

      var translateTransform = transformGroup.Children.OfType<TranslateTransform>().FirstOrDefault();

      if (translateTransform == null)
        transformGroup.Children.Add(new TranslateTransform());
    }

    #if SILVERLIGHT
    /// <summary>
    /// Determines whether the control is in the visual tree of the window.
    /// </summary>
    /// <param name="control">The control to test.</param>
    /// <returns>
    /// <c>true</c> if the control is in the visual tree; otherwise, <c>false</c>.
    /// </returns>
    private bool IsControlInVisualTree(Control control) //TODO: see WPF's IsVisualAncestor (called elsewhere in this code) to maybe replace this (in Compatibility library)
    {
      if (control != null)
      {
        DependencyObject parent = control;
        do
        {
          parent = VisualTreeHelper.GetParent(parent);
          if (parent == this)
            return true;
        }
        while (parent != null);
      }

      return false;
    }
    #endif

    /// <summary>
    /// Gets current window position taking into account animation effects.
    /// </summary>
    /// <returns>Current window position.</returns>
    private Point GetCurrentWindowPosition()
    {
      var transformGroup = contentRoot.RenderTransform as TransformGroup;
      var translateTransform = transformGroup.Children.OfType<TranslateTransform>().FirstOrDefault();
      var position = new Point(translateTransform.X, translateTransform.Y);

      // Round coordinates to avoid blurred window
      return position.Round();
    }

    /// <summary>
    /// Captures the mouse cursor.
    /// </summary>
    private void CaptureMouseCursor()
    {
      contentRoot.CaptureMouse();
      isMouseCaptured = true;
    }

    private bool CheckResizable()
    {
      return ResizeEnabled &&
             (windowState == WindowState.Normal || (windowState == WindowState.Maximized && ResizeMaximizedEnabled));
    }

    /// <summary>
    /// Determines whether the mouse is over buttons in the specified mouse position.
    /// </summary>
    /// <param name="position">The mouse position.</param>
    /// <param name="origin">Relative origin.</param>
    /// <returns><c>true</c> if mouse is mouse over buttons.</returns>
    private bool IsMouseOverButtons(Point position, UIElement origin)
    {
      return (minimizeButton.IsVisible() && minimizeButton.ContainsPoint(position, origin)) ||
             (maximizeButton.IsVisible() && maximizeButton.ContainsPoint(position, origin)) ||
             (restoreButton.IsVisible() && restoreButton.ContainsPoint(position, origin)) ||
             (closeButton.IsVisible() && closeButton.ContainsPoint(position, origin)) ||
             (screenshotButton.IsVisible() && screenshotButton.ContainsPoint(position, origin)) ||
             (helpButton.IsVisible() && helpButton.ContainsPoint(position, origin)) ||
             (optionsButton.IsVisible() && optionsButton.ContainsPoint(position, origin));
    }

    /// <summary>
    /// Moves the window to the specified coordinates.
    /// </summary>
    /// <param name="point">Coordinates of the window.</param>
    private void MoveWindow(Point point)
    {
      if (contentRoot != null && !point.IsNotSet())
      {
        // Round coordinates to avoid blurred window
        double x = Math.Round(Math.Max(0, point.X));
        double y = Math.Round(Math.Max(0, point.Y));

        var transformGroup = (contentRoot.RenderTransform as TransformGroup)
          #if !SILVERLIGHT
          .Clone()
          #endif
          ;
        var translateTransform = transformGroup.Children.OfType<TranslateTransform>().FirstOrDefault();
        if (translateTransform == null)
        {
          transformGroup.Children.Add(new TranslateTransform() { X = x, Y = y });
          #if !SILVERLIGHT
          contentRoot.RenderTransform = transformGroup;
          #endif
        }
        else
        {
          translateTransform.X = x;
          translateTransform.Y = y;
        }

        Point newPosition = new Point(x, y);

        if (Position != newPosition)
          Position = newPosition;
      }
    }

    #endregion

    #region Isolated Storage

    /// <summary>
    /// Restores the size and position stored in the IsolatedStorage on closing.
    /// </summary>
    public bool RestoreSizeAndPosition()
    {
      if (!IsWindowTagSet)
        return false;

      try
      {
        string positionKey = GetAppSettingsKey("Position");
        string sizeKey = GetAppSettingsKey("Size");

        bool loadPosition = localStorage.Contains(positionKey);
        if (loadPosition)
          Position = (Point)localStorage[positionKey];

        bool loadSize = localStorage.Contains(sizeKey);
        if (loadSize)
        {
          Size size = (Size)localStorage[sizeKey];
          Width = size.Width == 0 ? double.NaN : size.Width;
          Height = size.Height == 0 ? double.NaN : size.Height;
        }

        return (loadPosition || loadSize);
      }
      catch
      {
        return false;
      }
    }

    /// <summary>
    /// Saves current size and position of the window in the IsolatedStorage.
    /// The key of the settings is the Tag of the window (if not null).
    /// </summary>
    public bool SaveSizeAndPosition()
    {
      if (!IsWindowTagSet)
        return false;

      try
      {
        string positionKey = GetAppSettingsKey("Position");
        string sizeKey = GetAppSettingsKey("Size");

        Point point = windowState == WindowState.Normal ? Position : previousPosition;
        localStorage[positionKey] = point;

        Size size = windowState == WindowState.Normal ? new Size(Width, Height) : previousSize; //do not use (ActualWidth, ActualHeight) here, since at RestoreSizeAndPosition this value will be assigned to (Width, Height)
        localStorage[sizeKey] = size;

        return true;
      }
      catch
      {
        return false;
      }
    }

    /// <summary>
    /// Gets the application settings key used to store properties in the IsolatedStorage.
    /// </summary>
    /// <param name="key">The key of the property, e.g. "Position".</param>
    /// <returns>Combined settings key or empty string.</returns>
    private string GetAppSettingsKey(string key)
    {
      string tag = this.Tag as string;
      return tag + ":" + key;
    }

    #endregion

  }
}
