using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Crystal.Plot2D.Common
{
  public class Header : ContentControl, IPlotterElement
  {
    public Header()
    {
      FontSize = 12;
      HorizontalAlignment = HorizontalAlignment.Center;
      Margin = new Thickness(0, 0, 0, 3);
    }

    public PlotterBase Plotter { get; private set; }

    public void OnPlotterAttached(PlotterBase _plotter)
    {
      Plotter = _plotter;
      _plotter.HeaderPanel.Children.Add(this);
    }

    public void OnPlotterDetaching(PlotterBase _plotter)
    {
      Plotter = null;
      _plotter.HeaderPanel.Children.Remove(this);
    }
  }
}
