using System.Windows;
using System.Windows.Controls;

namespace Crystal.Plot2D;

/// <summary>
///   Represents a text in Plotter's footer.
/// </summary>
public class Footer : ContentControl, IPlotterElement
{
  /// <summary>
  ///   Initializes a new instance of the <see cref="Footer"/> class.
  /// </summary>
  public Footer()
  {
    HorizontalAlignment = HorizontalAlignment.Center;
    Margin = new Thickness(left: 0, top: 0, right: 0, bottom: 3);
  }

  void IPlotterElement.OnPlotterAttached(PlotterBase thePlotter)
  {
    Plotter = thePlotter;
    thePlotter.FooterPanel.Children.Add(element: this);
  }

  void IPlotterElement.OnPlotterDetaching(PlotterBase thePlotter)
  {
    thePlotter.FooterPanel.Children.Remove(element: this);
    Plotter = null;
  }

  public PlotterBase Plotter { get; private set; }
}
