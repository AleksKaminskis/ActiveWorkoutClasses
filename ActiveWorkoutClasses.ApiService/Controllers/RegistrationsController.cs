using ActiveWorkoutClasses.Application.DTOs.Registrations;
using ActiveWorkoutClasses.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ActiveWorkoutClasses.ApiService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegistrationsController : ControllerBase
    {
        private readonly IRegistrationService _registrationService;
        private readonly ILogger<RegistrationsController> _logger;

        public RegistrationsController(
            IRegistrationService registrationService,
            ILogger<RegistrationsController> logger)
        {
            _registrationService = registrationService;
            _logger = logger;
        }

        /// <summary>
        /// Register a student for a class
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<RegistrationResultDto>> RegisterForClass(
            [FromBody] RegisterForClassDto registerDto)
        {
            _logger.LogInformation($"Registering student {registerDto.StudentId} for class {registerDto.WorkoutClassId}");

            var result = await _registrationService.RegisterForClassAsync(
                registerDto.StudentId,
                registerDto.WorkoutClassId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(
                nameof(GetRegistration),
                new { id = result.Registration!.Id },
                result);
        }

        /// <summary>
        /// Get a specific registration
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ClassRegistrationDto>> GetRegistration(Guid id)
        {
            var registration = await _registrationService.GetRegistrationByIdAsync(id);

            if (registration == null)
            {
                return NotFound(new { Message = $"Registration {id} not found" });
            }

            return Ok(registration);
        }

        /// <summary>
        /// Get all registrations for a student
        /// </summary>
        [HttpGet("student/{studentId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ClassRegistrationDto>>> GetStudentRegistrations(
            Guid studentId)
        {
            var registrations = await _registrationService.GetStudentRegistrationsAsync(studentId);
            return Ok(registrations);
        }

        /// <summary>
        /// Get upcoming registrations for a student
        /// </summary>
        [HttpGet("student/{studentId}/upcoming")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ClassRegistrationDto>>> GetUpcomingRegistrations(
            Guid studentId)
        {
            var registrations = await _registrationService.GetUpcomingRegistrationsAsync(studentId);
            return Ok(registrations);
        }

        /// <summary>
        /// Get today's registrations for a student
        /// </summary>
        [HttpGet("student/{studentId}/today")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ClassRegistrationDto>>> GetTodayRegistrations(
            Guid studentId)
        {
            var registrations = await _registrationService.GetTodayRegistrationsAsync(studentId);
            return Ok(registrations);
        }

        /// <summary>
        /// Get all registrations for a class
        /// </summary>
        [HttpGet("class/{classId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ClassRegistrationDto>>> GetClassRegistrations(
            Guid classId)
        {
            var registrations = await _registrationService.GetClassRegistrationsAsync(classId);
            return Ok(registrations);
        }

        /// <summary>
        /// Cancel a registration
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CancelRegistration(Guid id)
        {
            try
            {
                var cancelled = await _registrationService.CancelRegistrationAsync(id);

                if (!cancelled)
                {
                    return NotFound(new { Message = $"Registration {id} not found" });
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
