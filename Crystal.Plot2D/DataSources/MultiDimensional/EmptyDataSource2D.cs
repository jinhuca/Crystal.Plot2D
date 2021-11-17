using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Crystal.Plot2D.DataSources.MultiDimensional
{
  /// <summary>
  ///   Defines empty two-dimensional data source.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public sealed class EmptyDataSource2D<T> : IDataSource2D<T> where T : struct
  {
    public T[,] Data { get; } = new T[0, 0];
    public Point[,] Grid { get; } = new Point[0, 0];
    public int Width => 0;
    public int Height => 0;
    private void RaiseChanged() => Changed?.Invoke(this, EventArgs.Empty);
    public event EventHandler Changed;

    #region IDataSource2D<T> Members

    public Charts.Range<T>? Range => throw new NotImplementedException();
    public T? MissingValue => throw new NotImplementedException();

    #endregion

    #region IDataSource2D<T> Members

    public IDataSource2D<T> GetSubset(int x0, int y0, int countX, int countY, int stepX, int stepY)
      => throw new NotImplementedException();

    #endregion
  }
}
