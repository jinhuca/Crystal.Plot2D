using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Crystal.Plot2D;

public sealed class PenDescription : StandardDescription
{
  /// <summary>
  /// Initializes a new instance of the <see cref="PenDescription"/> class.
  /// </summary>
  public PenDescription() { }

  /// <summary>
  /// Initializes a new instance of the <see cref="PenDescription"/> class.
  /// </summary>
  /// <param name="description">Custom description.</param>
  public PenDescription(string description) : base(description: description) { }

  protected override LegendItem CreateLegendItemCore()
  {
    return new LineLegendItem(description: this);
  }

  protected override void AttachCore(UIElement graph)
  {
    base.AttachCore(element: graph);
    if (graph is LineGraph g)
    {
      SetBinding(dp: StrokeProperty, binding: new Binding(path: nameof(g.LinePen.Brush)) { Source = g });
      SetBinding(dp: StrokeThicknessProperty, binding: new Binding(path: nameof(g.LinePen.Thickness)) { Source = g });
    }
  }

  public Brush Stroke
  {
    get => (Brush)GetValue(dp: StrokeProperty);
    set => SetValue(dp: StrokeProperty, value: value);
  }

  public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register(
    name: nameof(Stroke),
    propertyType: typeof(Brush),
    ownerType: typeof(PenDescription),
    typeMetadata: new FrameworkPropertyMetadata(propertyChangedCallback: null));

  public double StrokeThickness
  {
    get => (double)GetValue(dp: StrokeThicknessProperty);
    set => SetValue(dp: StrokeThicknessProperty, value: value);
  }

  public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(
    name: nameof(StrokeThickness),
    propertyType: typeof(double),
    ownerType: typeof(PenDescription),
    typeMetadata: new FrameworkPropertyMetadata(defaultValue: 0.0));

}
