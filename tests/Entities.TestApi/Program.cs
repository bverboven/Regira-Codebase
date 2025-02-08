using System.Text.Json.Serialization;
using Entities.TestApi.Infrastructure;
using Entities.TestApi.Infrastructure.Courses;
using Entities.TestApi.Infrastructure.Departments;
using Entities.TestApi.Infrastructure.Persons;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Regira.Entities.DependencyInjection.Extensions;
using Regira.Entities.EFcore.Attachments;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Regira.Entities.EFcore.QueryBuilders.GlobalFilterBuilders;
using Regira.IO.Storage.FileSystem;
using Testing.Library.Contoso;
using Testing.Library.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
    .AddNewtonsoftJson(o =>
    {
        o.UseCamelCasing(true);
        var settings = o.SerializerSettings;
        settings.NullValueHandling = NullValueHandling.Ignore;
        settings.MissingMemberHandling = MissingMemberHandling.Ignore;
        settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        settings.DefaultValueHandling = DefaultValueHandling.Include;
        var converters = settings.Converters;
        converters.Add(new StringEnumConverter());
        //converters.Add(new BoolNumberConverter());
    });
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddProblemDetails();

builder.Services
    .AddHttpContextAccessor()
    .AddDbContext<ContosoContext>((_, db) => db.UseSqlite(ApiConfiguration.ConnectionString))
    .AddAutoMapper(c => c.AllowNullCollections = true)
    .UseEntities<ContosoContext>(o =>
    {
        o.AddDefaultQKeywordHelper();
        o.AddDefaultGlobalQueryFilters();
        o.AddGlobalFilterQueryBuilder<FilterHasNormalizedContentQueryBuilder>();
    })
    // FileSystem storage
    .ConfigureAttachmentService(_ => new BinaryFileService(new FileSystemOptions { RootFolder = ApiConfiguration.AttachmentsDirectory }))
    // Azure storage
    /*
    .ConfigureAttachmentService(_ => new BinaryBlobService(new AzureCommunicator(new AzureConfig
    {
        ConnectionString = builder.Configuration["Storage:Azure:ConnectionString"],
        ContainerName = "test-container"
    })))
    */
    // Attachment services
    .ConfigureTypedAttachmentService(db =>
    [
        db.CourseAttachments.ToDescriptor<Course>(),
        db.PersonAttachments.ToDescriptor<Person>()
    ])
    .For<Department>(e =>
    {
        e.AddMapping<DepartmentDto, DepartmentInputDto>();
        e.AddQueryFilter<DepartmentMax10YearsOldQueryFilter>();
    })
    .For<Course, int, CourseSearchObject, CourseRepository>(e =>
    {
        e.AddMapping<CourseDto, CourseInputDto>();
        e.HasAttachments<ContosoContext, Course, CourseAttachment>(a =>
        {
            a.AddMapping<CourseAttachmentDto, CourseAttachmentInputDto>();
        });
        // extra person filter
        e.AddTransient<IFilteredQueryBuilder<Person, PersonSearchObject>, CoursePersonQueryFilter>();
    })
    .For<Person, PersonManager, PersonSearchObject, PersonSortBy, PersonIncludes>(e =>
    {
        e.AddQueryFilter<PersonQueryFilter>();
        e.AddQueryBuilder<PersonQueryBuilder>();
        e.AddMapping<PersonDto, PersonInputDto>();
        e.HasRepository<PersonRepository>();
        e.HasManager<PersonManager>();
        e.HasAttachments<ContosoContext, Person, PersonAttachment>();
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// add minimal api endpoints
app.MapEndPoints();
// add controller mappings
app.MapControllers();

app.Run();

// Make Program public
// https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-7.0
// ReSharper disable once PartialTypeWithSinglePart
namespace Entities.TestApi
{
    public partial class Program;
}
