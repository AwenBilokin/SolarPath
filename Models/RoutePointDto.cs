namespace SolarPath.Web.Models;

/// <summary>
/// DTO для прийому точок маршруту з форми (уникає конфлікту з EF-трекінгом RoutePoint)
/// </summary>
public class RoutePointDto
{
    public double   Latitude   { get; set; }
    public double   Longitude  { get; set; }
    public string   Title      { get; set; } = "";
    public PointType PointType { get; set; }
    public int      OrderIndex { get; set; }
}
