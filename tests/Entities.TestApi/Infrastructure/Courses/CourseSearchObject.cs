using Regira.Entities.Models;

namespace Entities.TestApi.Infrastructure.Courses;

public class CourseSearchObject : SearchObject
{
    public int? DepartmentId { get; set; }
}

