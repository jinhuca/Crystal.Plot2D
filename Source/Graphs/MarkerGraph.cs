using System.Windows.Media;

namespace Crystal.Plot2D;

public class MarkerGraph<T> : PointsGraphBase where T : PointMarker
{
  public T Marker { get; set; }

  protected override void OnRenderCore(DrawingContext dc, RenderState state)
  {
    if (DataSource == null || Marker == null)
    {
      return;
    }
  }
}
