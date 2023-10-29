using System;

namespace Crystal.Plot2D.Isolines;

/// <summary>
///   Edge identifier - indicates which side of cell isoline crosses.
/// </summary>
internal enum Edge
{
  // todo check if everything is ok with None.
  None = 0,

  /// <summary>
  /// Isoline crosses left boundary of cell (bit 0)
  /// </summary>
  Left = 1,

  /// <summary>
  /// Isoline crosses top boundary of cell (bit 1)
  /// </summary>
  Top = 2,

  /// <summary>
  /// Isoline crosses right boundary of cell (bit 2)
  /// </summary>
  Right = 4,

  /// <summary>
  /// Isoline crosses bottom boundary of cell (bit 3)
  /// </summary>
  Bottom = 8
}

[Flags]
internal enum CellBitmask
{
  None = 0,
  LeftTop = 1,
  LeftBottom = 8,
  RightBottom = 4,
  RightTop = 2
}

internal static class IsolineExtensions
{
  internal static bool IsDiagonal(this CellBitmask bitmask)
  {
    return bitmask is (CellBitmask.RightBottom | CellBitmask.LeftTop) or (CellBitmask.LeftBottom | CellBitmask.RightTop);
  }

  internal static bool IsAppropriate(this SubCell sub, Edge edge)
  {
    return sub switch
    {
      SubCell.LeftBottom => edge is Edge.Left or Edge.Bottom,
      SubCell.LeftTop => edge is Edge.Left or Edge.Top,
      SubCell.RightBottom => edge is Edge.Right or Edge.Bottom,
      SubCell.RightTop => edge is Edge.Right or Edge.Top,
      _ => edge is Edge.Right or Edge.Top
    };
  }
}
