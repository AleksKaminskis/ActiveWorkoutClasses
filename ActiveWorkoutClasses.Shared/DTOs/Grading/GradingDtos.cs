using ActiveWorkoutClasses.Application.DTOs.Locations;
using ActiveWorkoutClasses.Domain.Enums;

namespace ActiveWorkoutClasses.Application.DTOs.Grading
{
    public class GradingEventDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime EventDate { get; set; }
        public LocationDto? Location { get; set; }
        public ClassType ClassType { get; set; }
        public string ClassTypeName { get; set; } = string.Empty;
        public bool IsPublished { get; set; }
        public bool ResultsPublished { get; set; }
        public List<EligibilityRuleDto> EligibilityRules { get; set; } = [];
        public int CandidateCount { get; set; }
    }

    public class CreateGradingEventDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime EventDate { get; set; }
        public int? LocationId { get; set; }
        public ClassType ClassType { get; set; }
        public List<EligibilityRuleDto> EligibilityRules { get; set; } = [];
    }

    public class UpdateGradingEventDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime EventDate { get; set; }
        public int? LocationId { get; set; }
    }

    public class EligibilityRuleDto
    {
        public int Id { get; set; }
        public EligibilityRuleType RuleType { get; set; }
        public int? IntValue { get; set; }
        public int? DaysValue { get; set; }
        public string? Description { get; set; }
    }

    public class GradingCandidateDto
    {
        public Guid StudentId { get; set; }
        public string StudentNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public bool IsEligible { get; set; }
        public bool? IsOverridden { get; set; }
        public bool EffectiveEligibility { get; set; }
        public List<string> EligibilityNotes { get; set; } = [];
        public string? CurrentBelt { get; set; }
        public GradingResultStatus? Status { get; set; }
    }

    public class RecordGradingResultDto
    {
        public Guid StudentId { get; set; }
        public GradingResultStatus Status { get; set; }
        public string? NewSkillLevel { get; set; }
        public string? InstructorNotes { get; set; }
    }

    public class OverrideEligibilityDto
    {
        public bool IsEligible { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public class EligibilityCheckResultDto
    {
        public Guid StudentId { get; set; }
        public string StudentNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public bool IsEligible { get; set; }
        public List<string> FailedRules { get; set; } = [];
        public List<string> PassedRules { get; set; } = [];
    }
}
