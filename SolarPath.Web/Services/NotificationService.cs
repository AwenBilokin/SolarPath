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
        _db = db;
        _logger = logger;
        _config = config;
    }

    // ── Публічні методи ────────────────────────────────────────────────

    public async Task NotifyBookingCreatedAsync(int bookingId)
    {
        var b = await LoadBookingAsync(bookingId);
        if (b == null) return;

        // Туристу: підтвердження оплати
        await SendAsync(
            to: b.Tourist!.Email!,
            toName: $"{b.Tourist.FirstName} {b.Tourist.LastName}",
            subject: $"✅ Оплата підтверджена — {b.Route!.Title}",
            html: TouristBookingCreatedHtml(b)
        );

        // Гіду: новий турист
        await SendAsync(
            to: b.Route.Guide!.Email!,
            toName: $"{b.Route.Guide.FirstName} {b.Route.Guide.LastName}",
            subject: $"🔔 Нове бронювання — {b.Route.Title}",
            html: GuideBookingCreatedHtml(b)
        );
    }

    public async Task NotifyBookingConfirmedAsync(int bookingId)
    {
        var b = await LoadBookingAsync(bookingId);
        if (b == null) return;

        await SendAsync(
            to: b.Tourist!.Email!,
            toName: $"{b.Tourist.FirstName} {b.Tourist.LastName}",
            subject: $"🎉 Бронювання підтверджено — {b.Route!.Title}",
            html: ConfirmedHtml(b)
        );
    }

    public async Task NotifyBookingCancelledAsync(int bookingId)
    {
        var b = await LoadBookingAsync(bookingId);
        if (b == null) return;

        await SendAsync(
            to: b.Tourist!.Email!,
            toName: $"{b.Tourist.FirstName} {b.Tourist.LastName}",
            subject: $"❌ Бронювання скасовано — {b.Route!.Title}",
            html: CancelledHtml(b)
        );
    }

    public async Task NotifyBookingCompletedAsync(int bookingId)
    {
        var b = await LoadBookingAsync(bookingId);
        if (b == null) return;

        await SendAsync(
            to: b.Tourist!.Email!,
            toName: $"{b.Tourist.FirstName} {b.Tourist.LastName}",
            subject: $"⭐ Як вам маршрут? Залиште відгук — {b.Route!.Title}",
            html: CompletedHtml(b)
        );
    }

    public async Task NotifyRefundProcessedAsync(int bookingId)
    {
        var b = await LoadBookingAsync(bookingId);
        if (b == null) return;

        await SendAsync(
            to: b.Tourist!.Email!,
            toName: $"{b.Tourist.FirstName} {b.Tourist.LastName}",
            subject: $"💰 Повернення коштів — {b.Route!.Title}",
            html: RefundHtml(b)
        );
    }

    // ── Відправка через SMTP ───────────────────────────────────────────

    private async Task SendAsync(string to, string toName, string subject, string html)
    {
        var host = _config["Smtp:Host"];
        var username = _config["Smtp:Username"];
        var password = _config["Smtp:Password"];
        var fromEmail = _config["Smtp:FromEmail"] ?? username;
        var fromName = _config["Smtp:FromName"] ?? "SolarPath";

        // Якщо SMTP не налаштовано — тільки лог
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password) || username.Contains("your-gmail"))
        {
            _logger.LogInformation("[Email STUB] To: {To} | Subject: {Subject}", to, subject);
            return;
        }

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(new MailboxAddress(toName, to));
            message.Subject = subject;
            message.Body = new BodyBuilder { HtmlBody = html }.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(host,
                int.Parse(_config["Smtp:Port"] ?? "587"),
                SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(username, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("[Email] Надіслано → {To} | {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Email] Помилка відправки → {To} | {Subject}", to, subject);
            // Не кидаємо виключення — email не повинен ламати основний flow
        }
    }

    // ── Завантаження бронювання ────────────────────────────────────────

    private async Task<Booking?> LoadBookingAsync(int bookingId) =>
        await _db.Bookings
            .Include(b => b.Route).ThenInclude(r => r.Guide)
            .Include(b => b.Tourist)
            .Include(b => b.Payment)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

    // ── HTML-шаблони листів ────────────────────────────────────────────

    private static string BaseHtml(string content) => $"""
        <!DOCTYPE html>
        <html lang="uk">
        <head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"></head>
        <body style="margin:0;padding:0;background:#f4f4f4;font-family:'Helvetica Neue',Arial,sans-serif;">
        <table width="100%" cellpadding="0" cellspacing="0" style="background:#f4f4f4;padding:30px 0;">
          <tr><td align="center">
            <table width="600" cellpadding="0" cellspacing="0" style="background:#fff;border-radius:12px;overflow:hidden;box-shadow:0 2px 12px rgba(0,0,0,.08);">
              <!-- HEADER -->
              <tr><td style="background:#16a34a;padding:28px 32px;">
                <h1 style="margin:0;color:#fff;font-size:24px;font-weight:700;">☀ SolarPath</h1>
                <p style="margin:4px 0 0;color:rgba(255,255,255,.8);font-size:13px;">Туристичні маршрути України</p>
              </td></tr>
              <!-- BODY -->
              <tr><td style="padding:32px;">
                {content}
              </td></tr>
              <!-- FOOTER -->
              <tr><td style="background:#f9fafb;padding:20px 32px;border-top:1px solid #e5e7eb;">
                <p style="margin:0;color:#9ca3af;font-size:12px;text-align:center;">
                  © 2026 SolarPath · <a href="https://solarpath.up.railway.app" style="color:#16a34a;">solarpath.up.railway.app</a><br>
                  Якщо ви не здійснювали це бронювання — проігноруйте цей лист.
                </p>
              </td></tr>
            </table>
          </td></tr>
        </table>
        </body></html>
        """;

    private static string InfoRow(string label, string value) =>
        $"""<tr>
           <td style="padding:8px 12px;color:#6b7280;font-size:13px;white-space:nowrap;border-bottom:1px solid #f3f4f6;">{label}</td>
           <td style="padding:8px 12px;color:#111827;font-size:13px;font-weight:600;border-bottom:1px solid #f3f4f6;">{value}</td>
         </tr>""";

    private static string InfoTable(Booking b) => $"""
        <table width="100%" cellpadding="0" cellspacing="0"
               style="background:#f9fafb;border-radius:8px;margin:20px 0;border:1px solid #e5e7eb;">
          {InfoRow("📍 Маршрут", b.Route!.Title)}
          {InfoRow("📅 Дата", b.ScheduledDate.ToString("dd MMMM yyyy"))}
          {InfoRow("👥 Учасники", $"{b.ParticipantsCount} ос.")}
          {InfoRow("💰 Сума", $"{b.TotalPrice:N0} ₴")}
          {InfoRow("🔖 Бронювання №", b.Id.ToString())}
        </table>
        """;

    private static string Btn(string url, string text) =>
        $"""<a href="{url}" style="display:inline-block;background:#16a34a;color:#fff;text-decoration:none;padding:12px 28px;border-radius:8px;font-size:15px;font-weight:600;margin-top:16px;">{text}</a>""";

    private string TouristBookingCreatedHtml(Booking b) => BaseHtml($"""
        <h2 style="margin:0 0 8px;color:#111827;font-size:20px;">Вашу оплату прийнято! 🎉</h2>
        <p style="color:#6b7280;margin:0 0 20px;">Привіт, <strong>{b.Tourist!.FirstName}</strong>! Ваше бронювання успішно оплачено та очікує підтвердження від гіда.</p>
        {InfoTable(b)}
        <p style="color:#6b7280;font-size:13px;">Гід підтвердить бронювання протягом 24 годин. Ви отримаєте додатковий лист.</p>
        {Btn("https://solarpath.up.railway.app/Booking/MyBookings", "Мої бронювання")}
        """);

    private string GuideBookingCreatedHtml(Booking b) => BaseHtml($"""
        <h2 style="margin:0 0 8px;color:#111827;font-size:20px;">Нове бронювання на ваш маршрут 🔔</h2>
        <p style="color:#6b7280;margin:0 0 20px;">Турист <strong>{b.Tourist!.FirstName} {b.Tourist.LastName}</strong> ({b.Tourist.Email}) забронював ваш маршрут.</p>
        {InfoTable(b)}
        <p style="color:#6b7280;font-size:13px;">Підтвердіть або відхиліть бронювання у вашому кабінеті.</p>
        {Btn("https://solarpath.up.railway.app/Guide/Dashboard", "Перейти до кабінету")}
        """);

    private string ConfirmedHtml(Booking b) => BaseHtml($"""
        <h2 style="margin:0 0 8px;color:#111827;font-size:20px;">Бронювання підтверджено! ✅</h2>
        <p style="color:#6b7280;margin:0 0 20px;">Привіт, <strong>{b.Tourist!.FirstName}</strong>! Гід підтвердив ваше бронювання. Готуйтесь до пригоди!</p>
        {InfoTable(b)}
        <p style="color:#6b7280;font-size:13px;">Зустрічайтесь з гідом у точці старту в зазначену дату та час.</p>
        {Btn($"https://solarpath.up.railway.app/Routes/Details/{b.RouteId}", "Деталі маршруту")}
        """);

    private string CancelledHtml(Booking b) => BaseHtml($"""
        <h2 style="margin:0 0 8px;color:#111827;font-size:20px;">Бронювання скасовано ❌</h2>
        <p style="color:#6b7280;margin:0 0 20px;">Привіт, <strong>{b.Tourist!.FirstName}</strong>. На жаль, бронювання скасовано.</p>
        {InfoTable(b)}
        <p style="color:#6b7280;font-size:13px;">Якщо кошти були сплачені — повернення буде оброблено протягом 5–10 робочих днів.</p>
        {Btn("https://solarpath.up.railway.app/Routes", "Обрати інший маршрут")}
        """);

    private string CompletedHtml(Booking b) => BaseHtml($"""
        <h2 style="margin:0 0 8px;color:#111827;font-size:20px;">Як вам пригода? ⭐</h2>
        <p style="color:#6b7280;margin:0 0 20px;">Привіт, <strong>{b.Tourist!.FirstName}</strong>! Ваш похід <strong>«{b.Route!.Title}»</strong> завершено. Сподіваємось, враження незабутні!</p>
        <p style="color:#6b7280;margin:0 0 20px;">Залиште відгук — це допоможе іншим туристам обрати кращий маршрут.</p>
        {Btn($"https://solarpath.up.railway.app/Reviews/Create/{b.RouteId}", "Залишити відгук")}
        """);

    private string RefundHtml(Booking b) => BaseHtml($"""
        <h2 style="margin:0 0 8px;color:#111827;font-size:20px;">Повернення коштів оброблено 💰</h2>
        <p style="color:#6b7280;margin:0 0 20px;">Привіт, <strong>{b.Tourist!.FirstName}</strong>! Повернення коштів за бронювання <strong>«{b.Route!.Title}»</strong> успішно оброблено.</p>
        <table width="100%" cellpadding="0" cellspacing="0" style="background:#f9fafb;border-radius:8px;margin:20px 0;border:1px solid #e5e7eb;">
          {InfoRow("💰 Сума повернення", $"{b.Payment?.Amount:N0} ₴")}
          {InfoRow("🔖 Бронювання №", b.Id.ToString())}
          {InfoRow("⏱ Термін зарахування", "5–10 робочих днів")}
        </table>
        {Btn("https://solarpath.up.railway.app/Routes", "Обрати інший маршрут")}
        """);
}
