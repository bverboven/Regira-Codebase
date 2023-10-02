using System.ComponentModel.DataAnnotations;

namespace Testing.Library.Contoso;

public class Instructor : Person
{
    [DataType(DataType.Date)]
    public DateTime HireDate { get; set; }
    
    public ICollection<Course>? Courses { get; set; }
    public OfficeAssignment? OfficeAssignment { get; set; }
}