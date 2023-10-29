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
  private const int Density = 12;

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
    var bottomDict_ = new Dictionary<int, Edge>();
    bottomDict_.Add(key: (int)CellBitmask.RightBottom, value: Edge.Right);
    bottomDict_.Add(value: Edge.Left,
      keys: new[]
      {
        CellBitmask.LeftTop,
        CellBitmask.LeftBottom | CellBitmask.RightBottom | CellBitmask.RightTop,
        CellBitmask.LeftTop | CellBitmask.RightBottom | CellBitmask.RightTop,
        CellBitmask.LeftBottom
      });
    bottomDict_.Add(value: Edge.Right,
      keys: new[]
      {
        CellBitmask.RightTop,
        CellBitmask.LeftBottom | CellBitmask.RightBottom | CellBitmask.LeftTop,
        CellBitmask.LeftBottom | CellBitmask.LeftTop | CellBitmask.RightTop
      });
    bottomDict_.Add(value: Edge.Top,
      keys: new[]
      {
        CellBitmask.RightBottom | CellBitmask.RightTop,
        CellBitmask.LeftBottom | CellBitmask.LeftTop
      });

    var leftDict_ = new Dictionary<int, Edge>();
    leftDict_.Add(value: Edge.Top,
      keys: new[]
      {
        CellBitmask.LeftTop,
        CellBitmask.LeftBottom | CellBitmask.RightBottom | CellBitmask.RightTop
      });
    leftDict_.Add(value: Edge.Right,
      keys: new[]
      {
        CellBitmask.LeftTop | CellBitmask.RightTop,
        CellBitmask.LeftBottom | CellBitmask.RightBottom
      });
    leftDict_.Add(value: Edge.Bottom,
      keys: new[]
      {
        CellBitmask.RightBottom | CellBitmask.RightTop | CellBitmask.LeftTop,
        CellBitmask.LeftBottom
      });

    var topDict_ = new Dictionary<int, Edge>();
    topDict_.Add(value: Edge.Right,
      keys: new[]
      {
        CellBitmask.RightTop,
        CellBitmask.LeftTop | CellBitmask.LeftBottom | CellBitmask.RightBottom
      });
    topDict_.Add(value: Edge.Right,
      keys: new[]
      {
        CellBitmask.RightBottom,
        CellBitmask.LeftTop | CellBitmask.LeftBottom | CellBitmask.RightTop
      });
    topDict_.Add(value: Edge.Left,
      keys: new[]
      {
        CellBitmask.RightBottom | CellBitmask.RightTop | CellBitmask.LeftTop,
        CellBitmask.LeftBottom,
        CellBitmask.LeftTop,
        CellBitmask.LeftBottom | CellBitmask.RightBottom | CellBitmask.RightTop
      });
    topDict_.Add(value: Edge.Bottom,
      keys: new[]
      {
        CellBitmask.RightBottom | CellBitmask.RightTop,
        CellBitmask.LeftTop | CellBitmask.LeftBottom
      });

    var rightDict_ = new Dictionary<int, Edge>();
    rightDict_.Add(value: Edge.Top,
      keys: new[]
      {
        CellBitmask.RightTop,
        CellBitmask.LeftBottom | CellBitmask.RightBottom | CellBitmask.LeftTop
      });
    rightDict_.Add(value: Edge.Left,
      keys: new[]
      {
        CellBitmask.LeftTop | CellBitmask.RightTop,
        CellBitmask.LeftBottom | CellBitmask.RightBottom
      });
    rightDict_.Add(value: Edge.Bottom,
      keys: new[]
      {
        CellBitmask.RightBottom,
        CellBitmask.LeftTop | CellBitmask.LeftBottom | CellBitmask.RightTop
      });

    dictChooser.Add(key: (int)Edge.Left, value: leftDict_);
    dictChooser.Add(key: (int)Edge.Right, value: rightDict_);
    dictChooser.Add(key: (int)Edge.Bottom, value: bottomDict_);
    dictChooser.Add(key: (int)Edge.Top, value: topDict_);
  }

  private Edge GetOutEdge(Edge inEdge, ValuesInCell cv, IrregularCell rect, double value)
  {
    // value smaller than all values in corners or 
    // value greater than all values in corners
    if (!cv.ValueBelongTo(value: value))
    {
      throw new IsolineGenerationException(message: Strings.Exceptions.IsolinesValueIsOutOfCell);
    }

    var cellVal_ = cv.GetCellValue(value: value);
    var dict_ = dictChooser[key: (int)inEdge];
    if (dict_.ContainsKey(key: (int)cellVal_))
    {
      var result_ = dict_[key: (int)cellVal_];
      switch (result_)
      {
        case Edge.Left:
          if (cv.LeftTop.IsNaN() || cv.LeftBottom.IsNaN())
          {
            result_ = Edge.None;
          }

          break;
        case Edge.Right:
          if (cv.RightTop.IsNaN() || cv.RightBottom.IsNaN())
          {
            result_ = Edge.None;
          }

          break;
        case Edge.Top:
          if (cv.RightTop.IsNaN() || cv.LeftTop.IsNaN())
          {
            result_ = Edge.None;
          }

          break;
        case Edge.Bottom:
          if (cv.LeftBottom.IsNaN() || cv.RightBottom.IsNaN())
          {
            result_ = Edge.None;
          }

          break;
      }
      return result_;
    }

    if (cellVal_.IsDiagonal())
    {
      return GetOutForOpposite(inEdge: inEdge, cellVal: cellVal_, value: value, cellValues: cv, rect: rect);
    }

    const double nearZero = 0.0001;
    const double nearOne = 1 - nearZero;

    var lt_ = cv.LeftTop;
    var rt_ = cv.RightTop;
    var rb_ = cv.RightBottom;
    var lb_ = cv.LeftBottom;

    switch (inEdge)
    {
      case Edge.Left:
        if (value == lt_)
        {
          value = nearOne * lt_ + nearZero * lb_;
        }
        else if (value == lb_)
        {
          value = nearOne * lb_ + nearZero * lt_;
        }
        else
        {
          return Edge.None;
        }
        
        // Now this is possible because of missing value
        //throw new IsolineGenerationException(Strings.Exceptions.IsolinesUnsupportedCase);
        break;
      case Edge.Top:
        if (value == rt_)
        {
          value = nearOne * rt_ + nearZero * lt_;
        }
        else if (value == lt_)
        {
          value = nearOne * lt_ + nearZero * rt_;
        }
        else
        {
          return Edge.None;
        }
        
        // Now this is possible because of missing value
        //throw new IsolineGenerationException(Strings.Exceptions.IsolinesUnsupportedCase);
        break;
      case Edge.Right:
        if (Math.Abs(value - rb_) < Constants.Constants.FloatComparisonTolerance)
        {
          value = nearOne * rb_ + nearZero * rt_;
        }
        else if (Math.Abs(value - rt_) < Constants.Constants.FloatComparisonTolerance)
        {
          value = nearOne * rt_ + nearZero * rb_;
        }
        else
        {
          return Edge.None;
        }
        
        // Now this is possible because of missing value
        //throw new IsolineGenerationException(Strings.Exceptions.IsolinesUnsupportedCase);
        break;
      case Edge.Bottom:
        if (Math.Abs(value - rb_) < Constants.Constants.FloatComparisonTolerance)
        {
          value = nearOne * rb_ + nearZero * lb_;
        }
        else if (Math.Abs(value - lb_) < Constants.Constants.FloatComparisonTolerance)
        {
          value = nearOne * lb_ + nearZero * rb_;
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
    var subCell_ = GetSubCell(inEdge: inEdge, value: value, vc: cellValues);
    var iterations_ = 1000; // max number of iterations
    do
    {
      var subValues_ = cellValues.GetSubCell(subCell: subCell_);
      var subRect_ = rect.GetSubRect(sub: subCell_);
      var outEdge_ = GetOutEdge(inEdge: inEdge, cv: subValues_, rect: subRect_, value: value);
      if (outEdge_ == Edge.None)
      {
        return Edge.None;
      }

      var isAppropriate_ = subCell_.IsAppropriate(edge: outEdge_);
      if (isAppropriate_)
      {
        var sValues_ = subValues_.GetSubCell(subCell: subCell_);

        var point_ = GetPointXY(edge: outEdge_, value: value, vc: subValues_, rect: subRect_);
        segments.AddPoint(p: point_);
        return outEdge_;
      }

      subCell_ = GetAdjacentEdge(sub: subCell_, edge: outEdge_);

      var e_ = (byte)outEdge_;
      inEdge = (Edge)(e_ > 2 ? e_ >> 2 : e_ << 2);
      iterations_--;
    } while (iterations_ >= 0);

    throw new IsolineGenerationException(message: Strings.Exceptions.IsolinesDataIsUndetailized);
  }

  private static SubCell GetAdjacentEdge(SubCell sub, Edge edge)
  {
    var res_ = SubCell.LeftBottom;

    switch (sub)
    {
      case SubCell.LeftBottom:
        res_ = edge == Edge.Top ? SubCell.LeftTop : SubCell.RightBottom;
        break;
      case SubCell.LeftTop:
        res_ = edge == Edge.Bottom ? SubCell.LeftBottom : SubCell.RightTop;
        break;
      case SubCell.RightBottom:
        res_ = edge == Edge.Top ? SubCell.RightTop : SubCell.LeftBottom;
        break;
      case SubCell.RightTop:
      default:
        res_ = edge == Edge.Bottom ? SubCell.RightBottom : SubCell.LeftTop;
        break;
    }

    return res_;
  }

  private static SubCell GetSubCell(Edge inEdge, double value, ValuesInCell vc)
  {
    var lb_ = vc.LeftBottom;
    var rb_ = vc.RightBottom;
    var rt_ = vc.RightTop;
    var lt_ = vc.LeftTop;

    var res_ = SubCell.LeftBottom;
    switch (inEdge)
    {
      case Edge.Left:
        res_ = Math.Abs(value: value - lb_) < Math.Abs(value: value - lt_) ? SubCell.LeftBottom : SubCell.LeftTop;
        break;
      case Edge.Top:
        res_ = Math.Abs(value: value - lt_) < Math.Abs(value: value - rt_) ? SubCell.LeftTop : SubCell.RightTop;
        break;
      case Edge.Right:
        res_ = Math.Abs(value: value - rb_) < Math.Abs(value: value - rt_) ? SubCell.RightBottom : SubCell.RightTop;
        break;
      case Edge.Bottom:
      default:
        res_ = Math.Abs(value: value - lb_) < Math.Abs(value: value - rb_) ? SubCell.LeftBottom : SubCell.RightBottom;
        break;
    }

    var subValues_ = vc.GetSubCell(subCell: res_);
    var valueInside_ = subValues_.ValueBelongTo(value: value);
    if (!valueInside_)
    {
      throw new IsolineGenerationException(message: Strings.Exceptions.IsolinesDataIsUndetailized);
    }

    return res_;
  }

  private static Point GetPoint(double value, double a1, double a2, Vector v1, Vector v2)
  {
    var ratio_ = (value - a1) / (a2 - a1);

    Verify.IsTrue(condition: 0 <= ratio_ && ratio_ <= 1);

    var r_ = (1 - ratio_) * v1 + ratio_ * v2;
    return new Point(x: r_.X, y: r_.Y);
  }

  private Point GetPointXY(Edge edge, double value, ValuesInCell vc, IrregularCell rect)
  {
    var lt_ = vc.LeftTop;
    var lb_ = vc.LeftBottom;
    var rb_ = vc.RightBottom;
    var rt_ = vc.RightTop;

    switch (edge)
    {
      case Edge.Left:
        return GetPoint(value: value, a1: lb_, a2: lt_, v1: rect.LeftBottom, v2: rect.LeftTop);
      case Edge.Top:
        return GetPoint(value: value, a1: lt_, a2: rt_, v1: rect.LeftTop, v2: rect.RightTop);
      case Edge.Right:
        return GetPoint(value: value, a1: rb_, a2: rt_, v1: rect.RightBottom, v2: rect.RightTop);
      case Edge.Bottom:
        return GetPoint(value: value, a1: lb_, a2: rb_, v1: rect.LeftBottom, v2: rect.RightBottom);
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

    return (edgeValue1 < value && value < edgeValue2) ||
           (edgeValue2 < value && value < edgeValue1);
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
    var vc_ = missingValue.IsNaN() ?
      new ValuesInCell(leftBottom: values[x, y],
        rightBottom: values[x + 1, y],
        rightTop: values[x + 1, y + 1],
        leftTop: values[x, y + 1]) :
      new ValuesInCell(leftBottom: values[x, y],
        rightBottom: values[x + 1, y],
        rightTop: values[x + 1, y + 1],
        leftTop: values[x, y + 1],
        missingValue: missingValue);

    IrregularCell rect_ = new(
      lb: grid[x, y],
      rb: grid[x + 1, y],
      rt: grid[x + 1, y + 1],
      lt: grid[x, y + 1]);

    var outEdge_ = GetOutEdge(inEdge: inEdge, cv: vc_, rect: rect_, value: value);
    if (outEdge_ == Edge.None)
    {
      newX = newY = -1; // Impossible cell indices
      return Edge.None;
    }

    // Drawing new segment
    var point_ = GetPointXY(edge: outEdge_, value: value, vc: vc_, rect: rect_);
    newX = point_.X;
    newY = point_.Y;
    segments.AddPoint(p: point_);
    processed[x, y] = true;

    // Whether out-edge already was passed?
    if (IsPassed(edge: outEdge_, i: x, j: y, edges: edges)) // line is closed
    {
      //MakeEdgePassed(outEdge, x, y); // boundaries should be marked as passed too
      return Edge.None;
    }

    // Make this edge passed
    MakeEdgePassed(edge: outEdge_, i: x, j: y);

    // Getting next cell's indices
    switch (outEdge_)
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
    int s_ = x, t_ = y;

    var vc_ = missingValue.IsNaN() ?
      new ValuesInCell(leftBottom: values[x, y],
        rightBottom: values[x + 1, y],
        rightTop: values[x + 1, y + 1],
        leftTop: values[x, y + 1]) :
      new ValuesInCell(leftBottom: values[x, y],
        rightBottom: values[x + 1, y],
        rightTop: values[x + 1, y + 1],
        leftTop: values[x, y + 1],
        missingValue: missingValue);

    IrregularCell rect_ = new(
      lb: grid[x, y],
      rb: grid[x + 1, y],
      rt: grid[x + 1, y + 1],
      lt: grid[x, y + 1]);

    var point_ = GetPointXY(edge: inEdge, value: value, vc: vc_, rect: rect_);

    segments.StartLine(p: point_, value01: (value - minMax.Min) / (minMax.Max - minMax.Min), realValue: value);

    MakeEdgePassed(edge: inEdge, i: x, j: y);

    //processed[x, y] = true;

    double x2_, y2_;
    do
    {
      inEdge = TrackLine(inEdge: inEdge, value: value, x: ref s_, y: ref t_, newX: out x2_, newY: out y2_);
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
    var currentRatio_ = (value - minMax.Min) / (minMax.Max - minMax.Min);

    if (currentRatio_ < 0 || currentRatio_ > 1)
    {
      return; // No contour lines for such value
    }

    var xSize_ = dataSource.Width;
    var ySize_ = dataSource.Height;
    int x_, y_;
    for (x_ = 0; x_ < xSize_; x_++)
    {
      for (y_ = 0; y_ < ySize_; y_++)
      {
        edges[x_, y_] = 0;
      }
    }

    processed = new bool[xSize_, ySize_];

    // Looking in boundaries.
    // left
    for (y_ = 1; y_ < ySize_; y_++)
    {
      if (BelongsToEdge(value: value, edgeValue1: values[0, y_ - 1], edgeValue2: values[0, y_], onBoundary: true) &&
        (edges[0, y_ - 1] & (byte)Edge.Left) == 0)
      {
        TrackLineNonRecursive(inEdge: Edge.Left, value: value, x: 0, y: y_ - 1);
      }
    }

    // bottom
    for (x_ = 0; x_ < xSize_ - 1; x_++)
    {
      if (BelongsToEdge(value: value, edgeValue1: values[x_, 0], edgeValue2: values[x_ + 1, 0], onBoundary: true)
        && (edges[x_, 0] & (byte)Edge.Bottom) == 0)
      {
        TrackLineNonRecursive(inEdge: Edge.Bottom, value: value, x: x_, y: 0);
      }
    }

    // right
    x_ = xSize_ - 1;
    for (y_ = 1; y_ < ySize_; y_++)
    {
      // Is this correct?
      //if (BelongsToEdge(value, values[0, y - 1], values[0, y], true) &&
      //    (edges[0, y - 1] & (byte)Edge.Left) == 0)
      //{
      //    TrackLineNonRecursive(Edge.Left, value, 0, y - 1);
      //};

      if (BelongsToEdge(value: value, edgeValue1: values[x_, y_ - 1], edgeValue2: values[x_, y_], onBoundary: true) &&
        (edges[x_, y_ - 1] & (byte)Edge.Left) == 0)
      {
        TrackLineNonRecursive(inEdge: Edge.Right, value: value, x: x_ - 1, y: y_ - 1);
      }
    }

    // horizontals
    for (x_ = 1; x_ < xSize_ - 1; x_++)
    {
      for (y_ = 1; y_ < ySize_ - 1; y_++)
      {
        if ((edges[x_, y_] & (byte)Edge.Bottom) == 0 &&
          BelongsToEdge(value: value, edgeValue1: values[x_, y_], edgeValue2: values[x_ + 1, y_], onBoundary: false) &&
          !processed[x_, y_ - 1])
        {
          TrackLineNonRecursive(inEdge: Edge.Top, value: value, x: x_, y: y_ - 1);
        }
        if ((edges[x_, y_] & (byte)Edge.Bottom) == 0 &&
          BelongsToEdge(value: value, edgeValue1: values[x_, y_], edgeValue2: values[x_ + 1, y_], onBoundary: false) &&
          !processed[x_, y_])
        {
          TrackLineNonRecursive(inEdge: Edge.Bottom, value: value, x: x_, y: y_);
        }
        if ((edges[x_, y_] & (byte)Edge.Left) == 0 &&
          BelongsToEdge(value: value, edgeValue1: values[x_, y_], edgeValue2: values[x_, y_ - 1], onBoundary: false) &&
          !processed[x_ - 1, y_ - 1])
        {
          TrackLineNonRecursive(inEdge: Edge.Right, value: value, x: x_ - 1, y: y_ - 1);
        }
        if ((edges[x_, y_] & (byte)Edge.Left) == 0 &&
          BelongsToEdge(value: value, edgeValue1: values[x_, y_], edgeValue2: values[x_, y_ - 1], onBoundary: false) &&
          !processed[x_, y_ - 1])
        {
          TrackLineNonRecursive(inEdge: Edge.Left, value: value, x: x_, y: y_ - 1);
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
      var levels_ = GetLevelsForIsolines();

      foreach (var level_ in levels_)
      {
        PrepareCells(value: level_);
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

  private const double ShiftPercent = 0.05;
  private double[] GetLevelsForIsolines()
  {
	    var min_ = minMax.Min;
    var max_ = minMax.Max;

    var step_ = (max_ - min_) / (Density - 1);
    var delta_ = max_ - min_;

    var levels_ = new double[Density];
    levels_[0] = min_ + delta_ * ShiftPercent;
    levels_[levels_.Length - 1] = max_ - delta_ * ShiftPercent;

    for (var i_ = 1; i_ < levels_.Length - 1; i_++)
    {
      levels_[i_] = min_ + i_ * step_;
    }

    return levels_;
  }
}
