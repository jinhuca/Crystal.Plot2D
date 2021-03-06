using System.Collections.ObjectModel;

namespace Crystal.Plot2D.Charts
{
  /// <summary>
  ///   IsolineTextAnnotater defines methods to annotate isolines - create a list of labels with its position.
  /// </summary>
  public sealed class IsolineTextAnnotater
  {
    private double wayBeforeText = 10;

    /// <summary>
    ///   Gets or sets the distance between text labels.
    /// </summary>
    public double WayBeforeText
    {
      get { return wayBeforeText; }
      set { wayBeforeText = value; }
    }

    /// <summary>
    /// Annotates the specified isoline collection.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="visible">The visible rectangle.</param>
    /// <returns></returns>
    public Collection<IsolineTextLabel> Annotate(IsolineCollection collection, DataRect visible)
    {
      Collection<IsolineTextLabel> res = new();

      foreach (var line in collection.Lines)
      {
        double way = 0;

        var forwardSegments = line.GetSegments();
        var forwardEnumerator = forwardSegments.GetEnumerator();
        forwardEnumerator.MoveNext();

        foreach (var segment in line.GetSegments())
        {
          bool hasForwardSegment = forwardEnumerator.MoveNext();

          double length = segment.GetLength();
          way += length;
          if (way > wayBeforeText)
          {
            way = 0;

            var rotation = (segment.Max - segment.Min).ToAngle();
            if (hasForwardSegment)
            {
              var forwardSegment = forwardEnumerator.Current;
              rotation = (rotation + (forwardSegment.Max - forwardSegment.Min).ToAngle()) / 2;
            }

            res.Add(new IsolineTextLabel
            {
              Value = line.RealValue,
              Position = segment.Max,
              Rotation = rotation
            });
          }
        }
      }

      return res;
    }
  }
}
