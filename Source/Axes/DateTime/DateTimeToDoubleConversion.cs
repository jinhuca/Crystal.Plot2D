using System;

namespace Crystal.Plot2D.Charts;

internal sealed class DateTimeToDoubleConversion
{
  public DateTimeToDoubleConversion(double min, DateTime minDate, double max, DateTime maxDate)
  {
    this.min = min;
    length = max - min;
    ticksMin = minDate.Ticks;
    ticksLength = maxDate.Ticks - ticksMin;
  }

  private readonly double min;
  private readonly double length;
  private readonly long ticksMin;
  private readonly long ticksLength;

  internal DateTime FromDouble(double d)
  {
    double ratio = (d - min) / length;
    long tick = (long)(ticksMin + ticksLength * ratio);

    tick = MathHelper.Clamp(value: tick, min: DateTime.MinValue.Ticks, max: DateTime.MaxValue.Ticks);

    return new DateTime(ticks: tick);
  }

  internal double ToDouble(DateTime dt)
  {
    double ratio = (dt.Ticks - ticksMin) / (double)ticksLength;
    return min + ratio * length;
  }
}
