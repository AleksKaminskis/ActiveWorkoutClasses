using ActiveWorkoutClasses.Application.DTOs.Analytics;

namespace ActiveWorkoutClasses.Application.Interfaces
{
    public interface IAnalyticsService
    {
        Task<DashboardSummaryDto> GetDashboardSummaryAsync();
        Task<List<AttendanceTrendDto>> GetAttendanceTrendAsync(DateTime from, DateTime to, int? locationId = null);
        Task<List<ClassPopularityDto>> GetClassPopularityAsync(DateTime? from = null, DateTime? to = null);
        Task<List<LocationStatsDto>> GetLocationStatsAsync();
        Task<StudentAttendanceSummaryDto?> GetStudentAttendanceSummaryAsync(Guid studentId);
    }
}
