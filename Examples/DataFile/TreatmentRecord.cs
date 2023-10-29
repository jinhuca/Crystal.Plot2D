namespace DataFile
{
  public record struct TreatmentRecord
  {
    public SystemState State { get; set; }
    public int Index { get; set; }
    public DateTime Time { get; set; }
    public double TC1 { get; set; }
    public double TS1 { get; set; }
    public double FM1 { get; set; }
    public double PT1 { get; set; }
    public double PT2 { get; set; }
    public double PT3 { get; set; }
    public double PT4 { get; set; }
    public double PT5 { get; set; }
  }
}