using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SilverFlow.Controls.Extensions;

namespace SilverFlow.Controls
{
    /// <summary>
    /// Window icon containing a thumbnail and title of the windows.
    /// </summary>
    [TemplatePart(Name = PART_Border, Type = typeof(Border))]
    [TemplatePart(Name = PART_Title, Type = typeof(TextBlock))]
    [TemplatePart(Name = PART_Thumbnail, Type = typeof(Image))]
    [TemplatePart(Name = PART_IconContainer, Type = typeof(Border))]
    [TemplatePart(Name = PART_Overlay, Type = typeof(Border))]
    [StyleTypedProperty(Property = PROPERTY_IconBorderStyle, StyleTargetType = typeof(Border))]
    public class WindowIcon : Button
    {
        // Template parts
        private const string PART_Border = "PART_Border";
        private const string PART_Title = "PART_Title";
        private const string PART_Thumbnail = "PART_Thumbnail";
        private const string PART_IconContainer = "PART_IconContainer";
        private const string PART_Overlay = "PART_Overlay";

        // VSM states
        private const string VSMSTATE_StateNormal = "Normal";
        private const string VSMSTATE_StateMouseOver = "MouseOver";

        // Style typed properties
        private const string PROPERTY_IconBorderStyle = "IconBorderStyle";

        #region public Style IconBorderStyle

        /// <summary>
        /// Gets or sets the style of the WindowIcon.
        /// </summary>
        public Style IconBorderStyle
        {
            get { return GetValue(IconBorderStyleProperty) as Style; }
            set { SetValue(IconBorderStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="IconBar.IconBorderStyleProperty" /> dependency property.
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

        #region public ImageSource Thumbnail

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
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        #endregion

        #region public FrameworkElement Icon

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

        private Border border;
        private TextBlock title;
        private Image thumbnail;
        private Border iconContainer;
        private Border overlay;

        /// <summary>
        /// Gets or sets the window title that is displayed on the icon />.
        /// </summary>
        /// <value>The title displayed on the icon.</value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the FloatingWindow associated with the icon.
        /// </summary>
        /// <value>Floating window.</value>
        public FloatingWindow Window { get; set; }

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
            iconContainer = GetTemplatePart<Border>(PART_IconContainer);
            overlay = GetTemplatePart<Border>(PART_Overlay);

            SetStyles();
            SetIcon();

            title.Text = Title;
            title.FlowDirection = this.FlowDirection;
        }

        /// <summary>
        /// Sets styles that are applied for different template parts.
        /// </summary>
        private void SetStyles()
        {
            if (border != null && this.IconBorderStyle != null)
                border.Style = this.IconBorderStyle;
        }

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
