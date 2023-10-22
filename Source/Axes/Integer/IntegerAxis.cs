namespace Crystal.Plot2D.Charts;

public class IntegerAxis : AxisBase<int>
{
  public IntegerAxis()
    : base(new IntegerAxisControl(),
      d => (int)d,
      i => (double)i)
  {

  }
}
