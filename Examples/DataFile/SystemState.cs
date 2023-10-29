using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataFile
{
  public enum SystemState
  {
    Unknown,
    Idle,
    Ready,
    Inflation,
    Transition,
    Ablation,
    Thawing
  }
}
