﻿using System;

namespace Crystal.Plot2D.Axes;

public class ExtendedDaysStrategy : IDateTimeTicksStrategy
{
  private static readonly DifferenceIn[] diffs =
  {
    DifferenceIn.Year,
    DifferenceIn.Day,
    DifferenceIn.Hour,
    DifferenceIn.Minute,
    DifferenceIn.Second,
    DifferenceIn.Millisecond
  };

  public DifferenceIn GetDifference(System.TimeSpan span)
  {
    span = span.Duration();

    DifferenceIn diff;
    if (span.Days > 365)
    {
      diff = DifferenceIn.Year;
    }
    else if (span.Days > 0)
    {
      diff = DifferenceIn.Day;
    }
    else if (span.Hours > 0)
    {
      diff = DifferenceIn.Hour;
    }
    else if (span.Minutes > 0)
    {
      diff = DifferenceIn.Minute;
    }
    else if (span.Seconds > 0)
    {
      diff = DifferenceIn.Second;
    }
    else
    {
      diff = DifferenceIn.Millisecond;
    }

    return diff;
  }

  public bool TryGetLowerDiff(DifferenceIn diff, out DifferenceIn lowerDiff)
  {
    lowerDiff = diff;

    var index = Array.IndexOf(array: diffs, value: diff);
    if (index == -1)
    {
      return false;
    }

    if (index == diffs.Length - 1)
    {
      return false;
    }

    lowerDiff = diffs[index + 1];
    return true;
  }

  public bool TryGetBiggerDiff(DifferenceIn diff, out DifferenceIn biggerDiff)
  {
    biggerDiff = diff;

    var index = Array.IndexOf(array: diffs, value: diff);
    if (index == -1 || index == 0)
    {
      return false;
    }

    biggerDiff = diffs[index - 1];
    return true;
  }
}
