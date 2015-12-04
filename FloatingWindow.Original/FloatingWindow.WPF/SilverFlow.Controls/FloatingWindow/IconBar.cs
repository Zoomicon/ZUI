using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using SilverFlow.Controls.Extensions;

namespace SilverFlow.Controls
{
    /// <summary>
    /// IconBar containing window icons.
    /// </summary>
    [TemplatePart(Name = PART_LayoutRoot, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = PART_FixedBar, Type = typeof(Border))]
    [TemplatePart(Name = PART_SlidingBar, Type = typeof(Border))]
    [TemplatePart(Name = PART_Carousel, Type = typeof(StackPanel))]
    [TemplateVisualState(Name = VSMSTATE_StateOpen, GroupName = VSMGROUP_States)]
    [TemplateVisualState(Name = VSMSTATE_StateClosed, GroupName = VSMGROUP_States)]
    [StyleTypedProperty(Property = PROPERTY_TitleStyle, StyleTargetType = typeof(Border))]
    [StyleTypedProperty(Property = PROPERTY_WindowIconStyle, StyleTargetType = typeof(WindowIcon))]
    public class IconBar : ContentControl, INotifyPropertyChanged
    {
        // Template parts
        private const string PART_LayoutRoot = "PART_LayoutRoot";
        private const string PART_FixedBar = "PART_FixedBar";
        private const string PART_SlidingBar = "PART_SlidingBar";
        private const string PART_Carousel = "PART_Carousel";

        // VSM groups
        private const string VSMGROUP_States = "VisualStateGroup";

        // VSM states
        private const string VSMSTATE_StateOpen = "Open";
        private const string VSMSTATE_StateClosed = "Closed";

        // Style typed properties
        private const string PROPERTY_TitleStyle = "IconBarStyle";
        private const string PROPERTY_WindowIconStyle = "WindowIconStyle";

        // Animation duration in milliseconds
        private const double SlidingDurationInMilliseconds = 200;

        #region public Style IconBarStyle

