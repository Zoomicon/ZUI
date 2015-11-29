//Filename: IconbarZUI.cs
//Version: 20140904

using SilverFlow.Controls;
using System.Windows;
using System.Windows.Controls;
using ZoomAndPan;

namespace FloatingWindowZUI
{
  [TemplatePart(Name = PART_LayoutRoot, Type = typeof(FrameworkElement))]
  [TemplatePart(Name = PART_FixedBar, Type = typeof(Border))]
  [TemplatePart(Name = PART_SlidingBar, Type = typeof(Border))]
  [TemplatePart(Name = PART_Carousel, Type = typeof(StackPanel))]
  [TemplateVisualState(Name = VSMSTATE_StateOpen, GroupName = VSMGROUP_States)]
  [TemplateVisualState(Name = VSMSTATE_StateClosed, GroupName = VSMGROUP_States)]
  [StyleTypedProperty(Property = PROPERTY_IconbarStyle, StyleTargetType = typeof(Border))]
  [StyleTypedProperty(Property = PROPERTY_WindowIconStyle, StyleTargetType = typeof(WindowIcon))]
  public class IconbarZUI : Iconbar
  {

    #region --- Constructor ---

    #if !SILVERLIGHT
    static IconbarZUI()
    {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(IconbarZUI), new FrameworkPropertyMetadata(typeof(IconbarZUI)));
    }
    #endif

    public IconbarZUI()
    {
      DefaultStyleKey = typeof(IconbarZUI);
    }

    #endregion

    #region --- Events ---

    public override void OnApplyTemplate() //TODO: CHECK (not sure it's true) - if we want to be able to set a new template to IconbarZUI, then we need to override this (ancestor's implementation won't be called automatically)
    {
      base.OnApplyTemplate();
    }

    protected override void Icon_Click(object sender, RoutedEventArgs e)
    {
      base.Icon_Click(sender, e);

      WindowIcon icon = sender as WindowIcon;
      if (icon == null) return;

      FloatingWindow w = icon.Window;
      if (w == null) return;

      FrameworkElement host = w.HostPanel;
      if (host == null) return;

      ZoomAndPanControl zoomHost = host.Parent as ZoomAndPanControl;
      if (zoomHost == null) return;

      zoomHost.ScrollToCenter(w.BoundingRectangle); //zoomHost.ZoomTo(w.BoundingRectangle);
    }

    #endregion

  }

}
