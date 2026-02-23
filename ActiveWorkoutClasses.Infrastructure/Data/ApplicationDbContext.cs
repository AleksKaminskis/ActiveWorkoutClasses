using ActiveWorkoutClasses.Domain.Entities;
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

                entity.Property(e => e.Location)
                    .IsRequired()
                    .HasMaxLength(100);

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
