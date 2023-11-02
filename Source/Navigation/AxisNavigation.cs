using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Crystal.Plot2D.Axes;
using Crystal.Plot2D.Common;
using Crystal.Plot2D.Common.Auxiliary;
using Crystal.Plot2D.Transforms;

namespace Crystal.Plot2D.Navigation;

public sealed class AxisNavigation : DependencyObject, IPlotterElement
{
  public AxisPlacement Placement
  {
    get => (AxisPlacement)GetValue(dp: PlacementProperty);
    set => SetValue(dp: PlacementProperty, value: value);
  }

  public static readonly DependencyProperty PlacementProperty = DependencyProperty.Register(
    name: nameof(Placement),
    propertyType: typeof(AxisPlacement),
    ownerType: typeof(AxisNavigation),
    typeMetadata: new FrameworkPropertyMetadata(defaultValue: AxisPlacement.Left, propertyChangedCallback: OnPlacementChanged));

  private static void OnPlacementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    var navigation_ = (AxisNavigation)d;
    navigation_.OnPlacementChanged();
  }

  private Panel listeningPanel;
  private Panel ListeningPanel => listeningPanel;

  private void OnPlacementChanged()
  {
    SetListeningPanel();
  }

  private void SetListeningPanel()
  {
    if (plotter == null)
    {
      return;
    }

    var placement_ = Placement;
    switch (placement_)
    {
      case AxisPlacement.Left:
        listeningPanel = plotter.LeftPanel;
        break;
      case AxisPlacement.Right:
        listeningPanel = plotter.RightPanel;
        break;
      case AxisPlacement.Top:
        listeningPanel = plotter.TopPanel;
        break;
      case AxisPlacement.Bottom:
        listeningPanel = plotter.BottomPanel;
        break;
    }
  }

  private CoordinateTransform Transform => plotter.Viewport.Transform;

  private Panel hostPanel;

  #region IPlotterElement Members

  void IPlotterElement.OnPlotterAttached(PlotterBase plotter)
  {
    this.plotter = (PlotterBase)plotter;

    hostPanel = plotter.MainGrid;

    //hostPanel.Background = Brushes.Pink;

    SetListeningPanel();

    if (hostPanel != null)
    {
      hostPanel.MouseLeftButtonUp += OnMouseLeftButtonUp;
      hostPanel.MouseLeftButtonDown += OnMouseLeftButtonDown;
      hostPanel.MouseMove += OnMouseMove;
      hostPanel.MouseWheel += OnMouseWheel;

      hostPanel.MouseRightButtonDown += OnMouseRightButtonDown;
      hostPanel.MouseRightButtonUp += OnMouseRightButtonUp;
      hostPanel.LostFocus += OnLostFocus;
    }
  }

  private void OnLostFocus(object sender, RoutedEventArgs e)
  {
    //Debug.WriteLine("Lost Focus");
    RevertChanges();
    rmbPressed = false;
    lmbPressed = false;

    e.Handled = true;
  }

  #region Right button down

  private DataRect rmbDragStartRect;
  private void OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
  {
    OnMouseRightButtonDown(e: e);
  }

  private void OnMouseRightButtonDown(MouseButtonEventArgs e)
  {
    rmbInitialPosition = e.GetPosition(relativeTo: listeningPanel);

    var foundActivePlotter_ = UpdateActivePlotter(e: e);
    if (foundActivePlotter_)
    {
      rmbPressed = true;
      dragStartInViewport = rmbInitialPosition.ScreenToViewport(transform: activePlotter.Transform);
      rmbDragStartRect = activePlotter.Visible;

      listeningPanel.Background = fillBrush;
      listeningPanel.CaptureMouse();

      //e.Handled = true;
    }
  }

  #endregion

  #region Right button up

  private void OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
  {
    OnMouseRightButtonUp(e: e);
  }

  private void OnMouseRightButtonUp(MouseButtonEventArgs e)
  {
    if (rmbPressed)
    {
      rmbPressed = false;
      RevertChanges();

      //e.Handled = true;
    }
  }

  #endregion

  private void RevertChanges()
  {
    listeningPanel.ClearValue(dp: FrameworkElement.CursorProperty);
    listeningPanel.Background = Brushes.Transparent;
    listeningPanel.ReleaseMouseCapture();
  }

  private const double WheelZoomSpeed = 1.2;
  private void OnMouseWheel(object sender, MouseWheelEventArgs e)
  {
    var mousePos_ = e.GetPosition(relativeTo: listeningPanel);

    Rect listeningPanelBounds_ = new(size: listeningPanel.RenderSize);
    if (!listeningPanelBounds_.Contains(point: mousePos_))
    {
      return;
    }

    var foundActivePlotter_ = UpdateActivePlotter(e: e);
    if (!foundActivePlotter_)
    {
      return;
    }

    var delta_ = -e.Delta;

    var zoomTo_ = mousePos_.ScreenToViewport(transform: activePlotter.Transform);

    double zoomSpeed_ = Math.Abs(value: delta_ / Mouse.MouseWheelDeltaForOneLine);
    zoomSpeed_ *= WheelZoomSpeed;
    if (delta_ < 0)
    {
      zoomSpeed_ = 1 / zoomSpeed_;
    }

    var visible_ = activePlotter.Viewport.Visible.Zoom(to: zoomTo_, ratio: zoomSpeed_);
    var oldVisible_ = activePlotter.Viewport.Visible;
    if (Placement.IsBottomOrTop())
    {
      visible_.YMin = oldVisible_.YMin;
      visible_.Height = oldVisible_.Height;
    }
    else
    {
      visible_.XMin = oldVisible_.XMin;
      visible_.Width = oldVisible_.Width;
    }
    activePlotter.Viewport.Visible = visible_;

    e.Handled = true;
  }

  private const int RmbZoomScale = 200;
  private void OnMouseMove(object sender, MouseEventArgs e)
  {
    if (lmbPressed)
    {
      // panning: 
      if (e.LeftButton == MouseButtonState.Released)
      {
        lmbPressed = false;
        RevertChanges();
        return;
      }

      var screenMousePos_ = e.GetPosition(relativeTo: listeningPanel);
      var dataMousePos_ = screenMousePos_.ScreenToViewport(transform: activePlotter.Transform);
      var visible_ = activePlotter.Viewport.Visible;
      double delta_;
      if (Placement.IsBottomOrTop())
      {
        delta_ = (dataMousePos_ - dragStartInViewport).X;
        visible_.XMin -= delta_;
      }
      else
      {
        delta_ = (dataMousePos_ - dragStartInViewport).Y;
        visible_.YMin -= delta_;
      }

      if (screenMousePos_ != lmbInitialPosition)
      {
        listeningPanel.Cursor = Placement.IsBottomOrTop() ? Cursors.ScrollWE : Cursors.ScrollNS;
      }

      activePlotter.Viewport.Visible = visible_;

      e.Handled = true;
    }
    else if (rmbPressed)
    {
      // one direction zooming:
      if (e.RightButton == MouseButtonState.Released)
      {
        rmbPressed = false;
        RevertChanges();
        return;
      }

      var screenMousePos_ = e.GetPosition(relativeTo: listeningPanel);
      var isHorizontal_ = Placement.IsBottomOrTop();
      var delta_ = isHorizontal_ ? (screenMousePos_ - rmbInitialPosition).X : (screenMousePos_ - rmbInitialPosition).Y;

      if (delta_ < 0)
      {
        delta_ = 1 / Math.Exp(d: -delta_ / RmbZoomScale);
      }
      else
      {
        delta_ = Math.Exp(d: delta_ / RmbZoomScale);
      }

      var center_ = dragStartInViewport;

      var visible_ = isHorizontal_ ? rmbDragStartRect.ZoomX(to: center_, ratio: delta_) : rmbDragStartRect.ZoomY(to: center_, ratio: delta_);

      if (screenMousePos_ != lmbInitialPosition)
      {
        listeningPanel.Cursor = Placement.IsBottomOrTop() ? Cursors.ScrollWE : Cursors.ScrollNS;
      }


      activePlotter.Viewport.Visible = visible_;

      //e.Handled = true;
    }
  }

  private Point lmbInitialPosition;
  private Point LmbInitialPosition => lmbInitialPosition;

  private Point rmbInitialPosition;
  private readonly SolidColorBrush fillBrush = new SolidColorBrush(color: Color.FromRgb(r: 255, g: 228, b: 209)).MakeTransparent(opacity: 0.2);
  private bool lmbPressed;
  private bool rmbPressed;
  private Point dragStartInViewport;
  private PlotterBase activePlotter;

  #region Left button down

  private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
  {
    OnMouseLeftButtonDown(e: e);
  }

  private void OnMouseLeftButtonDown(MouseButtonEventArgs e)
  {
    lmbInitialPosition = e.GetPosition(relativeTo: listeningPanel);

    var foundActivePlotter_ = UpdateActivePlotter(e: e);
    if (foundActivePlotter_)
    {
      lmbPressed = true;
      dragStartInViewport = lmbInitialPosition.ScreenToViewport(transform: activePlotter.Transform);

      listeningPanel.Background = fillBrush;
      listeningPanel.CaptureMouse();

      e.Handled = true;
    }
  }

  #endregion

  private bool UpdateActivePlotter(MouseEventArgs e)
  {
    var axes_ = listeningPanel.Children.OfType<GeneralAxis>();

    foreach (var axis_ in axes_)
    {
      var positionInAxis_ = e.GetPosition(relativeTo: axis_);
      Rect axisBounds_ = new(size: axis_.RenderSize);
      if (axisBounds_.Contains(point: positionInAxis_))
      {
        activePlotter = axis_.Plotter;

        return true;
      }
    }

    return false;
  }

  #region Left button up

  private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
  {
    OnMouseLeftButtonUp(e: e);
  }

  private void OnMouseLeftButtonUp(MouseButtonEventArgs e)
  {
    if (lmbPressed)
    {
      lmbPressed = false;
      RevertChanges();

      e.Handled = true;
    }
  }

  #endregion

  public void OnPlotterDetaching(PlotterBase plotter)
  {
    if (plotter.MainGrid != null)
    {
      hostPanel.MouseLeftButtonUp -= OnMouseLeftButtonUp;
      hostPanel.MouseLeftButtonDown -= OnMouseLeftButtonDown;
      hostPanel.MouseMove -= OnMouseMove;
      hostPanel.MouseWheel -= OnMouseWheel;

      hostPanel.MouseRightButtonDown -= OnMouseRightButtonDown;
      hostPanel.MouseRightButtonUp -= OnMouseRightButtonUp;

      hostPanel.LostFocus -= OnLostFocus;
    }
    listeningPanel = null;
    this.plotter = null;
  }

  private PlotterBase plotter;
  PlotterBase IPlotterElement.Plotter => plotter;

  #endregion

  public override string ToString()
  {
    return "AxisNavigation: " + Placement;
  }
}
