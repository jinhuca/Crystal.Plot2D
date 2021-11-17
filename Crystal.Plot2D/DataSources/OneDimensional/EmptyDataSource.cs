﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Crystal.Plot2D.DataSources.OneDimensional
{
  /// <summary>
  ///   Empty data source - for testing purposes, represents data source with 0 points inside.
  /// </summary>
  public class EmptyDataSource : IPointDataSource
  {
    #region IPointDataSource Members

    public IPointEnumerator GetEnumerator(DependencyObject context) => new EmptyPointEnumerator();

    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
    private void RaiseDataChanged() => DataChanged?.Invoke(this, EventArgs.Empty);

    public event EventHandler DataChanged;

    #endregion

    private sealed class EmptyPointEnumerator : IPointEnumerator
    {
      public bool MoveNext() => false;

      public void GetCurrent(ref Point p)
      {
        // nothing to do
      }

      public void ApplyMappings(DependencyObject target)
      {
        // nothing to do
      }

      public void Dispose() { }
    }
  }
}
