using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using SilverFlow.Controls.Extensions;

namespace SilverFlow.Controls
{
    /// <summary>
    /// A Content Control containing floating windows.
    /// </summary>
    [TemplatePart(Name = PART_Root, Type = typeof(Grid))]
    [TemplatePart(Name = PART_ContentRoot, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = PART_HostCanvas, Type = typeof(Canvas))]
    [TemplatePart(Name = PART_ModalCanvas, Type = typeof(Canvas))]
    [TemplatePart(Name = PART_IconBarContainer, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = PART_Overlay, Type = typeof(Grid))]
    [TemplatePart(Name = PART_IconBar, Type = typeof(IconBar))]
    [TemplatePart(Name = PART_BottomBar, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = PART_BootstrapButton, Type = typeof(BootstrapButton))]
    [TemplatePart(Name = PART_BarContent, Type = typeof(ContentControl))]
    [TemplateVisualState(Name = VSMSTATE_VisibleOverlay, GroupName = VSMGROUP_Overlay)]
    [TemplateVisualState(Name = VSMSTATE_HiddenOverlay, GroupName = VSMGROUP_Overlay)]
    [StyleTypedProperty(Property = PROPERTY_BottomBarStyle, StyleTargetType = typeof(Border))]
    [StyleTypedProperty(Property = PROPERTY_BootstrapButtonStyle, StyleTargetType = typeof(BootstrapButton))]
    [StyleTypedProperty(Property = PROPERTY_WindowIconStyle, StyleTargetType = typeof(WindowIcon))]
    public class FloatingWindowHost : ContentControl
    {
        #region Constants

        // Template parts
        private const string PART_Root = "PART_Root";
        private const string PART_ContentRoot = "PART_ContentRoot";
        private const string PART_HostCanvas = "PART_HostCanvas";
        private const string PART_ModalCanvas = "PART_ModalCanvas";
        private const string PART_IconBarContainer = "PART_IconBarContainer";
        private const string PART_Overlay = "PART_Overlay";
        private const string PART_IconBar = "PART_IconBar";
        private const string PART_BottomBar = "PART_BottomBar";
        private const string PART_BootstrapButton = "PART_BootstrapButton";
        private const string PART_BarContent = "PART_BarContent";

        // VSM groups
        private const string VSMGROUP_Overlay = "OverlayStates";

        // VSM states
        private const string VSMSTATE_VisibleOverlay = "VisibleOverlay";
        private const string VSMSTATE_HiddenOverlay = "HiddenOverlay";

        // Style typed properties
        private const string PROPERTY_BottomBarStyle = "BottomBarStyle";
        private const string PROPERTY_BootstrapButtonStyle = "BootstrapButtonStyle";
        private const string PROPERTY_WindowIconStyle = "WindowIconStyle";

        // Thickness of resizing area.
        private const double SnapinDistanceDefaultValue = 5.0;

        // Default icon size
        private const double DefaultIconWidth = 120;
        private const double DefaultIconHeight = 70;

        #endregion

        #region Member Fields

        /// <summary>
        /// Current ZIndex of a child element
        /// </summary>
        private static int zIndex = 1;

        private Grid root;
        private FrameworkElement contentRoot;
        private Canvas hostCanvas;
        private Canvas modalCanvas;
        private FrameworkElement iconBarContainer;
        private Grid overlay;
        private IconBar iconBar;
        private FrameworkElement bottomBar;
        private BootstrapButton bootstrapButton;
        private ContentControl barContent;

        private bool templateIsApplied;

        #endregion Member Fields

        #region public Style BottomBarStyle

        /// <summary>
        /// Gets or sets the style of the BottomBar.
        /// </summary>
        public Style BottomBarStyle
        {
            get { return GetValue(BottomBarStyleProperty) as Style; }
            set { SetValue(BottomBarStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="FloatingWindowHost.BottomBarStyleProperty" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty BottomBarStyleProperty =
            DependencyProperty.Register(
                "BottomBarStyle",
                typeof(Style),
                typeof(FloatingWindowHost),
                new PropertyMetadata(BottomBarStylePropertyChanged));

        /// <summary>
        /// BottomBarStyle PropertyChangedCallback call back static function.
        /// </summary>
        /// <param name="d">FloatingWindowHost object whose BottomBarStyle property is changed.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void BottomBarStylePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FloatingWindowHost host = (FloatingWindowHost)d;
            if (host != null && host.bottomBar != null)
            {
                host.bottomBar.Style = e.NewValue as Style;
            }
        }

        #endregion

        #region public Style BootstrapButtonStyle

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
            if (host != null && host.bottomBar != null)
            {
                host.bootstrapButton.Style = e.NewValue as Style;
            }
        }

        #endregion

        #region public Style WindowIconStyle

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
            if (host != null && host.iconBar != null)
            {
                host.iconBar.WindowIconStyle = e.NewValue as Style;
            }
        }

