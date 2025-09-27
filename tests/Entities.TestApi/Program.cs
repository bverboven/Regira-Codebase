using Entities.TestApi.Infrastructure;
using Entities.TestApi.Infrastructure.Courses;
using Entities.TestApi.Infrastructure.Departments;
using Entities.TestApi.Infrastructure.Enrollments;
using Entities.TestApi.Infrastructure.Persons;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Regira.DAL.EFcore.Services;
using Regira.Entities.DependencyInjection.Preppers;
using Regira.Entities.DependencyInjection.QueryBuilders;
using Regira.Entities.DependencyInjection.ServiceBuilders.Extensions;
using Regira.Entities.EFcore.Attachments;
using Regira.Entities.EFcore.Normalizing;
using Regira.Entities.EFcore.Primers;
using Regira.Entities.EFcore.QueryBuilders.GlobalFilterBuilders;
using Regira.Entities.Mapping.AutoMapper;
using Regira.Entities.Models.Abstractions;
using Regira.IO.Storage.FileSystem;
using Serilog;
using Serilog.Events;
using System.Text.Json.Serialization;
using Regira.Entities.Mapping.Mapster;
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
builder.Host.UseSerilog((_, config) => config
    .WriteTo.Console()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
);

builder.Services
    .AddProblemDetails();

builder.Services
    .AddHttpContextAccessor()
    .AddDbContext<ContosoContext>((sp, db) =>
    {
        db.UseSqlite(ApiConfiguration.ConnectionString)
            .EnableSensitiveDataLogging()
            .AddPrimerInterceptors(sp)
            .AddNormalizerInterceptors(sp)
            .AddAutoTruncateInterceptors();
    })
    .UseEntities<ContosoContext>(o =>
    {
        o.UseDefaults();
        o.AddGlobalFilterQueryBuilder<FilterHasNormalizedContentQueryBuilder>();
        o.AddPrepper<IHasAggregateKey>(x => x.AggregateKey ??= Guid.NewGuid());
        o.UseAutoMapper();
        //o.UseMapsterMapping();
    })
    // Entity types
    .AddEnrollments()
    .AddCourses()
    .AddDepartments()
    .AddPersons()
    // Attachments
    // FileSystem storage
    .WithAttachments(_ => new BinaryFileService(new FileSystemOptions { RootFolder = ApiConfiguration.AttachmentsDirectory }))
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
    ]);

/*
var useMapster = false;
var useAutoMapper = true;

// mapping
if (useMapster)
{
    builder.Services
        .UseMapsterMapping(config =>
        {
            config
                // Course
                .Map<Course, CourseDto, CourseInputDto>(e =>
                {
                    e.HasAttachments<CourseAttachment, CourseAttachmentDto, CourseAttachmentInputDto>();
                })
                // Department
                .Map<Department, DepartmentDto, DepartmentInputDto>()
                // Person
                .Map<Person, PersonDto, PersonInputDto>();
        });
}

if (useAutoMapper)
{
    // AutoMapper
    builder.Services
        .UseAutoMapperMapping()
        // Course
        .Map<Course, CourseDto, CourseInputDto>(e =>
        {
            e.HasAttachments<CourseAttachment, CourseAttachmentDto, CourseAttachmentInputDto>();
        })
        // Department
        .Map<Department, DepartmentDto, DepartmentInputDto>()
        // Person
        .Map<Person, PersonDto, PersonInputDto>()
        ;
}
*/
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
