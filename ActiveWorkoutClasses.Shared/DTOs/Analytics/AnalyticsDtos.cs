namespace ActiveWorkoutClasses.Application.DTOs.Analytics
{
    public class AttendanceTrendDto
    {
        public DateTime Date { get; set; }
        public int AttendanceCount { get; set; }
        public int RegistrationCount { get; set; }
    }

    public class ClassPopularityDto
    {
        public string ClassType { get; set; } = string.Empty;
        public int TotalSessions { get; set; }
        public int TotalAttendees { get; set; }
        public double AvgAttendance { get; set; }
    }

    public class LocationStatsDto
    {
        public int LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public int TotalClasses { get; set; }
        public int TotalAttendance { get; set; }
    }

    public class DashboardSummaryDto
    {
        public int TotalStudents { get; set; }
        public int TotalInstructors { get; set; }
        public int TotalClassesThisMonth { get; set; }
        public int TotalAttendanceThisMonth { get; set; }
        public double AttendanceRateThisMonth { get; set; }
        public List<AttendanceTrendDto> AttendanceTrend { get; set; } = [];
        public List<ClassPopularityDto> TopClasses { get; set; } = [];
        public List<LocationStatsDto> ActiveLocations { get; set; } = [];
    }

    public class StudentAttendanceSummaryDto
    {
        public Guid StudentId { get; set; }
        public int TotalAttendance { get; set; }
        public int TotalRegistrations { get; set; }
        public double AttendanceRate { get; set; }
        public int CurrentStreak { get; set; }
        public List<AttendanceTrendDto> MonthlyTrend { get; set; } = [];
        public List<ClassPopularityDto> AttendanceByType { get; set; } = [];
    }
}
