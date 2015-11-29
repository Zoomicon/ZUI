//Filename: Iconbar.cs
//Version: 20140904

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

#if SILVERLIGHT
using SilverFlow.Controls.Helpers;
using Compatibility;
#endif

namespace SilverFlow.Controls
{
  /// <summary>
  /// Iconbar containing window icons.
  /// </summary>
  [TemplatePart(Name = PART_LayoutRoot, Type = typeof(FrameworkElement))]
  [TemplatePart(Name = PART_FixedBar, Type = typeof(Border))]
  [TemplatePart(Name = PART_SlidingBar, Type = typeof(Border))]
  [TemplatePart(Name = PART_Carousel, Type = typeof(StackPanel))]
  [TemplateVisualState(Name = VSMSTATE_StateOpen, GroupName = VSMGROUP_States)]
  [TemplateVisualState(Name = VSMSTATE_StateClosed, GroupName = VSMGROUP_States)]
  [StyleTypedProperty(Property = PROPERTY_IconbarStyle, StyleTargetType = typeof(Border))]
  [StyleTypedProperty(Property = PROPERTY_WindowIconStyle, StyleTargetType = typeof(WindowIcon))]
  public class Iconbar : ContentControl, INotifyPropertyChanged
  {

    #region --- Constants ---

    // Template parts
    public const string PART_LayoutRoot = "PART_LayoutRoot";
    public const string PART_FixedBar = "PART_FixedBar";
    public const string PART_SlidingBar = "PART_SlidingBar";
    public const string PART_Carousel = "PART_Carousel";

    // VSM groups
    public const string VSMGROUP_States = "VisualStateGroup";

    // VSM states
    public const string VSMSTATE_StateOpen = "Open";
    public const string VSMSTATE_StateClosed = "Closed";

    // Style typed properties
    public const string PROPERTY_IconbarStyle = "IconbarStyle";
    public const string PROPERTY_WindowIconStyle = "WindowIconStyle";

    // Animation duration in milliseconds
    private const double SlidingDurationInMilliseconds = 200;

    #endregion

    #region --- Fields ---

    private FrameworkElement layoutRoot;
    private Border fixedBar;
    private Border slidingBar;
    private StackPanel carousel;
    private Storyboard closingStoryboard;
    private Storyboard openingStoryboard;
    private bool isOpen; //=false
    private double slidingBarPosition;

    #endregion

    #region --- Constructor ---

    #if !SILVERLIGHT
    static Iconbar()
    {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(Iconbar), new FrameworkPropertyMetadata(typeof(Iconbar)));
    }
    #endif

    /// <summary>
    /// Initializes a new instance of the <see cref="Iconbar"/> class.
    /// </summary>
    public Iconbar()
    {
      DefaultStyleKey = typeof(Iconbar);
    }

    #endregion

    #region --- Properties ---

    #region IconbarStyle

