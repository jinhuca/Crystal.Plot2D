using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Crystal.Plot2D.Charts;
using Crystal.Plot2D.Common;
using Crystal.Plot2D.Common.Auxiliary;
using Crystal.Plot2D.Transforms;

namespace Crystal.Plot2D.Navigation;

/// <summary>
/// Represents a navigation methods upon one axis - mouse panning and zooming.
/// </summary>
[Obsolete(message: "Will be removed soon. Use AxisNavigation instead.")]
public class OldAxisNavigation : ContentGraph
{
  /// <summary>
  /// Initializes a new instance of the <see cref="AxisNavigation"/> class.
  /// </summary>
  public OldAxisNavigation()
  {
    Focusable = false;

    SetHorizontalOrientation();
    Content = content;
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="AxisNavigation"/> class.
  /// </summary>
  /// <param name="orientation">The orientation.</param>
  public OldAxisNavigation(Orientation orientation)
  {
    Orientation = orientation;
    Content = content;
  }

  private void SetHorizontalOrientation()
  {
    Grid.SetColumn(element: this, value: 1);
    Grid.SetRow(element: this, value: 2);
  }

  private void SetVerticalOrientation()
  {
    // todo should automatically search for location of axes as they can be 
    // not only from the left or bottom.
    Grid.SetColumn(element: this, value: 0);
    Grid.SetRow(element: this, value: 1);
  }

  private Orientation orientation = Orientation.Horizontal;
  /// <summary>
  /// Gets or sets the orientation of AxisNavigation.
  /// </summary>
  /// <value>The orientation.</value>
  public Orientation Orientation
  {
    get => orientation;
    set
    {
      if (orientation != value)
      {
        orientation = value;
        OnOrientationChanged();
      }
    }
  }

  private void OnOrientationChanged()
  {
    switch (orientation)
    {
      case Orientation.Horizontal:
        SetHorizontalOrientation();
        break;
      case Orientation.Vertical:
        SetVerticalOrientation();
        break;
    }
  }

  private bool lmbPressed;
  private Point dragStart;

  private CoordinateTransform Transform => Plotter2D.Viewport.Transform;

  protected override Panel HostPanel => Plotter2D.MainGrid;

  private readonly Panel content = new Canvas { Background = Brushes.Transparent };
  private readonly SolidColorBrush fillBrush = new SolidColorBrush(color: Color.FromRgb(r: 255, g: 228, b: 209)).MakeTransparent(opacity: 0.2);

  private Point lmbInitialPosition;
  protected Point LmbInitialPosition => lmbInitialPosition;

  protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
  {
    base.OnMouseLeftButtonDown(e: e);

    lmbInitialPosition = e.GetPosition(relativeTo: this);

    dragStart = lmbInitialPosition.ScreenToViewport(transform: Transform);
    lmbPressed = true;

    content.Background = fillBrush;

    CaptureMouse();
  }

  protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
  {
    base.OnMouseLeftButtonUp(e: e);
    lmbPressed = false;

    ClearValue(dp: CursorProperty);
    content.Background = Brushes.Transparent;

    ReleaseMouseCapture();
  }

  protected override void OnMouseMove(MouseEventArgs e)
  {
    if (lmbPressed)
    {
      var mousePos = e.GetPosition(relativeTo: this).ScreenToViewport(transform: Transform);

      var visible = Plotter2D.Viewport.Visible;
      double delta;
      if (orientation == Orientation.Horizontal)
      {
        delta = (mousePos - dragStart).X;
        visible.XMin -= delta;
      }
      else
      {
        delta = (mousePos - dragStart).Y;
        visible.YMin -= delta;
      }

      if (e.GetPosition(relativeTo: this) != lmbInitialPosition)
      {
        Cursor = orientation == Orientation.Horizontal ? Cursors.ScrollWE : Cursors.ScrollNS;
      }

      Plotter2D.Viewport.Visible = visible;
    }
  }

  private const double wheelZoomSpeed = 1.2;
  protected override void OnMouseWheel(MouseWheelEventArgs e)
  {
    var mousePos = e.GetPosition(relativeTo: this);
    var delta = -e.Delta;

    var zoomTo = mousePos.ScreenToViewport(transform: Transform);

    double zoomSpeed = Math.Abs(value: delta / Mouse.MouseWheelDeltaForOneLine);
    zoomSpeed *= wheelZoomSpeed;
    if (delta < 0)
    {
      zoomSpeed = 1 / zoomSpeed;
    }

    var visible = Plotter2D.Viewport.Visible.Zoom(to: zoomTo, ratio: zoomSpeed);
    var oldVisible = Plotter2D.Viewport.Visible;
    if (orientation == Orientation.Horizontal)
    {
      visible.YMin = oldVisible.YMin;
      visible.Height = oldVisible.Height;
    }
    else
    {
      visible.XMin = oldVisible.XMin;
      visible.Width = oldVisible.Width;
    }
    Plotter2D.Viewport.Visible = visible;

    e.Handled = true;
  }

  protected override void OnLostFocus(RoutedEventArgs e)
  {
    base.OnLostFocus(e: e);

    ClearValue(dp: CursorProperty);
    content.Background = Brushes.Transparent;

    ReleaseMouseCapture();
  }
}
