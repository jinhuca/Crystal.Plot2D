using System;

namespace Crystal.Plot2D.Charts
{
  public interface IValueConversion<T>
  {
    Func<T, double> ConvertToDouble { get; set; }
    Func<double, T> ConvertFromDouble { get; set; }
  }
}
