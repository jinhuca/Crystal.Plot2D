﻿namespace Crystal.Plot2D.Axes;

public class DefaultDateTimeTicksStrategy : IDateTimeTicksStrategy
{
  public virtual DifferenceIn GetDifference(System.TimeSpan span)
  {
    span = span.Duration();

    DifferenceIn diff;
    if (span.Days > 365)
    {
      diff = DifferenceIn.Year;
    }
    else if (span.Days > 30)
    {
      diff = DifferenceIn.Month;
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

  public virtual bool TryGetLowerDiff(DifferenceIn diff, out DifferenceIn lowerDiff)
  {
    lowerDiff = diff;

    var code = (int)diff;
    var res = code > (int)DifferenceIn.Smallest;
    if (res)
    {
      lowerDiff = (DifferenceIn)(code - 1);
    }
    return res;
  }

  public virtual bool TryGetBiggerDiff(DifferenceIn diff, out DifferenceIn biggerDiff)
  {
    biggerDiff = diff;

    var code = (int)diff;
    var res = code < (int)DifferenceIn.Biggest;
    if (res)
    {
      biggerDiff = (DifferenceIn)(code + 1);
    }
    return res;
  }
}
