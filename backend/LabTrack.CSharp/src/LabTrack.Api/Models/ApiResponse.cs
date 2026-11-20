namespace LabTrack.Api.Models.Contracts;

public class PagedResponse<T>
{
    public IReadOnlyList<T> Items { get; set; } = new List<T>();
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int TotalCount { get; set; }
}

