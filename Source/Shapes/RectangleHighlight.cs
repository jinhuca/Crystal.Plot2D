using System.Windows;
using System.Windows.Media;

namespace Crystal.Plot2D.Charts
{
  /// <summary>
  /// Represents a rectangle with corners bound to viewport coordinates.
  /// </summary>
  public sealed class RectangleHighlight : ViewportShape
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="RectangleHighlight"/> class.
    /// </summary>
    public RectangleHighlight() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RectangleHighlight"/> class.
    /// </summary>
    /// <param name="bounds">The bounds.</param>
    public RectangleHighlight(Rect bounds)
    {
      Bounds = bounds;
    }

    private DataRect rect = DataRect.Empty;
    public DataRect Bounds
    {
      get { return rect; }
      set
      {
        if (rect != value)
        {
          rect = value;
          UpdateUIRepresentation();
        }
      }
    }

    protected override void UpdateUIRepresentationCore()
    {
      var transform = Plotter.Viewport.Transform;

      Point p1 = rect.XMaxYMax.DataToScreen(transform);
      Point p2 = rect.XMinYMin.DataToScreen(transform);
      rectGeometry.Rect = new Rect(p1, p2);
    }

    private readonly RectangleGeometry rectGeometry = new();
    protected override Geometry DefiningGeometry
    {
      get { return rectGeometry; }
    }
  }
}
