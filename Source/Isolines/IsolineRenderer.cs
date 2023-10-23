using Crystal.Plot2D.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace Crystal.Plot2D.Charts;

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
    var visibleMinMax = dataSource.GetMinMax(area: Plotter2D.Visible);
    double totalDelta = collection.Max - collection.Min;
    double visibleMinMaxRatio = totalDelta / visibleMinMax.GetLength();
    double defaultDelta = totalDelta / 12;

    if (true)
    {
      double number = Math.Ceiling(a: visibleMinMaxRatio * 4);
      number = Math.Pow(x: 2, y: Math.Ceiling(a: Math.Log(d: number) / Math.Log(d: 2)));
      double delta = totalDelta / number;
      double x = collection.Min + Math.Ceiling(a: (visibleMinMax.Min - collection.Min) / delta) * delta;

      List<double> result = new();
      while (x < visibleMinMax.Max)
      {
        result.Add(item: x);
        x += delta;
      }

      return result;
    }
  }

  protected void RenderIsolineCollection(DrawingContext dc, double strokeThickness, IsolineCollection collection, CoordinateTransform transform)
  {
    foreach (LevelLine line in collection)
    {
      StreamGeometry lineGeometry = new();
      using (var context = lineGeometry.Open())
      {
        context.BeginFigure(startPoint: line.StartPoint.ViewportToScreen(transform: transform), isFilled: false, isClosed: false);
        if (!UseBezierCurves)
        {
          context.PolyLineTo(points: line.OtherPoints.ViewportToScreen(transform: transform).ToArray(), isStroked: true, isSmoothJoin: true);
        }
        else
        {
          context.PolyBezierTo(points: BezierBuilder.GetBezierPoints(points: line.AllPoints.ViewportToScreen(transform: transform).ToArray()).Skip(count: 1).ToArray(), isStroked: true, isSmoothJoin: true);
        }
      }
      lineGeometry.Freeze();

      Pen pen = new(brush: new SolidColorBrush(color: Palette.GetColor(t: line.Value01)), thickness: strokeThickness);

      dc.DrawGeometry(brush: null, pen: pen, geometry: lineGeometry);
    }
  }

}
