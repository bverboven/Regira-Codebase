using Entities.TestApi.Infrastructure;
using Entities.TestApi.Models;
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Attachments.Abstractions;
using Regira.Entities.Web.Controllers.Abstractions;
using Testing.Library.Contoso;

namespace Entities.TestApi.Controllers;

[ApiController]
[Route("courses")]
public class CourseController : EntityControllerBase<Course, CourseSearchObject, CourseDto, CourseInputDto>
{
}

[ApiController]
[Route("courses")]
public class CourseAttachmentController : EntityAttachmentControllerBase<CourseAttachment, CourseAttachmentDto, CourseAttachmentInputDto>
{
}