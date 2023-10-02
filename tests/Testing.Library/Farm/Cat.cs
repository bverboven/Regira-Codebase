namespace Testing.Library.Farm;

public class Cat : Pet
{
    public Cat(string name, string educationLevel)
        : base(name)
    {
        EducationLevel = educationLevel;
    }

    public string EducationLevel { get; set; }
    public override string Species => "Felis catus";

    public override string ToString()
        => $"Cat '{Name}' ({Species}/{Id}) with education '{EducationLevel}' eats {Food?.ToString() ?? "<Unknown>"}";
}