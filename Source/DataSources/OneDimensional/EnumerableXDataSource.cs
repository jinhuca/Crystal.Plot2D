using System.Collections.Generic;

namespace Crystal.Plot2D.DataSources;

internal sealed class EnumerableXDataSource<T> : EnumerableDataSource<T>
{
  public EnumerableXDataSource(IEnumerable<T> data) : base(data: data) { }
}
