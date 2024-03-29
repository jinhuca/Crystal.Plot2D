﻿using System.Collections.Generic;
using Crystal.Plot2D.Isolines;

namespace Crystal.Plot2D.Common.Auxiliary;

internal static class DictionaryExtensions
{
  internal static void Add<TKey, TValue>(this Dictionary<TKey, TValue> dict, TValue value, params TKey[] keys)
  {
    foreach (var key in keys)
    {
      dict.Add(key: key, value: value);
    }
  }

  internal static void Add(this Dictionary<int, Edge> dict, Edge value, params CellBitmask[] keys)
  {
    foreach (var key in keys)
    {
      dict.Add(key: (int)key, value: value);
    }
  }
}
