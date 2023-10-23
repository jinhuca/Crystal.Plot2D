using System;
using System.Collections.Generic;
using System.Windows;

namespace Crystal.Plot2D.Common;

public sealed class PlotterEventHelper
{
  private readonly RoutedEvent @event;
  internal PlotterEventHelper(RoutedEvent @event) => this.@event = @event;

  // todo use a weakReference here
  private readonly Dictionary<DependencyObject, EventHandler<PlotterChangedEventArgs>> handlers = new();

  public void Subscribe(DependencyObject target, EventHandler<PlotterChangedEventArgs> handler)
  {
    if (target == null)
    {
      throw new ArgumentNullException(paramName: nameof(target));
    }

    if (handler == null)
    {
      throw new ArgumentNullException(paramName: nameof(handler));
    }

    handlers.Add(key: target, value: handler);
  }

  internal void Notify(FrameworkElement target, PlotterChangedEventArgs args)
  {
    if (args.RoutedEvent == @event && handlers.TryGetValue(target, out var handler))
    {
      handler(sender: target, e: args);
    }
  }
}
