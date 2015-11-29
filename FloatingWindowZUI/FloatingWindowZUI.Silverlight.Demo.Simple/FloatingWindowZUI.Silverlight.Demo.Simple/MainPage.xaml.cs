//Filename MainPage.xaml.cs
//Version: 20150703

using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using SilverFlow.Controls;
using SilverFlow.Controls.Extensions;

namespace FloatingWindowZUI.Demo
{
  public partial class MainPage : UserControl
  {

    private int nWindows = 1;

    public MainPage()
    {
      InitializeComponent();
      ShowNewWindow(false); //you can also instantiate FloatingWindows in XAML (see MainPage.xaml)
    }

    private void ShowNewWindow(bool withIcon)
    {
      FloatingWindow window = (withIcon)? new WindowWithIcon() : new FloatingWindow();

      if (Application.Current.Host.Settings.EnableGPUAcceleration) //GPU acceleration can been turned on at HTML/ASPX page or at OOB settings for OOB apps
        window.CacheMode = new BitmapCache(); //must do this before setting the "Scale" property, since it will set "RenderAtScale" property of the BitmapCache
      
      window.Scale = 1d / host.ZoomHost.ContentScale; //TODO: !!! don't use host.ContentScale, has bug and is always 1
          
      host.Add(window);

      string title = "Window " + nWindows++;
      window.Title = title;
      window.IconText = title;

      window.Activated += (s, a) =>
      {
        Debug.WriteLine("Activated: {0}", window.IconText);
      };

      window.Deactivated += (s, a) =>
      {
        Debug.WriteLine("Deactivated: {0}", window.IconText);
      };

      window.HelpRequested += (s, a) =>
      {
        Debug.WriteLine("Help button pressed");
        MessageBox.Show("Help...");
      };

      window.OptionsRequested += (s, a) =>
      {
        Debug.WriteLine("Options button pressed");
        MessageBox.Show("Options...");
      };

      //Point startPoint = new Point(host.ZoomHost.ContentOffsetX + host.ZoomHost.ContentViewportWidth / 2, host.ZoomHost.ContentOffsetY + host.ZoomHost.ContentViewportHeight / 2); //Center at current view
      //window.Show(startPoint, true);

      window.Show(); //show centered
    }

    private void ShowNewWindow_Click(object sender, RoutedEventArgs e)
    {
      ShowNewWindow(false);
    }

    private void ShowWindowWithIcon_Click(object sender, RoutedEventArgs e)
    {
      ShowNewWindow(true);
    }

    private void ShowIconbar_Click(object sender, RoutedEventArgs e)
    {
      host.IsIconbarVisible = true;
    }

    private void HideIconbar_Click(object sender, RoutedEventArgs e)
    {
      host.IsIconbarVisible = false;
    }

    private void CloseWindows_Click(object sender, RoutedEventArgs e)
    {
      host.CloseAllWindows();
    }

  }
}
