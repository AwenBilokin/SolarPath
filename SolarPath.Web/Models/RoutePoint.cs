namespace SolarPath.Web.Models;

public class RoutePoint
{
    public int Id          { get; set; }
    public string Title    { get; set; } = string.Empty;
    public string? Description { get; set; }
    public double Latitude  { get; set; }
    public double Longitude { get; set; }
    public PointType PointType { get; set; }
    public int OrderIndex  { get; set; }

    public int RouteId     { get; set; }
    public Route Route     { get; set; } = null!;
}
