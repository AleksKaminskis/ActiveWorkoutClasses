namespace ActiveWorkoutClasses.Application.DTOs.Users
{
    /// <summary>
    /// User list with pagination
    /// </summary>
    public class UserListDto
    {
        public List<UserDto> Users { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}
