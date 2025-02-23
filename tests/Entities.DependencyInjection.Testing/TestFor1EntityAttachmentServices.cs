using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.Abstractions;
using Regira.Entities.Attachments.Models;
using Regira.Entities.DependencyInjection.Attachments;
using Regira.Entities.DependencyInjection.Extensions;
using Regira.Entities.EFcore.Attachments;
using Regira.Entities.EFcore.Normalizing;
using Regira.Entities.EFcore.Normalizing.Abstractions;
using Regira.Entities.EFcore.Primers;
using Regira.Entities.EFcore.Primers.Abstractions;
using Regira.Entities.EFcore.QueryBuilders;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Regira.Entities.EFcore.QueryBuilders.GlobalFilterBuilders;
using Regira.Entities.EFcore.Services;
using Regira.Entities.Models;
using Regira.IO.Storage.FileSystem;
using Testing.Library.Contoso;
using Testing.Library.Data;

namespace Entities.DependencyInjection.Testing;

[TestFixture]
public class TestFor1EntityAttachmentServices
{
    [Test]
    public void Without_Defaults()
    {
        using var sp = new ServiceCollection()
            .AddDbContext<ContosoContext>()
            .UseEntities<ContosoContext>()
            .WithAttachments(_ => new BinaryFileService(new FileSystemOptions()))
            .For<Course>(e =>
            {
                e.HasAttachments<ContosoContext, Course, CourseAttachment>();
            })
            .BuildServiceProvider();

        var entityNormalizer = sp.GetService<IEntityNormalizer>();
        var primers = sp.GetServices<IEntityPrimer>().ToArray();
        var globalFilters = sp.GetServices<IGlobalFilteredQueryBuilder>().ToArray();
        var queryFilters = sp.GetServices<IFilteredQueryBuilder<CourseAttachment, int, EntityAttachmentSearchObject>>().ToArray();
        var sortableBuilder = sp.GetService<ISortedQueryBuilder<CourseAttachment, int>>();
        var includableBuilder = sp.GetService<IIncludableQueryBuilder<CourseAttachment, int>>();
        var queryBuilder = sp.GetService<IQueryBuilder<CourseAttachment, int, EntityAttachmentSearchObject, EntitySortBy, EntityIncludes>>();
        var entityReadService2 = sp.GetService<IEntityReadService<CourseAttachment, int>>();
        var entityReadService3 = sp.GetService<IEntityReadService<CourseAttachment, int, EntityAttachmentSearchObject>>();
        var entityWriteService = sp.GetService<IEntityWriteService<CourseAttachment, int>>();
        var repo1 = sp.GetService<IEntityRepository<CourseAttachment>>();
        var repo2 = sp.GetService<IEntityRepository<CourseAttachment, int>>();
        var repo3 = sp.GetService<IEntityRepository<CourseAttachment, int, EntityAttachmentSearchObject>>();
        var entityService1 = sp.GetService<IEntityService<CourseAttachment>>();
        var entityService2 = sp.GetService<IEntityService<CourseAttachment, int>>();
        var entityService3 = sp.GetService<IEntityService<CourseAttachment, int, EntityAttachmentSearchObject>>();

        Assert.That(entityNormalizer, Is.Null);
        Assert.That(globalFilters, Is.Empty);
        Assert.That(queryFilters.First(), Is.TypeOf<EntityAttachmentFilteredQueryBuilder<CourseAttachment, EntityAttachmentSearchObject>>());
        Assert.That(sortableBuilder, Is.Null);
        Assert.That(includableBuilder, Is.TypeOf<IncludableQueryBuilder<CourseAttachment, int>>());
        Assert.That(queryBuilder, Is.TypeOf<QueryBuilder<CourseAttachment, int, EntityAttachmentSearchObject>>());
        Assert.That(primers.First(), Is.TypeOf<AttachmentPrimer>());
        Assert.That(entityReadService2, Is.TypeOf<EntityReadService<ContosoContext, CourseAttachment, int, EntityAttachmentSearchObject>>());
        Assert.That(entityReadService3, Is.TypeOf<EntityReadService<ContosoContext, CourseAttachment, int, EntityAttachmentSearchObject>>());
        Assert.That(entityWriteService, Is.TypeOf<EntityWriteService<ContosoContext, CourseAttachment, int>>());
        Assert.That(repo1, Is.TypeOf<EntityAttachmentRepository<ContosoContext, Course, CourseAttachment, EntityAttachmentSearchObject>>());
        Assert.That(repo2, Is.TypeOf<EntityAttachmentRepository<ContosoContext, Course, CourseAttachment, EntityAttachmentSearchObject>>());
        Assert.That(repo3, Is.TypeOf<EntityAttachmentRepository<ContosoContext, Course, CourseAttachment, EntityAttachmentSearchObject>>());
        Assert.That(entityService1, Is.TypeOf<EntityAttachmentRepository<ContosoContext, Course, CourseAttachment, EntityAttachmentSearchObject>>());
        Assert.That(entityService2, Is.TypeOf<EntityAttachmentRepository<ContosoContext, Course, CourseAttachment, EntityAttachmentSearchObject>>());
        Assert.That(entityService3, Is.TypeOf<EntityAttachmentRepository<ContosoContext, Course, CourseAttachment, EntityAttachmentSearchObject>>());
    }
    [Test]
    public void With_Defaults()
    {
        using var sp = new ServiceCollection()
            .AddDbContext<ContosoContext>()
            .UseEntities<ContosoContext>(e => e.UseDefaults())
            .WithAttachments(_ => new BinaryFileService(new FileSystemOptions()))
            .For<Course>(e =>
            {
                e.HasAttachments<ContosoContext, Course, CourseAttachment>();
            })
            .BuildServiceProvider();

        var entityNormalizer = sp.GetService<IEntityNormalizer>();
        var primers = sp.GetServices<IEntityPrimer>().ToArray();
        var globalFilters = sp.GetServices<IGlobalFilteredQueryBuilder>().ToArray();
        var queryFilters = sp.GetServices<IFilteredQueryBuilder<CourseAttachment, int, EntityAttachmentSearchObject>>().ToArray();
        var sortableBuilder = sp.GetService<ISortedQueryBuilder<CourseAttachment, int>>();
        var includableBuilder = sp.GetService<IIncludableQueryBuilder<CourseAttachment, int>>();
        var queryBuilder = sp.GetService<IQueryBuilder<CourseAttachment, int, EntityAttachmentSearchObject, EntitySortBy, EntityIncludes>>();
        var entityReadService2 = sp.GetService<IEntityReadService<CourseAttachment, int>>();
        var entityReadService3 = sp.GetService<IEntityReadService<CourseAttachment, int, EntityAttachmentSearchObject>>();
        var entityWriteService = sp.GetService<IEntityWriteService<CourseAttachment, int>>();
        var repo1 = sp.GetService<IEntityRepository<CourseAttachment>>();
        var repo2 = sp.GetService<IEntityRepository<CourseAttachment, int>>();
        var repo3 = sp.GetService<IEntityRepository<CourseAttachment, int, EntityAttachmentSearchObject>>();
        var entityService1 = sp.GetService<IEntityService<CourseAttachment>>();
        var entityService2 = sp.GetService<IEntityService<CourseAttachment, int>>();
        var entityService3 = sp.GetService<IEntityService<CourseAttachment, int, EntityAttachmentSearchObject>>();

        Assert.That(entityNormalizer, Is.TypeOf<DefaultEntityNormalizer>());
        Assert.That(globalFilters, Is.Not.Empty);
        Assert.That(globalFilters.OfType<FilterIdsQueryBuilder<int>>(), Is.Not.Empty);
        Assert.That(globalFilters.OfType<FilterArchivablesQueryBuilder<int>>(), Is.Not.Empty);
        Assert.That(globalFilters.OfType<FilterHasCreatedQueryBuilder<int>>(), Is.Not.Empty);
        Assert.That(globalFilters.OfType<FilterHasLastModifiedQueryBuilder<int>>(), Is.Not.Empty);

        Assert.That(primers.Length, Is.EqualTo(4));
        Assert.That(primers.OfType<HasCreatedDbPrimer>(), Is.Not.Empty);
        Assert.That(primers.OfType<HasLastModifiedDbPrimer>(), Is.Not.Empty);
        Assert.That(primers.OfType<ArchivablePrimer>(), Is.Not.Empty);
        Assert.That(primers.OfType<AttachmentPrimer>(), Is.Not.Empty);

        Assert.That(queryFilters.First(), Is.TypeOf<EntityAttachmentFilteredQueryBuilder<CourseAttachment, EntityAttachmentSearchObject>>());
        Assert.That(sortableBuilder, Is.Null);
        Assert.That(includableBuilder, Is.TypeOf<IncludableQueryBuilder<CourseAttachment, int>>());
        Assert.That(queryBuilder, Is.TypeOf<QueryBuilder<CourseAttachment, int, EntityAttachmentSearchObject>>());
        Assert.That(entityReadService2, Is.TypeOf<EntityReadService<ContosoContext, CourseAttachment, int, EntityAttachmentSearchObject>>());
        Assert.That(entityReadService3, Is.TypeOf<EntityReadService<ContosoContext, CourseAttachment, int, EntityAttachmentSearchObject>>());
        Assert.That(entityWriteService, Is.TypeOf<EntityWriteService<ContosoContext, CourseAttachment, int>>());
        Assert.That(repo1, Is.TypeOf<EntityAttachmentRepository<ContosoContext, Course, CourseAttachment, EntityAttachmentSearchObject>>());
        Assert.That(repo2, Is.TypeOf<EntityAttachmentRepository<ContosoContext, Course, CourseAttachment, EntityAttachmentSearchObject>>());
        Assert.That(repo3, Is.TypeOf<EntityAttachmentRepository<ContosoContext, Course, CourseAttachment, EntityAttachmentSearchObject>>());
        Assert.That(entityService1, Is.TypeOf<EntityAttachmentRepository<ContosoContext, Course, CourseAttachment, EntityAttachmentSearchObject>>());
        Assert.That(entityService2, Is.TypeOf<EntityAttachmentRepository<ContosoContext, Course, CourseAttachment, EntityAttachmentSearchObject>>());
        Assert.That(entityService3, Is.TypeOf<EntityAttachmentRepository<ContosoContext, Course, CourseAttachment, EntityAttachmentSearchObject>>());
    }
}