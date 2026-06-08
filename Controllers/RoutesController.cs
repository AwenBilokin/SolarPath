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

    public RoutesController(IRouteService rs, ApplicationDbContext db,
        UserManager<ApplicationUser> um, IWebHostEnvironment env)
    { _routeService = rs; _db = db; _userManager = um; _env = env; }

    public async Task<IActionResult> Index(int? categoryId, DifficultyLevel? difficulty,
        decimal? maxPrice, string? search)
    {
        var routes = await _routeService.GetPublishedAsync(categoryId, difficulty, maxPrice, search);
        ViewBag.Categories = await _db.Categories.ToListAsync();
        ViewBag.SelectedCategory  = categoryId;
        ViewBag.SelectedDifficulty = difficulty;
        ViewBag.MaxPrice  = maxPrice;
        ViewBag.Search    = search;
        return View(routes);
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
    public async Task<IActionResult> Create(Models.Route model, IFormFile? image)
    {
        ModelState.Remove("Guide"); ModelState.Remove("Category");
        ModelState.Remove("GuideId");
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = new SelectList(await _db.Categories.ToListAsync(), "Id", "Name");
            return View(model);
        }
        model.GuideId = _userManager.GetUserId(User)!;
        model.AvailableSlots = model.MaxParticipants;
        if (image != null && image.Length > 0)
        {
            var uploads = Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploads);
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
            using var fs = System.IO.File.Create(Path.Combine(uploads, fileName));
            await image.CopyToAsync(fs);
            model.ImageUrl = $"/uploads/{fileName}";
        }
        await _routeService.CreateAsync(model);
        TempData["Success"] = "Маршрут успішно створено!";
        return RedirectToAction(nameof(Index));
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
    public async Task<IActionResult> Edit(int id, Models.Route model, IFormFile? image)
    {
        var existing = await _db.Routes.FindAsync(id);
        if (existing == null) return NotFound();
        ModelState.Remove("Guide"); ModelState.Remove("Category");
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = new SelectList(await _db.Categories.ToListAsync(), "Id", "Name");
            return View(model);
        }
        existing.Title = model.Title; existing.Description = model.Description;
        existing.Difficulty = model.Difficulty; existing.DistanceKm = model.DistanceKm;
        existing.DurationMinutes = model.DurationMinutes; existing.MaxParticipants = model.MaxParticipants;
        existing.PricePerPerson = model.PricePerPerson; existing.CategoryId = model.CategoryId;
        existing.SeasonStart = model.SeasonStart; existing.SeasonEnd = model.SeasonEnd;
        existing.GeoData = model.GeoData;
        if (image != null && image.Length > 0)
        {
            var uploads = Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploads);
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
            using var fs = System.IO.File.Create(Path.Combine(uploads, fileName));
            await image.CopyToAsync(fs);
            existing.ImageUrl = $"/uploads/{fileName}";
        }
        await _routeService.UpdateAsync(existing);
        TempData["Success"] = "Маршрут оновлено!";
        return RedirectToAction(nameof(Index));
    }
}
