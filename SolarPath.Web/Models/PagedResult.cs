namespace SolarPath.Web.Models;

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; set; } = new List<T>();
    public int TotalCount  { get; set; }
    public int Page        { get; set; } = 1;
    public int PageSize    { get; set; } = 9;

    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
    public bool HasPrevious => Page > 1;
    public bool HasNext     => Page < TotalPages;
}
