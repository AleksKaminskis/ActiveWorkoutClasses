using ActiveWorkoutClasses.Application.DTOs.Schedules;

namespace ActiveWorkoutClasses.Application.Interfaces
{
    public interface IRecurringScheduleService
    {
        Task<List<RecurringScheduleDto>> GetAllAsync();
        Task<RecurringScheduleDto?> GetByIdAsync(int id);
        Task<RecurringScheduleDto> CreateAsync(CreateRecurringScheduleDto dto);
        Task<RecurringScheduleDto?> UpdateAsync(int id, UpdateRecurringScheduleDto dto);
        Task<bool> DeleteAsync(int id);
        Task<int> MaterialiseAsync(int scheduleId, int? weeksAhead = null);
        Task<int> CleanGeneratedClassesAsync(int scheduleId);
    }
}
