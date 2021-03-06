using System.Collections.Specialized;
using System.Windows;

namespace Crystal.Plot2D
{
  public class DataHeightConstraint : ViewportConstraint, ISupportAttachToViewport
  {
    private double yEnlargeCoeff = 1.1;
    public double YEnlargeCoeff
    {
      get { return yEnlargeCoeff; }
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
        var visual = viewport.Plotter.VisualBindings[plotterElement];
        var points = PointsGraphBase.GetVisiblePoints(visual);
        if (points != null)
        {
          // searching for indices of chart's visible points which are near left and right borders of newDataRect
          double startX = newDataRect.XMin;
          double endX = newDataRect.XMax;

          if (points[0].X > endX || points[points.Count - 1].X < startX)
          {
            continue;
          }

          int startIndex = -1;

          // we assume that points are sorted by x values ascending
          if (startX <= points[0].X)
          {
            startIndex = 0;
          }
          else
          {
            for (int i = 1; i < points.Count - 1; i++)
            {
              if (points[i].X <= startX && startX < points[i + 1].X)
              {
                startIndex = i;
                break;
              }
            }
          }

          int endIndex = points.Count;

          if (points[points.Count - 1].X < endX)
          {
            endIndex = points.Count;
          }
          else
          {
            for (int i = points.Count - 1; i >= 1; i--)
            {
              if (points[i - 1].X <= endX && endX < points[i].X)
              {
                endIndex = i;
                break;
              }
            }
          }

          Rect bounds = Rect.Empty;
          for (int i = startIndex; i < endIndex; i++)
          {
            bounds.Union(points[i]);
          }
          if (startIndex > 0)
          {
            Point pt = GetInterpolatedPoint(startX, points[startIndex], points[startIndex - 1]);
            bounds.Union(pt);
          }
          if (endIndex < points.Count - 1)
          {
            Point pt = GetInterpolatedPoint(endX, points[endIndex], points[endIndex + 1]);
            bounds.Union(pt);
          }

          overallBounds.Union(bounds);
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

        newDataRect = new DataRect(newDataRect.XMin, y, newDataRect.Width, height);
        newDataRect = DataRectExtensions.ZoomY(newDataRect, newDataRect.GetCenter(), yEnlargeCoeff);
      }

      return newDataRect;
    }

    private static Point GetInterpolatedPoint(double x, Point p1, Point p2)
    {
      double xRatio = (x - p1.X) / (p2.X - p1.X);
      double y = (1 - xRatio) * p1.Y + xRatio * p2.Y;

      return new Point(x, y);
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
}
