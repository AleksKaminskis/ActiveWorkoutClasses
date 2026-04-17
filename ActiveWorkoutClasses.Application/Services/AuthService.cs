using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.Text;
using ActiveWorkoutClasses.Application.DTOs.Auth;
using ActiveWorkoutClasses.Application.Interfaces;
using ActiveWorkoutClasses.Domain.Entities;
using ActiveWorkoutClasses.Domain.Enums;
using ActiveWorkoutClasses.Infrastructure.Data;
using ActiveWorkoutClasses.Infrastructure.Security;
using ActiveWorkoutClasses.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ActiveWorkoutClasses.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher _hasher;
        private readonly StudentNumberGenerator _studentNumberGenerator;
        private readonly IConfiguration _config;

        public AuthService(
            ApplicationDbContext context,
            IPasswordHasher hasher,
            StudentNumberGenerator studentNumberGenerator,
            IConfiguration config)
        {
            _context = context;
            _hasher = hasher;
            _studentNumberGenerator = studentNumberGenerator;
            _config = config;
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email.ToLowerInvariant() && u.IsActive);

            if (user is null || !_hasher.VerifyPassword(dto.Password, user.PasswordHash))
                return null;

            var token = GenerateJwt(user);
            var expiryDays = int.TryParse(_config["Jwt:ExpiryDays"], out var days) ? days : 7;

            return new AuthResponseDto
            {
                AccessToken = token,
                TokenType = "Bearer",
                ExpiresIn = expiryDays * 86400,
                User = MapToUserDto(user)
            };
        }

        public async Task<Guid> RegisterStudentAsync(RegisterRequestDto dto)
        {
            var emailLower = dto.Email.ToLowerInvariant();
            if (await _context.Users.AnyAsync(u => u.Email == emailLower))
                throw new InvalidOperationException("Email is already registered.");

            var studentNumber = await _studentNumberGenerator.GenerateAsync();

            var student = new Student
            {
                Id = Guid.NewGuid(),
                Email = emailLower,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PhoneNumber = dto.PhoneNumber,
                Role = UserRole.Student,
                IsActive = true,
                PasswordHash = _hasher.HashPassword(dto.Password),
                StudentNumber = studentNumber,
                EmergencyContact = dto.EmergencyContact,
                MedicalNotes = dto.MedicalNotes,
                MembershipStartDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(student);
            await _context.SaveChangesAsync();
            return student.Id;
        }

        public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user is null) return false;

            if (!_hasher.VerifyPassword(dto.CurrentPassword, user.PasswordHash))
                return false;

            user.PasswordHash = _hasher.HashPassword(dto.NewPassword);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<UserDto?> GetCurrentUserAsync(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            return user is null ? null : MapToUserDto(user);
        }

        private string GenerateJwt(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiryDays = int.TryParse(_config["Jwt:ExpiryDays"], out var days) ? days : 7;

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email!),
                new(JwtRegisteredClaimNames.Name, $"{user.FirstName} {user.LastName}"),
                new(ClaimTypes.Role, user.Role.ToString())
            };

            if (user is Student student)
                claims.Add(new Claim("studentNumber", student.StudentNumber));

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(expiryDays),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static UserDto MapToUserDto(User user) => new()
        {
            Id = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = $"{user.FirstName} {user.LastName}",
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            Role = user.Role,
            IsActive = user.IsActive
        };
    }
}
