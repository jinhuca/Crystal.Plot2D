using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using Crystal.Plot2D.Common;
using Crystal.Plot2D.Common.Auxiliary;
using Crystal.Plot2D.Transforms;

namespace Crystal.Plot2D.Navigation;

/// <summary>
/// Provides common methods of mouse navigation around viewport.
/// </summary>
public sealed class MouseNavigation : NavigationBase
{
  /// <summary>
  /// Initializes a new instance of the <see cref="MouseNavigation"/> class.
  /// </summary>
  public MouseNavigation() { }

  private AdornerLayer adornerLayer;

  private AdornerLayer AdornerLayer
  {
    get
    {
      if (adornerLayer == null)
      {
        adornerLayer = AdornerLayer.GetAdornerLayer(visual: this);
        if (adornerLayer != null)
        {
          adornerLayer.IsHitTestVisible = false;
        }
      }

      return adornerLayer;
    }
  }

  protected override void OnPlotterAttached(PlotterBase plotter)
  {
    base.OnPlotterAttached(plotter: plotter);

    Mouse.AddMouseDownHandler(element: Parent, handler: OnMouseDown);
    Mouse.AddMouseMoveHandler(element: Parent, handler: OnMouseMove);
    Mouse.AddMouseUpHandler(element: Parent, handler: OnMouseUp);
    Mouse.AddMouseWheelHandler(element: Parent, handler: OnMouseWheel);

    plotter.KeyDown += OnParentKeyDown;
  }

  protected override void OnPlotterDetaching(PlotterBase plotter)
  {
    plotter.KeyDown -= OnParentKeyDown;

    Mouse.RemoveMouseDownHandler(element: Parent, handler: OnMouseDown);
    Mouse.RemoveMouseMoveHandler(element: Parent, handler: OnMouseMove);
    Mouse.RemoveMouseUpHandler(element: Parent, handler: OnMouseUp);
    Mouse.RemoveMouseWheelHandler(element: Parent, handler: OnMouseWheel);

    base.OnPlotterDetaching(plotter: plotter);
  }

  private void OnParentKeyDown(object sender, KeyEventArgs e)
  {
    if (e.Key == Key.Escape || e.Key == Key.Back)
    {
      if (isZooming)
      {
        isZooming = false;
        zoomRect = null;
        ReleaseMouseCapture();
        RemoveSelectionAdorner();

        e.Handled = true;
      }
    }
  }

  private void OnMouseWheel(object sender, MouseWheelEventArgs e)
  {
    if (!e.Handled)
    {
      var mousePos = e.GetPosition(relativeTo: this);
      var delta = -e.Delta;
      MouseWheelZoom(mousePos: mousePos, wheelRotationDelta: delta);

      e.Handled = true;
    }
  }

#if DEBUG
  public override string ToString()
  {
    if (!string.IsNullOrEmpty(value: Name))
    {
      return Name;
    }
    return base.ToString();
  }
#endif

  private bool adornerAdded;
  private RectangleSelectionAdorner selectionAdorner;
  private void AddSelectionAdorner()
  {
    if (!adornerAdded)
    {
      var layer = AdornerLayer;
      if (layer != null)
      {
        selectionAdorner = new RectangleSelectionAdorner(element: this) { Border = zoomRect };

        layer.Add(adorner: selectionAdorner);
        adornerAdded = true;
      }
    }
  }

  private void RemoveSelectionAdorner()
  {
    var layer = AdornerLayer;
    if (layer != null)
    {
      layer.Remove(adorner: selectionAdorner);
      adornerAdded = false;
    }
  }

  private void UpdateSelectionAdorner()
  {
    selectionAdorner.Border = zoomRect;
    selectionAdorner.InvalidateVisual();
  }

  private Rect? zoomRect;
  private const double wheelZoomSpeed = 1.2;
  private bool shouldKeepRatioWhileZooming;

  private bool isZooming;
  private bool IsZooming => isZooming;

  private bool isPanning;
  private bool IsPanning => isPanning;

