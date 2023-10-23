using Crystal.Plot2D.Common;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Crystal.Plot2D.Charts;

public class ViewportHostPanel : ViewportPanel, IPlotterElement
{
  static ViewportHostPanel()
  {
    EventManager.RegisterClassHandler(classType: typeof(ViewportHostPanel), routedEvent: PlotterBase.PlotterChangedEvent, handler: new PlotterChangedEventHandler(OnPlotterChanged));
  }

  public ViewportHostPanel()
  {
    RenderTransform = translateTransform;

#if false
			// for debug purposes
			Width = 100;
			Height = 100;
			Background = Brushes.PaleGreen.MakeTransparent(0.2);
#endif
  }

  private static void OnPlotterChanged(object sender, PlotterChangedEventArgs e)
  {
    ViewportHostPanel owner = (ViewportHostPanel)sender;

    if (owner.plotter != null && e.PreviousPlotter != null)
    {
      owner.viewport.PropertyChanged -= owner.Viewport_PropertyChanged;
      owner.viewport = null;
      owner.plotter = null;
    }
    if (owner.plotter == null && e.CurrentPlotter != null)
    {
      owner.plotter = (PlotterBase)e.CurrentPlotter;
      owner.viewport = owner.plotter.Viewport;
      owner.viewport.PropertyChanged += owner.Viewport_PropertyChanged;
    }
  }

  readonly TranslateTransform translateTransform = new();

  private readonly Canvas hostingCanvas = new();
  internal Canvas HostingCanvas => hostingCanvas;

  #region IPlotterElement Members

  private PlotterBase plotter;
  protected PlotterBase Plotter => plotter;

  Viewport2D viewport;
  public virtual void OnPlotterAttached(PlotterBase plotter)
  {
    this.plotter = (PlotterBase)plotter;
    viewport = this.plotter.Viewport;
    if (!IsMarkersHost)
    {
      plotter.CentralGrid.Children.Add(element: hostingCanvas);
    }
    if (Parent == null)
    {
      hostingCanvas.Children.Add(element: this);
    }
    this.plotter.Viewport.PropertyChanged += Viewport_PropertyChanged;
  }

  public virtual void OnPlotterDetaching(PlotterBase _plotter)
  {
    plotter.Viewport.PropertyChanged -= Viewport_PropertyChanged;
    if (!IsMarkersHost)
    {
      _plotter.CentralGrid.Children.Remove(element: hostingCanvas);
    }
    hostingCanvas.Children.Remove(element: this);
    plotter = null;
  }

  PlotterBase IPlotterElement.Plotter => plotter;

  protected override void OnChildDesiredSizeChanged(UIElement child)
  {
    InvalidatePosition(child: (FrameworkElement)child);
  }

  Vector prevVisualOffset;
  bool sizeChanged = true;
  DataRect visibleWhileCreation;
  protected virtual void Viewport_PropertyChanged(object sender, ExtendedPropertyChangedEventArgs e)
  {
    if (e.PropertyName == "Visible")
    {
      DataRect visible = (DataRect)e.NewValue;
      DataRect prevVisible = (DataRect)e.OldValue;

      if (prevVisible.Size.EqualsApproximately(size2: visible.Size) && !sizeChanged)
      {
        var transform = viewport.Transform;
        Point prevLocation = prevVisible.Location.ViewportToScreen(transform: transform);
        Point location = visible.Location.ViewportToScreen(transform: transform);

        Vector offset = prevLocation - location;
        translateTransform.X += offset.X;
        translateTransform.Y += offset.Y;
      }
      else
      {
        visibleWhileCreation = visible;
        translateTransform.X = 0;
        translateTransform.Y = 0;
        InvalidateMeasure();
      }

      sizeChanged = false;
    }
    else if (e.PropertyName == "Output")
    {
      sizeChanged = true;
      translateTransform.X = 0;
      translateTransform.Y = 0;
      InvalidateMeasure();
    }

    prevVisualOffset = VisualOffset;
  }

  #endregion

  protected internal override void OnChildAdded(FrameworkElement child)
  {
    if (plotter == null)
    {
      return;
    }

    InvalidatePosition(child: child);
    //Dispatcher.BeginInvoke(((Action)(()=> InvalidatePosition(child))), DispatcherPriority.ApplicationIdle);
  }

  private DataRect overallViewportBounds = DataRect.Empty;
  internal DataRect OverallViewportBounds
  {
    get => overallViewportBounds;
    set => overallViewportBounds = value;
  }

  private BoundsUnionMode boundsUnionMode;
  internal BoundsUnionMode BoundsUnionMode
  {
    get => boundsUnionMode;
    set => boundsUnionMode = value;
  }

