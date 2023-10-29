using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DataFile
{
  public class Procedure
  {
    public Procedure(GeneralInfo info, IDictionary<int, List<TreatmentRecord>> treatments)
    {
      Info = info;
      Treatments = treatments;
      TreatmentCount = treatments.Count;
    }

    public GeneralInfo Info { get; set; }
    public IDictionary<int, List<TreatmentRecord>> Treatments { get; set; }
    public int TreatmentCount { get; set; }
  }
}
