using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using SilverFlow.Controls;

namespace FloatingWindowWPF
{
    /// <summary>
    /// Interaction logic for Population.xaml
    /// </summary>
    public partial class Population : FloatingWindow
    {
        public Population()
        {
            InitializeComponent();
        }

        private void Population_Loaded(object sender, RoutedEventArgs e)
        {
            dataGrid.ItemsSource = GetPopulation();
        }

        private List<CityPopulation> GetPopulation()
        {
            var list = new List<CityPopulation>();
            XElement doc = XElement.Load(@"Population.xml");

            return (from el in doc.Elements()
                    select new CityPopulation
                    {
                        City = el.Element("City").Value,
                        State = el.Element("State").Value,
                        Population = el.Element("Population").Value
                    }
                   ).ToList();
        }
    }
}
