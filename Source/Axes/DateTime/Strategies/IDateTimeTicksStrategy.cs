using System;

namespace Crystal.Plot2D.Charts
{
  public interface IDateTimeTicksStrategy
  {
    DifferenceIn GetDifference(TimeSpan span);
    bool TryGetLowerDiff(DifferenceIn diff, out DifferenceIn lowerDiff);
    bool TryGetBiggerDiff(DifferenceIn diff, out DifferenceIn biggerDiff);
  }
}
