using ActiveWorkoutClasses.Application.DTOs.Analytics;
using ActiveWorkoutClasses.Application.Interfaces;
using ActiveWorkoutClasses.Domain.Entities;
using ActiveWorkoutClasses.Domain.Enums;
using ActiveWorkoutClasses.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ActiveWorkoutClasses.Application.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly ApplicationDbContext _context;

        public AnalyticsService(ApplicationDbContext context) => _context = context;

        public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
        {
            var now = DateTime.UtcNow;
            var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var monthEnd   = monthStart.AddMonths(1);

            var totalStudents = await _context.Users.OfType<Student>().CountAsync(s => s.IsActive);
            var totalInstructors = await _context.Users.OfType<Instructor>().CountAsync(i => i.IsActive);
            var classesThisMonth = await _context.WorkoutClasses
                .CountAsync(c => c.StartDateTime >= monthStart && c.StartDateTime < monthEnd && c.IsActive);
            var attendanceThisMonth = await _context.Attendances
                .Include(a => a.ClassRegistration).ThenInclude(r => r.WorkoutClass)
                .CountAsync(a => a.IsPresent && a.ClassRegistration.WorkoutClass.StartDateTime >= monthStart);

            var registrationsThisMonth = await _context.ClassRegistrations
                .Include(r => r.WorkoutClass)
                .CountAsync(r => r.WorkoutClass.StartDateTime >= monthStart && r.Status == RegistrationStatus.Registered);

            double attendanceRate = registrationsThisMonth > 0
                ? Math.Round((double)attendanceThisMonth / registrationsThisMonth * 100, 1)
                : 0;

            var trend = await GetAttendanceTrendAsync(now.AddDays(-30), now);
            var topClasses = await GetClassPopularityAsync(now.AddDays(-90), now);
            var locations = await GetLocationStatsAsync();

            return new DashboardSummaryDto
            {
                TotalStudents = totalStudents,
                TotalInstructors = totalInstructors,
                TotalClassesThisMonth = classesThisMonth,
                TotalAttendanceThisMonth = attendanceThisMonth,
                AttendanceRateThisMonth = attendanceRate,
                AttendanceTrend = trend,
                TopClasses = topClasses,
                ActiveLocations = locations
            };
        }

        public async Task<List<AttendanceTrendDto>> GetAttendanceTrendAsync(DateTime from, DateTime to, int? locationId = null)
        {
            var attendanceQuery = _context.Attendances
                .Include(a => a.ClassRegistration).ThenInclude(r => r.WorkoutClass)
                .Where(a => a.CheckInTime >= from && a.CheckInTime <= to && a.IsPresent);

            if (locationId.HasValue)
                attendanceQuery = attendanceQuery.Where(a => a.ClassRegistration.WorkoutClass.LocationId == locationId);

            var attendanceByDay = await attendanceQuery
                .GroupBy(a => a.CheckInTime!.Value.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Date, x => x.Count);

            var registrationByDay = await _context.ClassRegistrations
                .Include(r => r.WorkoutClass)
                .Where(r => r.RegistrationDate >= from && r.RegistrationDate <= to && r.Status == RegistrationStatus.Registered)
                .GroupBy(r => r.RegistrationDate.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Date, x => x.Count);

            var result = new List<AttendanceTrendDto>();
            for (var d = from.Date; d <= to.Date; d = d.AddDays(1))
            {
                result.Add(new AttendanceTrendDto
                {
                    Date = d,
                    AttendanceCount = attendanceByDay.GetValueOrDefault(d, 0),
                    RegistrationCount = registrationByDay.GetValueOrDefault(d, 0)
                });
            }
            return result;
        }

        public async Task<List<ClassPopularityDto>> GetClassPopularityAsync(DateTime? from = null, DateTime? to = null)
        {
            var query = _context.WorkoutClasses.AsQueryable();
            if (from.HasValue) query = query.Where(c => c.StartDateTime >= from.Value);
            if (to.HasValue) query = query.Where(c => c.StartDateTime <= to.Value);

            var classes = await query.Include(c => c.ClassRegistrations)
                .ThenInclude(r => r.Attendance)
                .ToListAsync();

            return classes
                .GroupBy(c => c.ClassType)
                .Select(g => new ClassPopularityDto
                {
                    ClassType = g.Key.ToString(),
                    TotalSessions = g.Count(),
                    TotalAttendees = g.Sum(c => c.ClassRegistrations.Sum(r => r.Attendance != null && r.Attendance.IsPresent ? 1 : 0)),
                    AvgAttendance = g.Any()
                        ? Math.Round(g.Average(c => (double)c.ClassRegistrations.Sum(r => r.Attendance != null && r.Attendance.IsPresent ? 1 : 0)), 1)
                        : 0
                })
                .OrderByDescending(x => x.TotalAttendees)
                .ToList();
        }

        public async Task<List<LocationStatsDto>> GetLocationStatsAsync()
        {
            var locations = await _context.Locations.Where(l => l.IsActive).ToListAsync();
            var result = new List<LocationStatsDto>();

            foreach (var loc in locations)
            {
                var classCount = await _context.WorkoutClasses.CountAsync(c => c.LocationId == loc.Id);
                var attendance = await _context.Attendances
                    .Include(a => a.ClassRegistration).ThenInclude(r => r.WorkoutClass)
                    .CountAsync(a => a.IsPresent && a.ClassRegistration.WorkoutClass.LocationId == loc.Id);

                result.Add(new LocationStatsDto
                {
                    LocationId = loc.Id,
                    LocationName = loc.Name,
                    TotalClasses = classCount,
                    TotalAttendance = attendance
                });
            }
            return result;
        }

        public async Task<StudentAttendanceSummaryDto?> GetStudentAttendanceSummaryAsync(Guid studentId)
        {
            var student = await _context.Users.OfType<Student>().FirstOrDefaultAsync(s => s.Id == studentId);
            if (student is null) return null;

            var registrations = await _context.ClassRegistrations
                .Where(r => r.StudentId == studentId && r.Status == RegistrationStatus.Registered)
                .CountAsync();

            var attended = await _context.Attendances
                .Include(a => a.ClassRegistration)
                .Where(a => a.ClassRegistration.StudentId == studentId && a.IsPresent)
                .CountAsync();

            var monthlyTrend = await GetAttendanceTrendAsync(DateTime.UtcNow.AddMonths(-6), DateTime.UtcNow);

            var byType = await _context.Attendances
                .Include(a => a.ClassRegistration).ThenInclude(r => r.WorkoutClass)
                .Where(a => a.ClassRegistration.StudentId == studentId && a.IsPresent)
                .GroupBy(a => a.ClassRegistration.WorkoutClass.ClassType)
                .Select(g => new ClassPopularityDto
                {
                    ClassType = g.Key.ToString(),
                    TotalSessions = g.Count(),
                    TotalAttendees = g.Count(),
                    AvgAttendance = g.Count()
                })
                .ToListAsync();

            // Calculate current streak (consecutive weeks with attendance)
            var recentAttendance = await _context.Attendances
                .Include(a => a.ClassRegistration)
                .Where(a => a.ClassRegistration.StudentId == studentId && a.IsPresent && a.CheckInTime.HasValue)
                .OrderByDescending(a => a.CheckInTime)
                .ToListAsync();

            int streak = CalculateStreak(recentAttendance);

            return new StudentAttendanceSummaryDto
            {
                StudentId = studentId,
                TotalAttendance = attended,
                TotalRegistrations = registrations,
                AttendanceRate = registrations > 0 ? Math.Round((double)attended / registrations * 100, 1) : 0,
                CurrentStreak = streak,
                MonthlyTrend = monthlyTrend,
                AttendanceByType = byType
            };
        }

        private static int CalculateStreak(List<Attendance> attendances)
        {
            if (attendances.Count == 0) return 0;
            int streak = 0;
            var currentWeek = ISOWeek(DateTime.UtcNow);
            foreach (var a in attendances)
            {
                if (ISOWeek(a.CheckInTime!.Value) == currentWeek - streak)
                    streak++;
                else
                    break;
            }
            return streak;
        }

        private static int ISOWeek(DateTime date)
            => System.Globalization.ISOWeek.GetWeekOfYear(date);
    }
}
