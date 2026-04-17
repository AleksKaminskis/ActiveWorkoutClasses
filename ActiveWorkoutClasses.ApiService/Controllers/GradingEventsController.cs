using System.Security.Claims;
using ActiveWorkoutClasses.Application.DTOs.Grading;
using ActiveWorkoutClasses.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ActiveWorkoutClasses.ApiService.Controllers
{
    [ApiController]
    [Route("api/grading")]
    [Authorize]
    public class GradingEventsController : ControllerBase
    {
        private readonly IGradingEventService _gradingService;

        public GradingEventsController(IGradingEventService gradingService) => _gradingService = gradingService;

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _gradingService.GetAllAsync());

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _gradingService.GetByIdAsync(id);
            return result is null ? NotFound() : Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateGradingEventDto dto)
        {
            var result = await _gradingService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateGradingEventDto dto)
        {
            var result = await _gradingService.UpdateAsync(id, dto);
            return result is null ? NotFound() : Ok(result);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _gradingService.DeleteAsync(id);
            return deleted ? NoContent() : NotFound();
        }

        [HttpPost("{id:int}/publish")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Publish(int id)
        {
            var result = await _gradingService.PublishAsync(id);
            return result is null ? NotFound() : Ok(result);
        }

        [HttpPost("{id:int}/run-eligibility")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RunEligibility(int id)
        {
            try
            {
                var results = await _gradingService.RunEligibilityCheckAsync(id);
                return Ok(results);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }

        [HttpGet("{id:int}/candidates")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> GetCandidates(int id)
            => Ok(await _gradingService.GetCandidatesAsync(id));

        [HttpPost("{id:int}/override/{studentId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> OverrideEligibility(int id, Guid studentId, [FromBody] OverrideEligibilityDto dto)
        {
            var performedBy = GetCurrentUserId();
            var success = await _gradingService.OverrideEligibilityAsync(id, studentId, dto, performedBy);
            return success ? Ok() : NotFound();
        }

        [HttpPost("{id:int}/results")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> RecordResult(int id, [FromBody] RecordGradingResultDto dto)
        {
            var success = await _gradingService.RecordResultAsync(id, dto);
            return success ? Ok() : NotFound();
        }

        [HttpPost("{id:int}/publish-results")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PublishResults(int id)
        {
            var result = await _gradingService.PublishResultsAsync(id);
            return result is null ? NotFound() : Ok(result);
        }

        [HttpGet("{id:int}/my-result")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetMyResult(int id)
        {
            var studentId = GetCurrentUserId();
            var result = await _gradingService.GetStudentResultAsync(id, studentId);
            return result is null ? NotFound() : Ok(result);
        }

        private Guid GetCurrentUserId()
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
        }
    }
}
