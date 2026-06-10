using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SolarPath.Web.Data;
using SolarPath.Web.Models;
using SolarPath.Web.Services;

namespace SolarPath.Web.Controllers;

[Authorize(Roles = "Administrator")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IBookingService _bookingService;

    public AdminController(ApplicationDbContext db, UserManager<ApplicationUser> um,
        RoleManager<IdentityRole> rm, IBookingService bs)
    { _db = db; _userManager = um; _roleManager = rm; _bookingService = bs; }

    public async Task<IActionResult> Index()
    {
        ViewBag.RoutesCount   = await _db.Routes.CountAsync();
        ViewBag.BookingsCount = await _db.Bookings.CountAsync();
        ViewBag.UsersCount    = await _db.Users.CountAsync();
        ViewBag.TotalRevenue  = await _db.Payments
            .Include(p => p.Booking)
            .Where(p => p.Booking.BookingStatus != BookingStatus.Cancelled
                     && p.Booking.BookingStatus != BookingStatus.CancelledByGuide
                     && p.Booking.BookingStatus != BookingStatus.Refunded
                     && p.RefundedAt == null)
            .SumAsync(p => (decimal?)p.Amount) ?? 0;
        ViewBag.ActiveRoutes  = await _db.Routes.CountAsync(r => r.RouteStatus == RouteStatus.Published);
        ViewBag.PendingRefunds = await _db.Bookings.CountAsync(b => b.BookingStatus == BookingStatus.RefundRequested);
        return View();
    }

    public async Task<IActionResult> Routes() =>
        View(await _db.Routes.Include(r => r.Category).Include(r => r.Guide).ToListAsync());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRoute(int id)
    {
        var route = await _db.Routes.FindAsync(id);
        if (route == null)
        {
            TempData["Error"] = "Маршрут не знайдено.";
            return RedirectToAction(nameof(Routes));
        }

        var activeBookings = await _db.Bookings
            .Where(b => b.RouteId == id
                     && b.BookingStatus != BookingStatus.Cancelled
                     && b.BookingStatus != BookingStatus.CancelledByGuide
                     && b.BookingStatus != BookingStatus.Refunded)
            .CountAsync();

        if (activeBookings > 0)
        {
            TempData["Error"] = $"Неможливо видалити маршрут — є {activeBookings} активних бронювань. Спочатку скасуйте їх.";
            return RedirectToAction(nameof(Routes));
        }

        var hasAnyBookings = await _db.Bookings.AnyAsync(b => b.RouteId == id);
        if (hasAnyBookings)
        {
            route.RouteStatus = RouteStatus.Archived;
            TempData["Success"] = "Маршрут переведено в архів (є історія бронювань).";
        }
        else
        {
            _db.Routes.Remove(route);
            TempData["Success"] = "Маршрут видалено.";
        }

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Routes));
    }

    public async Task<IActionResult> Bookings() =>
        View(await _db.Bookings
            .Include(b => b.Route)
            .Include(b => b.Tourist)
            .Include(b => b.Payment)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ProcessRefund(int id)
    {
        await _bookingService.ProcessRefundAsync(id);
        TempData["Success"] = "Повернення коштів оброблено.";
        return RedirectToAction(nameof(Bookings));
    }

    public async Task<IActionResult> Users()
    {
        var users = await _db.Users.ToListAsync();
        var vm = new List<(ApplicationUser User, IList<string> Roles)>();
        foreach (var u in users) vm.Add((u, await _userManager.GetRolesAsync(u)));
        ViewBag.AllRoles = _roleManager.Roles.Select(r => r.Name).ToList();
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetRole(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user != null)
        {
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, role);
            TempData["Success"] = $"Роль змінено на {role}.";
        }
        return RedirectToAction(nameof(Users));
    }
}
