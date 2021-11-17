using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystal.Plot2D.DataSources.MultiDimensional
{
  public interface INonUniformDataSource2D<T> : IDataSource2D<T> where T : struct
  {
    double[] XCoordinates { get; }
    double[] YCoordinates { get; }
  }
}
