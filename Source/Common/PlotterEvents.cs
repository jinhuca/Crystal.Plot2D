using System.Windows;

namespace Crystal.Plot2D.Common
{
  public static class PlotterEvents
  {
    internal static void Notify(FrameworkElement target, PlotterChangedEventArgs args)
    {
      PlotterAttachedEvent.Notify(target, args);
      PlotterChangedEvent.Notify(target, args);
      PlotterDetachingEvent.Notify(target, args);
    }

    public static PlotterEventHelper PlotterAttachedEvent => new(PlotterBase.PlotterAttachedEvent);

    public static PlotterEventHelper PlotterDetachingEvent => new(PlotterBase.PlotterDetachingEvent);

    public static PlotterEventHelper PlotterChangedEvent => new(PlotterBase.PlotterChangedEvent);
  }
}
