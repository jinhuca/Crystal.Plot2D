using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Crystal.Plot2D.Common;
using Crystal.Plot2D.Common.Auxiliary;
using Crystal.Plot2D.Transforms;

namespace Crystal.Plot2D.Isolines;

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
    var totalDelta = collection.Max - collection.Min;
    var visibleMinMaxRatio = totalDelta / visibleMinMax.GetLength();
    var defaultDelta = totalDelta / 12;

    if (true)
    {
      var number = Math.Ceiling(a: visibleMinMaxRatio * 4);
      number = Math.Pow(x: 2, y: Math.Ceiling(a: Math.Log(d: number) / Math.Log(d: 2)));
      var delta = totalDelta / number;
      var x = collection.Min + Math.Ceiling(a: (visibleMinMax.Min - collection.Min) / delta) * delta;

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
    foreach (var line in collection)
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
