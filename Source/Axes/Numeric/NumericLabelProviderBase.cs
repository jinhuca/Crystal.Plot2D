using System;
using System.Globalization;

namespace Crystal.Plot2D.Axes.Numeric;

public abstract class NumericLabelProviderBase : LabelProviderBase<double>
{
  private bool shouldRound = true;
  private int rounding;
  protected void Init(double[] ticks)
  {
    if (ticks.Length == 0)
    {
      return;
    }

    var start = ticks[0];
    var finish = ticks[ticks.Length - 1];

    if (start == finish)
    {
      shouldRound = false;
      return;
    }

    var delta = finish - start;

    rounding = (int)Math.Round(a: Math.Log10(d: delta));

    var newStart = RoundingHelper.Round(number: start, rem: rounding);
    var newFinish = RoundingHelper.Round(number: finish, rem: rounding);
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
      var round = Math.Min(val1: 15, val2: Math.Max(val1: -15, val2: rounding - 3)); // was rounding - 2
      res = RoundingHelper.Round(number: tickInfo.Tick, rem: round).ToString(provider: CultureInfo.InvariantCulture);
    }

    return res;
  }
}
