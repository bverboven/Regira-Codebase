using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.Abstractions;
using Regira.Entities.Attachments.Models;
using Regira.Entities.DependencyInjection.Extensions;
using Regira.Entities.EFcore.Attachments;
using Regira.Entities.EFcore.Normalizing.Abstractions;
using Regira.Entities.EFcore.Primers.Abstractions;
using Regira.Entities.EFcore.QueryBuilders;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Regira.Entities.EFcore.Services;
using Regira.Entities.Models;
using Regira.IO.Storage.FileSystem;
using Testing.Library.Data;

namespace Entities.DependencyInjection.Testing;

[TestFixture]
public class TestFor3AttachmentServices
{
    [Test]
    public void Without_Defaults3A()
    {
        using var sp = new ServiceCollection()
            .AddDbContext<ContosoContext>()
            .UseEntities<ContosoContext>()
            .WithAttachments(_ => new BinaryFileService(new FileSystemOptions()))
            .BuildServiceProvider();

        var entityNormalizer = sp.GetService<IEntityNormalizer>();
        var primers = sp.GetServices<IEntityPrimer>().ToArray();
        var globalFilters = sp.GetServices<IGlobalFilteredQueryBuilder>().ToArray();
        var queryFilters = sp.GetServices<IFilteredQueryBuilder<Attachment, int, AttachmentSearchObject<int>>>().ToArray();
        var sortableBuilder = sp.GetService<ISortedQueryBuilder<Attachment, int>>();
        var includableBuilder = sp.GetService<IIncludableQueryBuilder<Attachment, int>>();
        var queryBuilder = sp.GetService<IQueryBuilder<Attachment, int, AttachmentSearchObject<int>, EntitySortBy, EntityIncludes>>();
        var entityReadService2 = sp.GetService<IEntityReadService<Attachment, int>>();
        var entityReadService3 = sp.GetService<IEntityReadService<Attachment, int, AttachmentSearchObject<int>>>();
        var entityWriteService = sp.GetService<IEntityWriteService<Attachment, int>>();
        var repo2 = sp.GetService<IEntityRepository<Attachment, int>>();
        var repo3 = sp.GetService<IEntityRepository<Attachment, int, AttachmentSearchObject<int>>>();
        var entityService2 = sp.GetService<IEntityService<Attachment, int>>();
        var entityService3 = sp.GetService<IEntityService<Attachment, int, AttachmentSearchObject<int>>>();

        Assert.That(entityNormalizer, Is.Null);
        Assert.That(globalFilters, Is.Empty);
        Assert.That(queryFilters.First(), Is.TypeOf<AttachmentFilteredQueryBuilder<Attachment, int, AttachmentSearchObject<int>>>());
        Assert.That(sortableBuilder, Is.Null);
        Assert.That(includableBuilder, Is.Null);
        Assert.That(queryBuilder, Is.TypeOf<QueryBuilder<Attachment, int, AttachmentSearchObject<int>>>());
        Assert.That(primers.First(), Is.TypeOf<AttachmentPrimer>());
        Assert.That(entityReadService2, Is.TypeOf<EntityReadService<ContosoContext, Attachment, int, AttachmentSearchObject<int>>>());
        Assert.That(entityReadService3, Is.TypeOf<EntityReadService<ContosoContext, Attachment, int, AttachmentSearchObject<int>>>());
        Assert.That(entityWriteService, Is.TypeOf<EntityWriteService<ContosoContext, Attachment, int>>());
        Assert.That(repo2, Is.TypeOf<EntityRepository<Attachment, int, AttachmentSearchObject<int>>>());
        Assert.That(repo3, Is.TypeOf<EntityRepository<Attachment, int, AttachmentSearchObject<int>>>());
        Assert.That(entityService2, Is.TypeOf<AttachmentRepository<ContosoContext, Attachment, int, AttachmentSearchObject<int>>>());
        Assert.That(entityService3, Is.TypeOf<AttachmentRepository<ContosoContext, Attachment, int, AttachmentSearchObject<int>>>());
    }

    [Test]
    public void Without_Defaults3B()
    {
        using var sp = new ServiceCollection()
            .AddDbContext<ContosoContext>()
            .UseEntities<ContosoContext>()
            .WithAttachments<Attachment, int, AttachmentSearchObject>(_ => new BinaryFileService(new FileSystemOptions()))
            .BuildServiceProvider();

        var entityNormalizer = sp.GetService<IEntityNormalizer>();
        var primers = sp.GetServices<IEntityPrimer>().ToArray();
        var globalFilters = sp.GetServices<IGlobalFilteredQueryBuilder>().ToArray();
        var queryFilters = sp.GetServices<IFilteredQueryBuilder<Attachment, int, AttachmentSearchObject>>().ToArray();
        var sortableBuilder = sp.GetService<ISortedQueryBuilder<Attachment, int>>();
        var includableBuilder = sp.GetService<IIncludableQueryBuilder<Attachment, int>>();
        var queryBuilder = sp.GetService<IQueryBuilder<Attachment, int, AttachmentSearchObject, EntitySortBy, EntityIncludes>>();
        var entityReadService2 = sp.GetService<IEntityReadService<Attachment, int>>();
        var entityReadService3 = sp.GetService<IEntityReadService<Attachment, int, AttachmentSearchObject>>();
        var entityWriteService = sp.GetService<IEntityWriteService<Attachment, int>>();
        var repo2 = sp.GetService<IEntityRepository<Attachment, int>>();
        var repo3 = sp.GetService<IEntityRepository<Attachment, int, AttachmentSearchObject>>();
        var entityService2 = sp.GetService<IEntityService<Attachment, int>>();
        var entityService3 = sp.GetService<IEntityService<Attachment, int, AttachmentSearchObject>>();

        Assert.That(entityNormalizer, Is.Null);
        Assert.That(globalFilters, Is.Empty);
        Assert.That(queryFilters.First(), Is.TypeOf<AttachmentFilteredQueryBuilder<Attachment, int, AttachmentSearchObject>>());
        Assert.That(sortableBuilder, Is.Null);
        Assert.That(includableBuilder, Is.Null);
        Assert.That(queryBuilder, Is.TypeOf<QueryBuilder<Attachment, int, AttachmentSearchObject>>());
        Assert.That(primers.First(), Is.TypeOf<AttachmentPrimer>());
        Assert.That(entityReadService2, Is.TypeOf<EntityReadService<ContosoContext, Attachment, int, AttachmentSearchObject>>());
        Assert.That(entityReadService3, Is.TypeOf<EntityReadService<ContosoContext, Attachment, int, AttachmentSearchObject>>());
        Assert.That(entityWriteService, Is.TypeOf<EntityWriteService<ContosoContext, Attachment, int>>());
        Assert.That(repo2, Is.TypeOf<EntityRepository<Attachment, int, AttachmentSearchObject>>());
        Assert.That(repo3, Is.TypeOf<EntityRepository<Attachment, int, AttachmentSearchObject>>());
        Assert.That(entityService2, Is.TypeOf<AttachmentRepository<ContosoContext, Attachment, int, AttachmentSearchObject>>());
        Assert.That(entityService3, Is.TypeOf<AttachmentRepository<ContosoContext, Attachment, int, AttachmentSearchObject>>());
    }
}