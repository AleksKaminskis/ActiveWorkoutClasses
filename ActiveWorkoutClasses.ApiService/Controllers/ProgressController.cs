using System.Security.Claims;
using ActiveWorkoutClasses.Application.DTOs.Progress;
using ActiveWorkoutClasses.Application.Interfaces;
using ActiveWorkoutClasses.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ActiveWorkoutClasses.ApiService.Controllers
{
    [ApiController]
    [Route("api/progress")]
    [Authorize]
    public class ProgressController : ControllerBase
    {
        private readonly IProgressService _progressService;

        public ProgressController(IProgressService progressService) => _progressService = progressService;

        [HttpGet("me")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetMyProgress()
        {
            var studentId = GetCurrentUserId();
            var result = await _progressService.GetStudentProgressAsync(studentId);
            return result is null ? NotFound() : Ok(result);
        }

        [HttpGet("student/{studentId:guid}")]
        public async Task<IActionResult> GetStudentProgress(Guid studentId)
        {
            var result = await _progressService.GetStudentProgressAsync(studentId);
            return result is null ? NotFound() : Ok(result);
        }

        [HttpGet("student/{studentId:guid}/current/{discipline}")]
        public async Task<IActionResult> GetCurrentLevel(Guid studentId, ClassType discipline)
        {
            var result = await _progressService.GetCurrentLevelAsync(studentId, discipline);
            return result is null ? NotFound() : Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> Create([FromBody] CreateProgressRecordDto dto)
        {
            var instructorId = GetCurrentUserId();
            var result = await _progressService.CreateAsync(dto, instructorId);
            return CreatedAtAction(nameof(GetStudentProgress), new { studentId = dto.StudentId }, result);
        }

        private Guid GetCurrentUserId()
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
        }
    }
}
