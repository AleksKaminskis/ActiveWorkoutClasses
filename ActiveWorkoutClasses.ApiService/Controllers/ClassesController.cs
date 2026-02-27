using ActiveWorkoutClasses.Application.DTOs.Classes;
using ActiveWorkoutClasses.Application.Interfaces;
using ActiveWorkoutClasses.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace ActiveWorkoutClasses.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClassesController : ControllerBase
{
    private readonly IWorkoutClassService _classService;
    private readonly ILogger<ClassesController> _logger;

    public ClassesController(
        IWorkoutClassService classService,
        ILogger<ClassesController> logger)
    {
        _classService = classService;
        _logger = logger;
    }

    /// <summary>
    /// Get all active workout classes
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<WorkoutClassDto>>> GetAllClasses()
    {
        _logger.LogInformation("Getting all classes");
        var classes = await _classService.GetAllClassesAsync();
        return Ok(classes);
    }

    /// <summary>
    /// Get upcoming classes
    /// </summary>
    [HttpGet("upcoming")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<WorkoutClassDto>>> GetUpcomingClasses()
    {
        _logger.LogInformation("Getting upcoming classes");
        var classes = await _classService.GetUpcomingClassesAsync();
        return Ok(classes);
    }

    /// <summary>
    /// Get a specific class by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkoutClassDto>> GetClass(Guid id)
    {
        _logger.LogInformation("Getting class {ClassId}", id);
        var workoutClass = await _classService.GetClassByIdAsync(id);

        if (workoutClass == null)
        {
            return NotFound(new { Message = $"Class with ID {id} not found" });
        }

        return Ok(workoutClass);
    }

    /// <summary>
    /// Get classes by type
    /// </summary>
    [HttpGet("type/{classType}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<WorkoutClassDto>>> GetClassesByType(ClassType classType)
    {
        _logger.LogInformation("Getting classes of type {ClassType}", classType);
        var classes = await _classService.GetClassesByTypeAsync(classType);
        return Ok(classes);
    }

    /// <summary>
    /// Get classes with pagination
    /// </summary>
    [HttpGet("paged")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<WorkoutClassListDto>> GetClassesPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        _logger.LogInformation("Getting paged classes (page {Page}, size {PageSize})", page, pageSize);
        var result = await _classService.GetClassesPagedAsync(page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Create a new workout class
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WorkoutClassDto>> CreateClass(
        [FromBody] CreateWorkoutClassDto createDto)
    {
        _logger.LogInformation("Creating new class: {Title}", createDto.Title);

        try
        {
            var workoutClass = await _classService.CreateClassAsync(createDto);
            return CreatedAtAction(
                nameof(GetClass),
                new { id = workoutClass.Id },
                workoutClass
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating class");
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing workout class
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkoutClassDto>> UpdateClass(
        Guid id,
        [FromBody] UpdateWorkoutClassDto updateDto)
    {
        _logger.LogInformation("Updating class {ClassId}", id);

        try
        {
            var workoutClass = await _classService.UpdateClassAsync(id, updateDto);
            return Ok(workoutClass);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating class {ClassId}", id);
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Delete a workout class
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteClass(Guid id)
    {
        _logger.LogInformation("Deleting class {ClassId}", id);

        var deleted = await _classService.DeleteClassAsync(id);
        if (!deleted)
        {
            return NotFound(new { Message = $"Class with ID {id} not found" });
        }

        return NoContent();
    }

    /// <summary>
    /// Get available spots for a class
    /// </summary>
    [HttpGet("{id}/available-spots")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<int>> GetAvailableSpots(Guid id)
    {
        try
        {
            var spots = await _classService.GetAvailableSpotsAsync(id);
            return Ok(new { ClassId = id, AvailableSpots = spots });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }
}