    /// <summary>
    /// Gets or sets the style of the Iconbar.
    /// </summary>
    public Style IconbarStyle
    {
      get { return GetValue(IconbarStyleProperty) as Style; }
      set { SetValue(IconbarStyleProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="Iconbar.IconbarStyleProperty" /> dependency property.
    /// </summary>
    public static readonly DependencyProperty IconbarStyleProperty =
        DependencyProperty.Register(
            "IconbarStyle",
            typeof(Style),
            typeof(Iconbar),
            new PropertyMetadata(IconbarStylePropertyChanged));

    /// <summary>
    /// IconbarStyle PropertyChangedCallback call back static function.
    /// </summary>
    /// <param name="d">Iconbar object whose IconbarStyle property is changed.</param>
    /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
    private static void IconbarStylePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      Iconbar iconbar = (Iconbar)d;
      if (iconbar != null && iconbar.fixedBar != null)
      {
        iconbar.fixedBar.Style = e.NewValue as Style;
      }
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
    /// Identifies the <see cref="Iconbar.WindowIconStyleProperty" /> dependency property.
    /// </summary>
    public static readonly DependencyProperty WindowIconStyleProperty =
        DependencyProperty.Register(
            "WindowIconStyle",
            typeof(Style),
            typeof(Iconbar),
            null);

    #endregion

    #region IsOpen

    /// <summary>
    /// Gets or sets a value indicating whether the Iconbar is open.
    /// </summary>
    /// <value><c>true</c> if the Iconbar is open; otherwise, <c>false</c>.</value>
    public bool IsOpen
    {
      get { return isOpen; }
      set
      {
        if (value != isOpen)
        {
          isOpen = value;
          if (value)
          {
            if (carousel == null) ApplyTemplate();
            FillCarousel();
            SetSlidingBarPosition(0);
            VisualStateManager.GoToState(this, VSMSTATE_StateOpen, true);
          }
          else
            VisualStateManager.GoToState(this, VSMSTATE_StateClosed, true);

          OnPropertyChanged(new PropertyChangedEventArgs("IsOpen"));
        }
      }
    }

    #endregion

    #region FloatingWindowHost

    /// <summary>
    /// Gets or sets the FloatingWindowHost containing the Iconbar.
    /// </summary>
    /// <value>FloatingWindowHost containing the Iconbar.</value>
    public FloatingWindowHost FloatingWindowHost { get; set; }

    #endregion

    #endregion

    #region --- Methods ---

    /// <summary>
    /// Sets the sliding bar position.
    /// </summary>
    /// <param name="position">X-coordinate of the sliding bar.</param>
    private void SetSlidingBarPosition(double x)
    {
      slidingBarPosition = x;
      if (slidingBar != null)
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

    /// <summary>
    /// Updates the Iconbar if it is open.
    /// </summary>
    public void Update()
    {
      if (IsOpen)
      {
        FillCarousel();
      }
    }

    /// <summary>
    /// Removes the specified window from the Iconbar.
    /// </summary>
    /// <param name="window">The window to remove from the Iconbar.</param>
    public void Remove(FloatingWindow window)
    {
      if (window != null && carousel != null)
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
    /// Gets the storyboards defined in the <see cref="Iconbar" /> style.
    /// </summary>
    private void GetStoryboards()
    {
      var groups = VisualStateManager.GetVisualStateGroups(layoutRoot) as Collection<VisualStateGroup>;
      if (groups != null)
      {
        var states = (from stategroup in groups
                      where stategroup.Name == Iconbar.VSMGROUP_States
                      select stategroup.States).FirstOrDefault() as
                        #if SILVERLIGHT
                        Collection
                        #else
                        FreezableCollection
                        #endif
                        <VisualState>;

        if (states != null)
        {
          openingStoryboard = (from state in states
                               where state.Name == Iconbar.VSMSTATE_StateOpen
                               select state.Storyboard).FirstOrDefault();

          closingStoryboard = (from state in states
                               where state.Name == Iconbar.VSMSTATE_StateClosed
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
        closingStoryboard.Completed += Closing_Completed;

      if (openingStoryboard != null)
        openingStoryboard.Completed += Opening_Completed;

      if (fixedBar != null)
      {
        fixedBar.MouseMove += FixedBar_MouseMove;
        fixedBar.SizeChanged += FixedBar_SizeChanged;
      }
    }

    /// <summary>
    /// Unsubscribe from events. 
    /// </summary>
    private void UnsubscribeFromEvents()
    {
      if (closingStoryboard != null)
        closingStoryboard.Completed -= Closing_Completed;

      if (openingStoryboard != null)
        openingStoryboard.Completed -= Opening_Completed;

      if (fixedBar != null)
      {
        fixedBar.MouseMove += FixedBar_MouseMove;
        fixedBar.SizeChanged -= FixedBar_SizeChanged;
      }
    }

    /// <summary>
    /// Sets styles that are applied for different template parts.
    /// </summary>
    private void SetStyles()
    {
      if (fixedBar != null && this.IconbarStyle != null)
        fixedBar.Style = this.IconbarStyle;
    }

    /// <summary>
    /// Add windows icons to the carousel.
    /// </summary>
    private void FillCarousel()
    {
      ClearCarousel();

      foreach (var window in this.FloatingWindowHost.WindowsInIconbar)
      {
        WindowIcon icon = new WindowIcon()
        {
          Title = window.IconText,
          Thumbnail = window.WindowThumbnail,
          #if !SILVERLIGHT
          Icon = window.Icon as FrameworkElement,
          #endif
          FlowDirection = window.FlowDirection,
          Window = window,
          IconWidth = this.FloatingWindowHost.IconWidth,
          IconHeight = this.FloatingWindowHost.IconHeight
        };

        if (WindowIconStyle != null)
          icon.Style = WindowIconStyle; //removed access to Application.Current.Resources. Assuming the WindowIcon constructor loads default style using DefaultStyleKey, so overriding that only if WindowIconStyle is not null

        icon.Click += new RoutedEventHandler(Icon_Click);
        carousel.Children.Add(icon);
      }
    }

    /// <summary>
    /// Remove Icon Click event handlers and clear the carousel.
    /// </summary>
    private void ClearCarousel()
    {
      if (carousel == null) return;

      foreach (var windowIcon in carousel.Children.OfType<WindowIcon>())
      {
        windowIcon.Click -= new RoutedEventHandler(Icon_Click);

        #if !SILVERLIGHT

        // If the Icon is a FrameworkElement and we placed it into the container, remove it
        if (windowIcon.Icon != null)
          windowIcon.Icon.RemoveFromContainer();

        #endif
      }

      carousel.Children.Clear();
    }

    #endregion

    #region --- Events ---

    /// <summary>
    /// Occurs when the <see cref="Iconbar" /> is opened.
    /// </summary>
    public event EventHandler Opened;

    /// <summary>
    /// Occurs when the <see cref="Iconbar" /> is closed.
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
    /// Builds the visual tree for the <see cref="Iconbar" /> control 
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

    #region Animation

    /// <summary>
    /// Executed when the Closing storyboard ends.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="e">Event args.</param>
    private void Closing_Completed(object sender, EventArgs e)
    {
      IsOpen = false;
      ClearCarousel();
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

    #endregion

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

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
      base.OnMouseLeftButtonDown(e);

      //if (AutoHide) //TODO: check AutoHide
        IsOpen = false; //close the Iconbar
    }

    /// <summary>
    /// Handles the Click event of the Icon control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    protected virtual void Icon_Click(object sender, RoutedEventArgs e)
    {
      WindowIcon icon = sender as WindowIcon;

      if (icon != null && icon.Window != null)
      {
        //if (AutoHide) //TODO: check AutoHide
          IsOpen = false; //close the Iconbar
        icon.Window.RestoreWindow();
      }
    }

    /// <summary>
    /// Handles the MouseMove event of the FixedBar control. It implements "Carousel" logic.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.Windows.Input.MouseEventArgs"/> instance containing the event data.</param>
    private void FixedBar_MouseMove(object sender, MouseEventArgs e)
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
            {
              #if SILVERLIGHT
              // Get absolute mouse position
              Point mousePosition = e.GetPosition(null);

              Storyboard storyboard = slidingBar.AnimateDoubleProperty("(Canvas.Left)", null, x, SlidingDurationInMilliseconds);
              storyboard.Completed += (s, args) =>
                  {
                    // Select an icon on storyboard completion
                    // That is necessary because MouseOver state won't be proceeded correctly
                    SlidingBarStoryboardCompleted(mousePosition);
                    slidingBarPosition = x;
                  };
              #else
              SetSlidingBarPosition(x);
              #endif
            }
          }
        }
      }
    }

    #if SILVERLIGHT

    /// <summary>
    /// Handles the Completed event of the SlidingBarStoryboard control.
    /// </summary>
    /// <param name="mousePosition">Absolute mouse position.</param>
    private void SlidingBarStoryboardCompleted(Point mousePosition)
    {
      // Find selected icon
      var selectedIcon = (from item in carousel.FindElementsInCoordinates(mousePosition).OfType<WindowIcon>()
                          select item).FirstOrDefault();

      // Select an icon in mouse position
      foreach (var icon in carousel.Children.OfType<WindowIcon>())
      {
        icon.Selected = selectedIcon != null && icon == selectedIcon;
      }
    }

    #endif

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

    #endregion

  }
}
