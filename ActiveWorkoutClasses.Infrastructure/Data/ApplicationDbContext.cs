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

            // Configure WorkoutClass
            modelBuilder.Entity<WorkoutClass>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Description)
                    .HasMaxLength(1000);

                entity.Property(e => e.Location)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasIndex(e => e.StartDateTime);
                entity.HasIndex(e => e.ClassType);
            });

            // Configure ClassInstructor (many-to-many join table)
            modelBuilder.Entity<ClassInstructor>(entity =>
            {
                entity.HasKey(ci => new { ci.WorkoutClassId, ci.InstructorId });

                entity.HasOne(ci => ci.WorkoutClass)
                    .WithMany(wc => wc.ClassInstructors)
                    .HasForeignKey(ci => ci.WorkoutClassId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ci => ci.Instructor)
                    .WithMany(i => i.ClassInstructors)
                    .HasForeignKey(ci => ci.InstructorId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure ClassRegistration
            modelBuilder.Entity<ClassRegistration>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(cr => cr.Student)
                    .WithMany(s => s.ClassRegistrations)
                    .HasForeignKey(cr => cr.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(cr => cr.WorkoutClass)
                    .WithMany(wc => wc.ClassRegistrations)
                    .HasForeignKey(cr => cr.WorkoutClassId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Prevent duplicate registrations
                entity.HasIndex(cr => new { cr.StudentId, cr.WorkoutClassId })
                    .IsUnique();
            });

            // Configure Attendance
            modelBuilder.Entity<Attendance>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(a => a.ClassRegistration)
                    .WithOne(cr => cr.Attendance)
                    .HasForeignKey<Attendance>(a => a.ClassRegistrationId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.MarkedByInstructor)
                    .WithMany()
                    .HasForeignKey(a => a.MarkedByInstructorId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.Property(a => a.Notes)
                    .HasMaxLength(500);
            });

            // Configure Student
            modelBuilder.Entity<Student>(entity =>
            {
                entity.Property(s => s.EmergencyContact)
                    .HasMaxLength(200);

                entity.Property(s => s.MedicalNotes)
                    .HasMaxLength(1000);
            });

            // Configure Instructor
            modelBuilder.Entity<Instructor>(entity =>
            {
                entity.Property(i => i.Specialization)
                    .HasMaxLength(200);

                entity.Property(i => i.Bio)
                    .HasMaxLength(2000);

                entity.Property(i => i.Certifications)
                    .HasMaxLength(1000);

                entity.Property(i => i.HourlyRate)
                    .HasColumnType("decimal(18,2)");
            });
        }
    }
}
