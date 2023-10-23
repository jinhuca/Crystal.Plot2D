using Crystal.Plot2D.Charts;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Crystal.Plot2D.DataSources;

public static class DataSourceHelper
{
  public static IEnumerable<Point> GetPoints(IPointDataSource dataSource) => GetPoints(dataSource: dataSource, context: null);

  public static IEnumerable<Point> GetPoints(IPointDataSource dataSource, DependencyObject context)
  {
    if (dataSource == null)
    {
      throw new ArgumentNullException(paramName: nameof(dataSource));
    }

    if (context == null)
    {
      context = new DataSource2dContext();
    }

    using IPointEnumerator enumerator = dataSource.GetEnumerator(context: context);
    Point p = new();
    while (enumerator.MoveNext())
    {
      enumerator.GetCurrent(p: ref p);
      yield return p;
      p = new Point();
    }
  }
}