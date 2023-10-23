using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace Crystal.Plot2D;

public static class CoordinateTransformExtensions
{
  #region Points

  /// <summary>
  /// Transforms point in data coordinates to screen coordinates.
  /// </summary>
  /// <param name="dataPoint">Point in data coordinates</param>
  /// <param name="transform">CoordinateTransform used to perform transformation</param>
  /// <returns>Point in screen coordinates</returns>
  public static Point DataToScreen(this Point dataPoint, CoordinateTransform transform)
  {
    return transform.DataToScreen(dataPoint: dataPoint);
  }

  /// <summary>
  /// Transforms point in screen coordinates to data coordinates.
  /// </summary>
  /// <param name="screenPoint">Point in screen coordinates</param>
  /// <param name="transform">CoordinateTransform used to perform transformation</param>
  /// <returns>Point in data coordinates</returns>
  public static Point ScreenToData(this Point screenPoint, CoordinateTransform transform)
  {
    return transform.ScreenToData(screenPoint: screenPoint);
  }

  /// <summary>
  /// Transforms point in screen coordinates to viewport coordinates.
  /// </summary>
  /// <param name="screenPoint">Point in screen coordinates</param>
  /// <param name="transform">CoordinateTransform used to perform transformation</param>
  /// <returns>Point in viewport coordinates</returns>
  public static Point ScreenToViewport(this Point screenPoint, CoordinateTransform transform)
  {
    return transform.ScreenToViewport(screenPoint: screenPoint);
  }

  /// <summary>
  /// Transforms point in viewport coordinates to screen coordinates.
  /// </summary>
  /// <param name="viewportPoint">Point in viewport coordinates</param>
  /// <param name="transform">CoordinateTransform used to perform transformation</param>
  /// <returns>Point in screen coordinates</returns>
  public static Point ViewportToScreen(this Point viewportPoint, CoordinateTransform transform)
  {
    return transform.ViewportToScreen(viewportPoint: viewportPoint);
  }

  /// <summary>
  /// Transforms point in viewport coordinates to data coordinates.
  /// </summary>
  /// <param name="viewportPoint">Point in viewport coordinates</param>
  /// <param name="transform">CoordinateTransform used to perform transformation</param>
  /// <returns>Point in data coordinates</returns>
  public static Point ViewportToData(this Point viewportPoint, CoordinateTransform transform)
  {
    return transform.DataTransform.ViewportToData(pt: viewportPoint);
  }

  /// <summary>
  /// Transforms point in data coordinates to viewport coordinates.
  /// </summary>
  /// <param name="dataPoint">Point in data coordinates</param>
  /// <param name="transform">CoordinateTransform used to perform transformation</param>
  /// <returns>Point in viewport coordinates</returns>
  public static Point DataToViewport(this Point dataPoint, CoordinateTransform transform)
  {
    return transform.DataTransform.DataToViewport(pt: dataPoint);
  }

  /// <summary>
  /// Transforms point in data coordinates to viewport coordinates.
  /// </summary>
  /// <param name="dataPoint">Point in data coordinates</param>
  /// <param name="transform">CoordinateTransform used to perform transformation</param>
  /// <returns>Point in viewport coordinates</returns>
  public static Point DataToViewport(this Point dataPoint, DataTransform transform)
  {
    return transform.DataToViewport(pt: dataPoint);
  }

  #endregion

  #region Rects

  /// <summary>
  /// Transforms rectangle from screen coordinates to data coordinates.
  /// </summary>
  /// <param name="screenRect">Rectangle in screen coordinates</param>
  /// <param name="transform">CoordinateTransform used to perform transformation</param>
  /// <returns>Rectangle in data coordinates</returns>
  public static Rect ScreenToData(this Rect screenRect, CoordinateTransform transform)
  {
    Point p1 = screenRect.BottomLeft.ScreenToData(transform);
    Point p2 = screenRect.TopRight.ScreenToData(transform);

    return new Rect(point1: p1, point2: p2);
  }

  /// <summary>
  /// Transforms rectangle from data coordinates to screen coordinates.
  /// </summary>
  /// <param name="dataRect">Rectangle in data coordinates</param>
  /// <param name="transform">CoordinateTransform used to perform transformation</param>
  /// <returns>Rectangle in screen coordinates</returns>
  public static Rect DataToScreen(this DataRect dataRect, CoordinateTransform transform)
  {
    Point p1 = dataRect.XMaxYMax.DataToScreen(transform: transform);
    Point p2 = dataRect.XMinYMin.DataToScreen(transform: transform);

    return new Rect(point1: p1, point2: p2);
  }

  /// <summary>
  /// Transforms rectangle from screen coordinates to viewport coordinates.
  /// </summary>
  /// <param name="screenRect">Rectangle in screen coordinates</param>
  /// <param name="transform">CoordinateTransform used to perform transformation</param>
  /// <returns>Rectangle in viewport coordinates</returns>
  public static DataRect ScreenToViewport(this Rect screenRect, CoordinateTransform transform)
  {
    Point p1 = screenRect.BottomLeft.ScreenToViewport(transform);
    Point p2 = screenRect.TopRight.ScreenToViewport(transform);

    return new DataRect(point1: p1, point2: p2);
  }

  /// <summary>
  /// Transforms rectangle from viewport coordinates to screen coordinates.
  /// </summary>
  /// <param name="viewportRect">Rectangle in viewport coordinates</param>
  /// <param name="transform">CoordinateTransform used to perform transformation</param>
  /// <returns>Rectangle in screen coordinates</returns>
  public static Rect ViewportToScreen(this DataRect viewportRect, CoordinateTransform transform)
  {
    Point p1 = viewportRect.XMaxYMax.ViewportToScreen(transform);
    Point p2 = viewportRect.XMinYMin.ViewportToScreen(transform);

    return new Rect(point1: p1, point2: p2);
  }

