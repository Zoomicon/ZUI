using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace TestZoom
{
    public class Helper
    {
        public double ContentHeight
        {
            get { return DataModel.Instance.ContentHeight; }
        }
        public double ContentScale
        {
            get { return DataModel.Instance.ContentScale; }
        }
        public double ContentOffsetX
        {
            get { return DataModel.Instance.ContentOffsetX; }
        }
        public double ContentOffsetY
        {
            get { return DataModel.Instance.ContentOffsetY; }
        }
        public double ContentViewportWidth
        {
            get { return DataModel.Instance.ContentViewportWidth; }
        }
        public double ContentViewportHeight
        {
            get { return DataModel.Instance.ContentViewportHeight; }
        }

    }
}
