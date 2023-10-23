using System;

namespace Crystal.Plot2D;

public static class IPlotterElementExtensions
{
  public static void RemoveFromPlotter(this IPlotterElement element)
  {
    if (element == null)
    {
      throw new ArgumentNullException(paramName: nameof(element));
    }

    if (element.Plotter != null)
    {
      element.Plotter.Children.Remove(item: element);
    }
  }

  public static void AddToPlotter(this IPlotterElement element, PlotterBase plotter)
  {
    if (element == null)
    {
      throw new ArgumentNullException(paramName: nameof(element));
    }
    if (plotter == null)
    {
      throw new ArgumentNullException(paramName: nameof(plotter));
    }
    plotter.Children.Add(item: element);
  }
}
