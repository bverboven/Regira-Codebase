using System.Collections.ObjectModel;

namespace Regira.TreeList;

public class TreeView<T>(TreeList<T> tree)
    : ReadOnlyCollection<T>(tree.OrderByHierarchy().Select(n => n.Value).ToList())
{
    public TreeList<T> Tree { get; } = tree;
}