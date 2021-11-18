using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Crystal.Plot2D.Common
{
  [DebuggerDisplay("Count = {Count}")]
  internal sealed class ResourcePool<T>
  {
    private readonly List<T> pool = new();

    public T Get()
    {
      T item;

      if (pool.Count < 1)
      {
        item = default(T);
      }
      else
      {
        int index = pool.Count - 1;
        item = pool[index];
        pool.RemoveAt(index);
      }

      return item;
    }

    public void Put(T item)
    {
      if (item == null)
      {
        throw new ArgumentNullException("item");
      }

#if DEBUG
      if (pool.IndexOf(item) != -1)
      {
        Debugger.Break();
      }
#endif

      pool.Add(item);
    }

    public int Count => pool.Count;

    public void Clear() => pool.Clear();
  }
}