        #endregion

        #region public bool SnapinEnabled

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

        #region public double SnapinDistance

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

        #region public double SnapinMargin

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

        #region public bool ShowMinimizedOnlyInIconbar

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

        #region public Brush OverlayBrush

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

        #region public double IconWidth

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

        #region public double IconHeight

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

        #region public object Bar

        /// <summary>
        /// Gets or sets a control displayed in the BottomBar.
        /// </summary>
        /// <value>The control displayed in the BottomBar. The default is null.</value>
        public object Bar
        {
            get { return (double)GetValue(BarProperty); }
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
        /// Gets a collection of windows shown in the IconBar.
        /// </summary>
        /// <value>Collection of windows shown in the IconBar.</value>
        public IOrderedEnumerable<FloatingWindow> WindowsInIconBar
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
            get { return hostCanvas.Children.OfType<FloatingWindow>(); }
        }

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

        /// <summary>
        /// Initializes a new instance of the <see cref="FloatingWindowHost"/> class.
        /// </summary>
        public FloatingWindowHost()
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
            iconBarContainer = GetTemplatePart<FrameworkElement>(PART_IconBarContainer);
            overlay = GetTemplatePart<Grid>(PART_Overlay);
            iconBar = GetTemplatePart<IconBar>(PART_IconBar);
            bottomBar = GetTemplatePart<FrameworkElement>(PART_BottomBar);
            bootstrapButton = GetTemplatePart<BootstrapButton>(PART_BootstrapButton);
            barContent = GetTemplatePart<ContentControl>(PART_BarContent);

            iconBar.FloatingWindowHost = this;

            SetStyles();
            SubscribeToTemplatePartEvents();
            SubscribeToEvents();

            templateIsApplied = true;
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
        /// Adds the specified floating window to the collection of child elements of the host.
        /// </summary>
        /// <param name="window">The floating window.</param>
        /// <exception cref="ArgumentNullException">Floating window is null.</exception>
        public void Add(FloatingWindow window)
        {
            if (window == null)
                throw new ArgumentNullException("window");

            // Guarantee that the visual tree of the control is complete
            if (!templateIsApplied)
                templateIsApplied = ApplyTemplate();

            if (!hostCanvas.Children.Contains(window))
            {
                hostCanvas.Children.Add(window);
                window.FloatingWindowHost = this;
            }
        }

        /// <summary>
        /// Removes the specified floating window from the collection of child elements of the host.
        /// </summary>
        /// <param name="window">The floating window.</param>
        public void Remove(FloatingWindow window)
        {
            if (window != null)
            {
                hostCanvas.Children.Remove(window);
                modalCanvas.Children.Remove(window);
                iconBar.Remove(window);
            }
        }

        /// <summary>
        /// Closes all floating windows.
        /// </summary>
        public void CloseAllWindows()
        {
            HideIconBar();
            FloatingWindows.ToList().ForEach(x => x.Close());
        }

        /// <summary>
        /// Shows the IconBar.
        /// </summary>
        public void ShowIconBar()
        {
            iconBar.Show();
        }

        /// <summary>
        /// Hides the IconBar.
        /// </summary>
        public void HideIconBar()
        {
            iconBar.Hide();
        }

