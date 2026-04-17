using ActiveWorkoutClasses.Application.DTOs.Locations;
using ActiveWorkoutClasses.Application.Interfaces;
using ActiveWorkoutClasses.Domain.Entities;
using ActiveWorkoutClasses.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ActiveWorkoutClasses.Application.Services
{
    public class LocationService : ILocationService
    {
        private readonly ApplicationDbContext _context;

        public LocationService(ApplicationDbContext context) => _context = context;

        public async Task<List<LocationDto>> GetAllAsync(bool includeInactive = false)
        {
            var query = _context.Locations.AsQueryable();
            if (!includeInactive) query = query.Where(l => l.IsActive);
            return await query.OrderBy(l => l.Name).Select(l => MapToDto(l)).ToListAsync();
        }

        public async Task<LocationDto?> GetByIdAsync(int id)
        {
            var location = await _context.Locations.FindAsync(id);
            return location is null ? null : MapToDto(location);
        }

        public async Task<LocationDto> CreateAsync(CreateLocationDto dto)
        {
            var location = new Location
            {
                Name = dto.Name,
                Address = dto.Address,
                City = dto.City,
                Capacity = dto.Capacity,
                Notes = dto.Notes,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.Locations.Add(location);
            await _context.SaveChangesAsync();
            return MapToDto(location);
        }

        public async Task<LocationDto?> UpdateAsync(int id, UpdateLocationDto dto)
        {
            var location = await _context.Locations.FindAsync(id);
            if (location is null) return null;

            location.Name = dto.Name;
            location.Address = dto.Address;
            location.City = dto.City;
            location.Capacity = dto.Capacity;
            location.IsActive = dto.IsActive;
            location.Notes = dto.Notes;
            await _context.SaveChangesAsync();
            return MapToDto(location);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var location = await _context.Locations.FindAsync(id);
            if (location is null) return false;

            var hasClasses = await _context.WorkoutClasses.AnyAsync(c => c.LocationId == id);
            if (hasClasses)
            {
                location.IsActive = false;
            }
            else
            {
                _context.Locations.Remove(location);
            }
            await _context.SaveChangesAsync();
            return true;
        }

        private static LocationDto MapToDto(Location l) => new()
        {
            Id = l.Id,
            Name = l.Name,
            Address = l.Address,
            City = l.City,
            Capacity = l.Capacity,
            IsActive = l.IsActive,
            Notes = l.Notes
        };
    }
}