  private Point panningStartPointInViewport;
  private Point PanningStartPointInViewport => panningStartPointInViewport;

  private Point zoomStartPoint;

  private static bool IsShiftOrCtrl
  {
    get
    {
      var currKeys = Keyboard.Modifiers;
      return (currKeys | ModifierKeys.Shift) == currKeys ||
        (currKeys | ModifierKeys.Control) == currKeys;
    }
  }

  private bool ShouldStartPanning(MouseButtonEventArgs e)
  {
    return e.ChangedButton == MouseButton.Left && Keyboard.Modifiers == ModifierKeys.None;
  }

  private bool ShouldStartZoom(MouseButtonEventArgs e)
  {
    return e.ChangedButton == MouseButton.Left && IsShiftOrCtrl;
  }

  private Point panningStartPointInScreen;

  private void StartPanning(MouseButtonEventArgs e)
  {
    panningStartPointInScreen = e.GetPosition(relativeTo: this);
    panningStartPointInViewport = panningStartPointInScreen.ScreenToViewport(transform: Viewport.Transform);

    Plotter.UndoProvider.CaptureOldValue(target: Viewport, property: Viewport2D.VisibleProperty, oldValue: Viewport.Visible);

    isPanning = true;

    // not capturing mouse because this made some tools like PointSelector not
    // receive MouseUp events on markers;
    // Mouse will be captured later, in the first MouseMove handler call.
    // CaptureMouse();

    Viewport.PanningState = Viewport2DPanningState.Panning;

    //e.Handled = true;
  }

  private void StartZoom(MouseButtonEventArgs e)
  {
    zoomStartPoint = e.GetPosition(relativeTo: this);
    if (Viewport.Output.Contains(point: zoomStartPoint))
    {
      isZooming = true;
      AddSelectionAdorner();
      CaptureMouse();
      shouldKeepRatioWhileZooming = Keyboard.Modifiers == ModifierKeys.Shift;

      e.Handled = true;
    }
  }

  private void OnMouseDown(object sender, MouseButtonEventArgs e)
  {
    // dragging
    var shouldStartDrag = ShouldStartPanning(e: e);
    if (shouldStartDrag)
    {
      StartPanning(e: e);
    }

    // zooming
    var shouldStartZoom = ShouldStartZoom(e: e);
    if (shouldStartZoom)
    {
      StartZoom(e: e);
    }

    if (!Plotter.IsFocused)
    {
      //var window = Window.GetWindow(Plotter);
      //var focusWithinWindow = FocusManager.GetFocusedElement(window) != null;

      Plotter.Focus();

      //if (!focusWithinWindow)
      //{

      // this is done to prevent other tools like PointSelector from getting mouse click event when clicking on plotter
      // to activate window it's contained within
      e.Handled = true;

      //}
    }
  }

  private void OnMouseMove(object sender, MouseEventArgs e)
  {
    if (!isPanning && !isZooming)
    {
      return;
    }

    // dragging
    if (isPanning && e.LeftButton == MouseButtonState.Pressed)
    {
      if (!IsMouseCaptured)
      {
        CaptureMouse();
      }

      var endPoint = e.GetPosition(relativeTo: this).ScreenToViewport(transform: Viewport.Transform);

      var loc = Viewport.Visible.Location;
      var shift = panningStartPointInViewport - endPoint;
      loc += shift;

      // preventing unnecessary changes, if actually visible hasn't change.
      if (shift.X != 0 || shift.Y != 0)
      {
        Cursor = Cursors.ScrollAll;

        var visible = Viewport.Visible;

        visible.Location = loc;
        Viewport.Visible = visible;
      }

      e.Handled = true;
    }
    // zooming
    else if (isZooming && e.LeftButton == MouseButtonState.Pressed)
    {
      var zoomEndPoint = e.GetPosition(relativeTo: this);
      UpdateZoomRect(zoomEndPoint: zoomEndPoint);

      e.Handled = true;
    }
  }

