﻿using System;

namespace Crystal.Plot2D.Axes.Numeric;

/// <summary>
/// Represents a vertical axis with values of System.Double type.
/// Can be placed only from left or right side of plotter.
/// By default is placed from the left side.
/// </summary>
public sealed class VerticalAxis : NumericAxis
{
  /// <summary>
  /// Initializes a new instance of the <see cref="VerticalAxis"/> class.
  /// </summary>
  public VerticalAxis()
  {
    Placement = AxisPlacement.Left;
  }

  /// <summary>
  /// Validates the placement - e.g., vertical axis should not be placed from top or bottom, etc.
  /// If proposed placement if wrong, throws an ArgumentException.
  /// </summary>
  /// <param name="newPlacement">The new placement.</param>
  protected override void ValidatePlacement(AxisPlacement newPlacement)
  {
    if (newPlacement == AxisPlacement.Bottom || newPlacement == AxisPlacement.Top)
    {
      throw new ArgumentException(message: Strings.Exceptions.VerticalAxisCannotBeHorizontal);
    }
  }
}
