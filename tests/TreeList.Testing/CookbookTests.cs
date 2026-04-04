using Regira.TreeList;
using Regira.TreeList.Abstractions;
using TreeList.Testing.Infrastructure;

namespace TreeList.Testing;

/// <summary>
/// Unit tests for <see cref="TreeListExtensions"/> using a cookbook scenario (many-to-many relationships).
/// <para>
/// An ingredient can be used in multiple recipes, and a recipe has many ingredients.
/// The <see cref="CookbookItem.UsedInRecipes"/> collection models the many-to-many link:
/// an ingredient's parents in the tree are all the recipes it belongs to.
/// </para>
/// <para>
/// Cookbook used across all tests:
/// <code>
/// Pasta  (root recipe)          Pizza  (root recipe)          Salad  (root recipe)
/// ├── Tomato                    ├── Tomato                    ├── Tomato
/// ├── OliveOil                  ├── OliveOil                  ├── OliveOil
/// └── Flour                     ├── Flour                     └── Lettuce
///                               └── Mozzarella
/// </code>
/// Tomato, OliveOil appear under all three recipes (many-to-many).
/// Flour appears under Pasta and Pizza.
/// Mozzarella appears only under Pizza.
/// Lettuce appears only under Salad.
/// </para>
/// </summary>
[TestFixture]
[Parallelizable(ParallelScope.All)]
public class CookbookTests
{
    private class CookbookData
    {
        public TreeList<CookbookItem> Tree { get; init; } = null!;
        public CookbookItem Pasta { get; init; } = null!;
        public CookbookItem Pizza { get; init; } = null!;
        public CookbookItem Salad { get; init; } = null!;
        public CookbookItem Tomato { get; init; } = null!;
        public CookbookItem OliveOil { get; init; } = null!;
        public CookbookItem Flour { get; init; } = null!;
        public CookbookItem Mozzarella { get; init; } = null!;
        public CookbookItem Lettuce { get; init; } = null!;
        public CookbookItem[] AllItems { get; init; } = null!;
        public CookbookItem[] Recipes { get; init; } = null!;
        public CookbookItem[] Ingredients { get; init; } = null!;
    }

    private static CookbookData CreateData()
    {
        var pasta = new CookbookItem { Id = 1, Name = "Pasta" };
        var pizza = new CookbookItem { Id = 2, Name = "Pizza" };
        var salad = new CookbookItem { Id = 3, Name = "Salad" };

        var tomato = new CookbookItem { Id = 4, Name = "Tomato", UsedInRecipes = [pasta, pizza, salad] };
        var oliveOil = new CookbookItem { Id = 5, Name = "OliveOil", UsedInRecipes = [pasta, pizza, salad] };
        var flour = new CookbookItem { Id = 6, Name = "Flour", UsedInRecipes = [pasta, pizza] };
        var mozzarella = new CookbookItem { Id = 7, Name = "Mozzarella", UsedInRecipes = [pizza] };
        var lettuce = new CookbookItem { Id = 8, Name = "Lettuce", UsedInRecipes = [salad] };

        var allItems = new[] { pasta, pizza, salad, tomato, oliveOil, flour, mozzarella, lettuce };

        // Build tree: an item's parents are the recipes it is used in
        var tree = allItems.ToTreeList(item => (IEnumerable<CookbookItem>)item.UsedInRecipes);

        return new CookbookData
        {
            Tree = tree,
            Pasta = pasta,
            Pizza = pizza,
            Salad = salad,
            Tomato = tomato,
            OliveOil = oliveOil,
            Flour = flour,
            Mozzarella = mozzarella,
            Lettuce = lettuce,
            AllItems = allItems,
            Recipes = [pasta, pizza, salad],
            Ingredients = [tomato, oliveOil, flour, mozzarella, lettuce]
        };
    }

    // ─── ToTreeList overloads ──────────────────────────────────────────────────

    [Test]
    public void ToTreeList_WithManyToManyParentSelector_CorrectNodeCount()
    {
        var data = CreateData();

        // Pasta: 3 ingredients, Pizza: 4, Salad: 3 → 3 + 3 + 4 + 3 = 13 nodes total
        Assert.That(data.Tree.Count, Is.EqualTo(13));
    }

