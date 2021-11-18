using System.Windows;
using System.Windows.Media;

namespace Crystal.Plot2D
{
  public class RectanglePointMarker : ShapePointMarker
  {
    public override void Render(DrawingContext dc, Point screenPoint)
    {
      var rec = new Rect(screenPoint, new Size(this.Diameter / 2, this.Diameter / 2));
      dc.DrawRectangle(FillBrush, OutlinePen, rec);
    }
  }
}
