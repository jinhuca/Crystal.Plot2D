using System.Windows;
using System.Windows.Media;

namespace Crystal.Plot2D.Charts;

/// <summary>
///   Represents a polyline with points in Viewport coordinates.
/// </summary>
public sealed class ViewportPolyline : ViewportPolylineBase
{
  /// <summary>
  ///   Initializes a new instance of the <see cref="ViewportPolyline"/> class.
  /// </summary>
  public ViewportPolyline() { }

  protected override void UpdateUIRepresentationCore()
  {
    var transform = Plotter.Viewport.Transform;

    PathGeometry geometry = PathGeometry;

    PointCollection points = Points;
    geometry.Clear();

    if (points == null) { }
    else
    {
      PathFigure figure = new();
      if (points.Count > 0)
      {
        figure.StartPoint = points[index: 0].DataToScreen(transform: transform);
        if (points.Count > 1)
        {
          Point[] pointArray = new Point[points.Count - 1];
          for (int i = 1; i < points.Count; i++)
          {
            pointArray[i - 1] = points[index: i].DataToScreen(transform: transform);
          }
          figure.Segments.Add(value: new PolyLineSegment(points: pointArray, isStroked: true));
        }
      }
      geometry.Figures.Add(value: figure);
      geometry.FillRule = FillRule;
    }
  }
}
