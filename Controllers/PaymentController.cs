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

    public PaymentController(ApplicationDbContext db,
        UserManager<ApplicationUser> um, IConfiguration config)
    { _db = db; _userManager = um; _config = config; }

    [HttpPost, Authorize(Roles = "Tourist"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(int routeId, DateTime scheduledDate, int participantsCount)
    {
        var route = await _db.Routes.FindAsync(routeId);
        if (route == null || route.RouteStatus != RouteStatus.Published)
        {
            TempData["Error"] = "Маршрут не знайдено.";
            return RedirectToAction("Index", "Routes");
        }

        var booked = await _db.Bookings
            .Where(b => b.RouteId == routeId
                     && b.ScheduledDate.Date == scheduledDate.Date
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

        var booking = new Booking
        {
            RouteId           = routeId,
            TouristId         = userId,
            ScheduledDate     = scheduledDate,
            ParticipantsCount = participantsCount,
            TotalPrice        = totalPrice,
            BookingStatus     = BookingStatus.Pending
        };
        _db.Bookings.Add(booking);
        route.AvailableSlots = Math.Max(0, route.AvailableSlots - participantsCount);
        await _db.SaveChangesAsync();

        // Перевіряємо чи вставлені Stripe ключі
        var secretKey = _config["Stripe:SecretKey"] ?? "";
        if (string.IsNullOrEmpty(secretKey) || secretKey.Contains("ВСТАВСЮДИ"))
        {
            // Stripe не налаштований — симулюємо оплату
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
            TempData["Success"] = "Бронювання оформлено! (Демо-режим — Stripe не налаштований)";
            return RedirectToAction("MyBookings", "Booking");
        }

        // Справжній Stripe Checkout
        try
        {
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
                                Description = $"{participantsCount} особ(а) · {scheduledDate:dd.MM.yyyy}"
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
            // Якщо Stripe не вдалось — симулюємо
            booking.BookingStatus = BookingStatus.Paid;
            _db.Payments.Add(new Payment
            {
                BookingId            = booking.Id,
                Amount               = totalPrice,
                Currency             = "UAH",
                Method               = SolarPath.Web.Models.PaymentMethod.Card,
                GatewayTransactionId = "ERR-" + Guid.NewGuid().ToString("N")[..8].ToUpper(),
                PaidAt               = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
            TempData["Success"] = "Бронювання оформлено! (Помилка Stripe: " + ex.Message[..Math.Min(60, ex.Message.Length)] + ")";
            return RedirectToAction("MyBookings", "Booking");
        }
    }

    public async Task<IActionResult> Success(int bookingId, string session_id)
    {
        StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];
        var booking = await _db.Bookings.FindAsync(bookingId);
        if (booking == null) return NotFound();

        try
        {
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
        catch
        {
            TempData["Error"] = "Помилка перевірки оплати.";
        }
        return RedirectToAction("MyBookings", "Booking");
    }

    public async Task<IActionResult> Cancel(int bookingId)
    {
        var booking = await _db.Bookings.FindAsync(bookingId);
        if (booking != null)
        {
            var route = await _db.Routes.FindAsync(booking.RouteId);
            if (route != null) route.AvailableSlots += booking.ParticipantsCount;
            booking.BookingStatus = BookingStatus.Cancelled;
            await _db.SaveChangesAsync();
        }
        TempData["Error"] = "Оплату скасовано.";
        return RedirectToAction("Details", "Routes", new { id = booking?.RouteId });
    }
}
