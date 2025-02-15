using Testing.Library.Contoso;

namespace Testing.Library.Data;

public static class Departments
{
    public static readonly Department ComputerScience = new Department
    {
        Title = "Computer Science",
        Administrator = People.Bob
    };
    public static readonly Department BusinessAdministration = new Department
    {
        Title = "Business Administration",
        Administrator = People.Bob
    };
    public static readonly Department MechanicalEngineering = new Department
    {
        Title = "Mechanical Engineering",
        Administrator = People.Bill
    };
    public static readonly Department Biology = new Department
    {
        Title = "Biology",
        Administrator = People.David
    };
    public static readonly Department Psychology = new Department
    {
        Title = "Psychology",
        Administrator = People.David
    };

    public static IList<Department> All => [ComputerScience, BusinessAdministration, MechanicalEngineering, Biology, Psychology];
}