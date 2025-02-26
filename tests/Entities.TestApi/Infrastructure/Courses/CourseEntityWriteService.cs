using Regira.Entities.Abstractions;
using Regira.Entities.EFcore.Attachments;
using Regira.Entities.EFcore.Services;
using Regira.Entities.Extensions;
using Testing.Library.Contoso;
using Testing.Library.Data;

namespace Entities.TestApi.Infrastructure.Courses;

public class CourseEntityWriteService(ContosoContext dbContext, IEntityReadService<Course, int, CourseSearchObject> readService)
    : EntityWriteService<ContosoContext, Course>(dbContext, readService)
{
    public override void PrepareItem(Course item)
    {
        item.Attachments?.SetSortOrder();
    }

    public override async Task<Course?> Modify(Course item)
    {
        var original = await base.Modify(item);

        if (original != null)
        {
            DbContext.ModifyEntityAttachments(original, item);
        }

        return original;
    }
}