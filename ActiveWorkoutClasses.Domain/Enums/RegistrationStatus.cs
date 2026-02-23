namespace ActiveWorkoutClasses.Domain.Enums
{
    /// <summary>
    /// Status of a student's class registration
    /// </summary>
    public enum RegistrationStatus
    {
        Pending = 1,
        Registered = 2,
        Cancelled = 3,
        NoShow = 4,
        Waitlisted = 5
     }

}