//Filename: WindowIcon.cs
//Version: 20140904

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

#if !SILVERLIGHT
using SilverFlow.Controls.Extensions;
#endif

namespace SilverFlow.Controls
{
  /// <summary>
  /// Window icon containing a thumbnail and title of the windows.
  /// </summary>
  [TemplatePart(Name = PART_Border, Type = typeof(Border))]
  [TemplatePart(Name = PART_Title, Type = typeof(TextBlock))]
  [TemplatePart(Name = PART_Thumbnail, Type = typeof(Image))]
  #if !SILVERLIGHT
  [TemplatePart(Name = PART_IconContainer, Type = typeof(Border))]
  [TemplatePart(Name = PART_Overlay, Type = typeof(Border))]
  #endif
  [StyleTypedProperty(Property = PROPERTY_IconBorderStyle, StyleTargetType = typeof(Border))]
  public class WindowIcon : Button
  {

    #region --- Constants ---

    // Template parts
    private const string PART_Border = "PART_Border";
    private const string PART_Title = "PART_Title";
    private const string PART_Thumbnail = "PART_Thumbnail";
    #if !SILVERLIGHT
    private const string PART_IconContainer = "PART_IconContainer";
    private const string PART_Overlay = "PART_Overlay";
    #endif

    // VSM states
    private const string VSMSTATE_StateNormal = "Normal";
    private const string VSMSTATE_StateMouseOver = "MouseOver";

    // Style typed properties
    private const string PROPERTY_IconBorderStyle = "IconBorderStyle";

    #endregion

    #region --- Fields ---

    private Border border;
    private TextBlock title;
    private Image thumbnail;
    private bool selected;
    #if !SILVERLIGHT
    private Border iconContainer;
    private Border overlay;
    #endif

    #endregion

    #region --- Constructor ---

    #if !SILVERLIGHT
    static WindowIcon()
    {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(WindowIcon), new FrameworkPropertyMetadata(typeof(WindowIcon)));
    }
    #endif

    public WindowIcon()
    {
      DefaultStyleKey = typeof(WindowIcon); //This is required, else GetTemplatePart will fail to get PART_Border etc. for the WindowIcon //Make sure the respective XAML style doesn't have an x:Key attribute, but only a TargetType="controls:WindowIcon" 
    }

    #endregion

    #region --- Properties ---

    #region IconBorderStyle

