using SolarPath.Web.Models;

namespace SolarPath.Web.Services;

public interface IBookingService
{
    Task<bool> CheckAvailabilityAsync(int routeId, DateTime date, int participants);
    Task<Booking> CreateBookingAsync(int routeId, string touristId, DateTime date, int participants);
    Task ConfirmBookingAsync(int bookingId);
    Task CancelBookingAsync(int bookingId);
    Task<IEnumerable<Booking>> GetByTouristAsync(string touristId);
    Task<IEnumerable<Booking>> GetByGuideAsync(string guideId);
}
