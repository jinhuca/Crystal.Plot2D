﻿using System;

namespace Crystal.Plot2D.Axes.Numeric;

/// <summary>
/// Represents a horizontal axis with values of <see cref="double"/> type.
/// Can be placed only from bottom or top side of plotter.
/// By default is placed from the bottom side.
/// </summary>
public sealed class HorizontalAxis : NumericAxis
{
  /// <summary>
  /// Initializes a new instance of the <see cref="HorizontalAxis"/> class.
  /// </summary>
  public HorizontalAxis()
  {
    Placement = AxisPlacement.Bottom;
  }

  /// <summary>
  /// Validates the placement - e.g., vertical axis should not be placed from top or bottom, etc.
  /// If proposed placement is wrong, throws an ArgumentException.
  /// </summary>
  /// <param name="newPlacement">The new placement. </param>
  protected override void ValidatePlacement(AxisPlacement newPlacement)
  {
    if (newPlacement == AxisPlacement.Left || newPlacement == AxisPlacement.Right)
    {
      throw new ArgumentException(message: Strings.Exceptions.HorizontalAxisCannotBeVertical);
    }
  }
}
