﻿using System.Windows.Media;

namespace Crystal.Plot2D.Common
{
  public static class DoubleCollectionHelper
  {
    public static DoubleCollection Create(params double[] collection)
    {
      return new DoubleCollection(collection);
    }
  }
}
