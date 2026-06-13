namespace SolarPath.Web.Models;

/// <summary>
/// Допоміжний клас для визначення SVG-ілюстрації за категорією маршруту.
/// Використовується як fallback, коли ImageUrl не задано або фото не завантажилось.
/// </summary>
public static class CategoryImages
{
    // CategoryId -> локальна SVG-ілюстрація
    private static readonly Dictionary<int, string> ById = new()
    {
        [1] = "/images/routes/forest-1.svg",     // Пішохідні
        [2] = "/images/routes/bike-1.svg",        // Велосипедні
        [3] = "/images/routes/culture-1.svg",     // Культурні
        [4] = "/images/routes/water-1.svg",       // Водні
        [5] = "/images/routes/mountains-1.svg",   // Гірські
    };

    private static readonly Dictionary<string, string> ByName = new()
    {
        ["Пішохідні"]   = "/images/routes/forest-1.svg",
        ["Велосипедні"] = "/images/routes/bike-1.svg",
        ["Культурні"]   = "/images/routes/culture-1.svg",
        ["Водні"]       = "/images/routes/water-1.svg",
        ["Гірські"]     = "/images/routes/mountains-1.svg",
    };

    public static string GetByCategoryId(int categoryId) =>
        ById.TryGetValue(categoryId, out var path) ? path : "/images/route-placeholder.svg";

    public static string GetByCategoryName(string? categoryName) =>
        categoryName != null && ByName.TryGetValue(categoryName, out var path)
            ? path
            : "/images/route-placeholder.svg";
}
