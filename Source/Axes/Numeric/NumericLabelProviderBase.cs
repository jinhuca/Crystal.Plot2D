using System;
using System.Globalization;

namespace Crystal.Plot2D.Charts;

public abstract class NumericLabelProviderBase : LabelProviderBase<double>
{
  bool shouldRound = true;
  private int rounding;
  protected void Init(double[] ticks)
  {
    if (ticks.Length == 0)
    {
      return;
    }

    double start = ticks[0];
    double finish = ticks[ticks.Length - 1];

    if (start == finish)
    {
      shouldRound = false;
      return;
    }

    double delta = finish - start;

    rounding = (int)Math.Round(a: Math.Log10(d: delta));

    double newStart = RoundingHelper.Round(number: start, rem: rounding);
    double newFinish = RoundingHelper.Round(number: finish, rem: rounding);
    if (newStart == newFinish)
    {
      rounding--;
    }
  }

  protected override string GetStringCore(LabelTickInfo<double> tickInfo)
  {
    string res;
    if (!shouldRound)
    {
      res = tickInfo.Tick.ToString(provider: CultureInfo.InvariantCulture);
    }
    else
    {
      int round = Math.Min(val1: 15, val2: Math.Max(val1: -15, val2: rounding - 3)); // was rounding - 2
      res = RoundingHelper.Round(number: tickInfo.Tick, rem: round).ToString(provider: CultureInfo.InvariantCulture);
    }

    return res;
  }
}
