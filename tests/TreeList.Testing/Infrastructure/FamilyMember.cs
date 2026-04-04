namespace TreeList.Testing.Infrastructure;

public class FamilyMember
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public FamilyMember? Parent { get; set; }

    public override string ToString() => Name;
}
