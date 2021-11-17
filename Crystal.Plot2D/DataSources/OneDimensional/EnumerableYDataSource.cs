using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystal.Plot2D.DataSources.OneDimensional
{
  internal sealed class EnumerableYDataSource<T> : EnumerableDataSource<T>
  {
    public EnumerableYDataSource(IEnumerable<T> data) : base(data) { }
  }
}
