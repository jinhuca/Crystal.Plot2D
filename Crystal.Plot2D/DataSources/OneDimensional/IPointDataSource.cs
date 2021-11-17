using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Crystal.Plot2D.DataSources.OneDimensional
{
  /// <summary>
  /// Data source that returns sequence of 2D points.
  /// </summary>
  public interface IPointDataSource
  {
    /// <summary>
    /// Returns object to enumerate points in source.
    /// </summary>
    IPointEnumerator GetEnumerator(DependencyObject context);

    /// <summary>
    /// This event is raised when contents of source are changed.
    /// </summary>
    event EventHandler DataChanged;
  }
}
