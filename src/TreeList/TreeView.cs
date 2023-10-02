using System.Collections.ObjectModel;

namespace Regira.TreeList;

public class TreeView<T> : ReadOnlyCollection<T>
{
    public TreeList<T> Tree { get; }
    public TreeView(TreeList<T> tree)
        : base(tree.OrderByHierarchy().Select(n => n.Value).ToList())
    {
        Tree = tree;
    }
}