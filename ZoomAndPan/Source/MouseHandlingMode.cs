namespace ZoomAndPan
{

  /// <summary>
  /// Defines the current state of the mouse handling logic.
  /// </summary>
  public enum MouseHandlingMode
  {

    /// <summary>
    /// Not in any special mode.
    /// </summary>
    None,

    /// <summary>
    /// The user is left-mouse-button-dragging to pan the viewport. (if drag to select was supported shift key would be needed here)
    /// </summary>
    Panning,

    /// <summary>
    /// The user is holding down ctrl and left-clicking (or upwards-dragging), else right-clicking (or downwards-dragging) to zoom in or out.
    /// </summary>
    Zooming,

    /// <summary>
    /// The user is holding down ctrl and left-mouse-button-dragging to select a region to zoom to.
    /// </summary>
    DragZooming,

    /// <summary>
    /// The user is holding down ctrl and shift and drags to both pan and zoom
    /// </summary>
    PanningAndZooming,

  }
}