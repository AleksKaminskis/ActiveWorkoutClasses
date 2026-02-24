using ActiveWorkoutClasses.Application.DTOs.Users;
using ActiveWorkoutClasses.Application.Interfaces;
using ActiveWorkoutClasses.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace ActiveWorkoutClasses.ApiService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            IUserService userService,
            ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Get all users
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<UserDto>>> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        /// <summary>
        /// Get all students
        /// </summary>
        [HttpGet("students")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<UserDetailDto>>> GetAllStudents()
        {
            var students = await _userService.GetAllStudentsAsync();
            return Ok(students);
        }

        /// <summary>
        /// Get all instructors
        /// </summary>
        [HttpGet("instructors")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<UserDetailDto>>> GetAllInstructors()
        {
            var instructors = await _userService.GetAllInstructorsAsync();
            return Ok(instructors);
        }

        /// <summary>
        /// Get users by role
        /// </summary>
        [HttpGet("role/{role}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<UserDto>>> GetUsersByRole(UserRole role)
        {
            var users = await _userService.GetUsersByRoleAsync(role);
            return Ok(users);
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserDetailDto>> GetUser(Guid id)
        {
            var user = await _userService.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound(new { Message = $"User {id} not found" });
            }

            return Ok(user);
        }

        /// <summary>
        /// Get users with pagination
        /// </summary>
        [HttpGet("paged")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<UserListDto>> GetUsersPaged(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] UserRole? role = null)
        {
            var result = await _userService.GetUsersPagedAsync(page, pageSize, role);
            return Ok(result);
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UserDetailDto>> CreateUser(
            [FromBody] CreateUserDto createDto)
        {
            _logger.LogInformation("Creating user: {Email}", createDto.Email);

            try
            {
                var user = await _userService.CreateUserAsync(createDto);
                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return BadRequest(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Update a user
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserDetailDto>> UpdateUser(
            Guid id,
            [FromBody] UpdateUserDto updateDto)
        {
            _logger.LogInformation($"Updating user {id}");

            try
            {
                var user = await _userService.UpdateUserAsync(id, updateDto);
                return Ok(user);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating user {id}");
                return BadRequest(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Deactivate a user
        /// </summary>
        [HttpPost("{id}/deactivate")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeactivateUser(Guid id)
        {
            var deactivated = await _userService.DeactivateUserAsync(id);

            if (!deactivated)
            {
                return NotFound(new { Message = $"User {id} not found" });
            }

            return NoContent();
        }

        /// <summary>
        /// Activate a user
        /// </summary>
        [HttpPost("{id}/activate")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ActivateUser(Guid id)
        {
            var activated = await _userService.ActivateUserAsync(id);

            if (!activated)
            {
                return NotFound(new { Message = $"User {id} not found" });
            }

            return NoContent();
        }

        /// <summary>
        /// Delete a user permanently
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            _logger.LogWarning($"Permanently deleting user {id}");

            var deleted = await _userService.DeleteUserAsync(id);

            if (!deleted)
            {
                return NotFound(new { Message = $"User {id} not found" });
            }

            return NoContent();
        }
    }
}
