namespace SolarPath.Web.Models;

public class Category
{
    public int Id { get; set; }
    public string Name    { get; set; } = string.Empty;
    public string IconUrl { get; set; } = string.Empty;
    public ICollection<Route> Routes { get; set; } = new List<Route>();
}
