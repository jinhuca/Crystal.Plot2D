using Crystal.Plot2D.Common;
using System;
using System.Windows;

namespace Crystal.Plot2D.Charts;

/// <summary>
///   Represents a panel on which elements are arranged in coordinates in viewport space.
/// </summary>
public partial class ViewportPanel : IndividualArrangePanel
{
  static ViewportPanel()
  {
    Type thisType = typeof(ViewportPanel);
    PlotterBase.PlotterProperty.OverrideMetadata(forType: thisType, typeMetadata: new FrameworkPropertyMetadata { PropertyChangedCallback = OnPlotterChanged });
  }

  private static void OnPlotterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    ViewportPanel panel = (ViewportPanel)d;
    panel.OnPlotterChanged(currPlotter: (PlotterBase)e.NewValue, prevPlotter: (PlotterBase)e.OldValue);
  }

  private void OnPlotterChanged(PlotterBase currPlotter, PlotterBase prevPlotter)
  {
    if (currPlotter != null)
    {
      PlotterBase plotter2d = (PlotterBase)currPlotter;
      viewport = plotter2d.Viewport;
    }
    else
    {
      viewport = null;
    }
  }

  /// <summary>
  ///   Initializes a new instance of the <see cref="ViewportRectPanel"/> class.
  /// </summary>
  public ViewportPanel() { }

  #region Panel methods override

  protected internal override void OnChildAdded(FrameworkElement child)
  {
    InvalidatePosition(child: child);
  }

  protected virtual void InvalidatePosition(FrameworkElement child)
  {
    if (viewport == null)
    {
      return;
    }

    var transform = GetTransform(size: availableSize);

    Size elementSize = GetElementSize(child: child, availableSize: AvailableSize, transform: transform);
    child.Measure(availableSize: elementSize);

    Rect bounds = GetElementScreenBounds(transform: transform, child: child);
    if (!bounds.IsNaN())
    {
      child.Arrange(finalRect: bounds);
    }
  }

  private Size availableSize;
  protected Size AvailableSize
  {
    get => availableSize;
    set => availableSize = value;
  }

  protected override Size MeasureOverride(Size _availableSize)
  {
    availableSize = _availableSize;

    var transform = GetTransform(size: _availableSize);

    foreach (FrameworkElement child in InternalChildren)
    {
      if (child != null)
      {
        Size elementSize = GetElementSize(child: child, availableSize: _availableSize, transform: transform);
        child.Measure(availableSize: elementSize);
      }
    }

    if (_availableSize.Width.IsInfinite())
    {
      _availableSize.Width = 0;
    }
    if (_availableSize.Height.IsInfinite())
    {
      _availableSize.Height = 0;
    }
    return _availableSize;
  }

  protected virtual Size GetElementSize(FrameworkElement child, Size availableSize, CoordinateTransform transform)
  {
    Size res = availableSize;
    DataRect ownViewportBounds = GetViewportBounds(obj: child);

    if (!ownViewportBounds.IsEmpty)
    {
      res = ownViewportBounds.ViewportToScreen(transform: transform).Size;
    }
    else
    {
      double viewportWidth = GetViewportWidth(obj: child);
      double viewportHeight = GetViewportHeight(obj: child);
      bool hasViewportWidth = viewportWidth.IsNotNaN();
      bool hasViewportHeight = viewportHeight.IsNotNaN();
      double minScreenWidth = GetMinScreenWidth(obj: child);
      bool hasMinScreenWidth = minScreenWidth.IsNotNaN();

      double selfWidth = child.Width.IsNotNaN() ? child.Width : availableSize.Width;
      double width = hasViewportWidth ? viewportWidth : selfWidth;
      double selfHeight = child.Height.IsNotNaN() ? child.Height : availableSize.Height;
      double height = hasViewportHeight ? viewportHeight : selfHeight;

      if (width < 0)
      {
        width = 0;
      }

      if (height < 0)
      {
        height = 0;
      }

      DataRect bounds = new(size: new Size(width: width, height: height));
      Rect screenBounds = bounds.ViewportToScreen(transform: transform);

      res = new Size(width: hasViewportWidth ? screenBounds.Width : selfWidth,
        height: hasViewportHeight ? screenBounds.Height : selfHeight);

      if (hasMinScreenWidth && res.Width < minScreenWidth)
      {
        res.Width = minScreenWidth;
      }
    }

    if (res.Width.IsNaN())
    {
      res.Width = 0;
    }
    if (res.Height.IsNaN())
    {
      res.Height = 0;
    }

    return res;
  }

  protected Rect GetElementScreenBounds(CoordinateTransform transform, UIElement child)
  {
    Rect screenBounds = GetElementScreenBoundsCore(transform: transform, child: child);
    DataRect viewportBounds = screenBounds.ScreenToViewport(transform: transform);
    DataRect prevViewportBounds = GetActualViewportBounds(obj: child);
    SetPrevActualViewportBounds(obj: child, value: prevViewportBounds);
    SetActualViewportBounds(obj: child, value: viewportBounds);

    return screenBounds;
  }

  protected virtual Rect GetElementScreenBoundsCore(CoordinateTransform transform, UIElement child)
  {
    Rect bounds = new(x: 0, y: 0, width: 1, height: 1);

    DataRect ownViewportBounds = GetViewportBounds(obj: child);
    if (!ownViewportBounds.IsEmpty)
    {
      bounds = ownViewportBounds.ViewportToScreen(transform: transform);
    }
    else
    {
      double viewportX = GetX(obj: child);
      double viewportY = GetY(obj: child);

      if (viewportX.IsNaN() || viewportY.IsNaN())
      {
        //Debug.WriteLine("ViewportRectPanel: Position is not set!");
        return bounds;
      }

      double viewportWidth = GetViewportWidth(obj: child);
      if (viewportWidth < 0)
      {
        viewportWidth = 0;
      }
      double viewportHeight = GetViewportHeight(obj: child);
      if (viewportHeight < 0)
      {
        viewportHeight = 0;
      }

      bool hasViewportWidth = viewportWidth.IsNotNaN();
      bool hasViewportHeight = viewportHeight.IsNotNaN();

      DataRect r = new(size: new Size(width: hasViewportWidth ? viewportWidth : child.DesiredSize.Width,
                     height: hasViewportHeight ? viewportHeight : child.DesiredSize.Height));
      r = r.ViewportToScreen(transform: transform);

      double screenWidth = hasViewportWidth ? r.Width : child.DesiredSize.Width;
      double screenHeight = hasViewportHeight ? r.Height : child.DesiredSize.Height;

      double minScreenWidth = GetMinScreenWidth(obj: child);
      bool hasMinScreemWidth = minScreenWidth.IsNotNaN();

      if (hasViewportWidth && screenWidth < minScreenWidth)
      {
        screenWidth = minScreenWidth;
      }

      Point location = new Point(x: viewportX, y: viewportY).ViewportToScreen(transform: transform);
      double screenX = location.X;
      double screenY = location.Y;

      HorizontalAlignment horizAlignment = GetViewportHorizontalAlignment(obj: child);
      switch (horizAlignment)
      {
        case HorizontalAlignment.Stretch:
        case HorizontalAlignment.Center:
          screenX -= screenWidth / 2;
          break;
        case HorizontalAlignment.Left:
          break;
        case HorizontalAlignment.Right:
          screenX -= screenWidth;
          break;
      }

      VerticalAlignment vertAlignment = GetViewportVerticalAlignment(obj: child);
      switch (vertAlignment)
      {
        case VerticalAlignment.Bottom:
          screenY -= screenHeight;
          break;
        case VerticalAlignment.Center:
        case VerticalAlignment.Stretch:
          screenY -= screenHeight / 2;
          break;
        case VerticalAlignment.Top:
          break;
      }

      bounds = new Rect(x: screenX, y: screenY, width: screenWidth, height: screenHeight);
    }

    // applying screen offset
    double screenOffsetX = GetScreenOffsetX(obj: child);
    if (screenOffsetX.IsNaN())
    {
      screenOffsetX = 0;
    }
    double screenOffsetY = GetScreenOffsetY(obj: child);
    if (screenOffsetY.IsNaN())
    {
      screenOffsetY = 0;
    }

    Vector screenOffset = new(x: screenOffsetX, y: screenOffsetY);
    bounds.Offset(offsetVector: screenOffset);
    return bounds;
  }

  protected override Size ArrangeOverride(Size finalSize)
  {
    var transform = GetTransform(size: finalSize);

    foreach (UIElement child in InternalChildren)
    {
      if (child != null)
      {
        Rect bounds = GetElementScreenBounds(transform: transform, child: child);
        if (!bounds.IsNaN())
        {
          child.Arrange(finalRect: bounds);
        }
      }
    }

    return finalSize;
  }

  private CoordinateTransform GetTransform(Size size) 
    => viewport.Transform.WithRects(visibleRect: GetViewportBounds(obj: this), screenRect: new Rect(size: size));

  #endregion

  private Viewport2D viewport;
}