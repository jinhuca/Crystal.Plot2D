using System;
using Crystal.Plot2D.DataSources.OneDimensional;

namespace Crystal.Plot2D.Charts;

public interface IOneDimensionalChart
{
  IPointDataSource DataSource { get; set; }
  event EventHandler DataChanged;
}
