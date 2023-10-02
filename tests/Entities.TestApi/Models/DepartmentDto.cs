namespace Entities.TestApi.Models;

public class DepartmentDto
{
    public int Id { get; set; }
    public int? AdministratorId { get; set; }

    public string? Title { get; set; }
    public decimal Budget { get; set; }
    public DateTime StartDate { get; set; }

    public Guid ConcurrencyToken { get; set; }
    public PersonDto? Administrator { get; set; }
    public ICollection<CourseDto>? Courses { get; set; }
}