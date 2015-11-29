//Filename: ControlExtensions.cs
//Version: 20150721

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

using System.Collections.Generic;

#if SILVERLIGHT
using Decorator = System.Windows.Controls.Border;
using System.Globalization;
#endif

namespace SilverFlow.Controls.Extensions
{

    /// <summary>
    /// Useful WPF/Silverlight controls extensions
    /// </summary>
  public static class ControlExtensions
  {
    /// <summary>
    /// Sets the visible of the UIElement.
    /// </summary>
    /// <param name="uiElement">The UI element.</param>
    /// <param name="visible">Set Visible if <c>true</c>; otherwise - Collapsed.</param>
    public static void SetVisible(this UIElement uiElement, bool visible = true)
    {
      if (uiElement != null)
      {
        uiElement.Visibility = (visible) ? Visibility.Visible : Visibility.Collapsed;
      }
    }

    /// <summary>
    /// Determines whether the specified UI element is visible.
    /// </summary>
    /// <param name="uiElement">The UI element.</param>
    /// <returns><c>true</c> if the specified UI element is visible; otherwise, <c>false</c>.</returns>
    public static bool IsVisible(this UIElement uiElement)
    {
      return uiElement != null && uiElement.Visibility == Visibility.Visible;
    }

    /// <summary>
    /// Determines whether the specified element contains the point.
    /// </summary>
    /// <param name="element">This FrameworkElement.</param>
    /// <param name="point">The point to check.</param>
    /// <param name="origin">Relative origin (optional).</param>
    /// <returns><c>true</c> if the specified element contains the point; otherwise, <c>false</c>.</returns>
    public static bool ContainsPoint(this FrameworkElement element, Point point, UIElement origin = null)
    {
      bool result = false;

      if (element != null)
      {
        Point elementOrigin = (origin == null) ? new Point(0, 0) : element.GetRelativePosition(origin);
        Rect rect = new Rect(elementOrigin, new Size(element.ActualWidth, element.ActualHeight));
        result = rect.Contains(point);
      }

      return result;
    }

    /// <summary>
    /// Gets position of the element relative to another specified (root) element.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <param name="rootElement">The root element.</param>
    /// <returns>Relative position.</returns>
    /// <exception cref="ArgumentNullException">Input parameter is null.</exception>
    public static Point GetRelativePosition(this UIElement element, UIElement rootElement)
    {
      if (element == null)
        throw new ArgumentNullException("element");

      if (rootElement == null)
        throw new ArgumentNullException("rootElement");

      var transformFromClientToRoot = element.TransformToVisual(rootElement);
      return transformFromClientToRoot.Transform(new Point(0, 0));
    }

    /// <summary>
    /// Gets total horizontal value of the specified thickness.
    /// </summary>
    /// <param name="thickness">The thickness.</param>
    /// <returns>Horizontal thickness.</returns>
    /// <exception cref="ArgumentNullException">Input parameter is null.</exception>
    public static double Horizontal(this Thickness thickness)
    {
      if (thickness == null)
        throw new ArgumentNullException("thickness");

      return thickness.Left + thickness.Right;
    }

    /// <summary>
    /// Gets total vertical value of the specified thickness.
    /// </summary>
    /// <param name="thickness">The thickness.</param>
    /// <returns>Vertical thickness.</returns>
    /// <exception cref="ArgumentNullException">Input parameter is null.</exception>
    public static double Vertical(this Thickness thickness)
    {
      if (thickness == null)
        throw new ArgumentNullException("thickness");

      return thickness.Top + thickness.Bottom;
    }

    /// <summary>
    /// Gets the actual bounding rectangle of the FrameworkElement.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <returns>Actual bounding rectangle.</returns>
    public static Rect GetActualBoundingRectangle(this FrameworkElement element)
    {
      if (element == null)
        throw new ArgumentNullException("element");

      return new Rect(0, 0, element.ActualWidth, element.ActualHeight); //TODO: see where this is used, maybe causing issues when using scale parameter
    }

    /// <summary>
    /// Gets a PropertyPath for the TranslateTransform.X property.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <returns>A path to the property.</returns>
    public static PropertyPath GetPropertyPathForTranslateTransformX(this UIElement element)
    {
      return element.GetPropertyPathForTranslateTransform("X");
    }

    /// <summary>
    /// Gets a PropertyPath for the TranslateTransform.Y property.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <returns>A path to the property.</returns>
    public static PropertyPath GetPropertyPathForTranslateTransformY(this UIElement element)
    {
      return element.GetPropertyPathForTranslateTransform("Y");
    }

