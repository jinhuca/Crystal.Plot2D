using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataFile
{
  public record struct GeneralInfo
  {
    public string ConsoleSerialNumber { get; set; }
    public string HospitalName { get; set; }
    public string PhysicianFirstName { get; set; }
    public string PhysicianLastName { get; set; }
    public string PatientFirstName { get; set; }
    public string PatientLastName { get; set; }
    public Gender PatientGender { get; set; }
    public int PatientAge { get; set; }
  }
}
