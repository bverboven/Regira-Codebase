namespace TreeList.Testing.Infrastructure;

public class FamilyMember
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public ICollection<FamilyMember> Children { get; set; } = new List<FamilyMember>();

    public override string ToString() => Name;
}
