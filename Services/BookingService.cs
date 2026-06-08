using Microsoft.EntityFrameworkCore;
using SolarPath.Web.Data;
using SolarPath.Web.Models;

namespace SolarPath.Web.Services;

public class BookingService : IBookingService
{
    private readonly ApplicationDbContext _db;
    public BookingService(ApplicationDbContext db) => _db = db;

    public async Task<bool> CheckAvailabilityAsync(int routeId, DateTime date, int participants)
    {
        var route = await _db.Routes.FindAsync(routeId);
        if (route == null || route.RouteStatus != RouteStatus.Published) return false;

        var booked = await _db.Bookings
            .Where(b => b.RouteId == routeId && b.ScheduledDate.Date == date.Date
                     && b.BookingStatus != BookingStatus.Cancelled
                     && b.BookingStatus != BookingStatus.CancelledByGuide)
            .SumAsync(b => (int?)b.ParticipantsCount) ?? 0;

        return (booked + participants) <= route.MaxParticipants;
    }

    public async Task<Booking> CreateBookingAsync(int routeId, string touristId, DateTime date, int participants)
    {
        var route = await _db.Routes.FindAsync(routeId) ?? throw new InvalidOperationException("Маршрут не знайдено.");
        var booking = new Booking
        {
            RouteId           = routeId,
            TouristId         = touristId,
            ScheduledDate     = date,
            ParticipantsCount = participants,
            TotalPrice        = route.PricePerPerson * participants,
            BookingStatus     = BookingStatus.Pending
        };
        _db.Bookings.Add(booking);
        route.AvailableSlots = Math.Max(0, route.AvailableSlots - participants);
        await _db.SaveChangesAsync();

        // Simulate payment
        var payment = new Payment
        {
            BookingId             = booking.Id,
            Amount                = booking.TotalPrice,
            Currency              = "UAH",
            Method                = PaymentMethod.Card,
            GatewayTransactionId  = Guid.NewGuid().ToString("N")[..12].ToUpper(),
            PaidAt                = DateTime.UtcNow
        };
        _db.Payments.Add(payment);
        booking.BookingStatus = BookingStatus.Paid;
        await _db.SaveChangesAsync();

        return booking;
    }

    public async Task ConfirmBookingAsync(int bookingId)
    {
        var b = await _db.Bookings.FindAsync(bookingId);
        if (b != null) { b.BookingStatus = BookingStatus.Confirmed; b.ConfirmedAt = DateTime.UtcNow; await _db.SaveChangesAsync(); }
    }

    public async Task CancelBookingAsync(int bookingId)
    {
        var b = await _db.Bookings.FindAsync(bookingId);
        if (b != null)
        {
            var route = await _db.Routes.FindAsync(b.RouteId);
            if (route != null) route.AvailableSlots += b.ParticipantsCount;
            b.BookingStatus = BookingStatus.Cancelled;
            await _db.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Booking>> GetByTouristAsync(string touristId) =>
        await _db.Bookings.Include(b => b.Route).Include(b => b.Payment)
            .Where(b => b.TouristId == touristId)
            .OrderByDescending(b => b.CreatedAt).ToListAsync();

    public async Task<IEnumerable<Booking>> GetByGuideAsync(string guideId) =>
        await _db.Bookings.Include(b => b.Route).Include(b => b.Tourist).Include(b => b.Payment)
            .Where(b => b.Route.GuideId == guideId)
            .OrderByDescending(b => b.CreatedAt).ToListAsync();
}
