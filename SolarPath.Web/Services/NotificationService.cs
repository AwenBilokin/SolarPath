using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using MimeKit;
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

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<NotificationService> _logger;
    private readonly IConfiguration _config;

    public NotificationService(ApplicationDbContext db,
        ILogger<NotificationService> logger,
        IConfiguration config)
    {
        _db = db; _logger = logger; _config = config;
    }

    public async Task NotifyBookingCreatedAsync(int bookingId)
    {
        var b = await LoadBookingAsync(bookingId);
        if (b == null) return;
        await SendAsync(b.Tourist!.Email!, b.Tourist.FirstName + ' ' + b.Tourist.LastName,
            'Оплата підтверджена — ' + b.Route!.Title,
            TouristBookingCreatedHtml(b));
        await SendAsync(b.Route.Guide!.Email!, b.Route.Guide.FirstName + ' ' + b.Route.Guide.LastName,
            'Нове бронювання — ' + b.Route.Title,
            GuideBookingCreatedHtml(b));
    }

    public async Task NotifyBookingConfirmedAsync(int bookingId)
    {
        var b = await LoadBookingAsync(bookingId);
        if (b == null) return;
        await SendAsync(b.Tourist!.Email!, b.Tourist.FirstName + ' ' + b.Tourist.LastName,
            'Бронювання підтверджено — ' + b.Route!.Title,
            ConfirmedHtml(b));
    }

    public async Task NotifyBookingCancelledAsync(int bookingId)
    {
        var b = await LoadBookingAsync(bookingId);
        if (b == null) return;
        await SendAsync(b.Tourist!.Email!, b.Tourist.FirstName + ' ' + b.Tourist.LastName,
            'Бронювання скасовано — ' + b.Route!.Title,
            CancelledHtml(b));
    }

    public async Task NotifyBookingCompletedAsync(int bookingId)
    {
        var b = await LoadBookingAsync(bookingId);
        if (b == null) return;
        await SendAsync(b.Tourist!.Email!, b.Tourist.FirstName + ' ' + b.Tourist.LastName,
            'Як вам маршрут? Залиште відгук — ' + b.Route!.Title,
            CompletedHtml(b));
    }

    public async Task NotifyRefundProcessedAsync(int bookingId)
    {
        var b = await LoadBookingAsync(bookingId);
        if (b == null) return;
        await SendAsync(b.Tourist!.Email!, b.Tourist.FirstName + ' ' + b.Tourist.LastName,
            'Повернення коштів — ' + b.Route!.Title,
            RefundHtml(b));
    }

    // ── Send ──────────────────────────────────────────────────────────

    private async Task SendAsync(string to, string toName, string subject, string html)
    {
        var host     = _config["Smtp:Host"];
        var username = _config["Smtp:Username"];
        var password = _config["Smtp:Password"];
        var fromEmail = _config["Smtp:FromEmail"] ?? username;
        var fromName  = _config["Smtp:FromName"]  ?? "SolarPath";

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password) || (username?.Contains("your-gmail") ?? true))
        {
            _logger.LogInformation("[Email STUB] To: {To} | Subject: {Subject}", to, subject);
            return;
        }

        try
        {
            var msg = new MimeMessage();
            msg.From.Add(new MailboxAddress(fromName, fromEmail));
            msg.To.Add(new MailboxAddress(toName, to));
            msg.Subject = subject;
            msg.Body    = new BodyBuilder { HtmlBody = html }.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(host,
                int.Parse(_config["Smtp:Port"] ?? "587"),
                SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(username, password);
            await client.SendAsync(msg);
            await client.DisconnectAsync(true);

            _logger.LogInformation("[Email] Sent to {To}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Email] Failed to {To}", to);
        }
    }

    private async Task<Booking?> LoadBookingAsync(int id) =>
        await _db.Bookings
            .Include(b => b.Route).ThenInclude(r => r.Guide)
            .Include(b => b.Tourist)
            .Include(b => b.Payment)
            .FirstOrDefaultAsync(b => b.Id == id);

    // ── HTML helpers ──────────────────────────────────────────────────

    private static string Wrap(string body)
    {
        return "<!DOCTYPE html><html lang='uk'><head><meta charset='utf-8'></head>" +
            "<body style='margin:0;padding:0;background:#f4f4f4;font-family:Arial,sans-serif;'>" +
            "<table width='100%' cellpadding='0' cellspacing='0' style='padding:30px 0;'>" +
            "<tr><td align='center'>" +
            "<table width='580' cellpadding='0' cellspacing='0' style='background:#fff;border-radius:12px;overflow:hidden;box-shadow:0 2px 12px rgba(0,0,0,.08);'>" +
            "<tr><td style='background:#16a34a;padding:24px 32px;'>" +
            "<h1 style='margin:0;color:#fff;font-size:22px;'>&#9728; SolarPath</h1>" +
            "<p style='margin:4px 0 0;color:rgba(255,255,255,.8);font-size:12px;'>Туристичні маршрути України</p>" +
            "</td></tr>" +
            "<tr><td style='padding:28px 32px;'>" + body + "</td></tr>" +
            "<tr><td style='background:#f9fafb;padding:16px 32px;border-top:1px solid #e5e7eb;'>" +
            "<p style='margin:0;color:#9ca3af;font-size:11px;text-align:center;'>" +
            "&copy; 2026 SolarPath &middot; <a href='https://solarpath.up.railway.app' style='color:#16a34a;'>solarpath.up.railway.app</a></p>" +
            "</td></tr>" +
            "</table></td></tr></table></body></html>";
    }

    private static string InfoRow(string label, string value) =>
        "<tr><td style='padding:7px 10px;color:#6b7280;font-size:13px;border-bottom:1px solid #f3f4f6;'>" + label + "</td>" +
        "<td style='padding:7px 10px;color:#111;font-size:13px;font-weight:600;border-bottom:1px solid #f3f4f6;'>" + value + "</td></tr>";

    private static string InfoBox(Booking b) =>
        "<table width='100%' cellpadding='0' cellspacing='0' style='background:#f9fafb;border-radius:8px;margin:18px 0;border:1px solid #e5e7eb;'>" +
        InfoRow("Маршрут",   b.Route!.Title) +
        InfoRow("Дата",      b.ScheduledDate.ToString("dd.MM.yyyy")) +
        InfoRow("Учасники",  b.ParticipantsCount + " ос.") +
        InfoRow("Сума",      b.TotalPrice.ToString("N0") + " &#8372;") +
        InfoRow("Бронювання","#" + b.Id) +
        "</table>";

    private static string Btn(string url, string text) =>
        "<a href='" + url + "' style='display:inline-block;background:#16a34a;color:#fff;text-decoration:none;" +
        "padding:11px 26px;border-radius:8px;font-size:14px;font-weight:600;margin-top:14px;'>" + text + "</a>";

    private static string H2(string t) =>
        "<h2 style='margin:0 0 6px;color:#111;font-size:19px;'>" + t + "</h2>";

    private static string Sub(string t) =>
        "<p style='color:#6b7280;margin:0 0 18px;font-size:14px;'>" + t + "</p>";

    // ── Templates ─────────────────────────────────────────────────────

    private static string TouristBookingCreatedHtml(Booking b) => Wrap(
        H2("Вашу оплату прийнято! ") +
        Sub("Привіт, <strong>" + b.Tourist!.FirstName + "</strong>! Бронювання оплачено та очікує підтвердження від гіда.") +
        InfoBox(b) +
        Btn("https://solarpath.up.railway.app/Booking/MyBookings", "Мої бронювання"));

    private static string GuideBookingCreatedHtml(Booking b) => Wrap(
        H2("Нове бронювання на ваш маршрут") +
        Sub("Турист <strong>" + b.Tourist!.FirstName + " " + b.Tourist.LastName + "</strong> (" + b.Tourist.Email + ") забронював ваш маршрут.") +
        InfoBox(b) +
        Btn("https://solarpath.up.railway.app/Guide/Dashboard", "Перейти до кабінету"));

    private static string ConfirmedHtml(Booking b) => Wrap(
        H2("Бронювання підтверджено!") +
        Sub("Привіт, <strong>" + b.Tourist!.FirstName + "</strong>! Гід підтвердив ваше бронювання. Готуйтесь!") +
        InfoBox(b) +
        Btn("https://solarpath.up.railway.app/Routes/Details/" + b.RouteId, "Деталі маршруту"));

    private static string CancelledHtml(Booking b) => Wrap(
        H2("Бронювання скасовано") +
        Sub("Привіт, <strong>" + b.Tourist!.FirstName + "</strong>. На жаль, бронювання скасовано.") +
        InfoBox(b) +
        Btn("https://solarpath.up.railway.app/Routes", "Обрати інший маршрут"));

    private static string CompletedHtml(Booking b) => Wrap(
        H2("Як вам пригода?") +
        Sub("Привіт, <strong>" + b.Tourist!.FirstName + "</strong>! Похід завершено. Залиште відгук — це допоможе іншим туристам.") +
        Btn("https://solarpath.up.railway.app/Reviews/Create/" + b.RouteId, "Залишити відгук"));

    private static string RefundHtml(Booking b) => Wrap(
        H2("Повернення коштів оброблено") +
        Sub("Привіт, <strong>" + b.Tourist!.FirstName + "</strong>! Повернення за маршрут <strong>" + b.Route!.Title + "</strong> успішно оброблено.") +
        "<table width='100%' cellpadding='0' cellspacing='0' style='background:#f9fafb;border-radius:8px;margin:18px 0;border:1px solid #e5e7eb;'>" +
        InfoRow("Сума повернення", (b.Payment?.Amount.ToString("N0") ?? "0") + " &#8372;") +
        InfoRow("Бронювання", "#" + b.Id) +
        InfoRow("Термін", "5–10 робочих днів") +
        "</table>" +
        Btn("https://solarpath.up.railway.app/Routes", "Обрати інший маршрут"));
}
