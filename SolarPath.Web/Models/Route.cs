namespace SolarPath.Web.Models;

public class Route
{
    public int Id { get; set; }
    public string Title            { get; set; } = string.Empty;
    public string Description      { get; set; } = string.Empty;
    public DifficultyLevel Difficulty { get; set; }
    public double DistanceKm       { get; set; }
    public int    DurationMinutes  { get; set; }
    public int    MaxParticipants  { get; set; }
    public int    AvailableSlots   { get; set; }
    public decimal PricePerPerson  { get; set; }
    public DateTime? SeasonStart   { get; set; }
    public DateTime? SeasonEnd     { get; set; }
    public string? GeoData         { get; set; }
    public string? ImageUrl        { get; set; }
    public RouteStatus RouteStatus { get; set; } = RouteStatus.Draft;
    public DateTime CreatedAt      { get; set; } = DateTime.UtcNow;

    public int CategoryId          { get; set; }
    public Category Category       { get; set; } = null!;

    public string GuideId          { get; set; } = string.Empty;
    public ApplicationUser Guide   { get; set; } = null!;

    public ICollection<RoutePoint> Points   { get; set; } = new List<RoutePoint>();
    public ICollection<Booking>    Bookings { get; set; } = new List<Booking>();
    public ICollection<Review>     Reviews  { get; set; } = new List<Review>();

    public double AverageRating =>
        Reviews.Any() ? Reviews.Average(r => (double)r.Rating) : 0;
}
