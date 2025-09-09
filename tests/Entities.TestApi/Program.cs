using Entities.TestApi.Infrastructure;
using Entities.TestApi.Infrastructure.Courses;
using Entities.TestApi.Infrastructure.Departments;
using Entities.TestApi.Infrastructure.Enrollments;
using Entities.TestApi.Infrastructure.Persons;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Regira.DAL.EFcore.Services;
using Regira.Entities.Abstractions;
using Regira.Entities.Attachments.Models;
using Regira.Entities.DependencyInjection.Preppers;
using Regira.Entities.DependencyInjection.QueryBuilders;
using Regira.Entities.DependencyInjection.ServiceBuilders.Extensions;
using Regira.Entities.EFcore.Attachments;
using Regira.Entities.EFcore.Normalizing;
using Regira.Entities.EFcore.Primers;
using Regira.Entities.EFcore.QueryBuilders.GlobalFilterBuilders;
using Regira.Entities.Mapping.AutoMapper;
using Regira.Entities.Models.Abstractions;
using Regira.Entities.Web.Attachments.Models;
using Regira.IO.Storage.FileSystem;
using Serilog;
using Serilog.Events;
using System.Reflection;
using System.Text.Json.Serialization;
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
        //o.UseAutoMapper();
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
builder.Services
    .AddTransient<IEntityMapper, EntityMapper>()
    .AddTransient<AttachmentUriResolver<CourseAttachment, CourseAttachmentDto>>()
    .AddAutoMapper((_, e) =>
    {
        e.CreateMap<Attachment, AttachmentDto>();
        e.CreateMap<Attachment, AttachmentDto<int>>();
        e.CreateMap(typeof(Attachment<>), typeof(AttachmentDto<>));
        e.CreateMap<AttachmentInputDto, Attachment>();
        e.CreateMap<AttachmentInputDto<int>, Attachment>();
        e.CreateMap(typeof(AttachmentInputDto<>), typeof(Attachment<>));
        e.CreateMap<EntityAttachment, EntityAttachmentDto>();
        e.CreateMap(typeof(EntityAttachment<,,,>), typeof(EntityAttachmentDto<,,>));
        e.CreateMap<EntityAttachmentInputDto, EntityAttachment>();
        e.CreateMap(typeof(EntityAttachmentInputDto<,,>), typeof(EntityAttachment<,,,>));

        e.CreateMap<Course, CourseDto>();
        e.CreateMap<CourseInputDto, Course>();
        e.CreateMap<CourseAttachment, CourseAttachmentDto>();
        e.CreateMap<CourseAttachmentInputDto, CourseAttachment>();

        e.CreateMap<Department, DepartmentDto>();
        e.CreateMap<DepartmentInputDto, Department>();

        e.CreateMap<Person, PersonDto>();
        e.CreateMap<PersonInputDto, Person>();
    }, Array.Empty<Assembly>());


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
