using ActiveWorkoutClasses.Application.DTOs.Attendance;
using ActiveWorkoutClasses.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ActiveWorkoutClasses.ApiService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;
        private readonly ILogger<AttendanceController> _logger;

        public AttendanceController(
            IAttendanceService attendanceService,
            ILogger<AttendanceController> logger)
        {
            _attendanceService = attendanceService;
            _logger = logger;
        }

        /// <summary>
        /// Student check-in (self check-in)
        /// </summary>
        [HttpPost("checkin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CheckInResultDto>> CheckIn(
            [FromQuery] Guid registrationId,
            [FromQuery] Guid studentId)
        {
            _logger.LogInformation($"Student {studentId} checking in for registration {registrationId}");

            var result = await _attendanceService.StudentCheckInAsync(registrationId, studentId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Instructor marks student attendance
        /// </summary>
        [HttpPost("mark")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CheckInResultDto>> MarkAttendance(
            [FromQuery] Guid registrationId,
            [FromQuery] Guid instructorId,
            [FromQuery] bool isPresent,
            [FromQuery] string? notes = null)
        {
            _logger.LogInformation($"Instructor {instructorId} marking attendance for registration {registrationId}");

            var result = await _attendanceService.InstructorMarkAttendanceAsync(
                registrationId,
                instructorId,
                isPresent,
                notes);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Get attendance for a class
        /// </summary>
        [HttpGet("class/{classId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<StudentAttendanceDto>>> GetClassAttendance(
            Guid classId)
        {
            var attendance = await _attendanceService.GetClassAttendanceAsync(classId);
            return Ok(attendance);
        }

        /// <summary>
        /// Check if student can check-in now
        /// </summary>
        [HttpGet("can-checkin/{registrationId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<CheckInEligibilityDto>> CanCheckIn(Guid registrationId)
        {
            var eligibility = await _attendanceService.CanCheckInAsync(registrationId);
            return Ok(eligibility);
        }

        /// <summary>
        /// Get attendance statistics for a class
        /// </summary>
        [HttpGet("class/{classId}/stats")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AttendanceStatsDto>> GetClassStats(Guid classId)
        {
            try
            {
                var stats = await _attendanceService.GetClassAttendanceStatsAsync(classId);
                return Ok(stats);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Get student's attendance history
        /// </summary>
        [HttpGet("student/{studentId}/history")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<AttendanceDto>>> GetStudentHistory(Guid studentId)
        {
            var history = await _attendanceService.GetStudentAttendanceHistoryAsync(studentId);
            return Ok(history);
        }
    }
}
