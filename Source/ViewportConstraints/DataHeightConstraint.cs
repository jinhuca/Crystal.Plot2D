using System.Collections.Specialized;
using System.Windows;

namespace Crystal.Plot2D;

public class DataHeightConstraint : ViewportConstraint, ISupportAttachToViewport
{
  private double yEnlargeCoeff = 1.1;
  public double YEnlargeCoeff
  {
    get => yEnlargeCoeff;
    set
    {
      if (yEnlargeCoeff != value)
      {
        yEnlargeCoeff = value;
        RaiseChanged();
      }
    }
  }

  public override DataRect Apply(DataRect oldDataRect, DataRect newDataRect, Viewport2D viewport)
  {
    DataRect overallBounds = DataRect.Empty;

    foreach (var chart in viewport.ContentBoundsHosts)
    {
      var plotterElement = chart as IPlotterElement;
      var visual = viewport.Plotter.VisualBindings[element: plotterElement];
      var points = PointsGraphBase.GetVisiblePoints(obj: visual);
      if (points != null)
      {
        // searching for indices of chart's visible points which are near left and right borders of newDataRect
        double startX = newDataRect.XMin;
        double endX = newDataRect.XMax;

        if (points[index: 0].X > endX || points[index: points.Count - 1].X < startX)
        {
          continue;
        }

        int startIndex = -1;

        // we assume that points are sorted by x values ascending
        if (startX <= points[index: 0].X)
        {
          startIndex = 0;
        }
        else
        {
          for (int i = 1; i < points.Count - 1; i++)
          {
            if (points[index: i].X <= startX && startX < points[index: i + 1].X)
            {
              startIndex = i;
              break;
            }
          }
        }

        int endIndex = points.Count;

        if (points[index: points.Count - 1].X < endX)
        {
          endIndex = points.Count;
        }
        else
        {
          for (int i = points.Count - 1; i >= 1; i--)
          {
            if (points[index: i - 1].X <= endX && endX < points[index: i].X)
            {
              endIndex = i;
              break;
            }
          }
        }

        Rect bounds = Rect.Empty;
        for (int i = startIndex; i < endIndex; i++)
        {
          bounds.Union(point: points[index: i]);
        }
        if (startIndex > 0)
        {
          Point pt = GetInterpolatedPoint(x: startX, p1: points[index: startIndex], p2: points[index: startIndex - 1]);
          bounds.Union(point: pt);
        }
        if (endIndex < points.Count - 1)
        {
          Point pt = GetInterpolatedPoint(x: endX, p1: points[index: endIndex], p2: points[index: endIndex + 1]);
          bounds.Union(point: pt);
        }

        overallBounds.Union(rect: bounds);
      }
    }

    if (!overallBounds.IsEmpty)
    {
      double y = overallBounds.YMin;
      double height = overallBounds.Height;

      if (height == 0)
      {
        height = newDataRect.Height;
        y -= height / 2;
      }

      newDataRect = new DataRect(xMin: newDataRect.XMin, yMin: y, width: newDataRect.Width, height: height);
      newDataRect = DataRectExtensions.ZoomY(rect: newDataRect, to: newDataRect.GetCenter(), ratio: yEnlargeCoeff);
    }

    return newDataRect;
  }

  private static Point GetInterpolatedPoint(double x, Point p1, Point p2)
  {
    double xRatio = (x - p1.X) / (p2.X - p1.X);
    double y = (1 - xRatio) * p1.Y + xRatio * p2.Y;

    return new Point(x: x, y: y);
  }

  #region ISupportAttach Members

  void ISupportAttachToViewport.Attach(Viewport2D viewport)
  {
    ((INotifyCollectionChanged)viewport.ContentBoundsHosts).CollectionChanged += OnContentBoundsHostsChanged;

    foreach (var item in viewport.ContentBoundsHosts)
    {
      if (item is PointsGraphBase chart)
      {
        chart.ProvideVisiblePoints = true;
      }
    }
  }

  private void OnContentBoundsHostsChanged(object sender, NotifyCollectionChangedEventArgs e)
  {
    if (e.NewItems != null)
    {
      foreach (var item in e.NewItems)
      {
        if (item is PointsGraphBase chart)
        {
          chart.ProvideVisiblePoints = true;
        }
      }
    }

    // todo probably set ProvideVisiblePoints to false on OldItems
  }

  void ISupportAttachToViewport.Detach(Viewport2D viewport)
  {
    ((INotifyCollectionChanged)viewport.ContentBoundsHosts).CollectionChanged -= OnContentBoundsHostsChanged;
  }

  #endregion
}
