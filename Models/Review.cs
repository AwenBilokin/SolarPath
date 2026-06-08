using System.ComponentModel.DataAnnotations;

namespace SolarPath.Web.Models;

public class Review
{
    public int Id { get; set; }
    [Range(1, 5)]
    public int Rating { get; set; }
    public string? Text { get; set; }
    public bool IsVerified { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string TouristId       { get; set; } = string.Empty;
    public ApplicationUser Tourist { get; set; } = null!;

    public int RouteId            { get; set; }
    public Route Route            { get; set; } = null!;
}
