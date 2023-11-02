using System.Windows;
using System.Windows.Controls;
using Crystal.Plot2D.Common;

namespace Crystal.Plot2D.Navigation;

/// <summary>
/// Represents a horizontal scroll bar on the bottom of <see cref="PlotterBase"/>.
/// Uses Plotter.Plotter.Viewport.DataDomain property as a source of data about current position and position limits.
/// </summary>
public sealed class HorizontalScrollBar : PlotterScrollBar
{
  /// <summary>
  /// Initializes a new instance of the <see cref="HorizontalScrollBar"/> class.
  /// </summary>
  public HorizontalScrollBar()
  {
    ScrollBar.Orientation = Orientation.Horizontal;
  }

  protected override void UpdateScrollBar(Viewport2D viewport)
  {
    if (viewport != null && !viewport.Domain.IsEmpty)
    {
      var visibleRange_ = new Range<double>(min: viewport.Visible.XMin, max: viewport.Visible.XMax);
      var size_ = visibleRange_.Max - visibleRange_.Min;
      ScrollBar.ViewportSize = size_;

      var domainRange_ = new Range<double>(min: viewport.Domain.XMin, max: viewport.Domain.XMax);
      ScrollBar.Minimum = domainRange_.Min;
      ScrollBar.Maximum = domainRange_.Max - size_;

      ScrollBar.Value = visibleRange_.Min;

      ScrollBar.Visibility = Visibility.Visible;
    }
    else
    {
      ScrollBar.Visibility = Visibility.Collapsed;
    }
  }

  protected override DataRect CreateVisibleRect(DataRect rect, double value)
  {
    rect.XMin = value;
    return rect;
  }

  protected override Panel GetHostPanel(PlotterBase plotter)
  {
    return plotter.BottomPanel;
  }
}
