using System;

namespace Crystal.Plot2D.Axes;

public interface IValueConversion<T>
{
  Func<T, double> ConvertToDouble { get; set; }
  Func<double, T> ConvertFromDouble { get; set; }
}
