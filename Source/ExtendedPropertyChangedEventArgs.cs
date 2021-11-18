using System;
using System.Windows;

namespace Crystal.Plot2D
{
  public sealed class ExtendedPropertyChangedEventArgs : EventArgs
  {
    public string PropertyName { get; set; }
    public object OldValue { get; set; }
    public object NewValue { get; set; }

    public static ExtendedPropertyChangedEventArgs FromDependencyPropertyChanged(DependencyPropertyChangedEventArgs e)
      => new ExtendedPropertyChangedEventArgs { PropertyName = e.Property.Name, NewValue = e.NewValue, OldValue = e.OldValue };
  }
}
