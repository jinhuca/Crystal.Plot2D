using System.Text.Json;
using System.Text.Json.Serialization;
using DataFile;

namespace DataFileDemo
{
  internal class Program
  {
    static async Task Main(string[] args)
    {
      var p_ = CreateProcedureData();
      var options_ = new JsonSerializerOptions
      {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
      };
      var ps_ = JsonSerializer.Serialize(p_, options_);
      await File.WriteAllTextAsync(@"C:\Users\JinHu_PC\Documents\Repositories\Research\Crystal.Plot2D\Examples\DataFileDemo\bin\Debug\net6.0\temp.json", ps_);
      var temp_ = JsonSerializer.Deserialize<Procedure>(ps_, options_);
    }

    static Procedure CreateProcedureData()
    {
      GeneralInfo g_ = new GeneralInfo
      {
        ConsoleSerialNumber = "PX01-MTL024-1212",
        HospitalName = "Montreal General Hospital",
        PhysicianFirstName = "John",
        PhysicianLastName = "Heljsberg",
        PatientFirstName = "Bill",
        PatientLastName = "Bergeron",
        PatientAge = 35,
        PatientGender = Gender.Male
      };

      Dictionary<int, List<TreatmentRecord>> dic_ = new()
      {
        {
          1, new List<TreatmentRecord>
          {
            new() { State = SystemState.Inflation, Index = 0, Time = DateTime.Now, TC1 = 36.7 },
            new() { State = SystemState.Transition, Index = 1, Time = DateTime.Now.AddSeconds(1), TC1 = 35.0 },
            new() { State = SystemState.Ablation, Index = 2, Time = DateTime.Now.AddSeconds(1), TC1 = 34.0 }
          }
        },
        {
          2, new List<TreatmentRecord>
          {
            new() { State = SystemState.Inflation, Index = 0, TC1 = 36.8 },
            new() { State = SystemState.Transition, Index = 1, TC1 = 35.2 }
          }
        }
      };

      Procedure procedure_ = new Procedure(g_, dic_);

      return procedure_;
    }
  }
}