namespace HomeEase.Domain.Common;

public class PaginatedList<T>
{
    public List<T> Items { get; set; }
    public PaginationMetadata Pagination { get; set; }

    public PaginatedList(List<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        Pagination = new PaginationMetadata
        {
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}

public class PaginationMetadata
{
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasMore => PageNumber < TotalPages;
}
