//Filename: ZoomAndPanControl_IScrollInfo_Silverlight
//Version: 20120609
//Author: George Birbilis <zoomicon.com>

#if SILVERLIGHT

using System.Windows;
using Compatibility;

namespace ZoomAndPan
{
    /// <summary>
    /// This is an extension to the ZoomAndPanControl class that implements
    /// the IScrollInfo interface "MakeVisible" method for Silverlight.
    ///     
    /// </summary>   
    public partial class ZoomAndPanControl
    {
                
        #region Methods

        /// <summary>
        /// Bring the specified rectangle to view.
        /// </summary>
        public Rect MakeVisible(UIElement visual, Rect rectangle) //Silverlight
        {
            return MakeVisible(new Visual(visual), rectangle);
        }

        #endregion
        
    }

}

#endif