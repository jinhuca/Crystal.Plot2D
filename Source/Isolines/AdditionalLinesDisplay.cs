using System;
using System.Linq;
using System.Windows.Media;
using Crystal.Plot2D.Common.Auxiliary;
using Crystal.Plot2D.Transforms;

namespace Crystal.Plot2D.Isolines;

public sealed class AdditionalLinesDisplay : IsolineGraphBase
{
  protected override void OnPlotterAttached()
  {
    base.OnPlotterAttached();

    Plotter2D.Viewport.PropertyChanged += Viewport_PropertyChanged;
  }

  protected override void OnPlotterDetaching()
  {
    Plotter2D.Viewport.PropertyChanged -= Viewport_PropertyChanged;

    base.OnPlotterDetaching();
  }

  private void Viewport_PropertyChanged(object sender, ExtendedPropertyChangedEventArgs e)
  {
    InvalidateVisual();
  }

  protected override void OnRender(DrawingContext drawingContext)
  {
    if (Plotter2D == null)
    {
      return;
    }

    if (DataSource == null)
    {
      return;
    }

    if (Collection == null)
    {
      return;
    }

    if (Collection.Lines.Count == 0)
    {
      IsolineBuilder.DataSource = DataSource;
    }

    var dc = drawingContext;
    var dataSource = DataSource;
    var localMinMax = dataSource.GetMinMax();
    var globalMinMax = dataSource.Range.Value;
    var lengthsRatio = globalMinMax.GetLength() / localMinMax.GetLength();

    if (lengthsRatio > 16)
    {
      var log = Math.Round(a: Math.Log(a: lengthsRatio, newBase: 2));
      var number = 2 * Math.Pow(x: 2, y: log);
      var delta = globalMinMax.GetLength() / number;

      var start = Math.Floor(d: (localMinMax.Min - globalMinMax.Min) / delta) * delta + globalMinMax.Min;
      var end = localMinMax.Max;

      var transform = Plotter2D.Transform;
      var strokeThickness = StrokeThickness;

      var x = start;
      while (x < end)
      {
        var collection = IsolineBuilder.BuildIsoline(level: x);

        foreach (var line in collection)
        {
          StreamGeometry lineGeometry = new();
          using (var context = lineGeometry.Open())
          {
            context.BeginFigure(startPoint: line.StartPoint.ViewportToScreen(transform: transform), isFilled: false, isClosed: false);
            context.PolyLineTo(points: line.OtherPoints.ViewportToScreen(transform: transform).ToArray(), isStroked: true, isSmoothJoin: true);
          }
          lineGeometry.Freeze();

          var paletteRatio = (line.RealValue - globalMinMax.Min) / globalMinMax.GetLength();
          Pen pen = new(brush: new SolidColorBrush(color: Palette.GetColor(t: paletteRatio)), thickness: strokeThickness);

          dc.DrawGeometry(brush: null, pen: pen, geometry: lineGeometry);
        }

        x += delta;
      }
    }
    //dc.DrawRectangle(Brushes.Green.MakeTransparent(0.3), null, new Rect(RenderSize));
  }
}
