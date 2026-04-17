namespace ActiveWorkoutClasses.Application.DTOs.Classes
{
    /// <summary>
    /// Response with list of classes and pagination info
    /// </summary>
    public class WorkoutClassListDto
    {
        public List<WorkoutClassDto> Classes { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}
