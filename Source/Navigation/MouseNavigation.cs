using Crystal.Plot2D.Common;
using Crystal.Plot2D.Common.Auxiliary;
using Crystal.Plot2D.Transforms;
using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace Crystal.Plot2D.Navigation;

/// <inheritdoc />
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
      if(adornerLayer == null)
      {
        adornerLayer = AdornerLayer.GetAdornerLayer(visual: this);
        if(adornerLayer != null)
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
    if(e.Key == Key.Escape || e.Key == Key.Back)
    {
      if(isZooming)
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
    if(!e.Handled)
    {
      var mousePos_ = e.GetPosition(relativeTo: this);
      var delta_ = -e.Delta;
      MouseWheelZoom(mousePos: mousePos_, wheelRotationDelta: delta_);

      e.Handled = true;
    }
  }

#if DEBUG
  public override string ToString()
  {
    if(!string.IsNullOrEmpty(value: Name))
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
    if(!adornerAdded)
    {
      var layer_ = AdornerLayer;
      if(layer_ != null)
      {
        selectionAdorner = new RectangleSelectionAdorner(element: this) { Border = zoomRect };

        layer_.Add(adorner: selectionAdorner);
        adornerAdded = true;
      }
    }
  }

  private void RemoveSelectionAdorner()
  {
    var layer_ = AdornerLayer;
    if(layer_ != null)
    {
      layer_.Remove(adorner: selectionAdorner);
      adornerAdded = false;
    }
  }

  private void UpdateSelectionAdorner()
  {
    selectionAdorner.Border = zoomRect;
    selectionAdorner.InvalidateVisual();
  }

  private Rect? zoomRect;
  private const double WheelZoomSpeed = 1.2;
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
      var currKeys_ = Keyboard.Modifiers;
      return (currKeys_ | ModifierKeys.Shift) == currKeys_ ||
        (currKeys_ | ModifierKeys.Control) == currKeys_;
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
    if(Viewport.Output.Contains(point: zoomStartPoint))
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
    var shouldStartDrag_ = ShouldStartPanning(e: e);
    if(shouldStartDrag_)
    {
      StartPanning(e: e);
    }

    // zooming
    var shouldStartZoom_ = ShouldStartZoom(e: e);
    if(shouldStartZoom_)
    {
      StartZoom(e: e);
    }

    if(!Plotter.IsFocused)
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
    if(!isPanning && !isZooming)
    {
      return;
    }

    // dragging
    if(isPanning && e.LeftButton == MouseButtonState.Pressed)
    {
      if(!IsMouseCaptured)
      {
        CaptureMouse();
      }

      var endPoint_ = e.GetPosition(relativeTo: this).ScreenToViewport(transform: Viewport.Transform);

      var loc_ = Viewport.Visible.Location;
      var shift_ = panningStartPointInViewport - endPoint_;
      loc_ += shift_;

      // preventing unnecessary changes, if actually visible hasn't change.
      if(shift_.X != 0 || shift_.Y != 0)
      {
        Cursor = Cursors.ScrollAll;

        var visible_ = Viewport.Visible;

        visible_.Location = loc_;
        Viewport.Visible = visible_;
      }

      e.Handled = true;
    }
    // zooming
    else if(isZooming && e.LeftButton == MouseButtonState.Pressed)
    {
      var zoomEndPoint_ = e.GetPosition(relativeTo: this);
      UpdateZoomRect(zoomEndPoint: zoomEndPoint_);

      e.Handled = true;
    }
  }

  private static bool IsShiftPressed()
  {
    return Keyboard.IsKeyDown(key: Key.LeftShift) || Keyboard.IsKeyDown(key: Key.RightShift);
  }

  private void UpdateZoomRect(Point zoomEndPoint)
  {
    var output_ = Viewport.Output;
    Rect tmpZoomRect_ = new(point1: zoomStartPoint, point2: zoomEndPoint);
    tmpZoomRect_ = Rect.Intersect(rect1: tmpZoomRect_, rect2: output_);

    shouldKeepRatioWhileZooming = IsShiftPressed();
    if(shouldKeepRatioWhileZooming)
    {
      var currZoomRatio_ = tmpZoomRect_.Width / tmpZoomRect_.Height;
      var zoomRatio_ = output_.Width / output_.Height;
      if(currZoomRatio_ < zoomRatio_)
      {
        var oldHeight_ = tmpZoomRect_.Height;
        var height_ = tmpZoomRect_.Width / zoomRatio_;
        tmpZoomRect_.Height = height_;
        if(!tmpZoomRect_.Contains(point: zoomStartPoint))
        {
          tmpZoomRect_.Offset(offsetX: 0, offsetY: oldHeight_ - height_);
        }
      }
      else
      {
        var oldWidth_ = tmpZoomRect_.Width;
        var width_ = tmpZoomRect_.Height * zoomRatio_;
        tmpZoomRect_.Width = width_;
        if(!tmpZoomRect_.Contains(point: zoomStartPoint))
        {
          tmpZoomRect_.Offset(offsetX: oldWidth_ - width_, offsetY: 0);
        }
      }
    }

    zoomRect = tmpZoomRect_;
    UpdateSelectionAdorner();
  }

  private void OnMouseUp(object sender, MouseButtonEventArgs e)
  {
    OnParentMouseUp(e: e);
  }

  private void OnParentMouseUp(MouseButtonEventArgs e)
  {
    if(isPanning && e.ChangedButton == MouseButton.Left)
    {
      isPanning = false;
      StopPanning(e: e);
    }
    else if(isZooming && e.ChangedButton == MouseButton.Left)
    {
      isZooming = false;
      StopZooming();
    }
  }

  private void StopZooming()
  {
    if(!zoomRect.HasValue) return;
    var output_ = Viewport.Output;

    var p1_ = zoomRect.Value.TopLeft.ScreenToViewport(transform: Viewport.Transform);
    var p2_ = zoomRect.Value.BottomRight.ScreenToViewport(transform: Viewport.Transform);
    DataRect newVisible_ = new(point1: p1_, point2: p2_);
    Viewport.Visible = newVisible_;

    zoomRect = null;
    ReleaseMouseCapture();
    RemoveSelectionAdorner();
  }

  private void StopPanning(MouseButtonEventArgs e)
  {
    Plotter.UndoProvider.CaptureNewValue(target: Plotter.Viewport, property: Viewport2D.VisibleProperty, newValue: Viewport.Visible);

    if(!Plotter.IsFocused)
    {
      Plotter.Focus();
    }

    Plotter.Viewport.PanningState = Viewport2DPanningState.NotPanning;

    ReleaseMouseCapture();
    ClearValue(dp: CursorProperty);
  }

  protected override void OnLostFocus(RoutedEventArgs e)
  {
    if(isZooming)
    {
      RemoveSelectionAdorner();
      isZooming = false;
    }
    if(isPanning)
    {
      Plotter.Viewport.PanningState = Viewport2DPanningState.NotPanning;
      isPanning = false;
    }
    ReleaseMouseCapture();
    base.OnLostFocus(e: e);
  }

  private void MouseWheelZoom(Point mousePos, double wheelRotationDelta)
  {
    var zoomTo_ = mousePos.ScreenToViewport(transform: Viewport.Transform);

    var zoomSpeed_ = Math.Abs(value: wheelRotationDelta / Mouse.MouseWheelDeltaForOneLine);
    zoomSpeed_ *= WheelZoomSpeed;
    if(wheelRotationDelta < 0)
    {
      zoomSpeed_ = 1 / zoomSpeed_;
    }
    Viewport.Visible = Viewport.Visible.Zoom(to: zoomTo_, ratio: zoomSpeed_);
  }
}
