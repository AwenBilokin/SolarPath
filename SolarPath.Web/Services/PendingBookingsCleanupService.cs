using Microsoft.EntityFrameworkCore;
using SolarPath.Web.Data;
using SolarPath.Web.Models;

namespace SolarPath.Web.Services;

/// <summary>
/// Фоновий сервіс: автоматично скасовує бронювання у статусі Pending
/// (тобто очікують оплату), якщо вони "зависли" довше PendingTimeout.
/// Звільняє зайняті слоти маршруту.
/// </summary>
public class PendingBookingsCleanupService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<PendingBookingsCleanupService> _logger;

    // Скільки часу бронювання може чекати на оплату до автоматичного скасування
    private static readonly TimeSpan PendingTimeout = TimeSpan.FromMinutes(10);

    // Як часто перевіряти
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(1);

    public PendingBookingsCleanupService(IServiceProvider services, ILogger<PendingBookingsCleanupService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupExpiredBookingsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка під час очищення зависших Pending-бронювань");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }
    }

    private async Task CleanupExpiredBookingsAsync(CancellationToken ct)
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var cutoff = DateTime.UtcNow - PendingTimeout;

        var expired = await db.Bookings
            .Where(b => b.BookingStatus == BookingStatus.Pending && b.CreatedAt < cutoff)
            .ToListAsync(ct);

        if (expired.Count == 0) return;

        foreach (var booking in expired)
        {
            booking.BookingStatus = BookingStatus.Cancelled;

            var route = await db.Routes.FindAsync(new object?[] { booking.RouteId }, ct);
            if (route != null)
                route.AvailableSlots += booking.ParticipantsCount;

            _logger.LogInformation(
                "Бронювання #{BookingId} автоматично скасовано — очікування оплати перевищило {Timeout} хв.",
                booking.Id, PendingTimeout.TotalMinutes);
        }

        await db.SaveChangesAsync(ct);
    }
}
