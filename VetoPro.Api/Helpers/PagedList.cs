using Microsoft.EntityFrameworkCore;

namespace VetoPro.Api.Helpers;

/// <summary>
/// A generic list wrapper that supports pagination metadata.
/// </summary>
/// <typeparam name="T">The type of items in the list.</typeparam>
public class PagedList<T>
{
    public int CurrentPage { get; private set; }
    public int TotalPages { get; private set; }
    public int PageSize { get; private set; }
    public int TotalCount { get; private set; }
    public bool HasPrevious => CurrentPage > 1;
    public bool HasNext => CurrentPage < TotalPages;
    public List<T> Items { get; private set; }

    private PagedList(List<T> items, int count, int pageNumber, int pageSize)
    {
        TotalCount = count;
        PageSize = pageSize;
        CurrentPage = pageNumber;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        Items = items;
    }

    /// <summary>
    /// Factory method to create a PagedList asynchronously from an IQueryable source.
    /// </summary>
    public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
    {
        // Get the total count *before* pagination
        var count = await source.CountAsync();

        // Apply pagination (Skip and Take)
        var items = await source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedList<T>(items, count, pageNumber, pageSize);
    }
}