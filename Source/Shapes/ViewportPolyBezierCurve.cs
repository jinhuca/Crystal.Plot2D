using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Crystal.Plot2D.Common;
using Crystal.Plot2D.Common.Auxiliary;
using Crystal.Plot2D.Transforms;

namespace Crystal.Plot2D.Shapes;

public class ViewportPolyBezierCurve : ViewportPolylineBase
{
  /// <summary>
  /// Initializes a new instance of the <see cref="ViewportPolyBezierCurve"/> class.
  /// </summary>
  public ViewportPolyBezierCurve() { }

  public PointCollection BezierPoints
  {
    get => (PointCollection)GetValue(dp: BezierPointsProperty);
    set => SetValue(dp: BezierPointsProperty, value: value);
  }

  public static readonly DependencyProperty BezierPointsProperty = DependencyProperty.Register(
    name: nameof(BezierPoints),
    propertyType: typeof(PointCollection),
    ownerType: typeof(ViewportPolyBezierCurve),
    typeMetadata: new FrameworkPropertyMetadata(defaultValue: null, propertyChangedCallback: OnPropertyChanged));

  private bool buildBezierPoints = true;
  public bool BuildBezierPoints
  {
    get => buildBezierPoints;
    set => buildBezierPoints = value;
  }

  private bool updating;
  protected override void UpdateUIRepresentationCore()
  {
    if (updating)
    {
      return;
    }

    updating = true;

    var transform = Plotter.Viewport.Transform;

    var geometry = PathGeometry;

    var points = Points;

    geometry.Clear();

    if (BezierPoints != null)
    {
      points = BezierPoints;

      var screenPoints = points.DataToScreen(transform: transform).ToArray();
      PathFigure figure = new();
      figure.StartPoint = screenPoints[0];
      figure.Segments.Add(value: new PolyBezierSegment(points: screenPoints.Skip(skipCount: 1), isStroked: true));
      geometry.Figures.Add(value: figure);
      geometry.FillRule = FillRule;
    }
    else if (points == null) { }
    else
    {
      PathFigure figure = new();
      if (points.Count > 0)
      {
        Point[] bezierPoints = null;
        figure.StartPoint = points[index: 0].DataToScreen(transform: transform);
        if (points.Count > 1)
        {
          var screenPoints = points.DataToScreen(transform: transform).ToArray();

          bezierPoints = BezierBuilder.GetBezierPoints(points: screenPoints).Skip(count: 1).ToArray();

          figure.Segments.Add(value: new PolyBezierSegment(points: bezierPoints, isStroked: true));
        }

        if (bezierPoints != null && buildBezierPoints)
        {
          Array.Resize(array: ref bezierPoints, newSize: bezierPoints.Length + 1);
          Array.Copy(sourceArray: bezierPoints, sourceIndex: 0, destinationArray: bezierPoints, destinationIndex: 1, length: bezierPoints.Length - 1);
          bezierPoints[0] = figure.StartPoint;

          BezierPoints = new PointCollection(collection: bezierPoints.ScreenToData(transform: transform));
        }
      }

      geometry.Figures.Add(value: figure);
      geometry.FillRule = FillRule;
    }

    updating = false;
  }
}
