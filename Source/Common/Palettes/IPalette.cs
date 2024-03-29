﻿using System;
using System.Windows.Media;

namespace Crystal.Plot2D.Common.Palettes;

/// <summary>
///   Represents a color palette, which can generate color by interpolation coefficient.
/// </summary>
public interface IPalette
{
  /// <summary>
  ///   Gets the color by interpolation coefficient.
  /// </summary>
  /// <param name="t">
  ///   Interpolation coefficient, should belong to [0..1].
  /// </param>
  /// <returns>
  ///   Color.
  /// </returns>
  Color GetColor(double t);

  /// <summary>
  /// Occurs when palette changes.
  /// </summary>
  event EventHandler Changed;
}
