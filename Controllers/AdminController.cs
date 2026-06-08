using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SolarPath.Web.Data;
using SolarPath.Web.Models;

namespace SolarPath.Web.Controllers;

[Authorize(Roles = "Administrator")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminController(ApplicationDbContext db, UserManager<ApplicationUser> um, RoleManager<IdentityRole> rm)
    { _db = db; _userManager = um; _roleManager = rm; }

    public async Task<IActionResult> Index()
    {
        ViewBag.RoutesCount   = await _db.Routes.CountAsync();
        ViewBag.BookingsCount = await _db.Bookings.CountAsync();
        ViewBag.UsersCount    = await _db.Users.CountAsync();
        ViewBag.TotalRevenue = await _db.Payments.Include(p => p.Booking).Where(p => p.Booking.BookingStatus != BookingStatus.Cancelled && p.Booking.BookingStatus != BookingStatus.CancelledByGuide && p.Booking.BookingStatus != BookingStatus.Refunded && p.RefundedAt == null).SumAsync(p => (decimal?)p.Amount) ?? 0;
        return View();
    }

    public async Task<IActionResult> Routes() =>
        View(await _db.Routes.Include(r => r.Category).Include(r => r.Guide).ToListAsync());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRoute(int id)
    {
        var route = await _db.Routes.FindAsync(id);
        if (route != null) { _db.Routes.Remove(route); await _db.SaveChangesAsync(); }
        TempData["Success"] = "Маршрут видалено.";
        return RedirectToAction(nameof(Routes));
    }

    public async Task<IActionResult> Bookings() =>
        View(await _db.Bookings.Include(b => b.Route).Include(b => b.Tourist).Include(b => b.Payment).ToListAsync());

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
            TempData["Success"] = $"Роль користувача змінено на {role}.";
        }
        return RedirectToAction(nameof(Users));
    }
}
