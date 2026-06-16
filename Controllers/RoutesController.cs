using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SolarPath.Web.Data;
using SolarPath.Web.Models;
using SolarPath.Web.Services;

namespace SolarPath.Web.Controllers;

public class RoutesController : Controller
{
    private readonly IRouteService _routeService;
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _env;
    private readonly ICloudinaryService _cloudinary;

    public RoutesController(IRouteService rs, ApplicationDbContext db,
        UserManager<ApplicationUser> um, IWebHostEnvironment env, ICloudinaryService cloudinary)
    { _routeService = rs; _db = db; _userManager = um; _env = env; _cloudinary = cloudinary; }

    public async Task<IActionResult> Index(int? categoryId, DifficultyLevel? difficulty,
        decimal? maxPrice, string? search, int page = 1)
    {
        const int pageSize = 9;
        var result = await _routeService.GetPublishedPagedAsync(categoryId, difficulty, maxPrice, search, page, pageSize);
        ViewBag.Categories = await _db.Categories.ToListAsync();
        ViewBag.SelectedCategory  = categoryId;
        ViewBag.SelectedDifficulty = difficulty;
        ViewBag.MaxPrice  = maxPrice;
        ViewBag.Search    = search;
        return View(result);
    }

    [HttpGet]
    public async Task<IActionResult> Suggestions(string q)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
            return Json(Array.Empty<object>());

        var s = q.Trim().ToLower();

        var routeRows = await _db.Routes
            .Include(r => r.Category)
            .Where(r => r.RouteStatus == RouteStatus.Published && r.Title.ToLower().Contains(s))
            .OrderBy(r => r.Title)
            .Take(6)
            .Select(r => new { r.Id, r.Title, CategoryName = r.Category.Name, r.PricePerPerson })
            .ToListAsync();

        var routeMatches = routeRows.Select(r => new
        {
            type     = "route",
            id       = r.Id,
            title    = r.Title,
            subtitle = r.CategoryName,
            price    = (decimal?)r.PricePerPerson,
            url      = Url.Action("Details", "Routes", new { id = r.Id })
        });

        var categoryRows = await _db.Categories
            .Where(c => c.Name.ToLower().Contains(s))
            .Take(3)
            .Select(c => new { c.Id, c.Name })
            .ToListAsync();

        var categoryMatches = categoryRows.Select(c => new
        {
            type     = "category",
            id       = c.Id,
            title    = c.Name,
            subtitle = "Категорія",
            price    = (decimal?)null,
            url      = Url.Action("Index", "Routes", new { categoryId = c.Id })
        });