  protected override void InvalidatePosition(FrameworkElement child)
  {
    invalidatePositionCalls++;

    if (viewport == null)
    {
      return;
    }

    if (child.Visibility != Visibility.Visible)
    {
      return;
    }

    var transform = viewport.Transform.WithScreenOffset(x: -translateTransform.X, y: -translateTransform.Y);
    Size elementSize = GetElementSize(child: child, availableSize: AvailableSize, transform: transform);
    child.Measure(availableSize: elementSize);

    Rect bounds = GetElementScreenBounds(transform: transform, child: child);
    child.Arrange(finalRect: bounds);

    var viewportBounds = Viewport2D.GetContentBounds(obj: this);
    if (!viewportBounds.IsEmpty)
    {
      overallViewportBounds = viewportBounds;
    }

    UniteWithBounds(transform: transform, bounds: bounds);

    if (!InBatchAdd)
    {
      Viewport2D.SetContentBounds(obj: this, value: overallViewportBounds);
      ContentBoundsChanged.Raise(sender: this);
    }
  }

  private void UniteWithBounds(CoordinateTransform transform, Rect bounds)
  {
    var childViewportBounds = bounds.ScreenToViewport(transform: transform);
    if (boundsUnionMode == BoundsUnionMode.Bounds)
    {
      overallViewportBounds.Union(rect: childViewportBounds);
    }
    else
    {
      overallViewportBounds.Union(point: childViewportBounds.GetCenter());
    }
  }

  int invalidatePositionCalls;
  internal override void BeginBatchAdd()
  {
    base.BeginBatchAdd();
    invalidatePositionCalls = 0;
  }

  internal override void EndBatchAdd()
  {
    base.EndBatchAdd();
    if (plotter == null)
    {
      return;
    }

    UpdateContentBounds(recalculate: Children.Count > 0 && invalidatePositionCalls == 0);
  }

  public void UpdateContentBounds()
  {
    UpdateContentBounds(recalculate: true);
  }

  private void UpdateContentBounds(bool recalculate)
  {
    if (recalculate)
    {
      var transform = plotter.Viewport.Transform.WithScreenOffset(x: -translateTransform.X, y: -translateTransform.Y);
      overallViewportBounds = DataRect.Empty;
      foreach (FrameworkElement child in Children)
      {
        if (child != null)
        {
          if (child.Visibility != Visibility.Visible)
          {
            continue;
          }

          Rect bounds = GetElementScreenBounds(transform: transform, child: child);
          UniteWithBounds(transform: transform, bounds: bounds);
        }
      }
    }

    Viewport2D.SetContentBounds(obj: this, value: overallViewportBounds);
    ContentBoundsChanged.Raise(sender: this);
  }

  protected override Size ArrangeOverride(Size finalSize)
  {
    if (plotter == null)
    {
      return finalSize;
    }

    var transform = plotter.Viewport.Transform.WithScreenOffset(x: -translateTransform.X, y: -translateTransform.Y);

    overallViewportBounds = DataRect.Empty;
    foreach (UIElement child in InternalChildren)
    {
      if (child != null)
      {
        if (child.Visibility != Visibility.Visible)
        {
          continue;
        }

        Rect bounds = GetElementScreenBounds(transform: transform, child: child);
        child.Arrange(finalRect: bounds);
        UniteWithBounds(transform: transform, bounds: bounds);
      }
    }

    if (!InBatchAdd)
    {
      Viewport2D.SetContentBounds(obj: this, value: overallViewportBounds);
      ContentBoundsChanged.Raise(sender: this);
    }

    return finalSize;
  }

  public event EventHandler ContentBoundsChanged;

  protected override Size MeasureOverride(Size availableSize)
  {
    AvailableSize = availableSize;

    if (plotter == null)
    {
      if (availableSize.Width.IsInfinite() || availableSize.Height.IsInfinite())
      {
        return new Size();
      }
      return availableSize;
    }

    var transform = plotter.Viewport.Transform;

    foreach (FrameworkElement child in InternalChildren)
    {
      if (child != null)
      {
        Size elementSize = GetElementSize(child: child, availableSize: availableSize, transform: transform);
        child.Measure(availableSize: elementSize);
      }
    }

    if (availableSize.Width.IsInfinite())
    {
      availableSize.Width = 0;
    }
    if (availableSize.Height.IsInfinite())
    {
      availableSize.Height = 0;
    }
    return availableSize;
  }
}

public enum BoundsUnionMode
{
  Center,
  Bounds
}