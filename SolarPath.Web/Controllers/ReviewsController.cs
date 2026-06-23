using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SolarPath.Web.Data;
using SolarPath.Web.Models;

namespace SolarPath.Web.Controllers;

[Authorize(Roles = "Tourist")]
public class ReviewsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public ReviewsController(ApplicationDbContext db, UserManager<ApplicationUser> um)
    { _db = db; _userManager = um; }

    public async Task<IActionResult> Create(int routeId)
    {
        var route = await _db.Routes.FindAsync(routeId);
        if (route == null) return NotFound();

        var userId = _userManager.GetUserId(User)!;

        // Перевіряємо чи є завершене бронювання
        var hasCompleted = await _db.Bookings.AnyAsync(b =>
            b.RouteId == routeId &&
            b.TouristId == userId &&
            b.BookingStatus == BookingStatus.Completed);

        if (!hasCompleted)
        {
            TempData["Error"] = "Відгук можна залишити лише після завершеного походу.";
            return RedirectToAction("Details", "Routes", new { id = routeId });
        }

        // Перевіряємо чи вже є відгук
        var alreadyReviewed = await _db.Reviews.AnyAsync(r =>
            r.RouteId == routeId && r.TouristId == userId);

        if (alreadyReviewed)
        {
            TempData["Error"] = "Ви вже залишили відгук про цей маршрут.";
            return RedirectToAction("Details", "Routes", new { id = routeId });
        }

        ViewBag.Route = route;
        return View(new Review { RouteId = routeId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Review model)
    {
        var userId = _userManager.GetUserId(User)!;
        ModelState.Remove("Tourist");
        ModelState.Remove("TouristId");
        ModelState.Remove("Route");

        if (!ModelState.IsValid)
        {
            ViewBag.Route = await _db.Routes.FindAsync(model.RouteId);
            return View(model);
        }

        model.TouristId  = userId;
        model.CreatedAt  = DateTime.UtcNow;
        model.IsVerified = true; // верифіковано бо є завершене бронювання

        _db.Reviews.Add(model);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Дякуємо за відгук!";
        return RedirectToAction("Details", "Routes", new { id = model.RouteId });
    }
}