  private static bool IsShiftPressed()
  {
    return Keyboard.IsKeyDown(key: Key.LeftShift) || Keyboard.IsKeyDown(key: Key.RightShift);
  }

  private void UpdateZoomRect(Point zoomEndPoint)
  {
    var output = Viewport.Output;
    Rect tmpZoomRect = new(point1: zoomStartPoint, point2: zoomEndPoint);
    tmpZoomRect = Rect.Intersect(rect1: tmpZoomRect, rect2: output);

    shouldKeepRatioWhileZooming = IsShiftPressed();
    if (shouldKeepRatioWhileZooming)
    {
      var currZoomRatio = tmpZoomRect.Width / tmpZoomRect.Height;
      var zoomRatio = output.Width / output.Height;
      if (currZoomRatio < zoomRatio)
      {
        var oldHeight = tmpZoomRect.Height;
        var height = tmpZoomRect.Width / zoomRatio;
        tmpZoomRect.Height = height;
        if (!tmpZoomRect.Contains(point: zoomStartPoint))
        {
          tmpZoomRect.Offset(offsetX: 0, offsetY: oldHeight - height);
        }
      }
      else
      {
        var oldWidth = tmpZoomRect.Width;
        var width = tmpZoomRect.Height * zoomRatio;
        tmpZoomRect.Width = width;
        if (!tmpZoomRect.Contains(point: zoomStartPoint))
        {
          tmpZoomRect.Offset(offsetX: oldWidth - width, offsetY: 0);
        }
      }
    }

    zoomRect = tmpZoomRect;
    UpdateSelectionAdorner();
  }

  private void OnMouseUp(object sender, MouseButtonEventArgs e)
  {
    OnParentMouseUp(e: e);
  }

  private void OnParentMouseUp(MouseButtonEventArgs e)
  {
    if (isPanning && e.ChangedButton == MouseButton.Left)
    {
      isPanning = false;
      StopPanning(e: e);
    }
    else if (isZooming && e.ChangedButton == MouseButton.Left)
    {
      isZooming = false;
      StopZooming();
    }
  }

  private void StopZooming()
  {
    if (zoomRect.HasValue)
    {
      var output = Viewport.Output;

      var p1 = zoomRect.Value.TopLeft.ScreenToViewport(transform: Viewport.Transform);
      var p2 = zoomRect.Value.BottomRight.ScreenToViewport(transform: Viewport.Transform);
      DataRect newVisible = new(point1: p1, point2: p2);
      Viewport.Visible = newVisible;

      zoomRect = null;
      ReleaseMouseCapture();
      RemoveSelectionAdorner();
    }
  }

  private void StopPanning(MouseButtonEventArgs e)
  {
    Plotter.UndoProvider.CaptureNewValue(target: Plotter.Viewport, property: Viewport2D.VisibleProperty, newValue: Viewport.Visible);

    if (!Plotter.IsFocused)
    {
      Plotter.Focus();
    }

    Plotter.Viewport.PanningState = Viewport2DPanningState.NotPanning;

    ReleaseMouseCapture();
    ClearValue(dp: CursorProperty);
  }

  protected override void OnLostFocus(RoutedEventArgs e)
  {
    if (isZooming)
    {
      RemoveSelectionAdorner();
      isZooming = false;
    }
    if (isPanning)
    {
      Plotter.Viewport.PanningState = Viewport2DPanningState.NotPanning;
      isPanning = false;
    }
    ReleaseMouseCapture();
    base.OnLostFocus(e: e);
  }

  private void MouseWheelZoom(Point mousePos, double wheelRotationDelta)
  {
    var zoomTo = mousePos.ScreenToViewport(transform: Viewport.Transform);

    var zoomSpeed = Math.Abs(value: wheelRotationDelta / Mouse.MouseWheelDeltaForOneLine);
    zoomSpeed *= wheelZoomSpeed;
    if (wheelRotationDelta < 0)
    {
      zoomSpeed = 1 / zoomSpeed;
    }
    Viewport.Visible = Viewport.Visible.Zoom(to: zoomTo, ratio: zoomSpeed);
  }
}
