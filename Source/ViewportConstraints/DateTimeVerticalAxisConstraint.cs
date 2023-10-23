using System;

namespace Crystal.Plot2D;

public sealed class DateTimeVerticalAxisConstraint : ViewportConstraint
{
  private readonly double minSeconds = new TimeSpan(ticks: DateTime.MinValue.Ticks).TotalSeconds;
  private readonly double maxSeconds = new TimeSpan(ticks: DateTime.MaxValue.Ticks).TotalSeconds;

  public override DataRect Apply(DataRect previousDataRect, DataRect proposedDataRect, Viewport2D viewport)
  {
    DataRect borderRect = DataRect.Create(xMin: proposedDataRect.XMin, yMin: minSeconds, xMax: proposedDataRect.XMax, yMax: maxSeconds);
    if (proposedDataRect.IntersectsWith(rect: borderRect))
    {
      return DataRect.Intersect(rect1: proposedDataRect, rect2: borderRect);
    }

    return previousDataRect;
  }
}
