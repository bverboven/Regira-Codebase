using System.Text;
using System.Text.Json.Serialization;
using Entities.TestApi.Infrastructure;
using Entities.TestApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Regira.Entities.Abstractions;
using Regira.Entities.Attachments.Models;
using Regira.Entities.DependencyInjection.Extensions;
using Regira.Entities.EFcore.Attachments;
using Regira.IO.Storage.FileSystem;
using Regira.Utilities;
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
    .UseEntities<ContosoContext>()
    .ConfigureAttachmentService(_ => new BinaryFileService(new FileSystemOptions { RootFolder = ApiConfiguration.AttachmentsDirectory }))
    //.ConfigureAttachmentService(_ => new BinaryBlobService(new AzureCommunicator(new AzureConfig
    //{
    //    ConnectionString = builder.Configuration["Storage:Azure:ConnectionString"],
    //    ContainerName = "test-container"
    //})))
    .ConfigureTypedAttachmentService(db =>
    [
        db.CourseAttachments.ToDescriptor<Course>(),
        db.PersonAttachments.ToDescriptor<Person>()
    ])
    .For<Department>(e => e.AddMapping<DepartmentDto, DepartmentInputDto>())
    .For<Course, CourseRepository>(e =>
    {
        e.AddMapping<CourseDto, CourseInputDto>();
        e.HasAttachments<ContosoContext, Course, CourseAttachment>(a =>
        {
            a.AddMapping<CourseAttachmentDto, CourseAttachmentInputDto>();
        });
    })
    .For<Person, PersonManager, PersonSearchObject, PersonSortBy, PersonIncludes>(e =>
    {
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

app.MapGet("/", () => "Hello");
app.MapPost("/db", (ContosoContext db) => db.Database.EnsureCreatedAsync());
app.MapDelete("/db", (ContosoContext db) => db.Database.EnsureDeletedAsync());
app.MapPost("/test-data", async (ContosoContext db, IEntityService<CourseAttachment> courseAttachmentService) =>
{
    await db.Database.EnsureCreatedAsync();

    var departments = Enumerable.Range(1, 5).Select((_, i) => new Department { Title = $"Department #{i}", Budget = i * 1000, StartDate = DateTime.Today.AddDays(i * 3) }).ToArray();
    var courses = Enumerable.Range(1, 50).Select((_, i) => new Course { Title = $"Course #{i}", Credits = Random.Shared.Next(1, 5), Department = departments[Random.Shared.Next(0, 4)] }).ToArray();

    db.Departments.AddRange(departments);
    db.Courses.AddRange(courses);

    await db.SaveChangesAsync();

    var courseAttachments = courses.Shuffle().Take(10).Select(x => new CourseAttachment
    {
        ObjectId = x.Id,
        Attachment = new Attachment
        {
            Bytes = Encoding.UTF8.GetBytes($"This is an attachment of course #{x.Id}"),
            FileName = $"courses/course-{x.Id}.txt",
            ContentType = "text/plain"
        }
    });

    foreach (var item in courseAttachments)
    {
        await courseAttachmentService.Add(item);
    }

    await courseAttachmentService.SaveChanges();
});
var departments = app.MapGroup("/minimal/departments")
    .WithOpenApi();
departments.MapGet("", async (ContosoContext db) => await db.Departments.ToListAsync());
departments.MapPut("", async (ContosoContext db, [FromBody] Department input) =>
{
    db.Departments.Add(input);
    await db.SaveChangesAsync();
});

app.MapControllers();

app.Run();

// Make Program public
// https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-7.0
// ReSharper disable once PartialTypeWithSinglePart
namespace Entities.TestApi
{
    public partial class Program;
}
