using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SolarPath.Web.Data;
using SolarPath.Web.Models;

namespace SolarPath.Web.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _db;
    public HomeController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var popular = await _db.Routes
            .Include(r => r.Category)
            .Include(r => r.Reviews)
            .Where(r => r.RouteStatus == RouteStatus.Published)
            .OrderByDescending(r => r.Reviews.Count)
            .Take(6)
            .ToListAsync();
        ViewBag.PopularRoutes = popular;
        return View();
    }
}
