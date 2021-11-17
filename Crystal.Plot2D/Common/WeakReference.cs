using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystal.Plot2D.Common
{
  internal sealed class WeakReference<T>
  {
    public WeakReference(WeakReference reference) => Reference = reference;
    public WeakReference(T referencedObject) => Reference = new WeakReference(referencedObject);
    public bool IsAlive => Reference.IsAlive;
    public T Target => (T)Reference.Target;
    public WeakReference Reference { get; }
  }
}
