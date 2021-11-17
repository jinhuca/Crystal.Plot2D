using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Crystal.Plot2D.Common
{
  /// <summary>
  /// Target of rendering
  /// </summary>
  public enum RenderTo
  {
    /// <summary>
    /// Rendering directly to screen
    /// </summary>
    Screen,
    /// <summary>
    /// Rendering to bitmap, which will be drawn to screen later.
    /// </summary>
    Image
  }

  public sealed class RenderState
  {
    public DataRect RenderVisible { get; }
    public RenderTo RenderingType { get; }
    public Rect Output { get; }
    public DataRect Visible { get; }

    internal RenderState(DataRect renderVisible, DataRect visible, Rect output, RenderTo renderingType)
    {
      RenderVisible = renderVisible;
      Visible = visible;
      Output = output;
      RenderingType = renderingType;
    }
  }
}
