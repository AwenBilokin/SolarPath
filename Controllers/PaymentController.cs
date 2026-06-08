using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;
using SolarPath.Web.Data;
using SolarPath.Web.Models;

namespace SolarPath.Web.Controllers;

[Authorize]
public class PaymentController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _config;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(ApplicationDbContext db,
        UserManager<ApplicationUser> um,
        IConfiguration config,
        ILogger<PaymentController> logger)
    { _db = db; _userManager = um; _config = config; _logger = logger; }

    [HttpPost, Authorize(Roles = "Tourist"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(int routeId, string scheduledDate, int participantsCount)
    {
        try
        {
            // Парсимо дату безпечно
            if (!DateTime.TryParse(scheduledDate, out var parsedDate))
            {
                TempData["Error"] = "Невірний формат дати. Спробуйте ще раз.";
                return RedirectToAction("Details", "Routes", new { id = routeId });
            }

            if (parsedDate.Date < DateTime.Today)
            {
                TempData["Error"] = "Дата не може бути в минулому.";
                return RedirectToAction("Details", "Routes", new { id = routeId });
            }

            if (participantsCount < 1) participantsCount = 1;

            var route = await _db.Routes.FindAsync(routeId);
            if (route == null || route.RouteStatus != RouteStatus.Published)
            {
                TempData["Error"] = "Маршрут не знайдено.";
                return RedirectToAction("Index", "Routes");
            }

            // Перевірка місць
            var booked = await _db.Bookings
                .Where(b => b.RouteId == routeId
                         && b.ScheduledDate.Date == parsedDate.Date
                         && b.BookingStatus != BookingStatus.Cancelled
                         && b.BookingStatus != BookingStatus.CancelledByGuide)
                .SumAsync(b => (int?)b.ParticipantsCount) ?? 0;

            if (booked + participantsCount > route.MaxParticipants)
            {
                TempData["Error"] = "На жаль, на обрану дату немає вільних місць.";
                return RedirectToAction("Details", "Routes", new { id = routeId });
            }

            var userId = _userManager.GetUserId(User)!;
            var totalPrice = route.PricePerPerson * participantsCount;

            // Створюємо бронювання
            var booking = new Booking
            {
                RouteId           = routeId,
                TouristId         = userId,
                ScheduledDate     = parsedDate,
                ParticipantsCount = participantsCount,
                TotalPrice        = totalPrice,
                BookingStatus     = BookingStatus.Pending
            };
            _db.Bookings.Add(booking);
            route.AvailableSlots = Math.Max(0, route.AvailableSlots - participantsCount);
            await _db.SaveChangesAsync();

            // Stripe ключ — підтримуємо обидва формати
            var secretKey = _config["Stripe:SecretKey"]
                         ?? _config["Stripe__SecretKey"]
                         ?? "";

            if (string.IsNullOrWhiteSpace(secretKey) || secretKey.Contains("YOUR_KEY"))
            {
                // Демо-режим
                booking.BookingStatus = BookingStatus.Paid;
                _db.Payments.Add(new Payment
                {
                    BookingId            = booking.Id,
                    Amount               = totalPrice,
                    Currency             = "UAH",
                    Method               = SolarPath.Web.Models.PaymentMethod.Card,
                    GatewayTransactionId = "DEMO-" + Guid.NewGuid().ToString("N")[..10].ToUpper(),
                    PaidAt               = DateTime.UtcNow
                });
                await _db.SaveChangesAsync();
                TempData["Success"] = $"Бронювання оформлено! Сума: {totalPrice} ₴ (Демо-режим)";
                return RedirectToAction("MyBookings", "Booking");
            }

            // Stripe Checkout
            StripeConfiguration.ApiKey = secretKey;
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency   = "uah",
                            UnitAmount = (long)(totalPrice * 100),
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name        = route.Title,
                                Description = $"{participantsCount} особ(а) · {parsedDate:dd.MM.yyyy}"
                            }
                        },
                        Quantity = 1
                    }
                },
                Mode       = "payment",
                SuccessUrl = $"{baseUrl}/Payment/Success?bookingId={booking.Id}&session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl  = $"{baseUrl}/Payment/Cancel?bookingId={booking.Id}",
            };

            var session = await new SessionService().CreateAsync(options);
            return Redirect(session.Url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Payment/Checkout error for routeId={RouteId}", routeId);
            TempData["Error"] = "Помилка при оформленні бронювання: " + ex.Message[..Math.Min(120, ex.Message.Length)];
            return RedirectToAction("Details", "Routes", new { id = routeId });
        }
    }

    public async Task<IActionResult> Success(int bookingId, string session_id)
    {
        try
        {
            StripeConfiguration.ApiKey = _config["Stripe:SecretKey"] ?? _config["Stripe__SecretKey"];
            var booking = await _db.Bookings.FindAsync(bookingId);
            if (booking == null) return NotFound();

            var session = await new SessionService().GetAsync(session_id);
            if (session.PaymentStatus == "paid")
            {
                booking.BookingStatus = BookingStatus.Paid;
                _db.Payments.Add(new Payment
                {
                    BookingId            = bookingId,
                    Amount               = booking.TotalPrice,
                    Currency             = "UAH",
                    Method               = SolarPath.Web.Models.PaymentMethod.Card,
                    GatewayTransactionId = session.PaymentIntentId ?? session.Id,
                    PaidAt               = DateTime.UtcNow
                });
                await _db.SaveChangesAsync();
                TempData["Success"] = "Оплату успішно прийнято! Очікуйте підтвердження гіда.";
            }
            else
            {
                TempData["Error"] = "Оплата не завершена.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Payment/Success error");
            TempData["Error"] = "Помилка перевірки оплати.";
        }
        return RedirectToAction("MyBookings", "Booking");
    }

    public async Task<IActionResult> Cancel(int bookingId)
    {
        try
        {
            var booking = await _db.Bookings.FindAsync(bookingId);
            if (booking != null)
            {
                var route = await _db.Routes.FindAsync(booking.RouteId);
                if (route != null) route.AvailableSlots += booking.ParticipantsCount;
                booking.BookingStatus = BookingStatus.Cancelled;
                await _db.SaveChangesAsync();
                TempData["Error"] = "Оплату скасовано.";
                return RedirectToAction("Details", "Routes", new { id = booking.RouteId });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Payment/Cancel error");
        }
        return RedirectToAction("Index", "Routes");
    }
}
