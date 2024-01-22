using Microsoft.EntityFrameworkCore;
using Regira.Entities.Abstractions;
using Regira.Entities.EFcore.Attachments;
using Regira.Entities.EFcore.Services;
using Regira.Entities.Extensions;
using Regira.Entities.Models;
using Testing.Library.Contoso;
using Testing.Library.Data;

namespace Entities.TestApi.Infrastructure;

public class CourseSearchObject : SearchObject
{
    public int? DepartmentId { get; set; }
}
public class CourseRepository : EntityRepository<ContosoContext, Course, int, CourseSearchObject>, IEntityRepository<Course>
{
    private readonly ContosoContext _dbContext;
    public CourseRepository(ContosoContext dbContext)
        : base(dbContext)
    {
        _dbContext = dbContext;
    }


    public override IQueryable<Course> Filter(IQueryable<Course> query, CourseSearchObject? so)
    {
        query = base.Filter(query, so);
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

    // Always include Attachments
    public override IQueryable<Course> Query => DbSet
        .Include(c => c.Attachments!)
        .ThenInclude(x => x.Attachment);
}