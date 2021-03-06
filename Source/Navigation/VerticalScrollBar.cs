using System;
using System.Windows;
using System.Windows.Controls;

namespace Crystal.Plot2D.Charts
{
  [Obsolete("Working wrongly.", true)]
  public sealed class VerticalScrollBar : PlotterScrollBar
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="VerticalScrollBar"/> class.
    /// </summary>
    public VerticalScrollBar()
    {
      ScrollBar.Orientation = Orientation.Vertical;
    }

    private Range<double> GetRange(Rect domain)
    {
      return new Range<double>(domain.Top, domain.Bottom);
    }

    protected override DataRect CreateVisibleRect(DataRect rect, double scrollValue)
    {
      rect.YMin = scrollValue;
      return rect;
    }

    protected override Panel GetHostPanel(PlotterBase plotter)
    {
      return plotter.LeftPanel;
    }

    protected override void UpdateScrollBar(Viewport2D viewport)
    {
      if (viewport != null && !viewport.Domain.IsEmpty)
      {
        if (ScrollBar.Track != null)
        {
          //ScrollBar.Track.IsDirectionReversed = true;
        }

        visibleRange = new Range<double>(viewport.Visible.YMin, viewport.Visible.YMax);
        domainRange = new Range<double>(viewport.Domain.YMin, viewport.Domain.YMax);

        double size = visibleRange.Max - visibleRange.Min;
        ScrollBar.ViewportSize = size;

        ScrollBar.Minimum = domainRange.Min + size;
        ScrollBar.Maximum = domainRange.Max;

        ScrollBar.Value = visibleRange.Min;
        ScrollBar.Visibility = Visibility.Visible;
      }
      else
      {
        ScrollBar.Visibility = Visibility.Collapsed;
      }
    }

    private Range<double> visibleRange;
    private Range<double> domainRange;
  }
}
