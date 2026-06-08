using Microsoft.EntityFrameworkCore;
using SolarPath.Web.Data;
using SolarPath.Web.Models;

namespace SolarPath.Web.Services;

public class RouteService : IRouteService
{
    private readonly ApplicationDbContext _db;
    public RouteService(ApplicationDbContext db) => _db = db;

    public async Task<IEnumerable<Models.Route>> GetPublishedAsync(
        int? categoryId, DifficultyLevel? difficulty, decimal? maxPrice, string? search)
    {
        var q = _db.Routes
            .Include(r => r.Category)
            .Include(r => r.Guide)
            .Include(r => r.Reviews)
            .Where(r => r.RouteStatus == RouteStatus.Published);

        if (categoryId.HasValue)  q = q.Where(r => r.CategoryId == categoryId);
        if (difficulty.HasValue)  q = q.Where(r => r.Difficulty == difficulty);
        if (maxPrice.HasValue)    q = q.Where(r => r.PricePerPerson <= maxPrice);
        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(r => r.Title.Contains(search) || r.Description.Contains(search));

        return await q.OrderByDescending(r => r.CreatedAt).ToListAsync();
    }

    public async Task<Models.Route?> GetByIdAsync(int id) =>
        await _db.Routes
            .Include(r => r.Category)
            .Include(r => r.Guide)
            .Include(r => r.Points.OrderBy(p => p.OrderIndex))
            .Include(r => r.Reviews).ThenInclude(rv => rv.Tourist)
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<Models.Route> CreateAsync(Models.Route route)
    {
        _db.Routes.Add(route);
        await _db.SaveChangesAsync();
        return route;
    }

    public async Task UpdateAsync(Models.Route route)
    {
        _db.Routes.Update(route);
        await _db.SaveChangesAsync();
    }

    public async Task PublishAsync(int id)
    {
        var route = await _db.Routes.FindAsync(id);
        if (route != null)
        {
            route.RouteStatus = RouteStatus.Published;
            await _db.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Models.Route>> GetByGuideAsync(string guideId) =>
        await _db.Routes
            .Include(r => r.Category)
            .Include(r => r.Bookings)
            .Where(r => r.GuideId == guideId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
}
