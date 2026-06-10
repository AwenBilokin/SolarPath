using Microsoft.EntityFrameworkCore;
using SolarPath.Web.Data;
using SolarPath.Web.Models;

namespace SolarPath.Web.Services;

public interface INotificationService
{
    Task NotifyBookingCreatedAsync(int bookingId);
    Task NotifyBookingConfirmedAsync(int bookingId);
    Task NotifyBookingCancelledAsync(int bookingId);
    Task NotifyBookingCompletedAsync(int bookingId);
    Task NotifyRefundProcessedAsync(int bookingId);
}

/// <summary>
/// Сервіс сповіщень. Поточна реалізація — stub з логуванням.
/// Продакшн-версія: підключити SMTP (SendGrid/MailKit) або Telegram Bot.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(ApplicationDbContext db, ILogger<NotificationService> logger)
    { _db = db; _logger = logger; }

    public async Task NotifyBookingCreatedAsync(int bookingId)
    {
        var booking = await _db.Bookings
            .Include(b => b.Route).ThenInclude(r => r.Guide)
            .Include(b => b.Tourist)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null) return;

        _logger.LogInformation(
            "[Notification] Бронювання #{Id} створено. Турист: {Tourist}, Маршрут: {Route}, Дата: {Date:dd.MM.yyyy}",
            bookingId, booking.Tourist?.Email, booking.Route?.Title, booking.ScheduledDate);

        _logger.LogInformation(
            "[Notification] → Гіду {Guide}: новий турист забронював ваш маршрут «{Route}»",
            booking.Route?.Guide?.Email, booking.Route?.Title);
    }

    public async Task NotifyBookingConfirmedAsync(int bookingId)
    {
        var booking = await _db.Bookings
            .Include(b => b.Route)
            .Include(b => b.Tourist)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null) return;

        _logger.LogInformation(
            "[Notification] Бронювання #{Id} підтверджено гідом. → Туристу {Tourist}: ваш похід «{Route}» {Date:dd.MM.yyyy} підтверджено!",
            bookingId, booking.Tourist?.Email, booking.Route?.Title, booking.ScheduledDate);
    }

    public async Task NotifyBookingCancelledAsync(int bookingId)
    {
        var booking = await _db.Bookings
            .Include(b => b.Route)
            .Include(b => b.Tourist)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null) return;

        _logger.LogInformation(
            "[Notification] Бронювання #{Id} скасовано. → Туристу {Tourist}: бронювання «{Route}» скасовано.",
            bookingId, booking.Tourist?.Email, booking.Route?.Title);
    }

    public async Task NotifyBookingCompletedAsync(int bookingId)
    {
        var booking = await _db.Bookings
            .Include(b => b.Route)
            .Include(b => b.Tourist)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null) return;

        _logger.LogInformation(
            "[Notification] Похід завершено #{Id}. → Туристу {Tourist}: залиште відгук про маршрут «{Route}»!",
            bookingId, booking.Tourist?.Email, booking.Route?.Title);
    }

    public async Task NotifyRefundProcessedAsync(int bookingId)
    {
        var booking = await _db.Bookings
            .Include(b => b.Tourist)
            .Include(b => b.Payment)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null) return;

        _logger.LogInformation(
            "[Notification] Повернення коштів #{Id}. → Туристу {Tourist}: {Amount} ₴ повернено на рахунок.",
            bookingId, booking.Tourist?.Email, booking.Payment?.Amount);
    }
}