    /// <summary>
    /// Gets or sets the style of the WindowIcon.
    /// </summary>
    public Style IconBorderStyle
    {
      get { return GetValue(IconBorderStyleProperty) as Style; }
      set { SetValue(IconBorderStyleProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="Iconbar.IconBorderStyleProperty" /> dependency property.
    /// </summary>
    public static readonly DependencyProperty IconBorderStyleProperty =
        DependencyProperty.Register(
            "IconBorderStyle",
            typeof(Style),
            typeof(WindowIcon),
            new PropertyMetadata(IconBorderStylePropertyChanged));

    /// <summary>
    /// IconBorderStyle PropertyChangedCallback call back static function.
    /// </summary>
    /// <param name="d">WindowIcon object whose IconBorderStyle property is changed.</param>
    /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
    private static void IconBorderStylePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      WindowIcon windowIcon = (WindowIcon)d;
      if (windowIcon != null && windowIcon.border != null)
      {
        windowIcon.border.Style = e.NewValue as Style;
      }
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
    /// Identifies the <see cref="WindowIcon.IconWidth" /> dependency property.
    /// </summary>
    /// <value>
    /// The identifier for the <see cref="WindowIcon.IconWidth" /> dependency property.
    /// </value>
    public static readonly DependencyProperty IconWidthProperty =
        DependencyProperty.Register(
        "IconWidth",
        typeof(double),
        typeof(WindowIcon),
        null);

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
    /// Identifies the <see cref="WindowIcon.IconHeight" /> dependency property.
    /// </summary>
    /// <value>
    /// The identifier for the <see cref="WindowIcon.IconHeight" /> dependency property.
    /// </value>
    public static readonly DependencyProperty IconHeightProperty =
        DependencyProperty.Register(
        "IconHeight",
        typeof(double),
        typeof(WindowIcon),
        null);

    #endregion

    #region Title

    /// <summary>
    /// Gets or sets the window title that is displayed on the icon />.
    /// </summary>
    /// <value>The title displayed on the icon.</value>
    public string Title { get; set; }

    #endregion

    #region Thumbnail

    /// <summary>
    /// Gets or sets the window's icon.
    /// </summary>
    /// <value>The window's icon.</value>
    public ImageSource Thumbnail
    {
      get { return (ImageSource)GetValue(ThumbnailProperty); }
      set { SetValue(ThumbnailProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="WindowIcon.Thumbnail" /> dependency property.
    /// </summary>
    /// <value>
    /// The identifier for the <see cref="WindowIcon.Thumbnail" /> dependency property.
    /// </value>
    public static readonly DependencyProperty ThumbnailProperty =
        DependencyProperty.Register(
        "Thumbnail",
        typeof(ImageSource),
        typeof(WindowIcon),
        new FrameworkPropertyMetadata(null
          #if !SILVERLIGHT
          , FrameworkPropertyMetadataOptions.AffectsRender
         #endif
         ));

    #endregion

    #if !SILVERLIGHT

    #region Icon

    /// <summary>
    /// Gets or sets a FrameworkElement that is displayed as an icon of the window. 
    /// </summary>
    /// <value>
    /// The FrameworkElement displayed as an icon of the window. The default is null.
    /// </value>
    public FrameworkElement Icon
    {
      get { return GetValue(IconProperty) as FrameworkElement; }
      set { SetValue(IconProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="WindowIcon.Icon" /> dependency property.
    /// </summary>
    /// <value>
    /// The identifier for the <see cref="WindowIcon.Icon" /> dependency property.
    /// </value>
    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(
        "Icon",
        typeof(FrameworkElement),
        typeof(WindowIcon),
        null);

    #endregion

    #endif

    #region FloatingWindow

    /// <summary>
    /// Gets or sets the FloatingWindow associated with the icon.
    /// </summary>
    /// <value>Floating window.</value>
    public FloatingWindow Window { get; set; }

    #endregion

    #region Selected

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="WindowIcon"/> is selected.
    /// </summary>
    /// <value><c>true</c> if selected; otherwise, <c>false</c>.</value>
    public bool Selected
    {
      get { return selected; }
      set
      {
        if (value != selected)
        {
          selected = value;
          VisualStateManager.GoToState(
              this,
              value ? VSMSTATE_StateMouseOver : VSMSTATE_StateNormal,
              true);
        }
      }
    }

    #endregion

    #endregion

    #region --- Methods ---

    /// <summary>
    /// Sets styles that are applied for different template parts.
    /// </summary>
    private void SetStyles()
    {
      if (border != null && this.IconBorderStyle != null)
        border.Style = this.IconBorderStyle;
    }

    #if !SILVERLIGHT

    /// <summary>
    /// Sets the window icon.
    /// </summary>
    private void SetIcon()
    {
      if (Thumbnail != null)
      {
        // Display a bitmap thumbnail using the Image control
        thumbnail.Source = Thumbnail;
      }
      else if (Icon != null && Icon is FrameworkElement)
      {
        ScaleIcon(Icon);

        // The Icon is a FrameworkElement and we place it into a container
        Icon.MoveToContainer(iconContainer);

        iconContainer.Width = Icon.Width;
        iconContainer.Height = Icon.Height;

        // And display an overlay, preventing interacting with internal elements of the FrameworkElement
        overlay.Visibility = System.Windows.Visibility.Visible;
      }
    }

    /// <summary>
    /// Scales down the icon to fit it into the window's thumbnail.
    /// </summary>
    /// <param name="element">The FrameworkElement.</param>
    private void ScaleIcon(FrameworkElement element)
    {
      double minScale = element.GetScaleFactorToFit(IconWidth, IconHeight);

      if (minScale < 1)
      {
        var transformGroup = element.RenderTransform as TransformGroup;
        transformGroup = (transformGroup == null) ? new TransformGroup() : transformGroup.Clone();

        var scaleTransform = new ScaleTransform(minScale, minScale);
        transformGroup.Children.Add(scaleTransform);

        element.Width = element.Width * minScale;
        element.Height = element.Height * minScale;
        element.RenderTransformOrigin = new Point(0, 0);
        element.RenderTransform = transformGroup;
      }
    }

    #endif

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

    #endregion

    #region --- Events ---

    /// <summary>
    /// Builds the visual tree for the <see cref="WindowIcon" /> control 
    /// when a new template is applied.
    /// </summary>
    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      border = GetTemplatePart<Border>(PART_Border);
      title = GetTemplatePart<TextBlock>(PART_Title);
      thumbnail = GetTemplatePart<Image>(PART_Thumbnail);
#if !SILVERLIGHT
      iconContainer = GetTemplatePart<Border>(PART_IconContainer);
      overlay = GetTemplatePart<Border>(PART_Overlay);
#endif

      SetStyles();
#if !SILVERLIGHT
      SetIcon();
#endif

      title.Text = Title;
      title.FlowDirection = this.FlowDirection;
#if SILVERLIGHT
      thumbnail.Source = Thumbnail;
#endif
    }

    #endregion

  }
}
