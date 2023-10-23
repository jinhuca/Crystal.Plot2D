using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Crystal.Plot2D.Common;

internal static class ObservableCollectionHelper
{
  public static void ApplyChanges<T>(this ObservableCollection<T> collection, NotifyCollectionChangedEventArgs args)
  {
    if (args.NewItems != null)
    {
      int startingIndex = args.NewStartingIndex;
      var newItems = args.NewItems;

      for (int i = 0; i < newItems.Count; i++)
      {
        T addedItem = (T)newItems[index: i];
        collection.Insert(index: startingIndex + i, item: addedItem);
      }
    }
    if (args.OldItems != null)
    {
      for (int i = 0; i < args.OldItems.Count; i++)
      {
        T removedItem = (T)args.OldItems[index: i];
        collection.Remove(item: removedItem);
      }
    }
  }
}
