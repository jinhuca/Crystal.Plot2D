using System;
using System.Windows;
using Crystal.Plot2D.DataSources.MultiDimensional;

namespace Crystal.Plot2D.Common.Auxiliary;

public static class IDataSource2DExtensions
{
  public static Range<double> GetMinMax(this double[,] data)
  {
    data.VerifyNotNull(paramName: "data");

    var width = data.GetLength(dimension: 0);
    var height = data.GetLength(dimension: 1);
    Verify.IsTrueWithMessage(condition: width > 0, message: Strings.Exceptions.ArrayWidthShouldBePositive);
    Verify.IsTrueWithMessage(condition: height > 0, message: Strings.Exceptions.ArrayHeightShouldBePositive);

    var min = data[0, 0];
    var max = data[0, 0];
    for (var x = 0; x < width; x++)
    {
      for (var y = 0; y < height; y++)
      {
        if (data[x, y] < min)
        {
          min = data[x, y];
        }
        if (data[x, y] > max)
        {
          max = data[x, y];
        }
      }
    }

    Range<double> res = new(min: min, max: max);
    return res;
  }

  public static Range<double> GetMinMax(this double[,] data, double missingValue)
  {
    data.VerifyNotNull(paramName: "data");

    var width = data.GetLength(dimension: 0);
    var height = data.GetLength(dimension: 1);
    Verify.IsTrueWithMessage(condition: width > 0, message: Strings.Exceptions.ArrayWidthShouldBePositive);
    Verify.IsTrueWithMessage(condition: height > 0, message: Strings.Exceptions.ArrayHeightShouldBePositive);

    var min = double.MaxValue;
    var max = double.MinValue;
    for (var x = 0; x < width; x++)
    {
      for (var y = 0; y < height; y++)
      {
        if (data[x, y] != missingValue && data[x, y] < min)
        {
          min = data[x, y];
        }
        if (data[x, y] != missingValue && data[x, y] > max)
        {
          max = data[x, y];
        }
      }
    }

    Range<double> res = new(min: min, max: max);
    return res;
  }

  public static Range<double> GetMinMax(this IDataSource2D<double> dataSource)
  {
    dataSource.VerifyNotNull(paramName: "dataSource");
    return GetMinMax(data: dataSource.Data);
  }

  public static Range<double> GetMinMax(this IDataSource2D<double> dataSource, double missingValue)
  {
    dataSource.VerifyNotNull(paramName: "dataSource");
    return GetMinMax(data: dataSource.Data, missingValue: missingValue);
  }

  public static Range<double> GetMinMax(this IDataSource2D<double> dataSource, DataRect area)
  {
    ArgumentNullException.ThrowIfNull(dataSource);

    var min = double.PositiveInfinity;
    var max = double.NegativeInfinity;
    var width = dataSource.Width;
    var height = dataSource.Height;
    var grid = dataSource.Grid;
    var data = dataSource.Data;
    for (var ix = 0; ix < width; ix++)
    {
      for (var iy = 0; iy < height; iy++)
      {
        if (area.Contains(point: grid[ix, iy]))
        {
          var value = data[ix, iy];
          if (value < min)
          {
            min = value;
          }
          if (value > max)
          {
            max = value;
          }
        }
      }
    }

    return min < max ? new Range<double>(min: min, max: max) : new Range<double>();
  }


  public static DataRect GetGridBounds(this Point[,] grid)
  {
    var minX = grid[0, 0].X;
    var maxX = minX;
    var minY = grid[0, 0].Y;
    var maxY = minY;

    var width = grid.GetLength(dimension: 0);
    var height = grid.GetLength(dimension: 1);
    for (var ix = 0; ix < width; ix++)
    {
      for (var iy = 0; iy < height; iy++)
      {
        var pt = grid[ix, iy];
        var x = pt.X;
        var y = pt.Y;
        if (x < minX)
        {
          minX = x;
        }
        if (x > maxX)
        {
          maxX = x;
        }

        if (y < minY)
        {
          minY = y;
        }
        if (y > maxY)
        {
          maxY = y;
        }
      }
    }
    return new DataRect(point1: new Point(x: minX, y: minY), point2: new Point(x: maxX, y: maxY));
  }

  public static DataRect GetGridBounds<T>(this IDataSource2D<T> dataSource) where T : struct => dataSource.Grid.GetGridBounds();

  public static DataRect GetGridBounds<T>(this INonUniformDataSource2D<T> dataSource) where T : struct
  {
    var xCoordinates = dataSource.XCoordinates;
    var yCoordinates = dataSource.YCoordinates;
    var xMin = xCoordinates[0];
    var xMax = xCoordinates[xCoordinates.Length - 1];

    var yMin = yCoordinates[0];
    var yMax = yCoordinates[yCoordinates.Length - 1];

    var contentBounds = DataRect.FromPoints(x1: xMin, y1: yMin, x2: xMax, y2: yMax);
    return contentBounds;
  }
}
