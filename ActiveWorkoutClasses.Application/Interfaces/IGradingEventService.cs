using ActiveWorkoutClasses.Application.DTOs.Grading;

namespace ActiveWorkoutClasses.Application.Interfaces
{
    public interface IGradingEventService
    {
        Task<List<GradingEventDto>> GetAllAsync();
        Task<GradingEventDto?> GetByIdAsync(int id);
        Task<GradingEventDto> CreateAsync(CreateGradingEventDto dto);
        Task<GradingEventDto?> UpdateAsync(int id, UpdateGradingEventDto dto);
        Task<bool> DeleteAsync(int id);
        Task<GradingEventDto?> PublishAsync(int id);
        Task<List<EligibilityCheckResultDto>> RunEligibilityCheckAsync(int gradingEventId);
        Task<List<GradingCandidateDto>> GetCandidatesAsync(int gradingEventId);
        Task<bool> OverrideEligibilityAsync(int gradingEventId, Guid studentId, OverrideEligibilityDto dto, Guid performedByUserId);
        Task<bool> RecordResultAsync(int gradingEventId, RecordGradingResultDto dto);
        Task<GradingEventDto?> PublishResultsAsync(int id);
        Task<GradingCandidateDto?> GetStudentResultAsync(int gradingEventId, Guid studentId);
    }
}
