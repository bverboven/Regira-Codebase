using Microsoft.EntityFrameworkCore;
using Regira.DAL.Paging;
using Regira.Entities.Abstractions;
using Regira.Entities.Attachments.Abstractions;
using Regira.Entities.Attachments.Extensions;

namespace Regira.Entities.EFcore.Attachments;

public interface ITypedAttachmentService : IEntityReadService<IEntityAttachment>;
public class TypedAttachmentService<TContext>(
    TContext dbContext,
    Func<TContext, IList<IAttachmentQuerySetDescriptor>>? querySetFactory = null)
    : ITypedAttachmentService
    where TContext : DbContext
{
    protected readonly TContext DbContext = dbContext;
    private readonly IList<IAttachmentQuerySetDescriptor>? _querySets = querySetFactory?.Invoke(dbContext);

    /// <summary>
    /// Example:
    /// <code>
    /// DbContext
    ///     .Set&lt;EntityAttachment1&gt;().Select(ToEntityAttachment&lt;Entity1&gt;())<br />
    ///     .Concat(DbContext.Set&lt;EntityAttachment2&gt;().Select(ToEntityAttachment&lt;Entity2&gt;()))
    ///     .Concat(...)
    /// </code>
    /// </summary>
    public virtual IQueryable<IEntityAttachment> Query => _querySets?.ConcatAll() ?? throw new NotImplementedException();

    public async Task<IEntityAttachment?> Details(int id)
        => (await List(new { id }, new PagingInfo { PageSize = 1 })).SingleOrDefault();
    public async Task<IList<IEntityAttachment>> List(object? so = null, PagingInfo? pagingInfo = null)
    {
        var query = Query
            .Filter(so?.ToSearchObject())
            .PageQuery(pagingInfo);
        var items = await query
#if NETSTANDARD2_0
            .AsNoTracking()
#else
            .AsNoTrackingWithIdentityResolution()
#endif
            .ToListAsync();

        return items;
    }
    public Task<int> Count(object? so)
        => Query.Filter(so?.ToSearchObject()).CountAsync();
}