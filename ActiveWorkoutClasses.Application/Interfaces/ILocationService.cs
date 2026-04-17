using ActiveWorkoutClasses.Application.DTOs.Locations;

namespace ActiveWorkoutClasses.Application.Interfaces
{
    public interface ILocationService
    {
        Task<List<LocationDto>> GetAllAsync(bool includeInactive = false);
        Task<LocationDto?> GetByIdAsync(int id);
        Task<LocationDto> CreateAsync(CreateLocationDto dto);
        Task<LocationDto?> UpdateAsync(int id, UpdateLocationDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
