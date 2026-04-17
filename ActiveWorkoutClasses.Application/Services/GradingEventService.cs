using System.Text.Json;
using ActiveWorkoutClasses.Application.DTOs.Grading;
using ActiveWorkoutClasses.Application.DTOs.Locations;
using ActiveWorkoutClasses.Application.Interfaces;
using ActiveWorkoutClasses.Domain.Entities;
using ActiveWorkoutClasses.Domain.Enums;
using ActiveWorkoutClasses.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ActiveWorkoutClasses.Application.Services
{
    public class GradingEventService : IGradingEventService
    {
        private readonly ApplicationDbContext _context;

        public GradingEventService(ApplicationDbContext context) => _context = context;

        public async Task<List<GradingEventDto>> GetAllAsync()
        {
            var events = await _context.GradingEvents
                .Include(e => e.Location)
                .Include(e => e.EligibilityRules)
                .Include(e => e.Results)
                .OrderByDescending(e => e.EventDate)
                .ToListAsync();
            return events.Select(MapToDto).ToList();
        }

        public async Task<GradingEventDto?> GetByIdAsync(int id)
        {
            var evt = await _context.GradingEvents
                .Include(e => e.Location)
                .Include(e => e.EligibilityRules)
                .Include(e => e.Results)
                .FirstOrDefaultAsync(e => e.Id == id);
            return evt is null ? null : MapToDto(evt);
        }

        public async Task<GradingEventDto> CreateAsync(CreateGradingEventDto dto)
        {
            var evt = new GradingEvent
            {
                Title = dto.Title,
                Description = dto.Description,
                EventDate = dto.EventDate,
                LocationId = dto.LocationId,
                ClassType = dto.ClassType,
                IsPublished = false,
                ResultsPublished = false,
                CreatedAt = DateTime.UtcNow
            };

            foreach (var rule in dto.EligibilityRules)
            {
                evt.EligibilityRules.Add(new EligibilityRule
                {
                    RuleType = rule.RuleType,
                    IntValue = rule.IntValue,
                    DaysValue = rule.DaysValue,
                    Description = rule.Description
                });
            }

            _context.GradingEvents.Add(evt);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(evt.Id) ?? MapToDto(evt);
        }

        public async Task<GradingEventDto?> UpdateAsync(int id, UpdateGradingEventDto dto)
        {
            var evt = await _context.GradingEvents.FindAsync(id);
            if (evt is null) return null;

            evt.Title = dto.Title;
            evt.Description = dto.Description;
            evt.EventDate = dto.EventDate;
            evt.LocationId = dto.LocationId;
            await _context.SaveChangesAsync();
            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var evt = await _context.GradingEvents.FindAsync(id);
            if (evt is null) return false;
            _context.GradingEvents.Remove(evt);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<GradingEventDto?> PublishAsync(int id)
        {
            var evt = await _context.GradingEvents.FindAsync(id);
            if (evt is null) return null;
            evt.IsPublished = true;
            await _context.SaveChangesAsync();
            return await GetByIdAsync(id);
        }

        public async Task<List<EligibilityCheckResultDto>> RunEligibilityCheckAsync(int gradingEventId)
        {
            var evt = await _context.GradingEvents
                .Include(e => e.EligibilityRules)
                .FirstOrDefaultAsync(e => e.Id == gradingEventId)
                ?? throw new InvalidOperationException("Grading event not found.");

            var students = await _context.Users
                .OfType<Student>()
                .Where(s => s.IsActive)
                .ToListAsync();

            var results = new List<EligibilityCheckResultDto>();

            foreach (var student in students)
            {
                var checkResult = await EvaluateStudentAsync(student, evt);
                results.Add(checkResult);

                // Upsert GradingResult
                var existing = await _context.GradingResults
                    .FirstOrDefaultAsync(r => r.GradingEventId == gradingEventId && r.StudentId == student.Id);

                if (existing is null)
                {
                    _context.GradingResults.Add(new GradingResult
                    {
                        GradingEventId = gradingEventId,
                        StudentId = student.Id,
                        IsEligible = checkResult.IsEligible,
                        EligibilityNotes = JsonSerializer.Serialize(
                            checkResult.FailedRules.Concat(checkResult.PassedRules).ToList())
                    });
                }
                else
                {
                    existing.IsEligible = checkResult.IsEligible;
                    existing.EligibilityNotes = JsonSerializer.Serialize(
                        checkResult.FailedRules.Concat(checkResult.PassedRules).ToList());
                    existing.IsOverriddenEligible = null; // reset overrides on re-run
                }
            }

            await _context.SaveChangesAsync();
            return results;
        }

        public async Task<List<GradingCandidateDto>> GetCandidatesAsync(int gradingEventId)
        {
            var results = await _context.GradingResults
                .Include(r => r.Student)
                .Where(r => r.GradingEventId == gradingEventId)
                .ToListAsync();

            return results.Select(r =>
            {
                var notes = string.IsNullOrEmpty(r.EligibilityNotes)
                    ? []
                    : JsonSerializer.Deserialize<List<string>>(r.EligibilityNotes) ?? [];

                return new GradingCandidateDto
                {
                    StudentId = r.StudentId,
                    StudentNumber = r.Student.StudentNumber,
                    FullName = $"{r.Student.FirstName} {r.Student.LastName}",
                    IsEligible = r.IsEligible,
                    IsOverridden = r.IsOverriddenEligible,
                    EffectiveEligibility = r.IsOverriddenEligible ?? r.IsEligible,
                    EligibilityNotes = notes,
                    Status = r.Status
                };
            }).ToList();
        }

        public async Task<bool> OverrideEligibilityAsync(int gradingEventId, Guid studentId, OverrideEligibilityDto dto, Guid performedByUserId)
        {
            var result = await _context.GradingResults
                .FirstOrDefaultAsync(r => r.GradingEventId == gradingEventId && r.StudentId == studentId);
            if (result is null) return false;

            result.IsOverriddenEligible = dto.IsEligible;

            _context.AuditLogs.Add(new AuditLog
            {
                EntityType = "GradingResult",
                EntityId = result.Id.ToString(),
                Action = "OverrideEligibility",
                NewValue = JsonSerializer.Serialize(new { dto.IsEligible }),
                PerformedByUserId = performedByUserId,
                PerformedAt = DateTime.UtcNow,
                Reason = dto.Reason
            });

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RecordResultAsync(int gradingEventId, RecordGradingResultDto dto)
        {
            var result = await _context.GradingResults
                .FirstOrDefaultAsync(r => r.GradingEventId == gradingEventId && r.StudentId == dto.StudentId);
            if (result is null) return false;

            result.Status = dto.Status;
            result.NewSkillLevel = dto.NewSkillLevel;
            result.InstructorNotes = dto.InstructorNotes;

            if (dto.Status == GradingResultStatus.Pass && !string.IsNullOrEmpty(dto.NewSkillLevel))
            {
                var evt = await _context.GradingEvents.FindAsync(gradingEventId);
                if (evt is not null)
                {
                    _context.ProgressRecords.Add(new ProgressRecord
                    {
                        StudentId = dto.StudentId,
                        Discipline = evt.ClassType,
                        SkillLevel = dto.NewSkillLevel,
                        PromotionDate = DateTime.UtcNow,
                        Notes = dto.InstructorNotes,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<GradingEventDto?> PublishResultsAsync(int id)
        {
            var evt = await _context.GradingEvents.FindAsync(id);
            if (evt is null) return null;
            evt.ResultsPublished = true;
            await _context.SaveChangesAsync();
            return await GetByIdAsync(id);
        }

        public async Task<GradingCandidateDto?> GetStudentResultAsync(int gradingEventId, Guid studentId)
        {
            var result = await _context.GradingResults
                .Include(r => r.Student)
                .FirstOrDefaultAsync(r => r.GradingEventId == gradingEventId && r.StudentId == studentId);
            if (result is null) return null;

            var notes = string.IsNullOrEmpty(result.EligibilityNotes)
                ? []
                : JsonSerializer.Deserialize<List<string>>(result.EligibilityNotes) ?? [];

            return new GradingCandidateDto
            {
                StudentId = result.StudentId,
                StudentNumber = result.Student.StudentNumber,
                FullName = $"{result.Student.FirstName} {result.Student.LastName}",
                IsEligible = result.IsEligible,
                IsOverridden = result.IsOverriddenEligible,
                EffectiveEligibility = result.IsOverriddenEligible ?? result.IsEligible,
                EligibilityNotes = notes,
                Status = result.Status
            };
        }

        // ── Eligibility Engine ────────────────────────────────────────────────

        private async Task<EligibilityCheckResultDto> EvaluateStudentAsync(Student student, GradingEvent evt)
        {
            var passed = new List<string>();
            var failed = new List<string>();

            foreach (var rule in evt.EligibilityRules)
            {
                var (ok, message) = await EvaluateRuleAsync(student, evt, rule);
                if (ok) passed.Add(message);
                else failed.Add(message);
            }

            return new EligibilityCheckResultDto
            {
                StudentId = student.Id,
                StudentNumber = student.StudentNumber,
                FullName = $"{student.FirstName} {student.LastName}",
                IsEligible = failed.Count == 0,
                FailedRules = failed,
                PassedRules = passed
            };
        }

        private async Task<(bool ok, string message)> EvaluateRuleAsync(Student student, GradingEvent evt, EligibilityRule rule)
        {
            switch (rule.RuleType)
            {
                case EligibilityRuleType.MinAttendanceCount:
                {
                    var count = await _context.Attendances
                        .Include(a => a.ClassRegistration)
                        .Where(a => a.ClassRegistration.StudentId == student.Id && a.IsPresent)
                        .CountAsync();
                    int required = rule.IntValue ?? 0;
                    bool ok = count >= required;
                    return (ok, $"MinAttendance: {count}/{required} classes attended — {(ok ? "PASS" : "FAIL")}");
                }
                case EligibilityRuleType.MinDaysSinceRegistration:
                {
                    int days = (int)(DateTime.UtcNow - student.MembershipStartDate).TotalDays;
                    int required = rule.DaysValue ?? 0;
                    bool ok = days >= required;
                    return (ok, $"MinDaysSinceRegistration: {days}/{required} days — {(ok ? "PASS" : "FAIL")}");
                }
                case EligibilityRuleType.MinDaysSinceLastGrading:
                {
                    var lastGrading = await _context.GradingResults
                        .Include(r => r.GradingEvent)
                        .Where(r => r.StudentId == student.Id && r.Status == GradingResultStatus.Pass)
                        .OrderByDescending(r => r.GradingEvent.EventDate)
                        .FirstOrDefaultAsync();

                    if (lastGrading is null)
                        return (true, "MinDaysSinceLastGrading: No prior grading — PASS (first grading)");

                    int days = (int)(DateTime.UtcNow - lastGrading.GradingEvent.EventDate).TotalDays;
                    int required = rule.DaysValue ?? 0;
                    bool ok = days >= required;
                    return (ok, $"MinDaysSinceLastGrading: {days}/{required} days — {(ok ? "PASS" : "FAIL")}");
                }
                case EligibilityRuleType.MinClassesOfType:
                {
                    var count = await _context.Attendances
                        .Include(a => a.ClassRegistration)
                            .ThenInclude(r => r.WorkoutClass)
                        .Where(a => a.ClassRegistration.StudentId == student.Id
                                 && a.IsPresent
                                 && a.ClassRegistration.WorkoutClass.ClassType == evt.ClassType)
                        .CountAsync();
                    int required = rule.IntValue ?? 0;
                    bool ok = count >= required;
                    return (ok, $"MinClassesOfType ({evt.ClassType}): {count}/{required} — {(ok ? "PASS" : "FAIL")}");
                }
                default:
                    return (true, $"Unknown rule type {rule.RuleType} — skipped");
            }
        }

        private static GradingEventDto MapToDto(GradingEvent e) => new()
        {
            Id = e.Id,
            Title = e.Title,
            Description = e.Description,
            EventDate = e.EventDate,
            ClassType = e.ClassType,
            ClassTypeName = e.ClassType.ToString(),
            IsPublished = e.IsPublished,
            ResultsPublished = e.ResultsPublished,
            Location = e.Location is null ? null : new LocationDto
            {
                Id = e.Location.Id,
                Name = e.Location.Name,
                Address = e.Location.Address,
                City = e.Location.City,
                Capacity = e.Location.Capacity,
                IsActive = e.Location.IsActive
            },
            EligibilityRules = e.EligibilityRules.Select(r => new EligibilityRuleDto
            {
                Id = r.Id,
                RuleType = r.RuleType,
                IntValue = r.IntValue,
                DaysValue = r.DaysValue,
                Description = r.Description
            }).ToList(),
            CandidateCount = e.Results?.Count ?? 0
        };
    }
}
