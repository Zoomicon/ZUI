using System.Windows;
using System.Windows.Controls;

namespace FloatingWindowControl
{
    public partial class MyChildWindow : ChildWindow
    {
        public MyChildWindow()
        {
            InitializeComponent();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}

