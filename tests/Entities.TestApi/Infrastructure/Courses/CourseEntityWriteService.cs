using Regira.Entities.Abstractions;
using Regira.Entities.EFcore.Attachments;
using Regira.Entities.EFcore.Preppers.Abstractions;
using Regira.Entities.EFcore.Services;
using Regira.Entities.Extensions;
using Testing.Library.Contoso;
using Testing.Library.Data;

namespace Entities.TestApi.Infrastructure.Courses;

public class CourseEntityWriteService(ContosoContext dbContext, IEntityReadService<Course, int, CourseSearchObject> readService, IEnumerable<IPrepper<Course, int>> preppers)
    : EntityWriteService<ContosoContext, Course>(dbContext, readService, preppers)
{
    public override Task PrepareItem(Course item, Course? _)
    {
        item.Attachments?.SetSortOrder();

        return Task.CompletedTask;
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