        var result = categoryMatches.Cast<object>().Concat(routeMatches.Cast<object>()).Take(8);
        return Json(result);
    }

    public async Task<IActionResult> Details(int id)
    {
        var route = await _routeService.GetByIdAsync(id);
        if (route == null) return NotFound();
        return View(route);
    }

    [Authorize(Roles = "Guide,Administrator")]
    public async Task<IActionResult> Create()
    {
        ViewBag.Categories = new SelectList(await _db.Categories.ToListAsync(), "Id", "Name");
        return View(new Models.Route());
    }

    [HttpPost, Authorize(Roles = "Guide,Administrator"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Models.Route model, IFormFile? image, List<RoutePointDto>? RoutePoints)
    {
        ModelState.Remove("Guide"); ModelState.Remove("Category");
        ModelState.Remove("GuideId"); ModelState.Remove("Points"); ModelState.Remove("RoutePoints");
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = new SelectList(await _db.Categories.ToListAsync(), "Id", "Name");
            return View(model);
        }
        model.GuideId = _userManager.GetUserId(User)!;
        model.AvailableSlots = model.MaxParticipants;
        if (image != null && image.Length > 0)
        {
            var url = await _cloudinary.UploadImageAsync(image);
            if (url != null) model.ImageUrl = url;
        }
        var saved = await _routeService.CreateAsync(model);

        if ((RoutePoints == null || RoutePoints.Count == 0) && Request.Form.ContainsKey("RoutePoints[0].LatitudeStr"))
        {
            RoutePoints = new List<RoutePointDto>();
            int i = 0;
            while (Request.Form.ContainsKey($"RoutePoints[{i}].LatitudeStr"))
            {
                RoutePoints.Add(new RoutePointDto
                {
                    LatitudeStr  = Request.Form[$"RoutePoints[{i}].LatitudeStr"].ToString(),
                    LongitudeStr = Request.Form[$"RoutePoints[{i}].LongitudeStr"].ToString(),
                    Title        = Request.Form[$"RoutePoints[{i}].Title"].ToString(),
                    PointType    = Enum.TryParse<PointType>(Request.Form[$"RoutePoints[{i}].PointType"], out var pt) ? pt : PointType.Checkpoint,
                    OrderIndex   = i
                });
                i++;
            }
        }

        if (RoutePoints != null && RoutePoints.Count > 0)
        {
            var routePoints = RoutePoints.Select((p, i) => new RoutePoint
            {
                RouteId    = saved.Id,
                Latitude   = p.Latitude,
                Longitude  = p.Longitude,
                Title      = p.Title,
                PointType  = p.PointType,
                OrderIndex = i
            }).ToList();
            _db.RoutePoints.AddRange(routePoints);
            await _db.SaveChangesAsync();
        }

        TempData["Success"] = "Маршрут успішно створено! Опублікуйте його, щоб туристи могли його побачити.";
        return RedirectToAction("Dashboard", "Guide");
    }

    [Authorize(Roles = "Guide,Administrator")]
    public async Task<IActionResult> Edit(int id)
    {
        var route = await _routeService.GetByIdAsync(id);
        if (route == null) return NotFound();
        var userId = _userManager.GetUserId(User);
        if (!User.IsInRole("Administrator") && route.GuideId != userId) return Forbid();
        ViewBag.Categories = new SelectList(await _db.Categories.ToListAsync(), "Id", "Name", route.CategoryId);
        return View(route);
    }

    [HttpPost, Authorize(Roles = "Guide,Administrator"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Models.Route model, IFormFile? image, List<RoutePointDto>? RoutePoints)
    {
        var existing = await _db.Routes.FindAsync(id);
        if (existing == null) return NotFound();
        ModelState.Remove("Guide"); ModelState.Remove("Category"); ModelState.Remove("Points"); ModelState.Remove("RoutePoints");
        ModelState.Remove("AvailableSlots"); ModelState.Remove("GuideId"); ModelState.Remove("CreatedAt");
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = new SelectList(await _db.Categories.ToListAsync(), "Id", "Name");
            var routeWithPoints = await _routeService.GetByIdAsync(id);
            return View(routeWithPoints);
        }

        if (image != null && image.Length > 0)
        {
            if (!string.IsNullOrWhiteSpace(existing.ImageUrl))
                await _cloudinary.DeleteImageAsync(existing.ImageUrl);
            var url = await _cloudinary.UploadImageAsync(image);
            if (url != null) existing.ImageUrl = url;
        }

        // Видаляємо старі точки
        var oldPoints = await _db.RoutePoints.Where(p => p.RouteId == id).ToListAsync();
        _db.RoutePoints.RemoveRange(oldPoints);
        await _db.SaveChangesAsync();

        // Оновлюємо поля маршруту після SaveChanges для точок
        existing.Title = model.Title; existing.Description = model.Description;
        existing.Difficulty = model.Difficulty; existing.DistanceKm = model.DistanceKm;
        existing.DurationMinutes = model.DurationMinutes; existing.MaxParticipants = model.MaxParticipants;
        existing.PricePerPerson = model.PricePerPerson; existing.CategoryId = model.CategoryId;
        existing.SeasonStart = model.SeasonStart; existing.SeasonEnd = model.SeasonEnd;
        existing.GeoData = model.GeoData;

        if ((RoutePoints == null || RoutePoints.Count == 0) && Request.Form.ContainsKey("RoutePoints[0].LatitudeStr"))
        {
            RoutePoints = new List<RoutePointDto>();
            int i = 0;
            while (Request.Form.ContainsKey($"RoutePoints[{i}].LatitudeStr"))
            {
                RoutePoints.Add(new RoutePointDto
                {
                    LatitudeStr  = Request.Form[$"RoutePoints[{i}].LatitudeStr"].ToString(),
                    LongitudeStr = Request.Form[$"RoutePoints[{i}].LongitudeStr"].ToString(),
                    Title        = Request.Form[$"RoutePoints[{i}].Title"].ToString(),
                    PointType    = Enum.TryParse<PointType>(Request.Form[$"RoutePoints[{i}].PointType"], out var pt) ? pt : PointType.Checkpoint,
                    OrderIndex   = i
                });
                i++;
            }
        }

        if (RoutePoints != null && RoutePoints.Count > 0)
        {
            var newPoints = RoutePoints.Select((p, i) => new RoutePoint
            {
                RouteId    = id,
                Latitude   = p.Latitude,
                Longitude  = p.Longitude,
                Title      = p.Title,
                PointType  = p.PointType,
                OrderIndex = i
            }).ToList();
            _db.RoutePoints.AddRange(newPoints);
            await _db.SaveChangesAsync();
        }

        _db.Entry(existing).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        await _db.SaveChangesAsync();

        TempData["Success"] = "Маршрут оновлено!";
        return RedirectToAction("Dashboard", "Guide");
    }
}
