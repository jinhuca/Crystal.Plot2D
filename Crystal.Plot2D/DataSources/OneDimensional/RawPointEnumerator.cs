using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystal.Plot2D.DataSources.OneDimensional
{
  public sealed class RawPointEnumerator : IPointEnumerator
  {
    public IEnumerator Enumerator { get; }

    public RawPointEnumerator(RawDataSource dataSource) => Enumerator = dataSource.Data.GetEnumerator();

    public bool MoveNext() => Enumerator.MoveNext();

    public void GetCurrent(ref Point p) => p = (Point)Enumerator.Current;

    public void ApplyMappings(DependencyObject target)
    {
      // do nothing here - no mapping supported
    }

    public void Dispose()
    {
      // do nothing here
    }
  }
}
