using System.Windows;
using System.Windows.Media;

namespace Crystal.Plot2D.Charts;

/// <summary>
/// Represents a segment with start and end points bound to viewport coordinates.
/// </summary>
public class Segment : ViewportShape
{
  /// <summary>
  /// Initializes a new instance of the <see cref="Segment"/> class.
  /// </summary>
  public Segment() { }

  /// <summary>
  /// Initializes a new instance of the <see cref="Segment"/> class.
  /// </summary>
  /// <param name="startPoint">The start point of segment.</param>
  /// <param name="endPoint">The end point of segment.</param>
  public Segment(Point startPoint, Point endPoint)
  {
    StartPoint = startPoint;
    EndPoint = endPoint;
  }

  /// <summary>
  /// Gets or sets the start point of segment.
  /// </summary>
  /// <value>The start point.</value>
  public Point StartPoint
  {
    get => (Point)GetValue(dp: StartPointProperty);
    set => SetValue(dp: StartPointProperty, value: value);
  }

  /// <summary>
  /// Identifies the StartPoint dependency property.
  /// </summary>
  public static readonly DependencyProperty StartPointProperty = DependencyProperty.Register(
    name: nameof(StartPoint),
    propertyType: typeof(Point),
    ownerType: typeof(Segment),
    typeMetadata: new FrameworkPropertyMetadata(defaultValue: new Point(), propertyChangedCallback: OnPointChanged));

  protected static void OnPointChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    Segment segment = (Segment)d;
    segment.UpdateUIRepresentation();
  }

  protected virtual void OnPointChanged() { }

  /// <summary>
  /// Gets or sets the end point of segment.
  /// </summary>
  /// <value>The end point.</value>
  public Point EndPoint
  {
    get => (Point)GetValue(dp: EndPointProperty);
    set => SetValue(dp: EndPointProperty, value: value);
  }

  /// <summary>
  /// Identifies the EndPoint dependency property.
  /// </summary>
  public static readonly DependencyProperty EndPointProperty = DependencyProperty.Register(
    name: nameof(EndPoint),
    propertyType: typeof(Point),
    ownerType: typeof(Segment),
    typeMetadata: new FrameworkPropertyMetadata(defaultValue: new Point(), propertyChangedCallback: OnPointChanged));

  protected override void UpdateUIRepresentationCore()
  {
    if (Plotter == null)
    {
      return;
    }

    var transform = Plotter.Viewport.Transform;

    Point p1 = StartPoint.DataToScreen(transform: transform);
    Point p2 = EndPoint.DataToScreen(transform: transform);

    lineGeometry.StartPoint = p1;
    lineGeometry.EndPoint = p2;
  }

  readonly LineGeometry lineGeometry = new();
  protected LineGeometry LineGeometry => lineGeometry;

  protected override Geometry DefiningGeometry => lineGeometry;
}
