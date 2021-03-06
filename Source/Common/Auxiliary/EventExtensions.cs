using Crystal.Plot2D.Common;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace Crystal.Plot2D
{
  public static class EventExtensions
  {
    [DebuggerStepThrough]
    [DebuggerHidden]
    public static void Raise<T>(this EventHandler<T> @event, object sender, T args) where T : EventArgs => @event?.Invoke(sender, args);

    [DebuggerStepThrough]
    [DebuggerHidden]
    public static void Raise(this EventHandler @event, object sender) => @event?.Invoke(sender, EventArgs.Empty);

    [DebuggerStepThrough]
    [DebuggerHidden]
    public static void Raise(this EventHandler @event, object sender, EventArgs args) => @event?.Invoke(sender, args);

    [DebuggerStepThrough]
    [DebuggerHidden]
    public static void Raise(this PropertyChangedEventHandler @event, object sender, string propertyName) => @event?.Invoke(sender, new PropertyChangedEventArgs(propertyName));

    /// <summary>
    /// Raises the specified event with Reset action.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="sender">The sender.</param>
    [DebuggerStepThrough]
    [DebuggerHidden]
    public static void Raise(this NotifyCollectionChangedEventHandler @event, object sender) => @event?.Invoke(sender, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

    [DebuggerStepThrough]
    [DebuggerHidden]
    public static void Raise(this NotifyCollectionChangedEventHandler @event, object sender, NotifyCollectionChangedAction action) => @event?.Invoke(sender, new NotifyCollectionChangedEventArgs(action));

    [DebuggerStepThrough]
    [DebuggerHidden]
    public static void Raise(this NotifyCollectionChangedEventHandler @event, object sender, NotifyCollectionChangedEventArgs e)
    {
      if (e == null)
      {
        throw new ArgumentNullException("e");
      }

      @event?.Invoke(sender, e);
    }

    [DebuggerStepThrough]
    [DebuggerHidden]
    public static void RaiseRoutedEvent(this UIElement sender, RoutedEvent routedEvent) => sender.RaiseEvent(new RoutedEventArgs(routedEvent));

    /// <summary>
    /// Raises the specified value changed event.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="event">The event.</param>
    /// <param name="sender">The sender of event.</param>
    /// <param name="prevValue">The previous value.</param>
    /// <param name="currValue">The current value.</param>
    [DebuggerStepThrough]
    [DebuggerHidden]
    public static void Raise<TValue>(this EventHandler<ValueChangedEventArgs<TValue>> @event, object sender, TValue prevValue, TValue currValue)
    {
      if (@event != null)
      {
        ValueChangedEventArgs<TValue> args = new(prevValue, currValue);
        @event(sender, args);
      }
    }

    /// <summary>
    /// Raises the specified value changed event.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="event">The event.</param>
    /// <param name="sender">The sender of event.</param>
    /// <param name="prevValue">The previous value.</param>
    /// <param name="currValue">The current value.</param>
    [DebuggerStepThrough]
    [DebuggerHidden]
    public static void Raise<TValue>(this EventHandler<ValueChangedEventArgs<TValue>> @event, object sender, object prevValue, object currValue)
    {
      if (@event != null)
      {
        ValueChangedEventArgs<TValue> args = new((TValue)prevValue, (TValue)currValue);
        @event(sender, args);
      }
    }
  }
}
