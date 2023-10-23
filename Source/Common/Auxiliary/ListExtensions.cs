using System;
using System.Collections.Generic;

namespace Crystal.Plot2D;

internal static class ListExtensions
{
  /// <summary>
  ///   Gets last element of list.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="list"></param>
  /// <returns></returns>
  internal static T GetLast<T>(this List<T> list)
  {
    if (list == null)
    {
      throw new ArgumentNullException(paramName: nameof(list));
    }
    if (list.Count == 0)
    {
      throw new InvalidOperationException(message: Strings.Exceptions.CannotGetLastElement);
    }
    return list[index: list.Count - 1];
  }

  internal static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
  {
    if (action == null)
    {
      throw new ArgumentNullException(paramName: nameof(action));
    }
    if (source == null)
    {
      throw new ArgumentNullException(paramName: nameof(source));
    }

    foreach (var item in source)
    {
      action(obj: item);
    }
  }
}
