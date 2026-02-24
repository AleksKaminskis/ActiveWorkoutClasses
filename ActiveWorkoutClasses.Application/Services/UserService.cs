using ActiveWorkoutClasses.Application.DTOs.Users;
using ActiveWorkoutClasses.Application.Interfaces;
using ActiveWorkoutClasses.Domain.Entities;
using ActiveWorkoutClasses.Domain.Enums;
using ActiveWorkoutClasses.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ActiveWorkoutClasses.Application.Services
{
    /// <summary>
    /// Service for user management business logic
    /// </summary>
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            var users = await _context.Users
                .Where(u => u.IsActive)
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToListAsync();

            return users.Select(MapToUserDto).ToList();
        }

        public async Task<List<UserDto>> GetUsersByRoleAsync(UserRole role)
        {
            var users = await _context.Users
                .Where(u => u.Role == role && u.IsActive)
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToListAsync();

            return users.Select(MapToUserDto).ToList();
        }

        public async Task<List<UserDetailDto>> GetAllStudentsAsync()
        {
            var students = await _context.Students
                .Where(s => s.IsActive)
                .OrderBy(s => s.LastName)
                .ThenBy(s => s.FirstName)
                .ToListAsync();

            return students.Select(MapToDetailDto).ToList();
        }

        public async Task<List<UserDetailDto>> GetAllInstructorsAsync()
        {
            var instructors = await _context.Instructors
                .Where(i => i.IsActive)
                .OrderBy(i => i.LastName)
                .ThenBy(i => i.FirstName)
                .ToListAsync();

            return instructors.Select(MapToDetailDto).ToList();
        }

        public async Task<UserDetailDto?> GetUserByIdAsync(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            return user == null ? null : MapToDetailDto(user);
        }

        public async Task<UserDto?> GetUserByEmailAsync(string email)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

            return user == null ? null : MapToUserDto(user);
        }

        public async Task<UserListDto> GetUsersPagedAsync(int page, int pageSize, UserRole? role = null)
        {
            var query = _context.Users.Where(u => u.IsActive);

            if (role.HasValue)
            {
                query = query.Where(u => u.Role == role.Value);
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var users = await query
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new UserListDto
            {
                Users = users.Select(MapToUserDto).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }

        public async Task<UserDetailDto> CreateUserAsync(CreateUserDto createDto)
        {
            // Check if email already exists
            if (await EmailExistsAsync(createDto.Email))
            {
                throw new InvalidOperationException($"User with email {createDto.Email} already exists");
            }

            User user;
            var now = DateTime.UtcNow;

            // Create appropriate user type based on role
            switch (createDto.Role)
            {
                case UserRole.Student:
                    if (string.IsNullOrWhiteSpace(createDto.EmergencyContact))
                    {
                        throw new ArgumentException("Emergency contact is required for students");
                    }

                    var student = new Student
                    {
                        Id = Guid.NewGuid(),
                        Email = createDto.Email,
                        FirstName = createDto.FirstName,
                        LastName = createDto.LastName,
                        PhoneNumber = createDto.PhoneNumber,
                        Role = UserRole.Student,
                        IsActive = true,
                        EmergencyContact = createDto.EmergencyContact,
                        MedicalNotes = createDto.MedicalNotes,
                        MembershipStartDate = createDto.MembershipStartDate ?? now,
                        MembershipEndDate = createDto.MembershipEndDate,
                        CreatedAt = now,
                        UpdatedAt = now
                    };

                    student.Validate();
                    user = student;
                    break;

                case UserRole.Instructor:
                    if (string.IsNullOrWhiteSpace(createDto.Specialization))
                    {
                        throw new ArgumentException("Specialization is required for instructors");
                    }
                    if (string.IsNullOrWhiteSpace(createDto.Bio))
                    {
                        throw new ArgumentException("Bio is required for instructors");
                    }
                    if (string.IsNullOrWhiteSpace(createDto.Certifications))
                    {
                        throw new ArgumentException("Certifications are required for instructors");
                    }

                    var instructor = new Instructor
                    {
                        Id = Guid.NewGuid(),
                        Email = createDto.Email,
                        FirstName = createDto.FirstName,
                        LastName = createDto.LastName,
                        PhoneNumber = createDto.PhoneNumber,
                        Role = UserRole.Instructor,
                        IsActive = true,
                        Specialization = createDto.Specialization,
                        Bio = createDto.Bio,
                        YearsOfExperience = createDto.YearsOfExperience ?? 0,
                        Certifications = createDto.Certifications,
                        HourlyRate = createDto.HourlyRate,
                        CreatedAt = now,
                        UpdatedAt = now
                    };

                    instructor.Validate();
                    user = instructor;
                    break;

                case UserRole.Admin:
                    user = new User
                    {
                        Id = Guid.NewGuid(),
                        Email = createDto.Email,
                        FirstName = createDto.FirstName,
                        LastName = createDto.LastName,
                        PhoneNumber = createDto.PhoneNumber,
                        Role = UserRole.Admin,
                        IsActive = true,
                        CreatedAt = now,
                        UpdatedAt = now
                    };

                    user.Validate();
                    break;

                default:
                    throw new ArgumentException("Invalid user role");
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return MapToDetailDto(user);
        }

        public async Task<UserDetailDto> UpdateUserAsync(Guid id, UpdateUserDto updateDto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {id} not found");
            }

            // Update common properties
            user.FirstName = updateDto.FirstName;
            user.LastName = updateDto.LastName;
            user.PhoneNumber = updateDto.PhoneNumber;
            user.IsActive = updateDto.IsActive;
            user.UpdatedAt = DateTime.UtcNow;

            // Update role-specific properties
            if (user is Student student)
            {
                if (!string.IsNullOrWhiteSpace(updateDto.EmergencyContact))
                {
                    student.EmergencyContact = updateDto.EmergencyContact;
                }
                student.MedicalNotes = updateDto.MedicalNotes;
                student.MembershipEndDate = updateDto.MembershipEndDate;

                student.Validate();
            }
            else if (user is Instructor instructor)
            {
                if (!string.IsNullOrWhiteSpace(updateDto.Specialization))
                {
                    instructor.Specialization = updateDto.Specialization;
                }
                if (!string.IsNullOrWhiteSpace(updateDto.Bio))
                {
                    instructor.Bio = updateDto.Bio;
                }
                if (updateDto.YearsOfExperience.HasValue)
                {
                    instructor.YearsOfExperience = updateDto.YearsOfExperience.Value;
                }
                if (!string.IsNullOrWhiteSpace(updateDto.Certifications))
                {
                    instructor.Certifications = updateDto.Certifications;
                }
                instructor.HourlyRate = updateDto.HourlyRate;

                instructor.Validate();
            }
            else
            {
                user.Validate();
            }

            await _context.SaveChangesAsync();

            return MapToDetailDto(user);
        }

        public async Task<bool> DeactivateUserAsync(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return false;
            }

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ActivateUserAsync(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return false;
            }

            user.IsActive = true;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteUserAsync(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return false;
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UserExistsAsync(Guid id)
        {
            return await _context.Users.AnyAsync(u => u.Id == id);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }

        #region Private Mapping Methods

        private UserDto MapToUserDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                RoleName = user.Role.ToString(),
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }

        private UserDetailDto MapToDetailDto(User user)
        {
            var dto = new UserDetailDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                RoleName = user.Role.ToString(),
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };

            // Add role-specific data
            if (user is Student student)
            {
                dto.MembershipStartDate = student.MembershipStartDate;
                dto.MembershipEndDate = student.MembershipEndDate;
                dto.EmergencyContact = student.EmergencyContact;
                dto.MedicalNotes = student.MedicalNotes;
                dto.HasActiveMembership = student.HasActiveMembership();
            }
            else if (user is Instructor instructor)
            {
                dto.Specialization = instructor.Specialization;
                dto.Bio = instructor.Bio;
                dto.YearsOfExperience = instructor.YearsOfExperience;
                dto.Certifications = instructor.Certifications;
                dto.HourlyRate = instructor.HourlyRate;
            }

            return dto;
        }

        #endregion
    }
}
