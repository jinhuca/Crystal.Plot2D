using Crystal.Plot2D.Common;
using System.Diagnostics;
using System.Windows;

namespace Crystal.Plot2D.Charts;

/// <summary>
/// Isoline's grid cell
/// </summary>
internal interface ICell
{
  Vector LeftTop { get; }
  Vector LeftBottom { get; }
  Vector RightTop { get; }
  Vector RightBottom { get; }
}

internal sealed class IrregularCell : ICell
{
  public IrregularCell(Vector leftBottom, Vector rightBottom, Vector rightTop, Vector leftTop)
  {
    this.leftBottom = leftBottom;
    this.rightBottom = rightBottom;
    this.rightTop = rightTop;
    this.leftTop = leftTop;
  }

  public IrregularCell(Point lb, Point rb, Point rt, Point lt)
  {
    leftTop = lt.ToVector();
    leftBottom = lb.ToVector();
    rightTop = rt.ToVector();
    rightBottom = rb.ToVector();
  }

  #region ICell Members

  private readonly Vector leftTop;
  public Vector LeftTop => leftTop;

  private readonly Vector leftBottom;
  public Vector LeftBottom => leftBottom;

  private readonly Vector rightTop;
  public Vector RightTop => rightTop;

  private readonly Vector rightBottom;
  public Vector RightBottom => rightBottom;

  #endregion

  #region Sides
  public Vector LeftSide => (leftBottom + leftTop) / 2;

  public Vector RightSide => (rightBottom + rightTop) / 2;

  public Vector TopSide => (leftTop + rightTop) / 2;

  public Vector BottomSide => (leftBottom + rightBottom) / 2;

  #endregion

  public Point Center => ((LeftSide + RightSide) / 2).ToPoint();

  public IrregularCell GetSubRect(SubCell sub)
  {
    switch (sub)
    {
      case SubCell.LeftBottom:
        return new IrregularCell(leftBottom: LeftBottom, rightBottom: BottomSide, rightTop: Center.ToVector(), leftTop: LeftSide);
      case SubCell.LeftTop:
        return new IrregularCell(leftBottom: LeftSide, rightBottom: Center.ToVector(), rightTop: TopSide, leftTop: LeftTop);
      case SubCell.RightBottom:
        return new IrregularCell(leftBottom: BottomSide, rightBottom: RightBottom, rightTop: RightSide, leftTop: Center.ToVector());
      case SubCell.RightTop:
      default:
        return new IrregularCell(leftBottom: Center.ToVector(), rightBottom: RightSide, rightTop: RightTop, leftTop: TopSide);
    }
  }
}

internal enum SubCell
{
  LeftBottom = 0,
  LeftTop = 1,
  RightBottom = 2,
  RightTop = 3
}

internal class ValuesInCell
{
	  readonly double min = double.MaxValue;
	  readonly double max = double.MinValue;

  /// <summary>Initializes values in four corners of cell</summary>
  /// <param name="leftBottom"></param>
  /// <param name="rightBottom"></param>
  /// <param name="rightTop"></param>
  /// <param name="leftTop"></param>
  /// <remarks>Some or all values can be NaN. That means that value is not specified (misssing)</remarks>
  public ValuesInCell(double leftBottom, double rightBottom, double rightTop, double leftTop)
  {
    this.leftTop = leftTop;
    this.leftBottom = leftBottom;
    this.rightTop = rightTop;
    this.rightBottom = rightBottom;

    // Find max and min values (with respect to possible NaN values)
    if (!double.IsNaN(d: leftTop))
    {
      if (min > leftTop)
      {
        min = leftTop;
      }

      if (max < leftTop)
      {
        max = leftTop;
      }
    }

    if (!double.IsNaN(d: leftBottom))
    {
      if (min > leftBottom)
      {
        min = leftBottom;
      }

      if (max < leftBottom)
      {
        max = leftBottom;
      }
    }

    if (!double.IsNaN(d: rightTop))
    {
      if (min > rightTop)
      {
        min = rightTop;
      }

      if (max < rightTop)
      {
        max = rightTop;
      }
    }

    if (!double.IsNaN(d: rightBottom))
    {
      if (min > rightBottom)
      {
        min = rightBottom;
      }

      if (max < rightBottom)
      {
        max = rightBottom;
      }
    }

    left = (leftTop + leftBottom) / 2;
    bottom = (leftBottom + rightBottom) / 2;
    right = (rightTop + rightBottom) / 2;
    top = (rightTop + leftTop) / 2;
  }

