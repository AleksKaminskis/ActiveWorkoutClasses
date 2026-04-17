using ActiveWorkoutClasses.Application.DTOs.Progress;
using ActiveWorkoutClasses.Application.Interfaces;
using ActiveWorkoutClasses.Domain.Entities;
using ActiveWorkoutClasses.Domain.Enums;
using ActiveWorkoutClasses.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ActiveWorkoutClasses.Application.Services
{
    public class ProgressService : IProgressService
    {
        private readonly ApplicationDbContext _context;

        public ProgressService(ApplicationDbContext context) => _context = context;

        public async Task<StudentProgressSummaryDto?> GetStudentProgressAsync(Guid studentId)
        {
            var student = await _context.Users.OfType<Student>().FirstOrDefaultAsync(s => s.Id == studentId);
            if (student is null) return null;

            var records = await GetRecords(studentId);
            var byDiscipline = records
                .GroupBy(r => r.DisciplineName)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(r => r.PromotionDate).First());

            return new StudentProgressSummaryDto
            {
                StudentId = studentId,
                FullName = $"{student.FirstName} {student.LastName}",
                StudentNumber = student.StudentNumber,
                AllRecords = records,
                CurrentLevelByDiscipline = byDiscipline
            };
        }

        public async Task<ProgressRecordDto?> GetCurrentLevelAsync(Guid studentId, ClassType discipline)
        {
            var record = await _context.ProgressRecords
                .Include(r => r.RecordedByInstructor)
                .Where(r => r.StudentId == studentId && r.Discipline == discipline)
                .OrderByDescending(r => r.PromotionDate)
                .FirstOrDefaultAsync();
            return record is null ? null : MapToDto(record);
        }

        public async Task<List<ProgressRecordDto>> GetHistoryAsync(Guid studentId)
            => await GetRecords(studentId);

        public async Task<ProgressRecordDto> CreateAsync(CreateProgressRecordDto dto, Guid recordedByInstructorId)
        {
            var record = new ProgressRecord
            {
                StudentId = dto.StudentId,
                RecordedByInstructorId = recordedByInstructorId,
                Discipline = dto.Discipline,
                SkillLevel = dto.SkillLevel,
                PromotionDate = dto.PromotionDate,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow
            };
            _context.ProgressRecords.Add(record);
            await _context.SaveChangesAsync();

            await _context.Entry(record).Reference(r => r.RecordedByInstructor).LoadAsync();
            return MapToDto(record);
        }

        private async Task<List<ProgressRecordDto>> GetRecords(Guid studentId)
        {
            var records = await _context.ProgressRecords
                .Include(r => r.RecordedByInstructor)
                .Where(r => r.StudentId == studentId)
                .OrderByDescending(r => r.PromotionDate)
                .ToListAsync();
            return records.Select(MapToDto).ToList();
        }

        private static ProgressRecordDto MapToDto(ProgressRecord r) => new()
        {
            Id = r.Id,
            Discipline = r.Discipline,
            DisciplineName = r.Discipline.ToString(),
            SkillLevel = r.SkillLevel,
            PromotionDate = r.PromotionDate,
            Notes = r.Notes,
            RecordedByInstructor = r.RecordedByInstructor is null
                ? null
                : $"{r.RecordedByInstructor.FirstName} {r.RecordedByInstructor.LastName}"
        };
    }
}
