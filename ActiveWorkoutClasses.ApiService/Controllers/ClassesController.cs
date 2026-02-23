using ActiveWorkoutClasses.Domain.Entities;
using ActiveWorkoutClasses.Domain.Enums;
using ActiveWorkoutClasses.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ActiveWorkoutClasses.ApiService.Controllers
{
    /// <summary>
    /// API endpoints for managing workout classes
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ClassesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ClassesController> _logger;

        public ClassesController(ApplicationDbContext context, ILogger<ClassesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all active workout classes
        /// </summary>
        /// <returns>List of all active classes</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<WorkoutClass>>> GetAllClasses()
        {
            _logger.LogInformation("Getting all active workout classes");

            var classes = await _context.WorkoutClasses
                .Include(c => c.ClassInstructors)
                    .ThenInclude(ci => ci.Instructor)
                .Where(c => c.IsActive)
                .OrderBy(c => c.StartDateTime)
                .ToListAsync();

            return Ok(classes);
        }

        /// <summary>
        /// Get upcoming classes (starting from now)
        /// </summary>
        /// <returns>List of upcoming classes</returns>
        [HttpGet("upcoming")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<WorkoutClass>>> GetUpcomingClasses()
        {
            _logger.LogInformation("Getting upcoming workout classes");

            var now = DateTime.UtcNow;
            var classes = await _context.WorkoutClasses
                .Include(c => c.ClassInstructors)
                    .ThenInclude(ci => ci.Instructor)
                .Where(c => c.IsActive && c.StartDateTime > now)
                .OrderBy(c => c.StartDateTime)
                .ToListAsync();

            return Ok(classes);
        }

        /// <summary>
        /// Get a specific workout class by ID
        /// </summary>
        /// <param name="id">Class ID</param>
        /// <returns>Workout class details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<WorkoutClass>> GetClass(Guid id)
        {
            _logger.LogInformation("Getting workout class {ClassId}", id);

            var workoutClass = await _context.WorkoutClasses
                .Include(c => c.ClassInstructors)
                    .ThenInclude(ci => ci.Instructor)
                .Include(c => c.ClassRegistrations)
                    .ThenInclude(cr => cr.Student)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (workoutClass == null)
            {
                _logger.LogWarning("Workout class {ClassId} not found", id);
                return NotFound(new { Message = $"Class with ID {id} not found" });
            }

            return Ok(workoutClass);
        }

        /// <summary>
        /// Get classes by type
        /// </summary>
        /// <param name="classType">Type of class (e.g., KravMaga, Boxing, Yoga)</param>
        /// <returns>List of classes of the specified type</returns>
        [HttpGet("type/{classType}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<WorkoutClass>>> GetClassesByType(ClassType classType)
        {
            _logger.LogInformation("Getting classes of type {ClassType}", classType);

            var classes = await _context.WorkoutClasses
                .Include(c => c.ClassInstructors)
                    .ThenInclude(ci => ci.Instructor)
                .Where(c => c.IsActive && c.ClassType == classType)
                .OrderBy(c => c.StartDateTime)
                .ToListAsync();

            return Ok(classes);
        }

        /// <summary>
        /// Create a new workout class
        /// </summary>
        /// <param name="workoutClass">Class details</param>
        /// <returns>Created class</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<WorkoutClass>> CreateClass([FromBody] WorkoutClass workoutClass)
        {
            _logger.LogInformation("Creating new workout class: {Title}", workoutClass.Title);

            try
            {
                // Validate the class
                workoutClass.Validate();

                // Set timestamps
                workoutClass.Id = Guid.NewGuid();
                workoutClass.CreatedAt = DateTime.UtcNow;
                workoutClass.UpdatedAt = DateTime.UtcNow;

                _context.WorkoutClasses.Add(workoutClass);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created workout class {ClassId}", workoutClass.Id);

                return CreatedAtAction(
                    nameof(GetClass),
                    new { id = workoutClass.Id },
                    workoutClass
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating workout class");
                return BadRequest(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing workout class
        /// </summary>
        /// <param name="id">Class ID</param>
        /// <param name="updatedClass">Updated class details</param>
        /// <returns>No content</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateClass(Guid id, [FromBody] WorkoutClass updatedClass)
        {
            _logger.LogInformation("Updating workout class {ClassId}", id);

            if (id != updatedClass.Id)
            {
                return BadRequest(new { Message = "ID mismatch" });
            }

            var existingClass = await _context.WorkoutClasses.FindAsync(id);
            if (existingClass == null)
            {
                return NotFound(new { Message = $"Class with ID {id} not found" });
            }

            try
            {
                // Validate updated data
                updatedClass.Validate();

                // Update properties
                existingClass.Title = updatedClass.Title;
                existingClass.Description = updatedClass.Description;
                existingClass.ClassType = updatedClass.ClassType;
                existingClass.StartDateTime = updatedClass.StartDateTime;
                existingClass.EndDateTime = updatedClass.EndDateTime;
                existingClass.MaxCapacity = updatedClass.MaxCapacity;
                existingClass.Location = updatedClass.Location;
                existingClass.IsActive = updatedClass.IsActive;
                existingClass.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated workout class {ClassId}", id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating workout class {ClassId}", id);
                return BadRequest(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Delete a workout class
        /// </summary>
        /// <param name="id">Class ID</param>
        /// <returns>No content</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteClass(Guid id)
        {
            _logger.LogInformation("Deleting workout class {ClassId}", id);

            var workoutClass = await _context.WorkoutClasses.FindAsync(id);
            if (workoutClass == null)
            {
                return NotFound(new { Message = $"Class with ID {id} not found" });
            }

            _context.WorkoutClasses.Remove(workoutClass);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted workout class {ClassId}", id);

            return NoContent();
        }

        /// <summary>
        /// Get all registered students for a specific class
        /// </summary>
        /// <param name="id">Class ID</param>
        /// <returns>List of registered students</returns>
        [HttpGet("{id}/students")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<ClassRegistration>>> GetClassStudents(Guid id)
        {
            _logger.LogInformation("Getting students for class {ClassId}", id);

            var classExists = await _context.WorkoutClasses.AnyAsync(c => c.Id == id);
            if (!classExists)
            {
                return NotFound(new { Message = $"Class with ID {id} not found" });
            }

            var registrations = await _context.ClassRegistrations
                .Include(cr => cr.Student)
                .Include(cr => cr.Attendance)
                .Where(cr => cr.WorkoutClassId == id && cr.Status == RegistrationStatus.Registered)
                .OrderBy(cr => cr.RegistrationDate)
                .ToListAsync();

            return Ok(registrations);
        }
    }
}
