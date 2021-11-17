using System;

namespace Crystal.Plot2D.DataSources.MultiDimensional
{
  /// <summary>
  ///   General interface for two-dimensional data grids. Contains two-dimensional array of data points.
  /// </summary>
  public interface IGridSource2D
  {
    /// <summary>
    ///   Gets the grid of data source.
    /// </summary>
    /// <value>
    ///   The grid.
    /// </value>
    Point[,] Grid { get; }

    /// <summary>
    ///   Gets data grid width.
    /// </summary>
    /// <value>
    ///   The width.
    /// </value>
    int Width { get; }

    /// <summary>
    ///   Gets data grid height.
    /// </summary>
    /// <value>
    ///   The height.
    /// </value>
    int Height { get; }

    /// <summary>
    ///   Occurs when data source changes.
    /// </summary>
    event EventHandler Changed;
  }
}