  /// <summary>
  /// Transforms rectangle from viewport coordinates to data coordinates.
  /// </summary>
  /// <param name="viewportRect">Rectangle in viewport coordinates</param>
  /// <param name="transform">CoordinateTransform used to perform transformation</param>
  /// <returns>Rectangle in data coordinates</returns>
  public static DataRect ViewportToData(this DataRect viewportRect, CoordinateTransform transform)
  {
    Point p1 = viewportRect.XMaxYMax.ViewportToData(transform);
    Point p2 = viewportRect.XMinYMin.ViewportToData(transform);

    return new DataRect(point1: p1, point2: p2);
  }

  /// <summary>
  /// Transforms rectangle from data coordinates to viewport coordinates.
  /// </summary>
  /// <param name="dataRect">Rectangle in data coordinates</param>
  /// <param name="transform">CoordinateTransform used to perform transformation</param>
  /// <returns>Rectangle in viewport coordinates</returns>
  public static DataRect DataToViewport(this DataRect dataRect, CoordinateTransform transform)
  {
    return new DataRect(dataRect.XMaxYMax.DataToViewport(transform), dataRect.XMinYMin.DataToViewport(transform));
  }

  /// <summary>
  /// Transforms rectangle from viewport coordinates to data coordinates.
  /// </summary>
  /// <param name="viewportRect">Rectangle in viewport coordinates</param>
  /// <param name="transform">CoordinateTransform used to perform transformation</param>
  /// <returns>Rectangle in data coordinates</returns>
  public static DataRect ViewportToData(this DataRect viewportRect, DataTransform transform)
  {
    Point p1 = transform.ViewportToData(pt: viewportRect.XMaxYMax);
    Point p2 = transform.ViewportToData(pt: viewportRect.XMinYMin);

    return new DataRect(point1: p1, point2: p2);
  }

  /// <summary>
  /// Transforms rectangle from data coordinates to viewport coordinates.
  /// </summary>
  /// <param name="dataRect">Rectangle in data coordinates</param>
  /// <param name="transform">CoordinateTransform used to perform transformation</param>
  /// <returns>Rectangle in viewport coordinates</returns>
  public static DataRect DataToViewport(this DataRect dataRect, DataTransform transform)
  {
    Point p1 = transform.DataToViewport(pt: dataRect.XMinYMin);
    Point p2 = transform.DataToViewport(pt: dataRect.XMaxYMax);

    return new DataRect(point1: p1, point2: p2);
  }

  #endregion

  #region Collections

  public static IEnumerable<Point> ViewportToScreen(this IEnumerable<Point> viewportPoints, CoordinateTransform transform)
  {
    foreach (var point in viewportPoints)
    {
      yield return point.ViewportToScreen(transform);
    }
  }

  public static IEnumerable<Point> DataToScreen(this IEnumerable<Point> dataPoints, CoordinateTransform transform)
  {
    foreach (var point in dataPoints)
    {
      yield return point.DataToScreen(transform: transform);
    }
  }

  /// <summary>
  /// Transforms list of points from data coordinates to screen coordinates.
  /// </summary>
  /// <param name="dataPoints">Points in data coordinates</param>
  /// <param name="transform">CoordinateTransform used to perform transformation</param>
  /// <returns>Points in screen coordinates</returns>
  public static List<Point> DataToScreenAsList(this IEnumerable<Point> dataPoints, CoordinateTransform transform)
  {
    List<Point> res;

    if (dataPoints is ICollection<Point> iCollection)
    {
      res = new List<Point>(capacity: iCollection.Count);
    }
    else
    {
      res = new List<Point>();
    }

    foreach (var point in dataPoints)
    {
      res.Add(item: transform.DataToScreen(dataPoint: point));
    }

    return res;
  }

  /// <summary>
  /// Transforms list of points from data coordinates to screen coordinates.
  /// </summary>
  /// <param name="transform">Coordinate transform used to perform transformation</param>
  /// <param name="dataPoints">Points in data coordinates</param>
  /// <returns>List of points in screen coordinates</returns>
  [SuppressMessage(category: "Microsoft.Design", checkId: "CA1002:DoNotExposeGenericLists")]
  public static List<Point> DataToScreenAsList(this CoordinateTransform transform, IEnumerable<Point> dataPoints)
  {
    return dataPoints.DataToScreenAsList(transform);
  }

  /// <summary>
  /// Transforms list of points from data coordinates to viewport coordinates.
  /// </summary>
  /// <param name="dataPoints">Points in data coordinates</param>
  /// <param name="transform">Data transform used to perform transformation</param>
  /// <returns>List of points in viewport coordinates</returns>
  public static IEnumerable<Point> DataToViewport(this IEnumerable<Point> dataPoints, DataTransform transform)
  {
    foreach (Point pt in dataPoints)
    {
      yield return pt.DataToViewport(transform);
    }
  }

  public static IEnumerable<Point> DataToViewport(this IEnumerable<Point> dataPoints, CoordinateTransform transform)
  {
    return dataPoints.DataToViewport(transform.DataTransform);
  }

  public static IEnumerable<Point> ScreenToViewport(this IEnumerable<Point> screenPoints, CoordinateTransform transform)
  {
    foreach (Point pt in screenPoints)
    {
      yield return pt.ScreenToViewport(transform);
    }
  }

  public static IEnumerable<Point> ScreenToData(this IEnumerable<Point> screenPoints, CoordinateTransform transform)
  {
    foreach (Point pt in screenPoints)
    {
      yield return pt.ScreenToData(transform);
    }
  }

  #endregion
}
