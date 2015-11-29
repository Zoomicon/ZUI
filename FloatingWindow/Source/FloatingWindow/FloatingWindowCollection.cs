//Filename: FloatingWindowCollection.cs
//Version: 20131213

using System.Collections.ObjectModel;
using System.Windows;

namespace SilverFlow.Controls
{

  public class FloatingWindowCollection : ObservableCollection<FloatingWindow>
  {

    public bool ShowScreenshotButton
    {
      set
      {
        foreach (FloatingWindow w in this)
          w.ShowScreenshotButton = value;
      }
    }
    
    public bool ShowHelpButton
    {
      set
      {
        foreach (FloatingWindow w in this)
          w.ShowHelpButton = value;
      }
    }

    public bool ShowOptionsButton
    {
      set
      {
        foreach (FloatingWindow w in this)
          w.ShowOptionsButton = value;
      }
    }

    public void RemoveAll() //TODO: Windows.Clear() doesn't seem to work
    {
      FloatingWindow[] c = new FloatingWindow[Count];
      CopyTo(c, 0);
      foreach (FloatingWindow w in c) Remove(w);
    }

    public Rect GetBoundingRectangle(bool notMinimized = true)
    {
        if (Count > 0)
        {
          Rect result = Items[0].BoundingRectangle;
          for (int i = 1; i < Count; i++)
            if (!notMinimized || Items[i].WindowState != WindowState.Minimized)
              result.Union(Items[i].BoundingRectangle);
          return result;
        }
        return Rect.Empty;
    }
    
  }

}
