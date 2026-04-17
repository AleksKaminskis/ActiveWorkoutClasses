using ActiveWorkoutClasses.Application.DTOs.Progress;
using ActiveWorkoutClasses.Domain.Enums;

namespace ActiveWorkoutClasses.Application.Interfaces
{
    public interface IProgressService
    {
        Task<StudentProgressSummaryDto?> GetStudentProgressAsync(Guid studentId);
        Task<ProgressRecordDto?> GetCurrentLevelAsync(Guid studentId, ClassType discipline);
        Task<List<ProgressRecordDto>> GetHistoryAsync(Guid studentId);
        Task<ProgressRecordDto> CreateAsync(CreateProgressRecordDto dto, Guid recordedByInstructorId);
    }
}
