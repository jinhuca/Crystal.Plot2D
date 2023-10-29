using Crystal.Plot2D.Common;
using Crystal.Plot2D.Common.Auxiliary;
using Crystal.Plot2D.DataSources.MultiDimensional;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Crystal.Plot2D.Isolines;

/// <summary>
/// Generates geometric object for isolines of the input 2d scalar field.
/// </summary>
public sealed class IsolineBuilder
{
  /// <summary>
  /// The density of isolines means the number of levels to draw.
  /// </summary>
  private const int density = 12;

  private bool[,] processed;

  /// <summary>Number to be treated as missing value. NaN if no missing value is specified</summary>
  private double missingValue = double.NaN;

  static IsolineBuilder()
  {
    SetCellDictionaries();
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="IsolineBuilder"/> class.
  /// </summary>
  public IsolineBuilder() { }

  /// <summary>
  /// Initializes a new instance of the <see cref="IsolineBuilder"/> class for specified 2d scalar data source.
  /// </summary>
  /// <param name="dataSource">The data source with 2d scalar data.</param>
  public IsolineBuilder(IDataSource2D<double> dataSource)
  {
    DataSource = dataSource;
  }

  public double MissingValue
  {
    get => missingValue;
    set => missingValue = value;
  }

  #region Private methods

  private static readonly Dictionary<int, Dictionary<int, Edge>> dictChooser = new();
  private static void SetCellDictionaries()
  {
    var bottomDict = new Dictionary<int, Edge>();
    bottomDict.Add(key: (int)CellBitmask.RightBottom, value: Edge.Right);
    bottomDict.Add(value: Edge.Left,
      keys: new[]
      {
        CellBitmask.LeftTop,
        CellBitmask.LeftBottom | CellBitmask.RightBottom | CellBitmask.RightTop,
        CellBitmask.LeftTop | CellBitmask.RightBottom | CellBitmask.RightTop,
        CellBitmask.LeftBottom
      });
    bottomDict.Add(value: Edge.Right,
      keys: new[]
      {
        CellBitmask.RightTop,
        CellBitmask.LeftBottom | CellBitmask.RightBottom | CellBitmask.LeftTop,
        CellBitmask.LeftBottom | CellBitmask.LeftTop | CellBitmask.RightTop
      });
    bottomDict.Add(value: Edge.Top,
      keys: new[]
      {
        CellBitmask.RightBottom | CellBitmask.RightTop,
        CellBitmask.LeftBottom | CellBitmask.LeftTop
      });

    var leftDict = new Dictionary<int, Edge>();
    leftDict.Add(value: Edge.Top,
      keys: new[]
      {
        CellBitmask.LeftTop,
        CellBitmask.LeftBottom | CellBitmask.RightBottom | CellBitmask.RightTop
      });
    leftDict.Add(value: Edge.Right,
      keys: new[]
      {
        CellBitmask.LeftTop | CellBitmask.RightTop,
        CellBitmask.LeftBottom | CellBitmask.RightBottom
      });
    leftDict.Add(value: Edge.Bottom,
      keys: new[]
      {
        CellBitmask.RightBottom | CellBitmask.RightTop | CellBitmask.LeftTop,
        CellBitmask.LeftBottom
      });

    var topDict = new Dictionary<int, Edge>();
    topDict.Add(value: Edge.Right,
      keys: new[]
      {
        CellBitmask.RightTop,
        CellBitmask.LeftTop | CellBitmask.LeftBottom | CellBitmask.RightBottom
      });
    topDict.Add(value: Edge.Right,
      keys: new[]
      {
        CellBitmask.RightBottom,
        CellBitmask.LeftTop | CellBitmask.LeftBottom | CellBitmask.RightTop
      });
    topDict.Add(value: Edge.Left,
      keys: new[]
      {
        CellBitmask.RightBottom | CellBitmask.RightTop | CellBitmask.LeftTop,
        CellBitmask.LeftBottom,
        CellBitmask.LeftTop,
        CellBitmask.LeftBottom | CellBitmask.RightBottom | CellBitmask.RightTop
      });
    topDict.Add(value: Edge.Bottom,
      keys: new[]
      {
        CellBitmask.RightBottom | CellBitmask.RightTop,
        CellBitmask.LeftTop | CellBitmask.LeftBottom
      });

    var rightDict = new Dictionary<int, Edge>();
    rightDict.Add(value: Edge.Top,
      keys: new[]
      {
        CellBitmask.RightTop,
        CellBitmask.LeftBottom | CellBitmask.RightBottom | CellBitmask.LeftTop
      });
    rightDict.Add(value: Edge.Left,
      keys: new[]
      {
        CellBitmask.LeftTop | CellBitmask.RightTop,
        CellBitmask.LeftBottom | CellBitmask.RightBottom
      });
    rightDict.Add(value: Edge.Bottom,
      keys: new[]
      {
        CellBitmask.RightBottom,
        CellBitmask.LeftTop | CellBitmask.LeftBottom | CellBitmask.RightTop
      });

    dictChooser.Add(key: (int)Edge.Left, value: leftDict);
    dictChooser.Add(key: (int)Edge.Right, value: rightDict);
    dictChooser.Add(key: (int)Edge.Bottom, value: bottomDict);
    dictChooser.Add(key: (int)Edge.Top, value: topDict);
  }

  private Edge GetOutEdge(Edge inEdge, ValuesInCell cv, IrregularCell rect, double value)
  {
    // value smaller than all values in corners or 
    // value greater than all values in corners
    if (!cv.ValueBelongTo(value: value))
    {
      throw new IsolineGenerationException(message: Strings.Exceptions.IsolinesValueIsOutOfCell);
    }

    var cellVal = cv.GetCellValue(value: value);
    var dict = dictChooser[key: (int)inEdge];
    if (dict.ContainsKey(key: (int)cellVal))
    {
      var result = dict[key: (int)cellVal];
      switch (result)
      {
        case Edge.Left:
          if (cv.LeftTop.IsNaN() || cv.LeftBottom.IsNaN())
          {
            result = Edge.None;
          }

          break;
        case Edge.Right:
          if (cv.RightTop.IsNaN() || cv.RightBottom.IsNaN())
          {
            result = Edge.None;
          }

          break;
        case Edge.Top:
          if (cv.RightTop.IsNaN() || cv.LeftTop.IsNaN())
          {
            result = Edge.None;
          }

          break;
        case Edge.Bottom:
          if (cv.LeftBottom.IsNaN() || cv.RightBottom.IsNaN())
          {
            result = Edge.None;
          }

          break;
      }
      return result;
    }
    else if (cellVal.IsDiagonal())
    {
      return GetOutForOpposite(inEdge: inEdge, cellVal: cellVal, value: value, cellValues: cv, rect: rect);
    }

    const double near_zero = 0.0001;
    const double near_one = 1 - near_zero;

    var lt = cv.LeftTop;
    var rt = cv.RightTop;
    var rb = cv.RightBottom;
    var lb = cv.LeftBottom;

    switch (inEdge)
    {
      case Edge.Left:
        if (value == lt)
        {
          value = near_one * lt + near_zero * lb;
        }
        else if (value == lb)
        {
          value = near_one * lb + near_zero * lt;
        }
        else
        {
          return Edge.None;
        }
        
        // Now this is possible because of missing value
        //throw new IsolineGenerationException(Strings.Exceptions.IsolinesUnsupportedCase);
        break;
      case Edge.Top:
        if (value == rt)
        {
          value = near_one * rt + near_zero * lt;
        }
        else if (value == lt)
        {
          value = near_one * lt + near_zero * rt;
        }
        else
        {
          return Edge.None;
        }
        
        // Now this is possible because of missing value
        //throw new IsolineGenerationException(Strings.Exceptions.IsolinesUnsupportedCase);
        break;
      case Edge.Right:
        if (value == rb)
        {
          value = near_one * rb + near_zero * rt;
        }
        else if (value == rt)
        {
          value = near_one * rt + near_zero * rb;
        }
        else
        {
          return Edge.None;
        }
        
        // Now this is possible because of missing value
        //throw new IsolineGenerationException(Strings.Exceptions.IsolinesUnsupportedCase);
        break;
      case Edge.Bottom:
        if (value == rb)
        {
          value = near_one * rb + near_zero * lb;
        }
        else if (value == lb)
        {
          value = near_one * lb + near_zero * rb;
        }
        else
        {
          return Edge.None;
        }
        // Now this is possible because of missing value
        //throw new IsolineGenerationException(Strings.Exceptions.IsolinesUnsupportedCase);
        break;
    }

    // Recursion?
    //return GetOutEdge(inEdge, cv, rect, value);

    return Edge.None;
  }

  private Edge GetOutForOpposite(Edge inEdge, CellBitmask cellVal, double value, ValuesInCell cellValues, IrregularCell rect)
  {
	    var subCell = GetSubCell(inEdge: inEdge, value: value, vc: cellValues);

    var iters = 1000; // max number of iterations
    do
    {
      var subValues = cellValues.GetSubCell(subCell: subCell);
      var subRect = rect.GetSubRect(sub: subCell);
      var outEdge = GetOutEdge(inEdge: inEdge, cv: subValues, rect: subRect, value: value);
      if (outEdge == Edge.None)
      {
        return Edge.None;
      }

      var isAppropriate = subCell.IsAppropriate(edge: outEdge);
      if (isAppropriate)
      {
        var sValues = subValues.GetSubCell(subCell: subCell);

        var point = GetPointXY(edge: outEdge, value: value, vc: subValues, rect: subRect);
        segments.AddPoint(p: point);
        return outEdge;
      }
      else
      {
        subCell = GetAdjacentEdge(sub: subCell, edge: outEdge);
      }

      var e = (byte)outEdge;
      inEdge = (Edge)(e > 2 ? e >> 2 : e << 2);
      iters--;
    } while (iters >= 0);

    throw new IsolineGenerationException(message: Strings.Exceptions.IsolinesDataIsUndetailized);
  }

  private static SubCell GetAdjacentEdge(SubCell sub, Edge edge)
  {
    var res = SubCell.LeftBottom;

    switch (sub)
    {
      case SubCell.LeftBottom:
        res = edge == Edge.Top ? SubCell.LeftTop : SubCell.RightBottom;
        break;
      case SubCell.LeftTop:
        res = edge == Edge.Bottom ? SubCell.LeftBottom : SubCell.RightTop;
        break;
      case SubCell.RightBottom:
        res = edge == Edge.Top ? SubCell.RightTop : SubCell.LeftBottom;
        break;
      case SubCell.RightTop:
      default:
        res = edge == Edge.Bottom ? SubCell.RightBottom : SubCell.LeftTop;
        break;
    }

    return res;
  }

  private static SubCell GetSubCell(Edge inEdge, double value, ValuesInCell vc)
  {
    var lb = vc.LeftBottom;
    var rb = vc.RightBottom;
    var rt = vc.RightTop;
    var lt = vc.LeftTop;

    var res = SubCell.LeftBottom;
    switch (inEdge)
    {
      case Edge.Left:
        res = Math.Abs(value: value - lb) < Math.Abs(value: value - lt) ? SubCell.LeftBottom : SubCell.LeftTop;
        break;
      case Edge.Top:
        res = Math.Abs(value: value - lt) < Math.Abs(value: value - rt) ? SubCell.LeftTop : SubCell.RightTop;
        break;
      case Edge.Right:
        res = Math.Abs(value: value - rb) < Math.Abs(value: value - rt) ? SubCell.RightBottom : SubCell.RightTop;
        break;
      case Edge.Bottom:
      default:
        res = Math.Abs(value: value - lb) < Math.Abs(value: value - rb) ? SubCell.LeftBottom : SubCell.RightBottom;
        break;
    }

    var subValues = vc.GetSubCell(subCell: res);
    var valueInside = subValues.ValueBelongTo(value: value);
    if (!valueInside)
    {
      throw new IsolineGenerationException(message: Strings.Exceptions.IsolinesDataIsUndetailized);
    }

    return res;
  }

  private static Point GetPoint(double value, double a1, double a2, Vector v1, Vector v2)
  {
    var ratio = (value - a1) / (a2 - a1);

    Verify.IsTrue(condition: 0 <= ratio && ratio <= 1);

    var r = (1 - ratio) * v1 + ratio * v2;
    return new Point(x: r.X, y: r.Y);
  }

  private Point GetPointXY(Edge edge, double value, ValuesInCell vc, IrregularCell rect)
  {
    var lt = vc.LeftTop;
    var lb = vc.LeftBottom;
    var rb = vc.RightBottom;
    var rt = vc.RightTop;

    switch (edge)
    {
      case Edge.Left:
        return GetPoint(value: value, a1: lb, a2: lt, v1: rect.LeftBottom, v2: rect.LeftTop);
      case Edge.Top:
        return GetPoint(value: value, a1: lt, a2: rt, v1: rect.LeftTop, v2: rect.RightTop);
      case Edge.Right:
        return GetPoint(value: value, a1: rb, a2: rt, v1: rect.RightBottom, v2: rect.RightTop);
      case Edge.Bottom:
        return GetPoint(value: value, a1: lb, a2: rb, v1: rect.LeftBottom, v2: rect.RightBottom);
      default:
        throw new InvalidOperationException();
    }
  }

  private bool BelongsToEdge(double value, double edgeValue1, double edgeValue2, bool onBoundary)
  {
    if (!double.IsNaN(d: missingValue) && (edgeValue1 == missingValue || edgeValue2 == missingValue))
    {
      return false;
    }

    if (onBoundary)
    {
      return (edgeValue1 <= value && value < edgeValue2) ||
      (edgeValue2 <= value && value < edgeValue1);
    }
    else
    {
      return (edgeValue1 < value && value < edgeValue2) ||
        (edgeValue2 < value && value < edgeValue1);
    }
  }

  private bool IsPassed(Edge edge, int i, int j, byte[,] edges)
  {
    switch (edge)
    {
      case Edge.Left:
        return i == 0 || (edges[i, j] & (byte)edge) != 0;
      case Edge.Bottom:
        return j == 0 || (edges[i, j] & (byte)edge) != 0;
      case Edge.Top:
        return j == edges.GetLength(dimension: 1) - 2 || (edges[i, j + 1] & (byte)Edge.Bottom) != 0;
      case Edge.Right:
        return i == edges.GetLength(dimension: 0) - 2 || (edges[i + 1, j] & (byte)Edge.Left) != 0;
      default:
        throw new InvalidOperationException();
    }
  }

  private void MakeEdgePassed(Edge edge, int i, int j)
  {
    switch (edge)
    {
      case Edge.Left:
      case Edge.Bottom:
        edges[i, j] |= (byte)edge;
        break;
      case Edge.Top:
        edges[i, j + 1] |= (byte)Edge.Bottom;
        break;
      case Edge.Right:
        edges[i + 1, j] |= (byte)Edge.Left;
        break;
      default:
        throw new InvalidOperationException();
    }
  }

  private Edge TrackLine(Edge inEdge, double value, ref int x, ref int y, out double newX, out double newY)
  {
    // Getting output edge
    var vc = missingValue.IsNaN() ?
      new ValuesInCell(leftBottom: values[x, y],
        rightBottom: values[x + 1, y],
        rightTop: values[x + 1, y + 1],
        leftTop: values[x, y + 1]) :
      new ValuesInCell(leftBottom: values[x, y],
        rightBottom: values[x + 1, y],
        rightTop: values[x + 1, y + 1],
        leftTop: values[x, y + 1],
        missingValue: missingValue);

    IrregularCell rect = new(
      lb: grid[x, y],
      rb: grid[x + 1, y],
      rt: grid[x + 1, y + 1],
      lt: grid[x, y + 1]);

    var outEdge = GetOutEdge(inEdge: inEdge, cv: vc, rect: rect, value: value);
    if (outEdge == Edge.None)
    {
      newX = newY = -1; // Impossible cell indices
      return Edge.None;
    }

    // Drawing new segment
    var point = GetPointXY(edge: outEdge, value: value, vc: vc, rect: rect);
    newX = point.X;
    newY = point.Y;
    segments.AddPoint(p: point);
    processed[x, y] = true;

    // Whether out-edge already was passed?
    if (IsPassed(edge: outEdge, i: x, j: y, edges: edges)) // line is closed
    {
      //MakeEdgePassed(outEdge, x, y); // boundaries should be marked as passed too
      return Edge.None;
    }

    // Make this edge passed
    MakeEdgePassed(edge: outEdge, i: x, j: y);

    // Getting next cell's indices
    switch (outEdge)
    {
      case Edge.Left:
        x--;
        return Edge.Right;
      case Edge.Top:
        y++;
        return Edge.Bottom;
      case Edge.Right:
        x++;
        return Edge.Left;
      case Edge.Bottom:
        y--;
        return Edge.Top;
      default:
        throw new InvalidOperationException();
    }
  }

  private void TrackLineNonRecursive(Edge inEdge, double value, int x, int y)
  {
    int s = x, t = y;

    var vc = missingValue.IsNaN() ?
      new ValuesInCell(leftBottom: values[x, y],
        rightBottom: values[x + 1, y],
        rightTop: values[x + 1, y + 1],
        leftTop: values[x, y + 1]) :
      new ValuesInCell(leftBottom: values[x, y],
        rightBottom: values[x + 1, y],
        rightTop: values[x + 1, y + 1],
        leftTop: values[x, y + 1],
        missingValue: missingValue);

    IrregularCell rect = new(
      lb: grid[x, y],
      rb: grid[x + 1, y],
      rt: grid[x + 1, y + 1],
      lt: grid[x, y + 1]);

    var point = GetPointXY(edge: inEdge, value: value, vc: vc, rect: rect);

    segments.StartLine(p: point, value01: (value - minMax.Min) / (minMax.Max - minMax.Min), realValue: value);

    MakeEdgePassed(edge: inEdge, i: x, j: y);

    //processed[x, y] = true;

    double x2, y2;
    do
    {
      inEdge = TrackLine(inEdge: inEdge, value: value, x: ref s, y: ref t, newX: out x2, newY: out y2);
    } while (inEdge != Edge.None);
  }

  #endregion

  private bool HasIsoline(int x, int y)
  {
    return edges[x, y] != 0 &&
           ((x < edges.GetLength(dimension: 0) - 1 && edges[x + 1, y] != 0) ||
            (y < edges.GetLength(dimension: 1) - 1 && edges[x, y + 1] != 0));
  }

  /// <summary>Finds isoline for specified reference value</summary>
  /// <param name="value">Reference value</param>
  private void PrepareCells(double value)
  {
    var currentRatio = (value - minMax.Min) / (minMax.Max - minMax.Min);

    if (currentRatio < 0 || currentRatio > 1)
    {
      return; // No contour lines for such value
    }

    var xSize = dataSource.Width;
    var ySize = dataSource.Height;
    int x, y;
    for (x = 0; x < xSize; x++)
    {
      for (y = 0; y < ySize; y++)
      {
        edges[x, y] = 0;
      }
    }

    processed = new bool[xSize, ySize];

    // Looking in boundaries.
    // left
    for (y = 1; y < ySize; y++)
    {
      if (BelongsToEdge(value: value, edgeValue1: values[0, y - 1], edgeValue2: values[0, y], onBoundary: true) &&
        (edges[0, y - 1] & (byte)Edge.Left) == 0)
      {
        TrackLineNonRecursive(inEdge: Edge.Left, value: value, x: 0, y: y - 1);
      }
    }

    // bottom
    for (x = 0; x < xSize - 1; x++)
    {
      if (BelongsToEdge(value: value, edgeValue1: values[x, 0], edgeValue2: values[x + 1, 0], onBoundary: true)
        && (edges[x, 0] & (byte)Edge.Bottom) == 0)
      {
        TrackLineNonRecursive(inEdge: Edge.Bottom, value: value, x: x, y: 0);
      }
    }

    // right
    x = xSize - 1;
    for (y = 1; y < ySize; y++)
    {
      // Is this correct?
      //if (BelongsToEdge(value, values[0, y - 1], values[0, y], true) &&
      //    (edges[0, y - 1] & (byte)Edge.Left) == 0)
      //{
      //    TrackLineNonRecursive(Edge.Left, value, 0, y - 1);
      //};

      if (BelongsToEdge(value: value, edgeValue1: values[x, y - 1], edgeValue2: values[x, y], onBoundary: true) &&
        (edges[x, y - 1] & (byte)Edge.Left) == 0)
      {
        TrackLineNonRecursive(inEdge: Edge.Right, value: value, x: x - 1, y: y - 1);
      }
    }

    // horizontals
    for (x = 1; x < xSize - 1; x++)
    {
      for (y = 1; y < ySize - 1; y++)
      {
        if ((edges[x, y] & (byte)Edge.Bottom) == 0 &&
          BelongsToEdge(value: value, edgeValue1: values[x, y], edgeValue2: values[x + 1, y], onBoundary: false) &&
          !processed[x, y - 1])
        {
          TrackLineNonRecursive(inEdge: Edge.Top, value: value, x: x, y: y - 1);
        }
        if ((edges[x, y] & (byte)Edge.Bottom) == 0 &&
          BelongsToEdge(value: value, edgeValue1: values[x, y], edgeValue2: values[x + 1, y], onBoundary: false) &&
          !processed[x, y])
        {
          TrackLineNonRecursive(inEdge: Edge.Bottom, value: value, x: x, y: y);
        }
        if ((edges[x, y] & (byte)Edge.Left) == 0 &&
          BelongsToEdge(value: value, edgeValue1: values[x, y], edgeValue2: values[x, y - 1], onBoundary: false) &&
          !processed[x - 1, y - 1])
        {
          TrackLineNonRecursive(inEdge: Edge.Right, value: value, x: x - 1, y: y - 1);
        }
        if ((edges[x, y] & (byte)Edge.Left) == 0 &&
          BelongsToEdge(value: value, edgeValue1: values[x, y], edgeValue2: values[x, y - 1], onBoundary: false) &&
          !processed[x, y - 1])
        {
          TrackLineNonRecursive(inEdge: Edge.Left, value: value, x: x, y: y - 1);
        }
      }
    }
  }

  /// <summary>
  /// Builds isoline data for 2d scalar field contained in data source.
  /// </summary>
  /// <returns>Collection of data describing built isolines.</returns>
  public IsolineCollection BuildIsoline()
  {
    VerifyDataSource();

    segments = new IsolineCollection();

    // Cannot draw isolines for fields with one dimension lesser than 2
    if (dataSource.Width < 2 || dataSource.Height < 2)
    {
      return segments;
    }

    Init();

    if (!minMax.IsEmpty)
    {
      values = dataSource.Data;
      var levels = GetLevelsForIsolines();

      foreach (var level in levels)
      {
        PrepareCells(value: level);
      }

      if (segments.Lines.Count > 0 && segments.Lines[index: segments.Lines.Count - 1].OtherPoints.Count == 0)
      {
        segments.Lines.RemoveAt(index: segments.Lines.Count - 1);
      }
    }
    return segments;
  }

  private void Init()
  {
    if (dataSource.Range.HasValue)
    {
      minMax = dataSource.Range.Value;
    }
    else
    {
      minMax = double.IsNaN(d: missingValue) ? dataSource.GetMinMax() : dataSource.GetMinMax(missingValue: missingValue);
    }

    if (dataSource.MissingValue.HasValue)
    {
      missingValue = dataSource.MissingValue.Value;
    }

    segments.Min = minMax.Min;
    segments.Max = minMax.Max;
  }

  /// <summary>
  /// Builds isoline data for the specified level in 2d scalar field.
  /// </summary>
  /// <param name="level">The level.</param>
  /// <returns></returns>
  public IsolineCollection BuildIsoline(double level)
  {
    VerifyDataSource();

    segments = new IsolineCollection();

    Init();

    if (!minMax.IsEmpty)
    {
      values = dataSource.Data;


      PrepareCells(value: level);

      if (segments.Lines.Count > 0 && segments.Lines[index: segments.Lines.Count - 1].OtherPoints.Count == 0)
      {
        segments.Lines.RemoveAt(index: segments.Lines.Count - 1);
      }
    }
    return segments;
  }

  private void VerifyDataSource()
  {
    if (dataSource == null)
    {
      throw new InvalidOperationException(message: Strings.Exceptions.IsolinesDataSourceShouldBeSet);
    }
  }

  private IsolineCollection segments;

  private double[,] values;
  private byte[,] edges;
  private Point[,] grid;

  private Range<double> minMax;
  private IDataSource2D<double> dataSource;
  /// <summary>
  /// Gets or sets the data source - 2d scalar field.
  /// </summary>
  /// <value>The data source.</value>
  public IDataSource2D<double> DataSource
  {
    get => dataSource;
    set
    {
      if (dataSource != value)
      {
        value.VerifyNotNull(paramName: "value");

        dataSource = value;
        grid = dataSource.Grid;
        edges = new byte[dataSource.Width, dataSource.Height];
      }
    }
  }

  private const double shiftPercent = 0.05;
  private double[] GetLevelsForIsolines()
  {
	    var min = minMax.Min;
    var max = minMax.Max;

    var step = (max - min) / (density - 1);
    var delta = max - min;

    var levels = new double[density];
    levels[0] = min + delta * shiftPercent;
    levels[levels.Length - 1] = max - delta * shiftPercent;

    for (var i = 1; i < levels.Length - 1; i++)
    {
      levels[i] = min + i * step;
    }

    return levels;
  }
}
