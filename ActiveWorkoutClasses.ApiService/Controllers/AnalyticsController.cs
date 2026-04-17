using System.Security.Claims;
using ActiveWorkoutClasses.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ActiveWorkoutClasses.ApiService.Controllers
{
    [ApiController]
    [Route("api/analytics")]
    [Authorize]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;

        public AnalyticsController(IAnalyticsService analyticsService) => _analyticsService = analyticsService;

        [HttpGet("dashboard")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetDashboard()
            => Ok(await _analyticsService.GetDashboardSummaryAsync());

        [HttpGet("attendance-trend")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAttendanceTrend(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] int? locationId = null)
        {
            var fromDate = from ?? DateTime.UtcNow.AddDays(-30);
            var toDate = to ?? DateTime.UtcNow;
            return Ok(await _analyticsService.GetAttendanceTrendAsync(fromDate, toDate, locationId));
        }

        [HttpGet("class-popularity")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetClassPopularity([FromQuery] DateTime? from, [FromQuery] DateTime? to)
            => Ok(await _analyticsService.GetClassPopularityAsync(from, to));

        [HttpGet("locations")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetLocationStats()
            => Ok(await _analyticsService.GetLocationStatsAsync());

        [HttpGet("my-attendance")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetMyAttendance()
        {
            var studentId = GetCurrentUserId();
            var result = await _analyticsService.GetStudentAttendanceSummaryAsync(studentId);
            return result is null ? NotFound() : Ok(result);
        }

        private Guid GetCurrentUserId()
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
        }
    }
}
