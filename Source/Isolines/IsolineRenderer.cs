using Crystal.Plot2D.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace Crystal.Plot2D.Charts
{
  public abstract class IsolineRenderer : IsolineGraphBase
  {
    protected override void UpdateDataSource()
    {
      if (DataSource != null)
      {
        IsolineBuilder.DataSource = DataSource;
        IsolineBuilder.MissingValue = MissingValue;
        Collection = IsolineBuilder.BuildIsoline();
      }
      else
      {
        Collection = null;
      }
    }

    protected IEnumerable<double> GetAdditionalLevels(IsolineCollection collection)
    {
      var dataSource = DataSource;
      var visibleMinMax = dataSource.GetMinMax(Plotter2D.Visible);
      double totalDelta = collection.Max - collection.Min;
      double visibleMinMaxRatio = totalDelta / visibleMinMax.GetLength();
      double defaultDelta = totalDelta / 12;

      if (true || 2 * defaultDelta < visibleMinMaxRatio)
      {
        double number = Math.Ceiling(visibleMinMaxRatio * 4);
        number = Math.Pow(2, Math.Ceiling(Math.Log(number) / Math.Log(2)));
        double delta = totalDelta / number;
        double x = collection.Min + Math.Ceiling((visibleMinMax.Min - collection.Min) / delta) * delta;

        List<double> result = new();
        while (x < visibleMinMax.Max)
        {
          result.Add(x);
          x += delta;
        }

        return result;
      }

      return Enumerable.Empty<double>();
    }

    protected void RenderIsolineCollection(DrawingContext dc, double strokeThickness, IsolineCollection collection, CoordinateTransform transform)
    {
      foreach (LevelLine line in collection)
      {
        StreamGeometry lineGeometry = new();
        using (var context = lineGeometry.Open())
        {
          context.BeginFigure(line.StartPoint.ViewportToScreen(transform), false, false);
          if (!UseBezierCurves)
          {
            context.PolyLineTo(line.OtherPoints.ViewportToScreen(transform).ToArray(), true, true);
          }
          else
          {
            context.PolyBezierTo(BezierBuilder.GetBezierPoints(line.AllPoints.ViewportToScreen(transform).ToArray()).Skip(1).ToArray(), true, true);
          }
        }
        lineGeometry.Freeze();

        Pen pen = new(new SolidColorBrush(Palette.GetColor(line.Value01)), strokeThickness);

        dc.DrawGeometry(null, pen, lineGeometry);
      }
    }

  }
}
