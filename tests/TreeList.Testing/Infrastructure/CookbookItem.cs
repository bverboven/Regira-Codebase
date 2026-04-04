namespace TreeList.Testing.Infrastructure;

public class CookbookItem
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;

    /// <summary>The recipes this item is used as an ingredient in (many-to-many).</summary>
    public IList<CookbookItem> UsedInRecipes { get; set; } = [];

    public override string ToString() => Name;
}
