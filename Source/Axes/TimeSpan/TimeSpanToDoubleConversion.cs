using System;

namespace Crystal.Plot2D.Charts;

internal sealed class TimeSpanToDoubleConversion
{
  public TimeSpanToDoubleConversion(TimeSpan minSpan, TimeSpan maxSpan)
    : this(min: 0, minSpan: minSpan, max: 1, maxSpan: maxSpan)
  { }

  public TimeSpanToDoubleConversion(double min, TimeSpan minSpan, double max, TimeSpan maxSpan)
  {
    this.min = min;
    length = max - min;
    ticksMin = minSpan.Ticks;
    ticksLength = maxSpan.Ticks - ticksMin;
  }

  private readonly double min;
  private readonly double length;
  private readonly long ticksMin;
  private readonly long ticksLength;

  internal TimeSpan FromDouble(double d)
  {
    double ratio = (d - min) / length;
    long ticks = (long)(ticksMin + ticksLength * ratio);

    ticks = MathHelper.Clamp(value: ticks, min: TimeSpan.MinValue.Ticks, max: TimeSpan.MaxValue.Ticks);

    return new TimeSpan(ticks: ticks);
  }

  internal double ToDouble(TimeSpan span)
  {
    double ratio = (span.Ticks - ticksMin) / (double)ticksLength;
    return min + ratio * length;
  }
}
