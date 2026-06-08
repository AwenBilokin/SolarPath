namespace SolarPath.Web.Models;

public class Booking
{
    public int Id                { get; set; }
    public DateTime ScheduledDate { get; set; }
    public int    ParticipantsCount { get; set; }
    public decimal TotalPrice    { get; set; }
    public BookingStatus BookingStatus { get; set; } = BookingStatus.Pending;
    public DateTime CreatedAt    { get; set; } = DateTime.UtcNow;
    public DateTime? ConfirmedAt { get; set; }

    public string TouristId      { get; set; } = string.Empty;
    public ApplicationUser Tourist { get; set; } = null!;

    public int RouteId           { get; set; }
    public Route Route           { get; set; } = null!;

    public Payment? Payment      { get; set; }
}

public class Payment
{
    public int Id                    { get; set; }
    public decimal Amount            { get; set; }
    public string Currency           { get; set; } = "UAH";
    public PaymentMethod Method      { get; set; }
    public string? GatewayTransactionId { get; set; }
    public DateTime PaidAt           { get; set; } = DateTime.UtcNow;
    public DateTime? RefundedAt      { get; set; }

    public int BookingId             { get; set; }
    public Booking Booking           { get; set; } = null!;
}