    [Test]
    public void ToTreeList_WithManyToManyParentSelector_RecipesAreRoots()
    {
        var data = CreateData();

        Assert.That(data.Tree.Roots, Has.Length.EqualTo(3));
        Assert.That(data.Tree.Roots.Select(n => n.Value), Is.EquivalentTo(data.Recipes));
    }

    [Test]
    public void ToTreeList_WithChildrenSelector_BuildsEquivalentTree()
    {
        var data = CreateData();
        var allItems = data.AllItems;
        var recipes = data.Recipes;

        var tree = allItems.ToTreeList(
            recipes,
            (ITreeNode<CookbookItem> node) => allItems.Where(item => item.UsedInRecipes.Contains(node.Value)));

        Assert.That(tree.Count, Is.EqualTo(data.Tree.Count));
        Assert.That(tree.Roots.Select(n => n.Value), Is.EquivalentTo(recipes));
    }

    // ─── Many-to-many: same ingredient under multiple recipes ─────────────────

    [Test]
    public void GetSelf_TomatoAppearsUnderAllThreeRecipes()
    {
        var data = CreateData();

        // Tomato is used in Pasta, Pizza, Salad → 3 tree nodes, all with Value == tomato
        var tomatoNodes = data.Tree.GetSelf(data.Tomato).ToArray();

        Assert.That(tomatoNodes, Has.Length.EqualTo(3));
        Assert.That(tomatoNodes.All(n => n.Value == data.Tomato), Is.True);
    }

    [Test]
    public void GetSelf_FlourAppearsUnderTwoRecipes()
    {
        var data = CreateData();

        var flourNodes = data.Tree.GetSelf(data.Flour).ToArray();

        Assert.That(flourNodes, Has.Length.EqualTo(2));
    }

    [Test]
    public void GetSelf_MozzarellaAppearsUnderOneRecipe()
    {
        var data = CreateData();

        var mozzarellaNodes = data.Tree.GetSelf(data.Mozzarella).ToArray();

        Assert.That(mozzarellaNodes, Has.Length.EqualTo(1));
    }

    [Test]
    public void GetSelf_MultipleIngredients_ReturnsAllMatchingNodes()
    {
        var data = CreateData();

        // Mozzarella (1 node) + Lettuce (1 node)
        var nodes = data.Tree.GetSelf(new[] { data.Mozzarella, data.Lettuce }).ToArray();

        Assert.That(nodes, Has.Length.EqualTo(2));
        Assert.That(nodes.Select(n => n.Value), Is.EquivalentTo(new[] { data.Mozzarella, data.Lettuce }));
    }

    // ─── GetParents / GetAncestors ────────────────────────────────────────────

    [Test]
    public void GetParents_ForTomatoNodes_ReturnsAllThreeRecipes()
    {
        var data = CreateData();

        var parentValues = data.Tree.GetSelf(data.Tomato).GetParents().Select(n => n.Value).ToArray();

        Assert.That(parentValues, Is.EquivalentTo(data.Recipes));
    }

    [Test]
    public void GetParents_ForRecipeNodes_IsEmpty()
    {
        var data = CreateData();

        var parents = data.Tree.Roots.GetParents().ToArray();

        Assert.That(parents, Is.Empty);
    }

    [Test]
    public void GetAncestors_ForIngredientNodes_ReturnsParentRecipes()
    {
        var data = CreateData();

        // Tomato has depth-1 parents only (no nested recipes in this tree)
        var ancestorValues = data.Tree.GetSelf(data.Tomato).GetAncestors().Select(n => n.Value).ToArray();

        Assert.That(ancestorValues, Is.EquivalentTo(data.Recipes));
    }

    // ─── GetRoots ─────────────────────────────────────────────────────────────

    [Test]
    public void GetRoots_ForAllNodes_ReturnsRecipes()
    {
        var data = CreateData();

        var roots = data.Tree.GetRoots().Select(n => n.Value).ToArray();

        Assert.That(roots, Is.EquivalentTo(data.Recipes));
    }

