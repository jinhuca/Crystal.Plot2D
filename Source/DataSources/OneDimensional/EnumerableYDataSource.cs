using System.Collections.Generic;

namespace Crystal.Plot2D.DataSources;

internal sealed class EnumerableYDataSource<T> : EnumerableDataSource<T>
{
  public EnumerableYDataSource(IEnumerable<T> data) : base(data) { }
}
