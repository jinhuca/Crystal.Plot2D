using Crystal.Plot2D.Common.Auxiliary;

namespace Crystal.Plot2D.Axes.TimeSpan;

internal sealed class TimeSpanToDoubleConversion
{
  public TimeSpanToDoubleConversion(System.TimeSpan minSpan, System.TimeSpan maxSpan)
    : this(min: 0, minSpan: minSpan, max: 1, maxSpan: maxSpan)
  { }

  public TimeSpanToDoubleConversion(double min, System.TimeSpan minSpan, double max, System.TimeSpan maxSpan)
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

  internal System.TimeSpan FromDouble(double d)
  {
    var ratio = (d - min) / length;
    var ticks = (long)(ticksMin + ticksLength * ratio);

    ticks = MathHelper.Clamp(value: ticks, min: System.TimeSpan.MinValue.Ticks, max: System.TimeSpan.MaxValue.Ticks);

    return new System.TimeSpan(ticks: ticks);
  }

  internal double ToDouble(System.TimeSpan span)
  {
    var ratio = (span.Ticks - ticksMin) / (double)ticksLength;
    return min + ratio * length;
  }
}