    /// <summary>
    /// Gets scale factor to fit the element to the specified size.
    /// </summary>
    /// <param name="element">The element to scale.</param>
    /// <param name="maxWidth">Maximal width of the element.</param>
    /// <param name="maxHeight">Maximal height of the element.</param>
    /// <returns>Scale factor required to fit the element to the specified size.</returns>
    public static double GetScaleFactorToFit(this FrameworkElement element, double maxWidth, double maxHeight)
    {
      if (element == null)
        throw new ArgumentNullException("element");

      double width = element.Width.IsNotSet() ? element.ActualWidth : element.Width;
      double height = element.Height.IsNotSet() ? element.ActualHeight : element.Height;

      double scaleX = maxWidth / width;
      double scaleY = maxHeight / height;
      double minScale = Math.Min(scaleX, scaleY);

      return minScale;
    }

    /// <summary>
    /// Moves the element to the new container. The container can be a Panel or a Decorator (Border in Silverlight) derived class.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <param name="newContainer">The new container of the element.</param>
    /// <exception cref="ArgumentNullException">New Container is null.</exception>
    public static void MoveToContainer(this FrameworkElement element, FrameworkElement newContainer)
    {
      if (element == null)
        throw new ArgumentNullException("element");

      if (newContainer == null)
        throw new ArgumentNullException("newContainer");

      // Remove the element from the old container
      element.RemoveFromContainer();

      if (newContainer is Panel)
      {
        var container = newContainer as Panel;
        if (!container.Children.Contains(element))
          container.Children.Add(element);
      }
      else if (newContainer is Decorator)
      {
        (newContainer as Decorator).Child = element;
      }
    }

    /// <summary>
    /// Removes the element from its container. The container can be a Panel or a Decorator (Border in Silverlight) derived class.
    /// </summary>
    /// <param name="element">The element.</param>
    public static void RemoveFromContainer(this FrameworkElement element)
    {
      if (element != null && element.Parent != null)
      {
        // Remove the element from the old container
        if (element.Parent is Panel)
        {
          (element.Parent as Panel).Children.Remove(element);
        }
        else if (element.Parent is Decorator)
        {
          (element.Parent as Decorator).Child = null;
        }
      }
    }

    /// <summary>
    /// Gets a PropertyPath for the TranslateTransform property.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <param name="translateBy">"X" or "Y" TranslateTransform property.</param>
    /// <returns>A path to the property.</returns>
    private static PropertyPath GetPropertyPathForTranslateTransform(this UIElement element, string translateBy) //this is private since it's an internal helper
    {
      if (element == null)
        throw new ArgumentNullException("element");

      var transformGroup = element.RenderTransform as TransformGroup;
      var translateTransform = transformGroup.Children.OfType<TranslateTransform>().FirstOrDefault();
      int translateTransformIndex = transformGroup.Children.IndexOf(translateTransform);

      string path = string.Format("(FrameworkElement.RenderTransform).(TransformGroup.Children)[{0}].(TranslateTransform.{1})",
          translateTransformIndex, translateBy);

      return new PropertyPath(path);
    }

#if SILVERLIGHT
/*  //Note: commented out, Compatibility library already defines this externsion method
 
    /// <summary>
    /// Retrieves a set of objects that are located within a specified point of an object's coordinate space.
    /// </summary>
    /// <param name="subtree">The object to search within.</param>
    /// <param name="intersectingPoint">The point to use as the determination point.</param>
    /// <returns>
    /// An enumerable set of System.Windows.UIElement objects that are determined
    /// to be located in the visual tree composition at the specified point and within
    /// the specified subtee.
    /// </returns>
    public static IEnumerable<UIElement> FindElementsInCoordinates(this UIElement subtree, Point intersectingPoint)
    {
      return VisualTreeHelper.FindElementsInHostCoordinates(intersectingPoint, subtree);
    }
*/
#endif

    public static bool IsVisualDescendentOf(this DependencyObject element, DependencyObject ancestor) //this can throw exception "Reference is not a valid visual Dependency Object"
    {
      if (element == null) return false;
      
      #if !SILVERLIGHT
      if (!(element is Visual) && !(element is Visual3D)) return false;
      #endif
      
      DependencyObject parent = VisualTreeHelper.GetParent(element);
      return (parent == ancestor) || IsVisualDescendentOf(parent, ancestor);
    }

    public static bool IsVisualAncestorOf(this DependencyObject element, DependencyObject descendent) //this can throw exception "Reference is not a valid visual Dependency Object"
    {
      return (descendent != null) && descendent.IsVisualDescendentOf(element);
    }

    public static bool IsFocused(this DependencyObject control)
    {
      #if SILVERLIGHT
      return FocusManager.GetFocusedElement() == control; //In Silverlight 5, passing an element parameter value that is not of type Window will result in a returned value of null according to http://msdn.microsoft.com/en-us/library/ms604088(v=vs.95).aspx
      #else
      return Keyboard.FocusedElement == control;
      #endif
    }

  }
}
