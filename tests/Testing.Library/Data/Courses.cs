using Testing.Library.Contoso;

namespace Testing.Library.Data;

public static class Courses
{
    public static Course GoingToTheMoon = new()
    {
        Title = "Going to the moon",
        Instructors = [People.Bob]
    };
    public static readonly Course BefriendingChina = new()
    {
        Title = "Befriending China",
        Instructors = [People.Bill]
    };
    public static readonly Course Cheating = new()
    {
        Title = "How to cheat",
        Instructors = [People.Bill]
    };

    public static readonly Course ArtificialIntelligence = new()
    {
        Title = "Artificial Intelligence",
        Department = Departments.ComputerScience,
        Instructors = [People.Bob],
        Credits = 2
    };
    public static readonly Course DataStructures = new()
    {
        Title = "Data Structures and Algorithms",
        Department = Departments.ComputerScience,
        Instructors = [People.Bob],
        Credits = 3
    };
    public static readonly Course Genetics = new()
    {
        Title = "Genetics",
        Department = Departments.Biology,
        Instructors = [People.David],
        Credits = 3
    };
    public static readonly Course Ecology = new()
    {
        Title = "Ecology",
        Department = Departments.Biology,
        Instructors = [People.David],
        Credits = 5
    };
    public static readonly Course MentalDisorders = new()
    {
        Title = "Mental disorders",
        Department = Departments.Psychology,
        Instructors = [People.Bill, People.Bob],
        Credits = 4
    };

    public static IList<Course> All =>
    [
        GoingToTheMoon, BefriendingChina, Cheating,
        ArtificialIntelligence, DataStructures,
        Genetics, Ecology,
        MentalDisorders
    ];
}