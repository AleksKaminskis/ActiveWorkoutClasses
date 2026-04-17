using ActiveWorkoutClasses.Domain.Enums;

namespace ActiveWorkoutClasses.Application.DTOs.Progress
{
    public class ProgressRecordDto
    {
        public int Id { get; set; }
        public ClassType Discipline { get; set; }
        public string DisciplineName { get; set; } = string.Empty;
        public string SkillLevel { get; set; } = string.Empty;
        public DateTime PromotionDate { get; set; }
        public string? Notes { get; set; }
        public string? RecordedByInstructor { get; set; }
    }

    public class CreateProgressRecordDto
    {
        public Guid StudentId { get; set; }
        public ClassType Discipline { get; set; }
        public string SkillLevel { get; set; } = string.Empty;
        public DateTime PromotionDate { get; set; }
        public string? Notes { get; set; }
    }

    public class StudentProgressSummaryDto
    {
        public Guid StudentId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string StudentNumber { get; set; } = string.Empty;
        public List<ProgressRecordDto> AllRecords { get; set; } = [];
        public Dictionary<string, ProgressRecordDto> CurrentLevelByDiscipline { get; set; } = [];
    }
}
