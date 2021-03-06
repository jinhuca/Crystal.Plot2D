using System;

namespace Crystal.Plot2D.Common
{
  public sealed class ValueChangedEventArgs<T> : EventArgs
  {
    public ValueChangedEventArgs(T prevValue, T currValue)
    {
      this.prevValue = prevValue;
      this.currValue = currValue;
    }

    private readonly T prevValue;
    public T PreviousValue
    {
      get { return prevValue; }
    }

    private readonly T currValue;
    public T CurrentValue
    {
      get { return currValue; }
    }
  }
}