    [Test]
    public void GetRoots_ForIngredientSubset_ReturnsCorrectRecipeRoots()
    {
        var data = CreateData();
        // All Lettuce nodes trace back to Salad only
        var lettuceNodes = data.Tree.GetSelf(data.Lettuce);

        var roots = lettuceNodes.GetRoots().Select(n => n.Value).ToArray();

        Assert.That(roots, Is.EquivalentTo(new[] { data.Salad }));
    }

    // ─── GetBottom ────────────────────────────────────────────────────────────

    [Test]
    public void GetBottom_ForTree_ReturnsAllIngredientNodes()
    {
        var data = CreateData();

        var bottomNodes = data.Tree.GetBottom().ToArray();

        // All 10 ingredient nodes are leaf nodes (3+4+3 = 10)
        Assert.That(bottomNodes, Has.Length.EqualTo(10));
        Assert.That(bottomNodes.All(n => !n.Children.Any()), Is.True);
    }

    [Test]
    public void GetBottom_IngredientValuesMatchExpected()
    {
        var data = CreateData();

        var bottomValues = data.Tree.GetBottom().Select(n => n.Value).Distinct().ToArray();

        Assert.That(bottomValues, Is.EquivalentTo(data.Ingredients));
    }

    // ─── GetChildren ──────────────────────────────────────────────────────────

    [Test]
    public void GetChildren_ForPasta_ContainsPastaIngredients()
    {
        var data = CreateData();
        var pastaNode = data.Tree.Roots.First(n => n.Value == data.Pasta);

        var childValues = pastaNode.GetChildren().Select(n => n.Value).ToArray();

        Assert.That(childValues, Is.EquivalentTo(new[] { data.Tomato, data.OliveOil, data.Flour }));
    }

    [Test]
    public void GetChildren_ForPizza_ContainsFourIngredients()
    {
        var data = CreateData();
        var pizzaNode = data.Tree.Roots.First(n => n.Value == data.Pizza);

        var childValues = pizzaNode.GetChildren().Select(n => n.Value).ToArray();

        Assert.That(childValues, Is.EquivalentTo(new[] { data.Tomato, data.OliveOil, data.Flour, data.Mozzarella }));
    }

    [Test]
    public void GetChildren_ForCollection_ReturnsAllDirectChildren()
    {
        var data = CreateData();
        var pastaAndSaladNodes = data.Tree.Roots.Where(n => n.Value == data.Pasta || n.Value == data.Salad);

        var childValues = pastaAndSaladNodes.GetChildren().Select(n => n.Value).ToArray();

        // Pasta: Tomato, OliveOil, Flour; Salad: Tomato, OliveOil, Lettuce (distinct nodes but duplicate values)
        Assert.That(childValues, Has.Length.EqualTo(6));
    }

    // ─── GetOffspring ─────────────────────────────────────────────────────────

    [Test]
    public void GetOffspring_ForRecipeNodes_ReturnsIngredientNodes()
    {
        var data = CreateData();

        var offspringCount = data.Tree.Roots.GetOffspring().Count();

        // Same as all ingredient nodes: 3 + 4 + 3 = 10
        Assert.That(offspringCount, Is.EqualTo(10));
    }

    // ─── WithOffspring ────────────────────────────────────────────────────────

    [Test]
    public void WithOffspring_ForPastaNode_IncludesPastaAndItsIngredients()
    {
        var data = CreateData();
        var pastaNodes = data.Tree.Where(n => n.Value == data.Pasta);

        var withOffspringValues = pastaNodes.WithOffspring().Select(n => n.Value).ToArray();

        Assert.That(withOffspringValues, Is.EquivalentTo(new[] { data.Pasta, data.Tomato, data.OliveOil, data.Flour }));
    }

    // ─── GetBrothers ──────────────────────────────────────────────────────────

    [Test]
    public void GetBrothers_ForRecipeNode_ReturnsOtherRecipes()
    {
        var data = CreateData();
        var pastaNode = data.Tree.Roots.First(n => n.Value == data.Pasta);

        // Pasta has no parent, so no brothers
        var brothers = pastaNode.GetBrothers().ToArray();

        Assert.That(brothers, Is.Empty);
    }

