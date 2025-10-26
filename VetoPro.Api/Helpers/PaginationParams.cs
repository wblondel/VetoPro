namespace VetoPro.Api.Helpers;

/// <summary>
/// Represents the pagination parameters received from the client.
/// </summary>
public class PaginationParams
{
    private const int MaxPageSize = 50;
    private int _pageSize = 10; // Default page size

    public int PageNumber { get; set; } = 1; // Default to first page

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value; // Enforce max size
    }
}