﻿using System;
using System.Windows;
using Crystal.Plot2D.Common;

namespace Crystal.Plot2D.DataSources.MultiDimensional;

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

  IDataSource2D<T> GetSubset();
  Range<T>? Range { get; }
  T? MissingValue { get; }
}

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
