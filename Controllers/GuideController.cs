using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SolarPath.Web.Models;
using SolarPath.Web.Services;

namespace SolarPath.Web.Controllers;

[Authorize(Roles = "Guide")]
public class GuideController : Controller
{
    private readonly IRouteService _routeService;
    private readonly IBookingService _bookingService;
    private readonly UserManager<ApplicationUser> _userManager;

    public GuideController(IRouteService rs, IBookingService bs, UserManager<ApplicationUser> um)
    { _routeService = rs; _bookingService = bs; _userManager = um; }

    public async Task<IActionResult> Dashboard()
    {
        var userId = _userManager.GetUserId(User)!;
        ViewBag.Routes   = await _routeService.GetByGuideAsync(userId);
        ViewBag.Bookings = await _bookingService.GetByGuideAsync(userId);
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(int id)
    {
        await _bookingService.ConfirmBookingAsync(id);
        TempData["Success"] = "Бронювання підтверджено.";
        return RedirectToAction(nameof(Dashboard));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id)
    {
        await _bookingService.CancelBookingAsync(id);
        TempData["Success"] = "Бронювання відхилено.";
        return RedirectToAction(nameof(Dashboard));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Publish(int id)
    {
        await _routeService.PublishAsync(id);
        TempData["Success"] = "Маршрут опубліковано!";
        return RedirectToAction(nameof(Dashboard));
    }
}
