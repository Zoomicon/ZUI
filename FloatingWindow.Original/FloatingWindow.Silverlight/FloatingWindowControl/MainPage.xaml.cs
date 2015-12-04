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
        private int nCharts = 1;
        private Point startPoint = new Point(50, 50);
        private DetailsForm detailsForm = null;

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

        private void ShowPopulation_Click(object sender, RoutedEventArgs e)
        {
            Population window = new Population();
            host.Add(window);

            window.Activated += (s, a) =>
            {
                Debug.WriteLine("Activated: {0}", window.IconText);
            };

            window.Deactivated += (s, a) =>
            {
                Debug.WriteLine("Deactivated: {0}", window.IconText);
            };

            window.Show(new Thickness(50, 100, 50, 150));
        }

        private void ShowDetailsForm_Click(object sender, RoutedEventArgs e)
        {
            // If the DetailsForm is not created yet, or it is already closed
            if (detailsForm == null)
            {
                // Create the window
                detailsForm = new DetailsForm();

                // Set detailsForm to null when the window is closed
                detailsForm.Closed += (s, ea) =>
                    {
                        detailsForm = null;
                    };

                // Add the window to the FloatingWindowHost
                host.Add(detailsForm);

                // Restore window size and position
                detailsForm.RestoreSizeAndPosition();

                detailsForm.Show();
            }
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

            window.Show(200, 100);
        }

        private void ShowWindowWithChart_Click(object sender, RoutedEventArgs e)
        {
            WindowWithChart window = new WindowWithChart();
            host.Add(window);
            string title = "Chart " + nCharts++;
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

            window.Show(460, 60);
        }

        private void ShowModalWindow_Click(object sender, RoutedEventArgs e)
        {
            MyModalWindow window = new MyModalWindow();
            host.Add(window);

            window.Activated += (s, a) =>
            {
                Debug.WriteLine("Activated: {0}", window.IconText);
            };

            window.Deactivated += (s, a) =>
            {
                Debug.WriteLine("Deactivated: {0}", window.IconText);
            };

            window.ShowModal();
            window.Closed += (s, a) =>
                {
                    bool? result = window.DialogResult;
                };
        }

        private void ShowChildWindow_Click(object sender, RoutedEventArgs e)
        {
            MyChildWindow childWindow = new MyChildWindow();
            childWindow.Show();
        }

        private void ShowIconbar_Click(object sender, RoutedEventArgs e)
        {
            host.ShowIconBar();
        }

        private void HideIconbar_Click(object sender, RoutedEventArgs e)
        {
            host.HideIconBar();
        }

        private void CloseWindows_Click(object sender, RoutedEventArgs e)
        {
            host.CloseAllWindows();
        }
    }
}
