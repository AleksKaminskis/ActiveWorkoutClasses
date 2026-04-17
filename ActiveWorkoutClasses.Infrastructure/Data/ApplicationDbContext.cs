using ActiveWorkoutClasses.Domain.Entities;
using ActiveWorkoutClasses.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ActiveWorkoutClasses.Infrastructure.Data
{
    /// <summary>
    /// Main database context for Active Workout Classes application
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets for each entity
        public DbSet<User> Users => Set<User>();
        public DbSet<Student> Students => Set<Student>();
        public DbSet<Instructor> Instructors => Set<Instructor>();
        public DbSet<WorkoutClass> WorkoutClasses => Set<WorkoutClass>();
        public DbSet<ClassInstructor> ClassInstructors => Set<ClassInstructor>();
        public DbSet<ClassRegistration> ClassRegistrations => Set<ClassRegistration>();
        public DbSet<Attendance> Attendances => Set<Attendance>();
        public DbSet<Location> Locations => Set<Location>();
        public DbSet<RecurringSchedule> RecurringSchedules => Set<RecurringSchedule>();
        public DbSet<GradingEvent> GradingEvents => Set<GradingEvent>();
        public DbSet<EligibilityRule> EligibilityRules => Set<EligibilityRule>();
        public DbSet<GradingResult> GradingResults => Set<GradingResult>();
        public DbSet<ProgressRecord> ProgressRecords => Set<ProgressRecord>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User inheritance (TPH - Table Per Hierarchy)
            modelBuilder.Entity<User>()
                .HasDiscriminator<string>("UserType")
                .HasValue<User>("User")
                .HasValue<Student>("Student")
                .HasValue<Instructor>("Instructor");

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
            
            // Add indexes for common queries
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Role);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.IsActive);

            // Configure WorkoutClass
            modelBuilder.Entity<WorkoutClass>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Required fields with max lengths
                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(1000);

                entity.Property(e => e.LocationName)
                    .HasMaxLength(100);

                // Optional FK to the Location entity
                entity.HasOne(e => e.Location)
                    .WithMany(l => l.Classes)
                    .HasForeignKey(e => e.LocationId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Optional FK to RecurringSchedule
                entity.HasOne(e => e.RecurringSchedule)
                    .WithMany(rs => rs.GeneratedClasses)
                    .HasForeignKey(e => e.RecurringScheduleId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.Property(e => e.ClassType)
                    .IsRequired();

                entity.Property(e => e.StartDateTime)
                    .IsRequired();

                entity.Property(e => e.EndDateTime)
                    .IsRequired();

                entity.Property(e => e.MaxCapacity)
                    .IsRequired();

                // Indexes for performance
                entity.HasIndex(e => e.StartDateTime)
                    .HasDatabaseName("IX_WorkoutClass_StartDateTime");

                entity.HasIndex(e => e.ClassType)
                    .HasDatabaseName("IX_WorkoutClass_ClassType");

                entity.HasIndex(e => e.IsActive)
                    .HasDatabaseName("IX_WorkoutClass_IsActive");

                // Composite index for common query: active classes by date
                entity.HasIndex(e => new { e.IsActive, e.StartDateTime })
                    .HasDatabaseName("IX_WorkoutClass_Active_StartDate");
            });

            // Configure ClassInstructor (many-to-many join table)
            modelBuilder.Entity<ClassInstructor>(entity =>
            {
                // Composite primary key
                entity.HasKey(ci => new { ci.WorkoutClassId, ci.InstructorId });

                // Relationship to WorkoutClass
                entity.HasOne(ci => ci.WorkoutClass)
                    .WithMany(wc => wc.ClassInstructors)
                    .HasForeignKey(ci => ci.WorkoutClassId)
                    .OnDelete(DeleteBehavior.Cascade); // Delete instructors when class is deleted

                // Relationship to Instructor
                entity.HasOne(ci => ci.Instructor)
                    .WithMany(i => i.ClassInstructors)
                    .HasForeignKey(ci => ci.InstructorId)
                    .OnDelete(DeleteBehavior.Cascade); // Delete assignments when instructor is deleted

                // Index for finding all classes taught by an instructor
                entity.HasIndex(ci => ci.InstructorId)
                    .HasDatabaseName("IX_ClassInstructor_InstructorId");
            });

            // Configure ClassRegistration
            modelBuilder.Entity<ClassRegistration>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Relationship to Student
                entity.HasOne(cr => cr.Student)
                    .WithMany(s => s.ClassRegistrations)
                    .HasForeignKey(cr => cr.StudentId)
                    .OnDelete(DeleteBehavior.Cascade); // Delete registrations when student is deleted

                // Relationship to WorkoutClass
                entity.HasOne(cr => cr.WorkoutClass)
                    .WithMany(wc => wc.ClassRegistrations)
                    .HasForeignKey(cr => cr.WorkoutClassId)
                    .OnDelete(DeleteBehavior.Cascade); // Delete registrations when class is deleted

                // Prevent duplicate registrations (same student can't register twice for same class)
                entity.HasIndex(cr => new { cr.StudentId, cr.WorkoutClassId })
                    .IsUnique()
                    .HasDatabaseName("IX_ClassRegistration_Student_Class_Unique");

                // Index for finding all registrations for a class
                entity.HasIndex(cr => cr.WorkoutClassId)
                    .HasDatabaseName("IX_ClassRegistration_WorkoutClassId");

                // Index for finding all registrations by a student
                entity.HasIndex(cr => cr.StudentId)
                    .HasDatabaseName("IX_ClassRegistration_StudentId");

                // Index for filtering by status
                entity.HasIndex(cr => cr.Status)
                    .HasDatabaseName("IX_ClassRegistration_Status");
            });

            // Configure Attendance
            modelBuilder.Entity<Attendance>(entity =>
            {
                entity.HasKey(e => e.Id);

                // One-to-One relationship with ClassRegistration
                entity.HasOne(a => a.ClassRegistration)
                    .WithOne(cr => cr.Attendance)
                    .HasForeignKey<Attendance>(a => a.ClassRegistrationId)
                    .OnDelete(DeleteBehavior.Cascade); // Delete attendance when registration is deleted

                // Relationship to Instructor (who marked the attendance)
                entity.HasOne(a => a.MarkedByInstructor)
                    .WithMany()
                    .HasForeignKey(a => a.MarkedByInstructorId)
                    .OnDelete(DeleteBehavior.NoAction); // Don't delete attendance if instructor is deleted

                entity.Property(a => a.Notes)
                    .HasMaxLength(500);

                entity.Property(a => a.CheckInMethod)
                    .IsRequired();

                entity.Property(a => a.IsPresent)
                    .IsRequired();

                // Index for finding attendance records by registration
                entity.HasIndex(a => a.ClassRegistrationId)
                    .IsUnique() // Each registration can have only one attendance record
                    .HasDatabaseName("IX_Attendance_ClassRegistrationId");
            });

            // Configure Student
            modelBuilder.Entity<Student>(entity =>
            {
                entity.Property(s => s.EmergencyContact)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(s => s.MedicalNotes)
                    .HasMaxLength(1000);

                entity.Property(s => s.MembershipStartDate)
                    .IsRequired();

                entity.Property(s => s.StudentNumber)
                    .HasMaxLength(20);

                entity.HasIndex(s => s.StudentNumber)
                    .IsUnique()
                    .HasFilter("[StudentNumber] IS NOT NULL AND [StudentNumber] != ''")
                    .HasDatabaseName("IX_Student_StudentNumber");
            });

            // Configure Instructor
            modelBuilder.Entity<Instructor>(entity =>
            {
                entity.Property(i => i.Specialization)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(i => i.Bio)
                    .IsRequired()
                    .HasMaxLength(2000);

                entity.Property(i => i.YearsOfExperience)
                    .IsRequired();

                entity.Property(i => i.Certifications)
                    .IsRequired()
                    .HasMaxLength(1000);

                // Decimal precision for hourly rate
                entity.Property(i => i.HourlyRate)
                    .HasColumnType("decimal(18,2)");
            });

            // Configure Location
            modelBuilder.Entity<Location>(entity =>
            {
                entity.HasKey(l => l.Id);
                entity.Property(l => l.Name).IsRequired().HasMaxLength(100);
                entity.Property(l => l.Address).IsRequired().HasMaxLength(200);
                entity.Property(l => l.City).IsRequired().HasMaxLength(100);
                entity.Property(l => l.Notes).HasMaxLength(500);
            });

            // Configure RecurringSchedule
            modelBuilder.Entity<RecurringSchedule>(entity =>
            {
                entity.HasKey(rs => rs.Id);
                entity.Property(rs => rs.Title).IsRequired().HasMaxLength(200);
                entity.Property(rs => rs.Description).HasMaxLength(1000);

                entity.HasOne(rs => rs.Location)
                    .WithMany(l => l.Schedules)
                    .HasForeignKey(rs => rs.LocationId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure GradingEvent
            modelBuilder.Entity<GradingEvent>(entity =>
            {
                entity.HasKey(ge => ge.Id);
                entity.Property(ge => ge.Title).IsRequired().HasMaxLength(200);
                entity.Property(ge => ge.Description).HasMaxLength(1000);

                entity.HasOne(ge => ge.Location)
                    .WithMany(l => l.GradingEvents)
                    .HasForeignKey(ge => ge.LocationId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasMany(ge => ge.EligibilityRules)
                    .WithOne(er => er.GradingEvent)
                    .HasForeignKey(er => er.GradingEventId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(ge => ge.Results)
                    .WithOne(gr => gr.GradingEvent)
                    .HasForeignKey(gr => gr.GradingEventId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure GradingResult
            modelBuilder.Entity<GradingResult>(entity =>
            {
                entity.HasKey(gr => gr.Id);

                entity.HasOne(gr => gr.Student)
                    .WithMany(s => s.GradingResults)
                    .HasForeignKey(gr => gr.StudentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(gr => new { gr.GradingEventId, gr.StudentId })
                    .IsUnique()
                    .HasDatabaseName("IX_GradingResult_Event_Student_Unique");

                entity.Property(gr => gr.EligibilityNotes).HasMaxLength(2000);
                entity.Property(gr => gr.NewSkillLevel).HasMaxLength(100);
                entity.Property(gr => gr.InstructorNotes).HasMaxLength(1000);
            });

            // Configure ProgressRecord
            modelBuilder.Entity<ProgressRecord>(entity =>
            {
                entity.HasKey(pr => pr.Id);

                entity.HasOne(pr => pr.Student)
                    .WithMany(s => s.ProgressRecords)
                    .HasForeignKey(pr => pr.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pr => pr.RecordedByInstructor)
                    .WithMany()
                    .HasForeignKey(pr => pr.RecordedByInstructorId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasIndex(pr => new { pr.StudentId, pr.Discipline })
                    .HasDatabaseName("IX_ProgressRecord_Student_Discipline");

                entity.Property(pr => pr.SkillLevel).IsRequired().HasMaxLength(100);
                entity.Property(pr => pr.Notes).HasMaxLength(1000);
            });

            // Configure AuditLog
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(al => al.Id);
                entity.Property(al => al.EntityType).IsRequired().HasMaxLength(100);
                entity.Property(al => al.EntityId).IsRequired().HasMaxLength(100);
                entity.Property(al => al.Action).IsRequired().HasMaxLength(100);
                entity.Property(al => al.Reason).HasMaxLength(500);

                entity.HasIndex(al => new { al.EntityType, al.EntityId })
                    .HasDatabaseName("IX_AuditLog_Entity");
                entity.HasIndex(al => al.PerformedAt)
                    .HasDatabaseName("IX_AuditLog_PerformedAt");
            });
        }

        /// <summary>
        /// Override SaveChanges to automatically update UpdatedAt timestamps
        /// </summary>
        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        /// <summary>
        /// Override SaveChangesAsync to automatically update UpdatedAt timestamps
        /// </summary>
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Automatically update UpdatedAt for modified entities
        /// </summary>
        private void UpdateTimestamps()
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.Entity is User user)
                {
                    user.UpdatedAt = DateTime.UtcNow;
                }
                else if (entry.Entity is WorkoutClass workoutClass)
                {
                    workoutClass.UpdatedAt = DateTime.UtcNow;
                }
                else if (entry.Entity is Attendance attendance)
                {
                    attendance.UpdatedAt = DateTime.UtcNow;
                }
            }
        }
    }
}
