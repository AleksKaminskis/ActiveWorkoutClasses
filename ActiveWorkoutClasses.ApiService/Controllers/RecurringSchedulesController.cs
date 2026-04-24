using ActiveWorkoutClasses.Application.DTOs.Schedules;
using ActiveWorkoutClasses.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ActiveWorkoutClasses.ApiService.Controllers
{
    [ApiController]
    [Route("api/schedules")]
    [Authorize(Roles = "Admin,Instructor")]
    public class RecurringSchedulesController : ControllerBase
    {
        private readonly IRecurringScheduleService _scheduleService;

        public RecurringSchedulesController(IRecurringScheduleService scheduleService) => _scheduleService = scheduleService;

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _scheduleService.GetAllAsync());

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _scheduleService.GetByIdAsync(id);
            return result is null ? NotFound() : Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateRecurringScheduleDto dto)
        {
            var result = await _scheduleService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateRecurringScheduleDto dto)
        {
            var result = await _scheduleService.UpdateAsync(id, dto);
            return result is null ? NotFound() : Ok(result);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _scheduleService.DeleteAsync(id);
            return deleted ? NoContent() : NotFound();
        }

        [HttpPost("{id:int}/materialise")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Materialise(int id, [FromQuery] int? weeksAhead = null)
        {
            var count = await _scheduleService.MaterialiseAsync(id, weeksAhead);
            return Ok(new { CreatedClasses = count });
        }

        [HttpDelete("{id:int}/classes")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CleanClasses(int id)
        {
            var count = await _scheduleService.CleanGeneratedClassesAsync(id);
            return Ok(new { DeletedClasses = count });
        }
    }
}
