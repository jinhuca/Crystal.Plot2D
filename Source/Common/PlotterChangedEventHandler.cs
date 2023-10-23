using System;
using System.Windows;

namespace Crystal.Plot2D.Common;

public class PlotterChangedEventArgs : RoutedEventArgs
{
  public PlotterChangedEventArgs(PlotterBase prevPlotter, PlotterBase currPlotter, RoutedEvent routedEvent) : base(routedEvent: routedEvent)
  {
    if (prevPlotter == null && currPlotter == null)
    {
      throw new ArgumentException(message: "Both Plotters cannot be null.");
    }

    PreviousPlotter = prevPlotter;
    CurrentPlotter = currPlotter;
  }
  public PlotterBase PreviousPlotter { get; }
  public PlotterBase CurrentPlotter { get; }
}