        /// <summary>
        /// Gets or sets the style of the IconBar.
        /// </summary>
        public Style IconBarStyle
        {
            get { return GetValue(IconBarStyleProperty) as Style; }
            set { SetValue(IconBarStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="IconBar.IconBarStyleProperty" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty IconBarStyleProperty =
            DependencyProperty.Register(
                "IconBarStyle",
                typeof(Style),
                typeof(IconBar),
                new PropertyMetadata(IconBarStylePropertyChanged));

        /// <summary>
        /// IconBarStyle PropertyChangedCallback call back static function.
        /// </summary>
        /// <param name="d">IconBar object whose IconBarStyle property is changed.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void IconBarStylePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            IconBar iconBar = (IconBar)d;
            if (iconBar != null && iconBar.fixedBar != null)
            {
                iconBar.fixedBar.Style = e.NewValue as Style;
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
        /// Identifies the <see cref="IconBar.WindowIconStyleProperty" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty WindowIconStyleProperty =
            DependencyProperty.Register(
                "WindowIconStyle",
                typeof(Style),
                typeof(IconBar),
                null);

        #endregion

        private FrameworkElement layoutRoot;
        private Border fixedBar;
        private Border slidingBar;
        private StackPanel carousel;
        private Storyboard closingStoryboard;
        private Storyboard openingStoryboard;
        private bool isOpen;
        private double slidingBarPosition;

        /// <summary>
        /// Gets or sets a value indicating whether the IconBar is open.
        /// </summary>
        /// <value><c>true</c> if the IconBar is open; otherwise, <c>false</c>.</value>
        public bool IsOpen
        {
            get { return isOpen; }
            private set
            {
                if (value != isOpen)
                {
                    isOpen = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("IsOpen"));
                }
            }
        }

        /// <summary>
        /// Gets or sets the FloatingWindowHost containing the IconBar.
        /// </summary>
        /// <value>FloatingWindowHost containing the IconBar.</value>
        public FloatingWindowHost FloatingWindowHost { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IconBar"/> class.
        /// </summary>
        public IconBar()
        {
            DefaultStyleKey = typeof(IconBar);
        }

        /// <summary>
        /// Occurs when the <see cref="IconBar" /> is opened.
        /// </summary>
        public event EventHandler Opened;

        /// <summary>
        /// Occurs when the <see cref="IconBar" /> is closed.
        /// </summary>
        public event EventHandler Closed;

        #region INotifyPropertyChanged implementation
        /// <summary>
        /// Occurs when a property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the <see cref="E:PropertyChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }
        #endregion

        /// <summary>
        /// Builds the visual tree for the <see cref="IconBar" /> control 
        /// when a new template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            UnsubscribeFromEvents();

            base.OnApplyTemplate();

            layoutRoot = GetTemplatePart<FrameworkElement>(PART_LayoutRoot);
            fixedBar = GetTemplatePart<Border>(PART_FixedBar);
            slidingBar = GetTemplatePart<Border>(PART_SlidingBar);
            carousel = GetTemplatePart<StackPanel>(PART_Carousel);

            SetStyles();
            GetStoryboards();
            SubscribeToEvents();
        }

        /// <summary>
        /// Shows the IconBar.
        /// </summary>
        public void Show()
        {
            if (!IsOpen)
            {
                FillCarousel();
                SetSlidingBarPosition(0);
                VisualStateManager.GoToState(this, VSMSTATE_StateOpen, true);
            }
        }

        /// <summary>
        /// Hides the IconBar.
        /// </summary>
        public void Hide()
        {
            if (IsOpen)
            {
                VisualStateManager.GoToState(this, VSMSTATE_StateClosed, true);
            }
        }

        /// <summary>
        /// Updates the IconBar if it is open.
        /// </summary>
        public void Update()
        {
            if (IsOpen)
            {
                FillCarousel();
            }
        }

        /// <summary>
        /// Removes the specified window from the IconBar.
        /// </summary>
        /// <param name="window">The window to remove from the IconBar.</param>
        public void Remove(FloatingWindow window)
        {
            if (window != null)
            {
                var icon = (from windowIcon in carousel.Children.OfType<WindowIcon>()
                            where windowIcon.Window == window
                            select windowIcon).FirstOrDefault();

                if (icon != null)
                {
                    icon.Click -= new RoutedEventHandler(Icon_Click);
                    carousel.Children.Remove(icon);
                }
            }
        }

        /// <summary>
        /// Gets the storyboards defined in the <see cref="IconBar" /> style.
        /// </summary>
        private void GetStoryboards()
        {
            var groups = VisualStateManager.GetVisualStateGroups(layoutRoot) as Collection<VisualStateGroup>;
            if (groups != null)
            {
                var states = (from stategroup in groups
                              where stategroup.Name == IconBar.VSMGROUP_States
                              select stategroup.States).FirstOrDefault() as FreezableCollection<VisualState>;

                if (states != null)
                {
                    openingStoryboard = (from state in states
                                         where state.Name == IconBar.VSMSTATE_StateOpen
                                         select state.Storyboard).FirstOrDefault();

                    closingStoryboard = (from state in states
                                         where state.Name == IconBar.VSMSTATE_StateClosed
                                         select state.Storyboard).FirstOrDefault();
                }
            }
        }

        /// <summary>
        /// Subscribes to the events after new template is applied. 
        /// </summary>
        private void SubscribeToEvents()
        {
            if (closingStoryboard != null)
                closingStoryboard.Completed += new EventHandler(Closing_Completed);

            if (openingStoryboard != null)
                openingStoryboard.Completed += new EventHandler(Opening_Completed);

            if (fixedBar != null)
                fixedBar.MouseMove += new MouseEventHandler(Bar_MouseMove);

            if (fixedBar != null)
                fixedBar.SizeChanged += new SizeChangedEventHandler(FixedBar_SizeChanged);
        }

        /// <summary>
        /// Unsubscribe from events. 
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            if (closingStoryboard != null)
                closingStoryboard.Completed -= new EventHandler(Closing_Completed);

            if (openingStoryboard != null)
                openingStoryboard.Completed -= new EventHandler(Opening_Completed);

            if (fixedBar != null)
                fixedBar.MouseMove += new MouseEventHandler(Bar_MouseMove);

            if (fixedBar != null)
                fixedBar.SizeChanged -= new SizeChangedEventHandler(FixedBar_SizeChanged);
        }

        /// <summary>
        /// Sets styles that are applied for different template parts.
        /// </summary>
        private void SetStyles()
        {
            if (fixedBar != null && this.IconBarStyle != null)
                fixedBar.Style = this.IconBarStyle;
        }

        /// <summary>
        /// Executed when the Closing storyboard ends.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event args.</param>
        private void Closing_Completed(object sender, EventArgs e)
        {
            IsOpen = false;
            OnClosed(EventArgs.Empty);
        }

        /// <summary>
        /// Executed when the Opening storyboard finishes.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event args.</param>
        private void Opening_Completed(object sender, EventArgs e)
        {
            IsOpen = true;
            OnOpened(EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="E:Opened"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void OnOpened(EventArgs e)
        {
            EventHandler handler = Opened;

            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Raises the <see cref="E:Closed"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void OnClosed(EventArgs e)
        {
            EventHandler handler = Closed;

            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Add windows icons to the carousel.
        /// </summary>
        private void FillCarousel()
        {
            ClearCarousel();

            Style style = this.WindowIconStyle ?? Application.Current.Resources["WindowIconStyle"] as Style;
            foreach (var window in this.FloatingWindowHost.WindowsInIconBar)
            {
                WindowIcon icon = new WindowIcon()
                {
                    Style = style,
                    Title = window.IconText,
                    Thumbnail = window.WindowThumbnail,
                    Icon = window.Icon as FrameworkElement,
                    FlowDirection = window.FlowDirection,
                    Window = window,
                    IconWidth = this.FloatingWindowHost.IconWidth,
                    IconHeight = this.FloatingWindowHost.IconHeight
                };

                icon.Click += new RoutedEventHandler(Icon_Click);
                carousel.Children.Add(icon);
            }
        }

        /// <summary>
        /// Remove Icon Click event handlers and clear the carousel.
        /// </summary>
        private void ClearCarousel()
        {
            if (carousel.Children.Count > 0)
            {
                foreach (var windowIcon in carousel.Children.OfType<WindowIcon>())
                {
                    windowIcon.Click -= new RoutedEventHandler(Icon_Click);

                    // If the Icon is a FrameworkElement and we placed it into the container, remove it
                    if (windowIcon.Icon != null)
                        windowIcon.Icon.RemoveFromContainer();
                }

                carousel.Children.Clear();
            }
        }

        /// <summary>
        /// Handles the Click event of the Icon control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void Icon_Click(object sender, RoutedEventArgs e)
        {
            WindowIcon icon = sender as WindowIcon;

            if (icon != null && icon.Window != null)
            {
                this.Hide();
                icon.Window.RestoreWindow();
            }
        }

        /// <summary>
        /// Handles the MouseMove event of the Bar control. It implements "Carousel" logic.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseEventArgs"/> instance containing the event data.</param>
        private void Bar_MouseMove(object sender, MouseEventArgs e)
        {
            var icon = carousel.Children.OfType<WindowIcon>().FirstOrDefault();

            // If there is at least one icon on the bar
            if (icon != null)
            {
                double a = e.GetPosition(fixedBar).X;
                double b = fixedBar.ActualWidth - fixedBar.Padding.Horizontal();
                double c = slidingBar.ActualWidth;

                // If sliding bar does not fit into the bar, shift the sliding bar
                if (c > b)
                {
                    double width = b - icon.ActualWidth;
                    if (width != 0)
                    {
                        a -= icon.ActualWidth / 2;
                        if (a < 0) a = 0;
                        if (a > width) a = width;

                        double x = Math.Round((a / width) * (b - c));

                        if (x != slidingBarPosition)
                            SetSlidingBarPosition(x);
                    }
                }
            }
        }

        /// <summary>
        /// Handles the SizeChanged event of the FixedBar control.
        /// Sets the initial position of the sliding bar.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.SizeChangedEventArgs"/> instance containing the event data.</param>
        private void FixedBar_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (slidingBarPosition != 0)
                SetSlidingBarPosition(0);
        }

        /// <summary>
        /// Sets the sliding bar position.
        /// </summary>
        /// <param name="position">X-coordinate of the sliding bar.</param>
        private void SetSlidingBarPosition(double x)
        {
            slidingBarPosition = x;
            Canvas.SetLeft(slidingBar, slidingBarPosition);
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
