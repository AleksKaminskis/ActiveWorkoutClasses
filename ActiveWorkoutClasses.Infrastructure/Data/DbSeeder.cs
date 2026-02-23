using ActiveWorkoutClasses.Domain.Entities;
using ActiveWorkoutClasses.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ActiveWorkoutClasses.Infrastructure.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Only seed if database is empty (check if any users exist)
            var hasUsers = await context.Users.AnyAsync();
            if (hasUsers)
            {
                return; // Database already seeded
            }

            // ========================================
            // 1. Create Admin User
            // ========================================
            var admin = new User
            {
                Id = Guid.NewGuid(),
                Email = "admin@activeworkout.com",
                FirstName = "Admin",
                LastName = "User",
                PhoneNumber = "+353871234567",
                Role = UserRole.Admin,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // ========================================
            // 2. Create Sample Instructors
            // ========================================
            var instructor1 = new Instructor
            {
                Id = Guid.NewGuid(),
                Email = "sarah.kravmaga@activeworkout.com",
                FirstName = "Sarah",
                LastName = "Cohen",
                PhoneNumber = "+353871234501",
                Role = UserRole.Instructor,
                IsActive = true,
                Specialization = "Krav Maga & Self-Defense",
                Bio = "Former IDF instructor with 12 years of experience teaching Krav Maga. Passionate about empowering students through practical self-defense techniques.",
                YearsOfExperience = 12,
                Certifications = "Krav Maga Global Instructor Level 3, First Aid Certified",
                HourlyRate = 45.00m,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var instructor2 = new Instructor
            {
                Id = Guid.NewGuid(),
                Email = "mike.boxing@activeworkout.com",
                FirstName = "Mike",
                LastName = "Murphy",
                PhoneNumber = "+353871234502",
                Role = UserRole.Instructor,
                IsActive = true,
                Specialization = "Boxing & HIIT",
                Bio = "Professional boxer turned fitness instructor. Specializes in high-intensity boxing workouts that build strength and endurance.",
                YearsOfExperience = 8,
                Certifications = "NASM CPT, Boxing Coach Level 2",
                HourlyRate = 40.00m,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var instructor3 = new Instructor
            {
                Id = Guid.NewGuid(),
                Email = "emma.yoga@activeworkout.com",
                FirstName = "Emma",
                LastName = "O'Brien",
                PhoneNumber = "+353871234503",
                Role = UserRole.Instructor,
                IsActive = true,
                Specialization = "Yoga & Pilates",
                Bio = "Certified yoga and Pilates instructor focused on mindfulness and core strength. Creates a welcoming environment for all fitness levels.",
                YearsOfExperience = 6,
                Certifications = "RYT 500, Pilates Mat Certification",
                HourlyRate = 35.00m,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // ========================================
            // 3. Create Sample Students
            // ========================================
            var student1 = new Student
            {
                Id = Guid.NewGuid(),
                Email = "john.student@email.com",
                FirstName = "John",
                LastName = "Doe",
                PhoneNumber = "+353871234601",
                Role = UserRole.Student,
                IsActive = true,
                MembershipStartDate = DateTime.UtcNow.AddMonths(-3),
                MembershipEndDate = DateTime.UtcNow.AddMonths(9),
                EmergencyContact = "Jane Doe - Wife - +353871234602",
                MedicalNotes = "No known medical conditions",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var student2 = new Student
            {
                Id = Guid.NewGuid(),
                Email = "mary.student@email.com",
                FirstName = "Mary",
                LastName = "Smith",
                PhoneNumber = "+353871234603",
                Role = UserRole.Student,
                IsActive = true,
                MembershipStartDate = DateTime.UtcNow.AddMonths(-1),
                MembershipEndDate = null, // Ongoing membership
                EmergencyContact = "Tom Smith - Brother - +353871234604",
                MedicalNotes = "Mild asthma - has inhaler",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var student3 = new Student
            {
                Id = Guid.NewGuid(),
                Email = "peter.student@email.com",
                FirstName = "Peter",
                LastName = "Walsh",
                PhoneNumber = "+353871234605",
                Role = UserRole.Student,
                IsActive = true,
                MembershipStartDate = DateTime.UtcNow,
                MembershipEndDate = DateTime.UtcNow.AddYears(1),
                EmergencyContact = "Lisa Walsh - Mother - +353871234606",
                MedicalNotes = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Add users to context
            context.Users.AddRange(admin, instructor1, instructor2, instructor3, student1, student2, student3);
            await context.SaveChangesAsync();

            // ========================================
            // 4. Create Sample Workout Classes
            // ========================================
            var class1 = new WorkoutClass
            {
                Id = Guid.NewGuid(),
                Title = "Morning Krav Maga Fundamentals",
                Description = "Learn the basics of Krav Maga self-defense. Perfect for beginners. Focus on strikes, blocks, and situational awareness.",
                ClassType = ClassType.KravMaga,
                StartDateTime = DateTime.UtcNow.Date.AddDays(1).AddHours(9), // Tomorrow at 9 AM
                EndDateTime = DateTime.UtcNow.Date.AddDays(1).AddHours(10), // 1 hour class
                MaxCapacity = 15,
                Location = "Studio A",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var class2 = new WorkoutClass
            {
                Id = Guid.NewGuid(),
                Title = "Boxing HIIT Blast",
                Description = "High-intensity boxing workout combining cardio and strength. Burn calories and build power!",
                ClassType = ClassType.Boxing,
                StartDateTime = DateTime.UtcNow.Date.AddDays(1).AddHours(18), // Tomorrow at 6 PM
                EndDateTime = DateTime.UtcNow.Date.AddDays(1).AddHours(19),
                MaxCapacity = 20,
                Location = "Main Hall",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var class3 = new WorkoutClass
            {
                Id = Guid.NewGuid(),
                Title = "Sunset Yoga Flow",
                Description = "Relaxing vinyasa flow yoga session. Suitable for all levels. Bring your own mat.",
                ClassType = ClassType.Yoga,
                StartDateTime = DateTime.UtcNow.Date.AddDays(2).AddHours(19), // Day after tomorrow at 7 PM
                EndDateTime = DateTime.UtcNow.Date.AddDays(2).AddHours(20),
                MaxCapacity = 12,
                Location = "Studio B",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var class4 = new WorkoutClass
            {
                Id = Guid.NewGuid(),
                Title = "Advanced Krav Maga Techniques",
                Description = "Advanced self-defense techniques including ground fighting and weapon defense. Prerequisite: 3+ months training.",
                ClassType = ClassType.KravMaga,
                StartDateTime = DateTime.UtcNow.Date.AddDays(3).AddHours(10), // 3 days from now
                EndDateTime = DateTime.UtcNow.Date.AddDays(3).AddHours(11).AddMinutes(30),
                MaxCapacity = 10,
                Location = "Studio A",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.WorkoutClasses.AddRange(class1, class2, class3, class4);
            await context.SaveChangesAsync();

            // ========================================
            // 5. Assign Instructors to Classes
            // ========================================
            var classInstructor1 = new ClassInstructor
            {
                WorkoutClassId = class1.Id,
                InstructorId = instructor1.Id,
                IsPrimaryInstructor = true,
                AssignedAt = DateTime.UtcNow
            };

            var classInstructor2 = new ClassInstructor
            {
                WorkoutClassId = class2.Id,
                InstructorId = instructor2.Id,
                IsPrimaryInstructor = true,
                AssignedAt = DateTime.UtcNow
            };

            var classInstructor3 = new ClassInstructor
            {
                WorkoutClassId = class3.Id,
                InstructorId = instructor3.Id,
                IsPrimaryInstructor = true,
                AssignedAt = DateTime.UtcNow
            };

            var classInstructor4 = new ClassInstructor
            {
                WorkoutClassId = class4.Id,
                InstructorId = instructor1.Id,
                IsPrimaryInstructor = true,
                AssignedAt = DateTime.UtcNow
            };

            // Add a second instructor to the advanced class
            var classInstructor5 = new ClassInstructor
            {
                WorkoutClassId = class4.Id,
                InstructorId = instructor2.Id, // Mike assists with the advanced class
                IsPrimaryInstructor = false,
                AssignedAt = DateTime.UtcNow
            };

            context.ClassInstructors.AddRange(classInstructor1, classInstructor2, classInstructor3, classInstructor4, classInstructor5);
            await context.SaveChangesAsync();

            // ========================================
            // 6. Create Sample Registrations
            // ========================================
            var registration1 = new ClassRegistration
            {
                Id = Guid.NewGuid(),
                StudentId = student1.Id,
                WorkoutClassId = class1.Id,
                RegistrationDate = DateTime.UtcNow.AddDays(-2),
                Status = RegistrationStatus.Registered
            };

            var registration2 = new ClassRegistration
            {
                Id = Guid.NewGuid(),
                StudentId = student2.Id,
                WorkoutClassId = class1.Id,
                RegistrationDate = DateTime.UtcNow.AddDays(-1),
                Status = RegistrationStatus.Registered
            };

            var registration3 = new ClassRegistration
            {
                Id = Guid.NewGuid(),
                StudentId = student1.Id,
                WorkoutClassId = class2.Id,
                RegistrationDate = DateTime.UtcNow.AddDays(-3),
                Status = RegistrationStatus.Registered
            };

            var registration4 = new ClassRegistration
            {
                Id = Guid.NewGuid(),
                StudentId = student3.Id,
                WorkoutClassId = class3.Id,
                RegistrationDate = DateTime.UtcNow,
                Status = RegistrationStatus.Registered
            };

            context.ClassRegistrations.AddRange(registration1, registration2, registration3, registration4);
            await context.SaveChangesAsync();

            Console.WriteLine("[+] Database seeded successfully!");
            Console.WriteLine($"   - 1 Admin user");
            Console.WriteLine($"   - 3 Instructors");
            Console.WriteLine($"   - 3 Students");
            Console.WriteLine($"   - 4 Workout classes");
            Console.WriteLine($"   - 5 Instructor assignments");
            Console.WriteLine($"   - 4 Student registrations");
        }
    }
}
