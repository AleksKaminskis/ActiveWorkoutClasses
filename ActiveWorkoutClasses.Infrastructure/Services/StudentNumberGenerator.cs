using ActiveWorkoutClasses.Domain.Entities;
using ActiveWorkoutClasses.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ActiveWorkoutClasses.Infrastructure.Services
{
    /// <summary>
    /// Generates unique, human-readable student identifiers in the format STU-YYYY-NNN.
    /// Example: STU-2025-047
    /// </summary>
    public class StudentNumberGenerator
    {
        private readonly ApplicationDbContext _context;

        public StudentNumberGenerator(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Generates the next available student number for the current year.
        /// Thread-safe only within a single request — the caller should handle
        /// unique constraint retries if concurrent registrations race.
        /// </summary>
        public async Task<string> GenerateAsync()
        {
            var year = DateTime.UtcNow.Year;
            var prefix = $"STU-{year}-";

            var count = await _context.Users
                .OfType<Student>()
                .CountAsync(s => s.StudentNumber.StartsWith(prefix));

            return $"{prefix}{(count + 1):D3}";
        }
    }
}
