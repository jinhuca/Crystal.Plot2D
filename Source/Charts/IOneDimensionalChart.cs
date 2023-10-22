using Crystal.Plot2D.DataSources;
using System;

namespace Crystal.Plot2D;

public interface IOneDimensionalChart
{
  IPointDataSource DataSource { get; set; }
  event EventHandler DataChanged;
}
