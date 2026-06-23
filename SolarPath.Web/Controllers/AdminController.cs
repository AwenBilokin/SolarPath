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
        // ── базова статистика ──────────────────────────────────────────────
        ViewBag.RoutesCount    = await _db.Routes.CountAsync();
        ViewBag.BookingsCount  = await _db.Bookings.CountAsync();
        ViewBag.UsersCount     = await _db.Users.CountAsync();
        ViewBag.ActiveRoutes   = await _db.Routes.CountAsync(r => r.RouteStatus == RouteStatus.Published);
        ViewBag.PendingRefunds = await _db.Bookings.CountAsync(b => b.BookingStatus == BookingStatus.RefundRequested);

        ViewBag.TotalRevenue = await _db.Payments
            .Include(p => p.Booking)
            .Where(p => p.Booking.BookingStatus != BookingStatus.Cancelled
                     && p.Booking.BookingStatus != BookingStatus.CancelledByGuide
                     && p.Booking.BookingStatus != BookingStatus.Refunded
                     && p.RefundedAt == null)
            .SumAsync(p => (decimal?)p.Amount) ?? 0;

        // ── нові: додаткова статистика ────────────────────────────────────
        ViewBag.ReviewsCount      = await _db.Reviews.CountAsync();
        ViewBag.AvgRating         = await _db.Reviews.AnyAsync()
            ? Math.Round(await _db.Reviews.AverageAsync(r => (double)r.Rating), 1) : 0.0;
        ViewBag.TouristsCount     = (await _userManager.GetUsersInRoleAsync("Tourist")).Count;
        ViewBag.GuidesCount       = (await _userManager.GetUsersInRoleAsync("Guide")).Count;
        ViewBag.CompletedBookings = await _db.Bookings.CountAsync(b => b.BookingStatus == BookingStatus.Completed);
        ViewBag.CancelledBookings = await _db.Bookings.CountAsync(
            b => b.BookingStatus == BookingStatus.Cancelled
              || b.BookingStatus == BookingStatus.CancelledByGuide);
        ViewBag.TotalParticipants = await _db.Bookings
            .Where(b => b.BookingStatus == BookingStatus.Completed
                     || b.BookingStatus == BookingStatus.InProgress)
            .SumAsync(b => (int?)b.ParticipantsCount) ?? 0;

        // ── нові реєстрації за 30 днів ─────────────────────────────────────
        var since30 = DateTime.UtcNow.AddDays(-30);
        ViewBag.NewUsersMonth = await _db.Users
            .CountAsync(u => u.RegisteredAt >= since30);

        // ── бронювання за останні 6 місяців (для графіка) ─────────────────
        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-5);
        var bookingsByMonth = await _db.Bookings
            .Where(b => b.CreatedAt >= sixMonthsAgo)
            .GroupBy(b => new { b.CreatedAt.Year, b.CreatedAt.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToListAsync();

        // заповнюємо всі 6 місяців (навіть якщо 0 бронювань)
        var months = new List<string>();
        var counts = new List<int>();
        for (int i = 5; i >= 0; i--)
        {
            var d = DateTime.UtcNow.AddMonths(-i);
            months.Add(d.ToString("MMM yyyy"));
            var found = bookingsByMonth.FirstOrDefault(x => x.Year == d.Year && x.Month == d.Month);
            counts.Add(found?.Count ?? 0);
        }
        ViewBag.ChartMonths = System.Text.Json.JsonSerializer.Serialize(months);
        ViewBag.ChartCounts = System.Text.Json.JsonSerializer.Serialize(counts);

        // ── дохід по місяцях ──────────────────────────────────────────────
        var revenueByMonth = await _db.Payments
            .Include(p => p.Booking)
            .Where(p => p.PaidAt >= sixMonthsAgo
                     && p.Booking.BookingStatus != BookingStatus.Cancelled
                     && p.Booking.BookingStatus != BookingStatus.Refunded
                     && p.RefundedAt == null)
            .GroupBy(p => new { p.PaidAt.Year, p.PaidAt.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(x => x.Amount) })
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToListAsync();

        var revenueCounts = new List<decimal>();
        for (int i = 5; i >= 0; i--)
        {
            var d = DateTime.UtcNow.AddMonths(-i);
            var found = revenueByMonth.FirstOrDefault(x => x.Year == d.Year && x.Month == d.Month);
            revenueCounts.Add(found?.Total ?? 0);
        }
        ViewBag.ChartRevenue = System.Text.Json.JsonSerializer.Serialize(revenueCounts);

        // ── топ-5 маршрутів за доходом ────────────────────────────────────
        var topRoutes = await _db.Bookings
            .Include(b => b.Route)
            .Where(b => b.BookingStatus == BookingStatus.Completed
                     || b.BookingStatus == BookingStatus.Paid
                     || b.BookingStatus == BookingStatus.Confirmed
                     || b.BookingStatus == BookingStatus.InProgress)
            .GroupBy(b => b.Route.Title)
            .Select(g => new {
                Title    = g.Key,
                Revenue  = g.Sum(b => b.TotalPrice),
                Bookings = g.Count()
            })
            .OrderByDescending(x => x.Revenue)
            .Take(5)
            .ToListAsync();
        ViewBag.TopRoutes = topRoutes;

        // ── останні 5 бронювань ───────────────────────────────────────────
        ViewBag.RecentBookings = await _db.Bookings
            .Include(b => b.Route)
            .Include(b => b.Tourist)
            .OrderByDescending(b => b.CreatedAt)
            .Take(5)
            .ToListAsync();

        // ── розбивка по статусах ──────────────────────────────────────────
        var statusGroups = await _db.Bookings
            .GroupBy(b => b.BookingStatus)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();
        ViewBag.StatusGroups = statusGroups;

        return View();
    }

    public async Task<IActionResult> Revenue(int page = 1)
    {
        const int pageSize = 15;
        page = Math.Max(1, page);

        var query = _db.Payments
            .Include(p => p.Booking).ThenInclude(b => b.Route)
            .Include(p => p.Booking).ThenInclude(b => b.Tourist)
            .Where(p => p.Booking.BookingStatus != BookingStatus.Cancelled
                     && p.Booking.BookingStatus != BookingStatus.CancelledByGuide
                     && p.Booking.BookingStatus != BookingStatus.Refunded
                     && p.RefundedAt == null)
            .OrderByDescending(p => p.PaidAt);

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        ViewBag.TotalRevenue = await query.SumAsync(p => (decimal?)p.Amount) ?? 0;

        return View(new PagedResult<Payment>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        });
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

    public async Task<IActionResult> Bookings(int page = 1)
    {
        const int pageSize = 15;
        page = Math.Max(1, page);

        var query = _db.Bookings
            .Include(b => b.Route)
            .Include(b => b.Tourist)
            .Include(b => b.Payment)
            .OrderByDescending(b => b.CreatedAt);

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return View(new PagedResult<Booking>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        });
    }

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