    [Test]
    public void GetBrothers_ForIngredientNode_ReturnsSiblingsInSameRecipe()
    {
        var data = CreateData();
        // Flour under Pasta: siblings are Tomato and OliveOil nodes under Pasta
        var flourUnderPasta = data.Tree.GetSelf(data.Flour).First(n => n.Parent!.Value == data.Pasta);

        var brotherValues = flourUnderPasta.GetBrothers().Select(n => n.Value).ToArray();

        Assert.That(brotherValues, Is.EquivalentTo(new[] { data.Tomato, data.OliveOil }));
    }

    // ─── OrderByHierarchy ─────────────────────────────────────────────────────

    [Test]
    public void OrderByHierarchy_WithKeySelector_EachRootPrecedesItsOwnOffspring()
    {
        var data = CreateData();

        // OrderByHierarchy visits: root, then ALL offspring of that root sorted by key, then next root, ...
        var ordered = data.Tree.OrderByHierarchy(n => n.Value.Name).ToArray();

        // Roots appear in alphabetical Name order: Pasta, Pizza, Salad
        var orderedRootValues = ordered.Where(n => data.Recipes.Contains(n.Value)).Select(n => n.Value.Name).ToArray();
        Assert.That(orderedRootValues, Is.Ordered);

        // Each recipe node appears before all of its own ingredient nodes
        foreach (var recipeNode in data.Tree.Roots)
        {
            int recipeIdx = Array.IndexOf(ordered, recipeNode);
            foreach (var ingredientNode in recipeNode.GetOffspring())
            {
                int ingredientIdx = Array.IndexOf(ordered, ingredientNode);
                Assert.That(ingredientIdx, Is.GreaterThan(recipeIdx), $"{ingredientNode.Value.Name} should come after {recipeNode.Value.Name}");
            }
        }
    }

    [Test]
    public void OrderByHierarchy_ContainsAllNodes()
    {
        var data = CreateData();

        var ordered = data.Tree.OrderByHierarchy().ToArray();

        Assert.That(ordered, Has.Length.EqualTo(data.Tree.Count));
    }

    // ─── ToTreeView ───────────────────────────────────────────────────────────

    [Test]
    public void ToTreeView_ContainsAllNodeValues()
    {
        var data = CreateData();

        var treeView = data.Tree.ToTreeView();

        Assert.That(treeView, Has.Count.EqualTo(data.Tree.Count));
    }

    [Test]
    public void ToTreeView_FirstValueIsFirstRecipeRoot()
    {
        var data = CreateData();

        var treeView = data.Tree.ToTreeView();

        // The first element is Pasta (first root in insertion order)
        Assert.That(treeView[0], Is.EqualTo(data.Pasta));
        // Each recipe value appears before the ingredient values under it
        int pastaIdx = treeView.IndexOf(data.Pasta);
        int pizzaIdx = treeView.IndexOf(data.Pizza);
        int saladIdx = treeView.IndexOf(data.Salad);
        Assert.That(pastaIdx, Is.LessThan(pizzaIdx));
        Assert.That(pizzaIdx, Is.LessThan(saladIdx));
    }

    // ─── ReverseTree ──────────────────────────────────────────────────────────

    [Test]
    public void ReverseTree_IngredientValuesBecome_RootValues()
    {
        var data = CreateData();

        var reverseTree = data.Tree.ReverseTree();

        var reverseRootValues = reverseTree.Roots.Select(n => n.Value).ToArray();
        Assert.That(reverseRootValues, Is.EquivalentTo(data.Ingredients));
    }

    [Test]
    public void ReverseTree_RecipeValuesAppearAsLeafNodes()
    {
        var data = CreateData();

        var reverseTree = data.Tree.ReverseTree();

        var recipeNodesInReverse = reverseTree.Where(n => data.Recipes.Contains(n.Value)).ToArray();
        Assert.That(recipeNodesInReverse, Is.Not.Empty);
        Assert.That(recipeNodesInReverse.All(n => !n.Children.Any()), Is.True);
    }

    [Test]
    public void ReverseTree_ContainsAllDistinctValues()
    {
        var data = CreateData();

        var reverseTree = data.Tree.ReverseTree();

        var originalDistinct = data.Tree.Select(n => n.Value).Distinct();
        var reverseDistinct = reverseTree.Select(n => n.Value).Distinct();
        Assert.That(reverseDistinct, Is.EquivalentTo(originalDistinct));
    }
}
