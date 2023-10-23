using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Crystal.Plot2D;

///<summary>
/// Helper class to draw semitransparent rectangle over the selection area.
///</summary>
public sealed class RectangleSelectionAdorner : Adorner
{

  private Rect? border;
  public Rect? Border
  {
    get => border;
    set => border = value;
  }

  public Brush Fill
  {
    get => (Brush)GetValue(dp: FillProperty);
    set => SetValue(dp: FillProperty, value: value);
  }

  public static readonly DependencyProperty FillProperty =
    DependencyProperty.Register(
      name: "InnerBrush",
      propertyType: typeof(Brush),
      ownerType: typeof(RectangleSelectionAdorner),
      typeMetadata: new FrameworkPropertyMetadata(
        defaultValue: new SolidColorBrush(color: Color.FromArgb(a: 60, r: 100, g: 100, b: 100)),
        flags: FrameworkPropertyMetadataOptions.AffectsRender));

  private Pen pen;
  public Pen Pen
  {
    get => pen;
    set => pen = value;
  }

  public RectangleSelectionAdorner(UIElement element)
    : base(adornedElement: element)
  {
    pen = new Pen(brush: Brushes.Black, thickness: 1.0);
  }

  protected override void OnRender(DrawingContext dc)
  {
    if (border.HasValue)
    {
      dc.DrawRectangle(brush: Fill, pen: pen, rectangle: border.Value);
    }
  }
}
