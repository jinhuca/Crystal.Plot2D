using System.Collections.ObjectModel;
using Crystal.Plot2D.Common;
using Crystal.Plot2D.Common.Auxiliary;

namespace Crystal.Plot2D.Isolines;

/// <summary>
///   IsolineTextAnnotator defines methods to annotate isolines - create a list of labels with its position.
/// </summary>
public sealed class IsolineTextAnnotator
{
  private double wayBeforeText = 10;

  /// <summary>
  ///   Gets or sets the distance between text labels.
  /// </summary>
  public double WayBeforeText
  {
    get => wayBeforeText;
    set => wayBeforeText = value;
  }

  /// <summary>
  /// Annotates the specified isoline collection.
  /// </summary>
  /// <param name="collection">The collection.</param>
  /// <param name="visible">The visible rectangle.</param>
  /// <returns></returns>
  public Collection<IsolineTextLabel> Annotate(IsolineCollection collection, DataRect visible)
  {
    Collection<IsolineTextLabel> res_ = new();

    foreach (var line_ in collection.Lines)
    {
      double way_ = 0;

      var forwardSegments_ = line_.GetSegments();
      var forwardEnumerator_ = forwardSegments_.GetEnumerator();
      forwardEnumerator_.MoveNext();

      foreach (var segment_ in line_.GetSegments())
      {
        var hasForwardSegment_ = forwardEnumerator_.MoveNext();

        var length_ = segment_.GetLength();
        way_ += length_;
        if (way_ > wayBeforeText)
        {
          way_ = 0;

          var rotation_ = (segment_.Max - segment_.Min).ToAngle();
          if (hasForwardSegment_)
          {
            var forwardSegment_ = forwardEnumerator_.Current;
            rotation_ = (rotation_ + (forwardSegment_.Max - forwardSegment_.Min).ToAngle()) / 2;
          }

          res_.Add(item: new IsolineTextLabel
          {
            Value = line_.RealValue,
            Position = segment_.Max,
            Rotation = rotation_
          });
        }
      }
    }

    return res_;
  }
}
