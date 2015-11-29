//Version: 20150703

using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using SilverFlow.Controls;
using SilverFlow.Controls.Extensions;

namespace FloatingWindowControl
{
  public partial class MainPage : UserControl
  {
    private int nWindows = 1;
    private Point startPoint = new Point(50, 50);

    public MainPage()
    {
      InitializeComponent();

      FloatingWindow window = new FloatingWindow();
      window.Title = "Centered Window";
      window.IconText = "Centered Window";
      host.Add(window);
      window.Show();
    }

    private void ShowNewWindow_Click(object sender, RoutedEventArgs e)
    {
      FloatingWindow window = new FloatingWindow();
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

      window.Show(startPoint);
      startPoint = startPoint.Add(20, 20);
    }

    private void ShowWindowWithIcon_Click(object sender, RoutedEventArgs e)
    {
      WindowWithIcon window = new WindowWithIcon();
      host.Add(window);

      window.Activated += (s, a) =>
      {
        Debug.WriteLine("Activated: {0}", window.IconText);
      };

      window.Deactivated += (s, a) =>
      {
        Debug.WriteLine("Deactivated: {0}", window.IconText);
      };

      window.Show(startPoint);
      startPoint = startPoint.Add(20, 20);
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
