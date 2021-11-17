using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystal.Plot2D.Common
{
  [Conditional("DEBUG")]
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
  internal sealed class SkipPropertyCheckAttribute : Attribute
  {
  }
}
