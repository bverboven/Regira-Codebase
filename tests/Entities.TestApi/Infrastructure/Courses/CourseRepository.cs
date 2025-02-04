using Microsoft.EntityFrameworkCore;
using Regira.DAL.Paging;
using Regira.Entities.Abstractions;
using Regira.Entities.EFcore.Attachments;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Regira.Entities.EFcore.Services;
using Regira.Entities.Extensions;
using Testing.Library.Contoso;
using Testing.Library.Data;

namespace Entities.TestApi.Infrastructure.Courses;

public class CourseRepository(ContosoContext dbContext, IQueryBuilder<Course, int, CourseSearchObject> queryBuilder)
    : EntityRepository<ContosoContext, Course, int, CourseSearchObject>(dbContext, queryBuilder), IEntityRepository<Course>
{
    private readonly ContosoContext _dbContext = dbContext;

    public override IQueryable<Course> Query(IQueryable<Course> query, CourseSearchObject? so, PagingInfo? pagingInfo = null)
    {
        query = base.Query(
            query
                .Include(c => c.Attachments!)
                .ThenInclude(x => x.Attachment),
            so,
            pagingInfo
        );

        if (so?.DepartmentId.HasValue == true)
        {
            query = query.Where(x => x.DepartmentId == so.DepartmentId);
        }

        return query;
    }

    public override void Modify(Course item, Course original)
    {
        base.Modify(item, original);

        _dbContext.ModifyEntityAttachments(original, item);
    }

    public override void PrepareItem(Course item)
    {
        item.Attachments?.SetSortOrder();
    }
}