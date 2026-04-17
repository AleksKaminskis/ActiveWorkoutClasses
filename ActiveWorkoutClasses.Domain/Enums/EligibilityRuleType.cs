namespace ActiveWorkoutClasses.Domain.Enums
{
    /// <summary>
    /// The type of criterion used by an <see cref="Entities.EligibilityRule"/>.
    /// </summary>
    public enum EligibilityRuleType
    {
        /// <summary>Student must have attended at least <c>IntValue</c> classes.</summary>
        MinAttendanceCount = 1,

        /// <summary>Student must have been registered for at least <c>DaysValue</c> days.</summary>
        MinDaysSinceRegistration = 2,

        /// <summary>Student's most recent grading (Pass) must be at least <c>DaysValue</c> days ago.</summary>
        MinDaysSinceLastGrading = 3,

        /// <summary>Student must have attended at least <c>IntValue</c> classes of the grading's discipline.</summary>
        MinClassesOfType = 4
    }
}