        /// <summary>
        /// Updates the IconBar if it is open.
        /// </summary>
        public void UpdateIconBar()
        {
            iconBar.Update();
        }

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
        /// Shows the overlay, moves the window to the "modal" layer and hides the IconBar.
        /// </summary>
        /// <param name="modalWindow">The modal window.</param>
        internal void ShowWindowAsModal(FloatingWindow modalWindow)
        {
            HideIconBar();
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
        internal void ShowWindow(Action<Point> action, Point point)
        {
            if (IsLayoutUpdated)
            {
                action(point);
            }
            else
            {
                this.Rendered += (s, e) => { action(point); };
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
            bootstrapButton.Click += new RoutedEventHandler(BootstrapButton_Click);
            iconBar.Opened += new EventHandler(IconBarVisibilityChanged);
            iconBar.Closed += new EventHandler(IconBarVisibilityChanged);
            hostCanvas.SizeChanged += new SizeChangedEventHandler(HostCanvas_SizeChanged);
            modalCanvas.SizeChanged += new SizeChangedEventHandler(ModalCanvas_SizeChanged);
            iconBarContainer.SizeChanged += new SizeChangedEventHandler(IconBarContainer_SizeChanged);
        }

        /// <summary>
        /// Unsubscribe from the events that are subscribed on the template part elements.
        /// </summary>
        private void UnsubscribeFromTemplatePartEvents()
        {
            if (bootstrapButton != null)
                bootstrapButton.Click -= new RoutedEventHandler(BootstrapButton_Click);

            if (iconBar != null)
                iconBar.Opened -= new EventHandler(IconBarVisibilityChanged);

            if (iconBar != null)
                iconBar.Closed -= new EventHandler(IconBarVisibilityChanged);

            if (hostCanvas != null)
                hostCanvas.SizeChanged -= new SizeChangedEventHandler(HostCanvas_SizeChanged);

            if (modalCanvas != null)
                modalCanvas.SizeChanged -= new SizeChangedEventHandler(ModalCanvas_SizeChanged);

            if (iconBarContainer != null)
                iconBarContainer.SizeChanged -= new SizeChangedEventHandler(IconBarContainer_SizeChanged);
        }

        /// <summary>
        /// Subscribes to the events the control shall handle.
        /// </summary>
        private void SubscribeToEvents()
        {
            this.MouseLeftButtonDown += new MouseButtonEventHandler(FloatingWindowHost_MouseLeftButtonDown);
            this.LayoutUpdated += new EventHandler(FloatingWindowHost_LayoutUpdated);
        }

        /// <summary>
        /// Unsubscribes from the subscribed events.
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            this.MouseLeftButtonDown -= new MouseButtonEventHandler(FloatingWindowHost_MouseLeftButtonDown);
            this.LayoutUpdated -= new EventHandler(FloatingWindowHost_LayoutUpdated);
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
                this.LayoutUpdated -= new EventHandler(FloatingWindowHost_LayoutUpdated);
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

            zIndex++;
            Canvas.SetZIndex(element, zIndex);
        }

        /// <summary>
        /// Attempts to set the focus on the FloatingWindow.
        /// </summary>
        /// <param name="window">The window.</param>
        private void SetFocusToActiveWindow(FloatingWindow window)
        {
            if (window != null && !window.TopMost)
                window.Focus();
        }

        /// <summary>
        /// Handles the MouseLeftButtonDown event of the FloatingWindowHost control. 
        /// Closes the IconBar on mouse left click.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void FloatingWindowHost_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = false;
            HideIconBar();
        }

        /// <summary>
        /// Handles the Click event of the BootstrapButton.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void BootstrapButton_Click(object sender, RoutedEventArgs e)
        {
            if (bootstrapButton.IsOpen)
                ShowIconBar();
            else
                HideIconBar();
        }

        /// <summary>
        /// Handles the IconBar Visibility Changed event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void IconBarVisibilityChanged(object sender, EventArgs e)
        {
            bootstrapButton.IsOpen = iconBar.IsOpen;
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
        /// Handles the SizeChanged event of the IconBarContainer control to set its clipping region.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.SizeChangedEventArgs"/> instance containing the event data.</param>
        private void IconBarContainer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            iconBarContainer.Clip = new RectangleGeometry()
            {
                Rect = iconBarContainer.GetActualBoundingRectangle()
            };
        }

        /// <summary>
        /// Sets styles that are applied for different template parts.
        /// </summary>
        private void SetStyles()
        {
            if (bottomBar != null && this.BottomBarStyle != null)
                bottomBar.Style = this.BottomBarStyle;

            if (bootstrapButton != null && this.BootstrapButtonStyle != null)
                bootstrapButton.Style = this.BootstrapButtonStyle;

            if (iconBar != null && this.WindowIconStyle != null)
                iconBar.WindowIconStyle = this.WindowIconStyle;
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
