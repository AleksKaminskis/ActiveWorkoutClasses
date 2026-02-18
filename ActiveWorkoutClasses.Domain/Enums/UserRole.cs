namespace ActiveWorkoutClasses.Domain.Enums
{
    /// <summary>
    /// Defines the roles available in the system
    /// </summary>
    public enum UserRole
    {
        /// <summary>
        /// Student - can view and register for classes, check-in
        /// </summary>
        Student,

        /// <summary>
        /// Instructor - can view their classes and mark attendance
        /// </summary>
        Instructor,

        /// <summary>
        /// Admin - full system access, can manage classes and users
        /// </summary>
        Admin
    }
}
