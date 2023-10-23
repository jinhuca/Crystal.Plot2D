using System.Windows;

namespace Crystal.Plot2D.Charts;

/// <summary>
/// Represents an infinite vertical line with x viewport coordinate.
/// </summary>
public sealed class VerticalLine : SimpleLine
{
  /// <summary>
  /// Initializes a new instance of the <see cref="VerticalLine"/> class.
  /// </summary>
  public VerticalLine() { }

  /// <summary>
  /// Initializes a new instance of the <see cref="VerticalLine"/> class with specified x coordinate.
  /// </summary>
  /// <param name="xCoordinate">The x coordinate.</param>
  public VerticalLine(double xCoordinate)
  {
    Value = xCoordinate;
  }

  protected override void UpdateUIRepresentationCore()
  {
    var transform = Plotter.Viewport.Transform;

    Point p1 = new Point(x: Value, y: Plotter.Viewport.Visible.YMin).DataToScreen(transform: transform);
    Point p2 = new Point(x: Value, y: Plotter.Viewport.Visible.YMax).DataToScreen(transform: transform);

    LineGeometry.StartPoint = p1;
    LineGeometry.EndPoint = p2;
  }
}
