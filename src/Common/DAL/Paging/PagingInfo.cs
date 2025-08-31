namespace Regira.DAL.Paging;

/// <summary>
/// Represents the paging information used for paginated data retrieval.
/// </summary>
public class PagingInfo
{
    public int PageSize { get; set; }
    public int Page { get; set; } = 1;
}