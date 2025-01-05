using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Fitness.Business.Models
{
    [JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata)]
    public class FitnessProgram 
    {
        [Key]
        public string ProgramCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Target { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public int MaxMembers { get; set; }

        [JsonIgnore]
        public ICollection<ProgramMembers> ProgramMembers { get; set; } = new List<ProgramMembers>();
    }
}
