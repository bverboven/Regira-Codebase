namespace Regira.DAL.Paging;

public static class QueryExtensions
{
    public static IQueryable<T> PageQuery<T>(this IQueryable<T> query, PagingInfo? info)
        => info == null ? query : query.PageQuery(info.PageSize, info.Page);
    public static IQueryable<T> PageQuery<T>(this IQueryable<T> query, int pageSize, int page = 1)
    {
        // make sure page is greater than 0
        page = Math.Max(1, page);

        if (pageSize > 0)
        {
            if (page > 1)
            {
                query = query.Skip((page - 1) * pageSize);
            }

            return query.Take(pageSize);
        }

        return query;
    }
}