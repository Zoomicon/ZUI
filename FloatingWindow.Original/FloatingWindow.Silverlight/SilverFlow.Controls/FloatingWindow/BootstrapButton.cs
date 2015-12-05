using System.Windows;
using System.Windows.Controls;

namespace SilverFlow.Controls
{
    /// <summary>
    /// Two-state button with an up/down arrow.
    /// </summary>
    [TemplateVisualState(Name = VSMSTATE_StateClose, GroupName = VSMGROUP_ButtonStates)]
    [TemplateVisualState(Name = VSMSTATE_StateOpen, GroupName = VSMGROUP_ButtonStates)]
    public class BootstrapButton : Button
    {
        // VSM groups
        private const string VSMGROUP_ButtonStates = "ButtonStates";

        // VSM states
        private const string VSMSTATE_StateOpen = "Open";
        private const string VSMSTATE_StateClose = "Close";

        #region public bool IsOpen

        /// <summary>
        /// Gets or sets a value indicating whether the bootstrap button is in the "Open" state.
        /// </summary>
        /// <value><c>true</c> if the bootstrap button is in the "Open" state; otherwise, <c>false</c>.</value>
        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="BootstrapButton.IsOpen" /> dependency property.
        /// </summary>
        /// <value>
        /// The identifier for the <see cref="BootstrapButton.IsOpen" /> dependency property.
        /// </value>
        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register(
            "IsOpen",
            typeof(bool),
            typeof(BootstrapButton),
            new PropertyMetadata(false, IsOpenPropertyChanged));

        /// <summary>
        /// IsOpenProperty PropertyChangedCallback call back static function.
        /// </summary>
        /// <param name="d">BootstrapButton object whose IsOpenProperty property is changed.</param>
        /// <param name="e">DependencyPropertyChangedEventArgs which contains the old and new values.</param>
        private static void IsOpenPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BootstrapButton button = (BootstrapButton)d;
            VisualStateManager.GoToState(
                button,
                (bool)e.NewValue ? VSMSTATE_StateClose : VSMSTATE_StateOpen, 
                true);
        }

        #endregion

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Controls.Primitives.ButtonBase.Click"/> event.
        /// </summary>
        protected override void OnClick()
        {
            // Toggle open/closed state
            IsOpen = !IsOpen;

            base.OnClick();
        }
    }
}
