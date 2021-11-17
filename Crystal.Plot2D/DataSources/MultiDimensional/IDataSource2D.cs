using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystal.Plot2D.DataSources.MultiDimensional
{
  /// <summary>
  ///   General interface for two-dimensional data source. Contains two-dimensional array of data items.
  /// </summary>
  /// <typeparam name="T">
  ///   Data type - type of each data piece.
  /// </typeparam>
  public interface IDataSource2D<T> : IGridSource2D where T : struct
  {
    /// <summary>
    ///   Gets two-dimensional data array.
    /// </summary>
    /// <value>
    ///   The data.
    /// </value>
    T[,] Data { get; }
    IDataSource2D<T> GetSubset(int x0, int y0, int countX, int countY, int stepX, int stepY);
    Range<T>? Range { get; }
    T? MissingValue { get; }
  }
}
