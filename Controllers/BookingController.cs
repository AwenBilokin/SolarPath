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
    public async Task<IActionResult> Cancel(int id)
    {
        await _bookingService.CancelBookingAsync(id);
        TempData["Success"] = "Бронювання скасовано.";
        return RedirectToAction(nameof(MyBookings));
    }

    [HttpPost, Authorize(Roles = "Tourist"), ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestRefund(int id)
    {
        await _bookingService.RequestRefundAsync(id);
        TempData["Success"] = "Запит на повернення коштів надіслано адміністратору.";
        return RedirectToAction(nameof(MyBookings));
    }
}
