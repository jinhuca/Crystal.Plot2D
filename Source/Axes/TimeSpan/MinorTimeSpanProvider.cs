using System;

namespace Crystal.Plot2D.Charts;

internal sealed class MinorTimeSpanTicksProvider : MinorTimeProviderBase<TimeSpan>
{
  public MinorTimeSpanTicksProvider(ITicksProvider<TimeSpan> owner) : base(provider: owner) { }

  protected override bool IsInside(TimeSpan value, Range<TimeSpan> range)
  {
    return range.Min < value && value < range.Max;
  }
}
