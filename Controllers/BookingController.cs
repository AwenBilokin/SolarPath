using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SolarPath.Web.Models;
using SolarPath.Web.Services;

namespace SolarPath.Web.Controllers;

[Authorize]
public class BookingController : Controller
{
    private readonly IBookingService _bookingService;
    private readonly UserManager<ApplicationUser> _userManager;

    public BookingController(IBookingService bs, UserManager<ApplicationUser> um)
    { _bookingService = bs; _userManager = um; }

    [Authorize(Roles = "Tourist")]
    public async Task<IActionResult> MyBookings()
    {
        var userId = _userManager.GetUserId(User)!;
        var bookings = await _bookingService.GetByTouristAsync(userId);
        return View(bookings);
    }

    [HttpPost, Authorize(Roles = "Tourist"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int routeId, DateTime scheduledDate, int participantsCount)
    {
        var available = await _bookingService.CheckAvailabilityAsync(routeId, scheduledDate, participantsCount);
        if (!available)
        {
            TempData["Error"] = "На жаль, на обрану дату немає вільних місць.";
            return RedirectToAction("Details", "Routes", new { id = routeId });
        }
        var userId = _userManager.GetUserId(User)!;
        await _bookingService.CreateBookingAsync(routeId, userId, scheduledDate, participantsCount);
        TempData["Success"] = "Бронювання успішно оформлено та оплачено!";
        return RedirectToAction(nameof(MyBookings));
    }

    [HttpPost, Authorize(Roles = "Tourist"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        await _bookingService.CancelBookingAsync(id);
        TempData["Success"] = "Бронювання скасовано.";
        return RedirectToAction(nameof(MyBookings));
    }
}
