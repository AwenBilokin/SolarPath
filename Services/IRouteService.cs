using SolarPath.Web.Models;

namespace SolarPath.Web.Services;

public interface IRouteService
{
    Task<IEnumerable<Models.Route>> GetPublishedAsync(int? categoryId, DifficultyLevel? difficulty, decimal? maxPrice, string? search);
    Task<Models.Route?> GetByIdAsync(int id);
    Task<Models.Route> CreateAsync(Models.Route route);
    Task UpdateAsync(Models.Route route);
    Task PublishAsync(int id);
    Task<IEnumerable<Models.Route>> GetByGuideAsync(string guideId);
}
