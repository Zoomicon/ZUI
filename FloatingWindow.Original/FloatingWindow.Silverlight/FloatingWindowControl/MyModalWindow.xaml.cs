using System.Windows;
using SilverFlow.Controls;

namespace FloatingWindowControl
{
    public partial class MyModalWindow : FloatingWindow
    {
        public MyModalWindow()
        {
            InitializeComponent();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }
    }
}