  public ValuesInCell(double leftBottom, double rightBottom, double rightTop, double leftTop, double missingValue)
  {
    DebugVerify.IsNotNaN(d: leftBottom);
    DebugVerify.IsNotNaN(d: rightBottom);
    DebugVerify.IsNotNaN(d: rightTop);
    DebugVerify.IsNotNaN(d: leftTop);

    // Copy values and find min and max with respect to possible missing values
    if (leftTop != missingValue)
    {
      this.leftTop = leftTop;
      if (min > leftTop)
      {
        min = leftTop;
      }

      if (max < leftTop)
      {
        max = leftTop;
      }
    }
    else
    {
      this.leftTop = double.NaN;
    }

    if (leftBottom != missingValue)
    {
      this.leftBottom = leftBottom;
      if (min > leftBottom)
      {
        min = leftBottom;
      }

      if (max < leftBottom)
      {
        max = leftBottom;
      }
    }
    else
    {
      this.leftBottom = double.NaN;
    }

    if (rightTop != missingValue)
    {
      this.rightTop = rightTop;
      if (min > rightTop)
      {
        min = rightTop;
      }

      if (max < rightTop)
      {
        max = rightTop;
      }
    }
    else
    {
      this.rightTop = double.NaN;
    }

    if (rightBottom != missingValue)
    {
      this.rightBottom = rightBottom;
      if (min > rightBottom)
      {
        min = rightBottom;
      }

      if (max < rightBottom)
      {
        max = rightBottom;
      }
    }
    else
    {
      this.rightBottom = double.NaN;
    }

    left = (this.leftTop + this.leftBottom) / 2;
    bottom = (this.leftBottom + this.rightBottom) / 2;
    right = (this.rightTop + this.rightBottom) / 2;
    top = (this.rightTop + this.leftTop) / 2;


    /*            
                if (leftTop != missingValue && )
                {
                    if (leftBottom != missingValue)
                        left = (leftTop + leftBottom) / 2;
                    else
                        left = Double.NaN;

                    if (rightTop != missingValue)
                        top = (leftTop + rightTop) / 2;
                    else
                        top = Double.NaN;
                }

                if (rightBottom != missingValue)
                {
                    if (leftBottom != missingValue)
                        bottom = (leftBottom + rightBottom) / 2;
                    else
                        bottom = Double.NaN;

                    if (rightTop != missingValue)
                        right = (rightTop + rightBottom) / 2;
                    else
                        right = Double.NaN;
                }*/
  }


  /*internal bool ValueBelongTo(double value)
		{
			IEnumerable<double> values = new double[] { leftTop, leftBottom, rightTop, rightBottom };

			return !(values.All(v => v > value) || values.All(v => v < value));
		}*/

  internal bool ValueBelongTo(double value)
  {
    return (min <= value && value <= max);
  }

  #region Edges
  [DebuggerBrowsable(state: DebuggerBrowsableState.Never)]
  private readonly double leftTop;
  public double LeftTop => leftTop;

  [DebuggerBrowsable(state: DebuggerBrowsableState.Never)]
  private readonly double leftBottom;
  public double LeftBottom => leftBottom;

  [DebuggerBrowsable(state: DebuggerBrowsableState.Never)]
  private readonly double rightTop;
  public double RightTop => rightTop;

  [DebuggerBrowsable(state: DebuggerBrowsableState.Never)]
  private readonly double rightBottom;
  public double RightBottom => rightBottom;

  #endregion

  #region Sides & center
  [DebuggerBrowsable(state: DebuggerBrowsableState.Never)]
  private readonly double left;
  public double Left => left;

  [DebuggerBrowsable(state: DebuggerBrowsableState.Never)]
  private readonly double right;
  public double Right => right;

  [DebuggerBrowsable(state: DebuggerBrowsableState.Never)]
  private readonly double top;
  public double Top => top;

  [DebuggerBrowsable(state: DebuggerBrowsableState.Never)]
  private readonly double bottom;
  public double Bottom => bottom;

  public double Center => (Left + Right) * 0.5;

  #endregion

  #region SubCells
  public ValuesInCell LeftTopCell => new(leftBottom: Left, rightBottom: Center, rightTop: Top, leftTop: LeftTop);

  public ValuesInCell RightTopCell => new(leftBottom: Center, rightBottom: Right, rightTop: RightTop, leftTop: Top);

  public ValuesInCell RightBottomCell => new(leftBottom: Bottom, rightBottom: RightBottom, rightTop: Right, leftTop: Center);

  public ValuesInCell LeftBottomCell => new(leftBottom: LeftBottom, rightBottom: Bottom, rightTop: Center, leftTop: Left);

  public ValuesInCell GetSubCell(SubCell subCell)
  {
    switch (subCell)
    {
      case SubCell.LeftBottom:
        return LeftBottomCell;
      case SubCell.LeftTop:
        return LeftTopCell;
      case SubCell.RightBottom:
        return RightBottomCell;
      case SubCell.RightTop:
      default:
        return RightTopCell;
    }
  }

  #endregion

  /// <summary>
  /// Returns bitmask of comparison of values at cell corners with reference value.
  /// Corresponding bit is set to one if value at cell corner is greater than reference value. 
  /// a------b
  /// | Cell |
  /// d------c
  /// </summary>
  /// <param name="a">Value at corner (see figure)</param>
  /// <param name="b">Value at corner (see figure)</param>
  /// <param name="c">Value at corner (see figure)</param>
  /// <param name="d">Value at corner (see figure)</param>
  /// <param name="value">Reference value</param>
  /// <returns>Bitmask</returns>
  public CellBitmask GetCellValue(double value)
  {
    CellBitmask n = CellBitmask.None;
    if (!double.IsNaN(d: leftTop) && leftTop > value)
    {
      n |= CellBitmask.LeftTop;
    }

    if (!double.IsNaN(d: leftBottom) && leftBottom > value)
    {
      n |= CellBitmask.LeftBottom;
    }

    if (!double.IsNaN(d: rightBottom) && rightBottom > value)
    {
      n |= CellBitmask.RightBottom;
    }

    if (!double.IsNaN(d: rightTop) && rightTop > value)
    {
      n |= CellBitmask.RightTop;
    }

    return n;
  }
}
