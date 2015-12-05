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
    [StyleTypedProperty(Property = PROPERTY_IconBorderStyle, StyleTargetType = typeof(Border))]
    public class WindowIcon : Button
    {
        // Template parts
        private const string PART_Border = "PART_Border";
        private const string PART_Title = "PART_Title";
        private const string PART_Thumbnail = "PART_Thumbnail";

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

        private Border border;
        private TextBlock title;
        private Image thumbnail;
        private bool selected;

        /// <summary>
        /// Gets or sets the window title that is displayed on the icon />.
        /// </summary>
        /// <value>The title displayed on the icon.</value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the thumbnail.
        /// </summary>
        /// <value>The thumbnail.</value>
        public ImageSource Thumbnail { get; set; }

        /// <summary>
        /// Gets or sets the FloatingWindow associated with the icon.
        /// </summary>
        /// <value>Floating window.</value>
        public FloatingWindow Window { get; set; }

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

            SetStyles();

            title.Text = Title;
            title.FlowDirection = this.FlowDirection;
            thumbnail.Source = Thumbnail;
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
