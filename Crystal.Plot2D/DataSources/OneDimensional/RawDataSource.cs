using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Crystal.Plot2D.DataSources.OneDimensional
{
  public sealed class RawDataSource : EnumerableDataSourceBase<Point>
  {
    public RawDataSource(params Point[] data) : base(data) { }
    public RawDataSource(IEnumerable<Point> data) : base(data) { }

    public override IPointEnumerator GetEnumerator(DependencyObject context)
    {
      return new RawPointEnumerator(this);
    }
  }
}
