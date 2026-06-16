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
    Task SendPaymentReceiptAsync(int bookingId);
}

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<NotificationService> _logger;
    private readonly IConfiguration _config;

    public NotificationService(ApplicationDbContext db, ILogger<NotificationService> logger, IConfiguration config)
    { _db = db; _logger = logger; _config = config; }

    // ── Email helper ──────────────────────────────────────────────────────
    private async Task SendEmailAsync(string toEmail, string toName, string subject, string htmlBody)
    {
        var host     = _config["Email:SmtpHost"];
        var portStr  = _config["Email:SmtpPort"];
        var user     = _config["Email:Username"];
        var pass     = _config["Email:Password"];
        var fromName = _config["Email:FromName"] ?? "SolarPath";

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
        {
            _logger.LogWarning("[Email] SMTP не налаштовано — пропускаємо надсилання до {Email}", toEmail);
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, user));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlBody };

        try
        {
            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(host, int.Parse(portStr ?? "587"), SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(user, pass);
            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);
            _logger.LogInformation("[Email] Надіслано до {Email}: {Subject}", toEmail, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Email] Помилка надсилання до {Email}", toEmail);
        }
    }

    private static string EmailLayout(string title, string body)
    {
        var css = @"
          body { font-family: 'Inter', Arial, sans-serif; background:#f6f8f6; margin:0; padding:0; }
          .wrap { max-width:560px; margin:32px auto; background:#fff; border-radius:14px; overflow:hidden; box-shadow:0 2px 12px rgba(0,0,0,.08); }
          .head { background:#15803d; padding:28px 32px; color:#fff; }
          .head h1 { margin:0; font-size:1.3rem; font-weight:700; }
          .head p  { margin:4px 0 0; opacity:.8; font-size:.85rem; }
          .body  { padding:28px 32px; color:#1a2e1f; font-size:.93rem; line-height:1.6; }
          .info-row { display:flex; justify-content:space-between; padding:8px 0; border-bottom:1px solid #f0f0f0; }
          .info-row:last-child { border-bottom:none; }
          .label { color:#6b7280; font-size:.85rem; }
          .value { font-weight:600; }
          .total { background:#f0fdf4; border-radius:8px; padding:14px 18px; margin:18px 0; display:flex; justify-content:space-between; align-items:center; }
          .total .amount { font-size:1.4rem; font-weight:800; color:#15803d; }
          .btn { display:inline-block; background:#15803d; color:#fff; text-decoration:none; border-radius:8px; padding:10px 22px; font-weight:600; font-size:.9rem; margin-top:16px; }
          .foot { background:#f6f8f6; padding:14px 32px; text-align:center; color:#9ca3af; font-size:.78rem; }";

        return $@"<!DOCTYPE html>
<html lang=""uk"">
<head><meta charset=""utf-8""/><style>{css}</style></head>
<body>
  <div class=""wrap"">
    <div class=""head""><h1>☀️ SolarPath</h1><p>{title}</p></div>
    <div class=""body"">{body}</div>
    <div class=""foot"">SolarPath · Туристичні маршрути України · Автоматичне повідомлення</div>
  </div>
</body></html>";
    }

    // ── Квитанція про оплату ──────────────────────────────────────────────
    public async Task SendPaymentReceiptAsync(int bookingId)
    {
        var booking = await _db.Bookings
            .Include(b => b.Route).ThenInclude(r => r.Guide)
            .Include(b => b.Tourist)
            .Include(b => b.Payment)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking?.Tourist?.Email == null) return;

        var body = $"""
            <p>Вітаємо, <strong>{booking.Tourist.FullName}</strong>!</p>
            <p>Ваша оплата успішно прийнята. Нижче деталі бронювання:</p>
            <div class="info-row"><span class="label">Маршрут</span><span class="value">{booking.Route?.Title}</span></div>
            <div class="info-row"><span class="label">Дата походу</span><span class="value">{booking.ScheduledDate:dd.MM.yyyy}</span></div>
            <div class="info-row"><span class="label">Учасників</span><span class="value">{booking.ParticipantsCount} осіб</span></div>
            <div class="info-row"><span class="label">Гід</span><span class="value">{booking.Route?.Guide?.FullName}</span></div>
            <div class="info-row"><span class="label">Номер бронювання</span><span class="value">#{booking.Id}</span></div>
            <div class="total"><span>Сплачено</span><span class="amount">{booking.TotalPrice:N0} ₴</span></div>
            <p style="color:#6b7280;font-size:.85rem;">Гід зв'яжеться з вами для підтвердження. Очікуйте повідомлення.</p>
            """;

        await SendEmailAsync(
            booking.Tourist.Email,
            booking.Tourist.FullName,
            $"✅ Квитанція про оплату — {booking.Route?.Title}",
            EmailLayout("Квитанція про оплату", body));
    }

    // ── Решта сповіщень ───────────────────────────────────────────────────
    public async Task NotifyBookingCreatedAsync(int bookingId)
    {
        var booking = await _db.Bookings
            .Include(b => b.Route).ThenInclude(r => r.Guide)
            .Include(b => b.Tourist)
            .FirstOrDefaultAsync(b => b.Id == bookingId);
        if (booking == null) return;

        _logger.LogInformation("[Notification] Бронювання #{Id} створено. Турист: {Tourist}, Маршрут: {Route}",
            bookingId, booking.Tourist?.Email, booking.Route?.Title);

        if (booking.Route?.Guide?.Email != null)
        {
            var body = $"""
                <p>Новий турист забронював ваш маршрут.</p>
                <div class="info-row"><span class="label">Маршрут</span><span class="value">{booking.Route.Title}</span></div>
                <div class="info-row"><span class="label">Турист</span><span class="value">{booking.Tourist?.FullName}</span></div>
                <div class="info-row"><span class="label">Дата</span><span class="value">{booking.ScheduledDate:dd.MM.yyyy}</span></div>
                <div class="info-row"><span class="label">Учасників</span><span class="value">{booking.ParticipantsCount}</span></div>
                <p style="color:#6b7280;font-size:.85rem;">Після оплати туристом — підтвердіть бронювання в кабінеті гіда.</p>
                """;
            await SendEmailAsync(booking.Route.Guide.Email, booking.Route.Guide.FullName,
                $"📋 Нове бронювання — {booking.Route.Title}", EmailLayout("Нове бронювання", body));
        }
    }

    public async Task NotifyBookingConfirmedAsync(int bookingId)
    {
        var booking = await _db.Bookings
            .Include(b => b.Route)
            .Include(b => b.Tourist)
            .FirstOrDefaultAsync(b => b.Id == bookingId);
        if (booking?.Tourist?.Email == null) return;

        var body = $"""
            <p>Вітаємо! Ваш похід підтверджено гідом.</p>
            <div class="info-row"><span class="label">Маршрут</span><span class="value">{booking.Route?.Title}</span></div>
            <div class="info-row"><span class="label">Дата</span><span class="value">{booking.ScheduledDate:dd.MM.yyyy}</span></div>
            <p style="color:#6b7280;font-size:.85rem;">Гід зв'яжеться з вами для уточнення деталей зустрічі.</p>
            """;
        await SendEmailAsync(booking.Tourist.Email, booking.Tourist.FullName,
            $"🎉 Похід підтверджено — {booking.Route?.Title}", EmailLayout("Бронювання підтверджено", body));
    }

    public async Task NotifyBookingCancelledAsync(int bookingId)
    {
        var booking = await _db.Bookings
            .Include(b => b.Route)
            .Include(b => b.Tourist)
            .FirstOrDefaultAsync(b => b.Id == bookingId);
        if (booking?.Tourist?.Email == null) return;

        var body = $"""
            <p>На жаль, ваше бронювання скасовано.</p>
            <div class="info-row"><span class="label">Маршрут</span><span class="value">{booking.Route?.Title}</span></div>
            <div class="info-row"><span class="label">Дата</span><span class="value">{booking.ScheduledDate:dd.MM.yyyy}</span></div>
            <p style="color:#6b7280;font-size:.85rem;">Якщо оплата була здійснена — зверніться до адміністратора для повернення коштів.</p>
            """;
        await SendEmailAsync(booking.Tourist.Email, booking.Tourist.FullName,
            $"❌ Бронювання скасовано — {booking.Route?.Title}", EmailLayout("Бронювання скасовано", body));
    }

    public async Task NotifyBookingCompletedAsync(int bookingId)
    {
        var booking = await _db.Bookings
            .Include(b => b.Route)
            .Include(b => b.Tourist)
            .FirstOrDefaultAsync(b => b.Id == bookingId);
        if (booking?.Tourist?.Email == null) return;

        var body = $"""
            <p>Похід завершено! Сподіваємось, вам сподобалось 🌿</p>
            <div class="info-row"><span class="label">Маршрут</span><span class="value">{booking.Route?.Title}</span></div>
            <p>Залиште відгук — це допоможе іншим туристам обрати маршрут.</p>
            <a class="btn" href="https://solarpath.up.railway.app/Reviews/Create?bookingId={booking.Id}">Залишити відгук</a>
            """;
        await SendEmailAsync(booking.Tourist.Email, booking.Tourist.FullName,
            $"⭐ Як вам похід? — {booking.Route?.Title}", EmailLayout("Похід завершено", body));
    }

    public async Task NotifyRefundProcessedAsync(int bookingId)
    {
        var booking = await _db.Bookings
            .Include(b => b.Tourist)
            .Include(b => b.Payment)
            .FirstOrDefaultAsync(b => b.Id == bookingId);
        if (booking?.Tourist?.Email == null) return;

        var body = $"""
            <p>Повернення коштів оброблено.</p>
            <div class="total"><span>Повернено</span><span class="amount">{booking.Payment?.Amount:N0} ₴</span></div>
            <p style="color:#6b7280;font-size:.85rem;">Кошти надійдуть на рахунок протягом 5-10 робочих днів залежно від вашого банку.</p>
            """;
        await SendEmailAsync(booking.Tourist.Email, booking.Tourist.FullName,
            "💸 Повернення коштів — SolarPath", EmailLayout("Повернення коштів", body));
    }
}
