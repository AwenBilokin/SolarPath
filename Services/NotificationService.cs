namespace SolarPath.Web.Services;

/// <summary>
/// Сервіс сповіщень — надсилає підтвердження туристу та повідомлення гіду.
/// У поточній версії реалізовано як заглушка (stub); 
/// продакшн-інтеграція з SMTP планується у наступній ітерації.
/// </summary>
public interface INotificationService
{
    Task NotifyBookingCreatedAsync(int bookingId);
    Task NotifyBookingConfirmedAsync(int bookingId);
    Task NotifyBookingCancelledAsync(int bookingId);
}

public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;
    public NotificationService(ILogger<NotificationService> logger) => _logger = logger;

    public Task NotifyBookingCreatedAsync(int bookingId)
    {
        _logger.LogInformation("[NotificationService] Booking {Id} created — tourist & guide notified (stub).", bookingId);
        return Task.CompletedTask;
    }

    public Task NotifyBookingConfirmedAsync(int bookingId)
    {
        _logger.LogInformation("[NotificationService] Booking {Id} confirmed — tourist notified (stub).", bookingId);
        return Task.CompletedTask;
    }

    public Task NotifyBookingCancelledAsync(int bookingId)
    {
        _logger.LogInformation("[NotificationService] Booking {Id} cancelled — tourist notified (stub).", bookingId);
        return Task.CompletedTask;
    }
}
