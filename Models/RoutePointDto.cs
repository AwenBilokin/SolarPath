namespace SolarPath.Web.Models;

/// <summary>
/// DTO для прийому точок маршруту з форми (уникає конфлікту з EF-трекінгом RoutePoint).
/// Latitude/Longitude приймаються як рядки щоб уникнути проблем з культурою сервера
/// (крапка vs кома як десятковий роздільник).
/// </summary>
public class RoutePointDto
{
    public string   LatitudeStr  { get; set; } = "0";
    public string   LongitudeStr { get; set; } = "0";
    public string   Title        { get; set; } = "";
    public PointType PointType   { get; set; }
    public int      OrderIndex   { get; set; }

    public double Latitude  => double.TryParse(LatitudeStr,  System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : 0;
    public double Longitude => double.TryParse(LongitudeStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : 0;
}
