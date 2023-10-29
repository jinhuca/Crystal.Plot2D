﻿using System;
using System.Diagnostics;
using Crystal.Plot2D.Common;
using Crystal.Plot2D.Common.Auxiliary;

namespace Crystal.Plot2D.Axes;

internal static class RoundingHelper
{
  internal static int GetDifferenceLog(double min, double max)
  {
    return (int)Math.Round(a: Math.Log10(d: Math.Abs(value: max - min)));
  }

  internal static double Round(double number, int rem)
  {
    if (rem <= 0)
    {
      rem = MathHelper.Clamp(value: -rem, min: 0, max: 15);
      return Math.Round(value: number, digits: rem);
    }
    else
    {
      var pow = Math.Pow(x: 10, y: rem - 1);
      var val = pow * Math.Round(a: number / Math.Pow(x: 10, y: rem - 1));
      return val;
    }
  }

  internal static double Round(double value, Range<double> range)
  {
    var log = GetDifferenceLog(min: range.Min, max: range.Max);

    return Round(number: value, rem: log);
  }

  internal static RoundingInfo CreateRoundedRange(double min, double max)
  {
    var delta = max - min;

    if (delta == 0)
    {
      return new RoundingInfo { Min = min, Max = max, Log = 0 };
    }

    var log = (int)Math.Round(a: Math.Log10(d: Math.Abs(value: delta))) + 1;

    var newMin = Round(number: min, rem: log);
    var newMax = Round(number: max, rem: log);
    if (newMin == newMax)
    {
      log--;
      newMin = Round(number: min, rem: log);
      newMax = Round(number: max, rem: log);
    }

    return new RoundingInfo { Min = newMin, Max = newMax, Log = log };
  }
}

[DebuggerDisplay(value: "{Min} - {Max}, Log = {Log}")]
internal sealed class RoundingInfo
{
  public double Min { get; set; }
  public double Max { get; set; }
  public int Log { get; set; }
}
