using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystal.Plot2D.DataSources.OneDimensional
{
  internal sealed class EnumerableXDataSource<T> : EnumerableDataSource<T>
  {
    public EnumerableXDataSource(IEnumerable<T> data) : base(data) { }
  }
}
