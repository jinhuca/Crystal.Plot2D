using System.Windows;
using System.Windows.Controls;

namespace Crystal.Plot2D;

public class Header : ContentControl, IPlotterElement
{
  public Header()
  {
    FontSize = 12;
    HorizontalAlignment = HorizontalAlignment.Center;
    Margin = new Thickness(left: 0, top: 0, right: 0, bottom: 3);
  }

  public PlotterBase Plotter { get; private set; }

  public void OnPlotterAttached(PlotterBase thePlotter)
  {
    Plotter = thePlotter;
    thePlotter.HeaderPanel.Children.Add(element: this);
  }

  public void OnPlotterDetaching(PlotterBase thePlotter)
  {
    Plotter = null;
    thePlotter.HeaderPanel.Children.Remove(element: this);
  }
}