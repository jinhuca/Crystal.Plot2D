using Crystal.Plot2D.Charts;
using System;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Crystal.Plot2D;

public abstract class PlotterScrollBar : IPlotterElement
{
  protected PlotterScrollBar()
  {
    scrollBar.Scroll += OnScroll;
  }

  private void OnScroll(object sender, ScrollEventArgs e)
  {
    DataRect visible = plotter.Viewport.Visible;
    visible = CreateVisibleRect(rect: visible, value: scrollBar.Value);
    plotter.Viewport.Visible = visible;
  }

  private readonly ScrollBar scrollBar = new();
  protected ScrollBar ScrollBar => scrollBar;

  private PlotterBase plotter;
  protected PlotterBase Plotter => plotter;

  private void OnViewportPropertyChanged(object sender, ExtendedPropertyChangedEventArgs e)
  {
    if (e.PropertyName == "Visible" || e.PropertyName == "Output")
    {
      UpdateScrollBar(viewport: (Viewport2D)sender);
    }
  }

  #region IPlotterElement Members

  void IPlotterElement.OnPlotterAttached(PlotterBase plotter)
  {
    GetHostPanel(plotter: plotter).Children.Add(element: scrollBar);

    PlotterBase plotter2d = (PlotterBase)plotter;
    this.plotter = plotter2d;
    var viewport = plotter2d.Viewport;
    viewport.PropertyChanged += OnViewportPropertyChanged;
    viewport.DomainChanged += OnViewportDomainChanged;

    UpdateScrollBar(viewport: viewport);
  }

  protected abstract void UpdateScrollBar(Viewport2D viewport);

  protected virtual void SetValue(Range<double> visibleRange, Range<double> domainRange)
  {
    scrollBar.Value = visibleRange.Min;
  }

  private void OnViewportDomainChanged(object sender, EventArgs e)
  {
    UpdateScrollBar(viewport: (Viewport2D)sender);
  }

  protected abstract DataRect CreateVisibleRect(DataRect rect, double value);
  protected abstract Panel GetHostPanel(PlotterBase plotter);

  void IPlotterElement.OnPlotterDetaching(PlotterBase plotter)
  {
    this.plotter.Viewport.PropertyChanged -= OnViewportPropertyChanged;
    this.plotter.Viewport.DomainChanged -= OnViewportDomainChanged;

    GetHostPanel(plotter: plotter).Children.Remove(element: scrollBar);

    UpdateScrollBar(viewport: null);

    this.plotter = null;
  }

  PlotterBase IPlotterElement.Plotter => plotter;

  #endregion
}
