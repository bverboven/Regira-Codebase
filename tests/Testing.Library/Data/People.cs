using Testing.Library.Contoso;

namespace Testing.Library.Data;

public static class People
{
    public static readonly Student John = new()
    {
        GivenName = "John",
        LastName = "Doe",
        Description = "This is a male test person",
        Email = "john.doe@email.com"
    };
    public static readonly Student Jane = new()
    {
        GivenName = "Jane",
        LastName = "Doe",
        Description = "This is a female test person",
        Phone = "001 234 567 890"
    };
    public static readonly Student Francois = new()
    {
        GivenName = "François",
        LastName = "Du sacre Cœur",
        Description = "Le poète parisien du xiiie siècle Rutebeuf se fait gravement l'écho de la faiblesse humaine, de l'incertitude et de la pauvreté à l'opposé des valeurs courtoises. Crème brûlée"
    };
    public static readonly Instructor Bob = new ()
    {
        GivenName = "Robert",
        LastName = "Kennedy",
        Description = "He's an American politician and lawyer, known for his roles as U.S. Attorney General and Senator, his advocacy for civil rights and social justice, and his tragic assassination in 1968 while campaigning for the presidency.",
        //Courses = [Courses.GoingToTheMoon, Courses.ArtificialIntelligence, Courses.DataStructures]
    };

    public static readonly Instructor Bill = new()
    {
        GivenName = "Richard",
        LastName = "Nixon",
        Description = "He's the 37th President of the United States, remembered for his foreign policy achievements and his involvement in the Watergate scandal, which led to his resignation in 1974.",
        //Courses = [Courses.Cheating, Courses.MentalDisorders]
    };

    public static readonly Instructor David = new()
    {
        GivenName = "David",
        LastName = "Attenborough",
        Description = "He's naturalist, and documentary filmmaker known for his captivating narration and lifelong dedication to exploring and preserving the natural world.",
        //Courses = [Courses.Genetics, Courses.Ecology]
    };

    public static IList<Person> All => [John, Jane, Francois, Bob, Bill];
}