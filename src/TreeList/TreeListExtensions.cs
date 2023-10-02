using Regira.TreeList.Abstractions;

namespace Regira.TreeList;

public static class TreeListExtensions
{
    public static TreeList<T> ToTreeList<T>(this IEnumerable<TreeNode<T>> collection)
    {
        return new TreeList<T>(collection);
    }
    public static TreeList<T> ToTreeList<T>(this IEnumerable<T> collection)
    {
        var items = collection as IList<T> ?? collection.ToList();
        var tree = new TreeList<T>(items.Count);
        foreach (var item in items)
            tree.AddValue(item);
        return tree;
    }

    public static TreeList<T> ToTreeList<T>(this IEnumerable<T> collection, Func<T, T> getParent, TreeList<T>.Options? options = null)
    {
        var list = collection as IList<T> ?? collection.ToList();
        return ToTreeList(list, i => new[] { getParent(i) }.Where(x => x != null), options);
    }
    public static TreeList<T> ToTreeList<T>(this IEnumerable<T> collection, Func<T, IEnumerable<T>> fkSelector, TreeList<T>.Options? options = null)
    {
        var items = collection as IList<T> ?? collection.ToList();
        var tree = new TreeList<T>(items.Count, options);
        tree.Fill(items, fkSelector);
        return tree;
    }
    public static TreeList<T> ToTreeList<T>(this IEnumerable<T> collection, IEnumerable<T> roots, Func<ITreeNode<T>, IEnumerable<T>> getChildren, TreeList<T>.Options? options = null)
    {
        var items = collection as IList<T> ?? collection.ToList();
        var tree = new TreeList<T>(items.Count, options);
        tree.Fill(roots, getChildren);
        return tree;
    }

    public static TreeView<T> ToTreeView<T>(this TreeList<T> tree)
    {
        return new TreeView<T>(tree);
    }

    public static IEnumerable<TreeNode<T>> OrderByHierarchy<T>(this IEnumerable<TreeNode<T>> collection)
    {
        return OrderByHierarchy<T, T>(collection);
    }
    public static IEnumerable<TreeNode<T>> OrderByHierarchy<T, TKey>(this IEnumerable<TreeNode<T>> collection, Func<TreeNode<T>, TKey>? keySelector = null)
    {
        IEnumerable<TreeNode<T>> GetOrderedCollection(IEnumerable<TreeNode<T>> items)
        {
            if (keySelector != null)
            {
                return items.OrderBy(keySelector);
            }

            return items;
        }

        foreach (var root in GetOrderedCollection(collection.GetRoots()))
        {
            yield return root;
            foreach (var child in GetOrderedCollection(root.GetOffspring()))
            {
                yield return child;
            }
        }
    }

    //TreeNode extensions
    public static TreeNode<T> GetRoot<T>(this TreeNode<T> item)
    {
        return item
            .GetAncestors()
            .FirstOrDefault(a => a.Parent == null) ?? item;
    }
    public static IEnumerable<TreeNode<T>> GetAncestors<T>(this TreeNode<T> item)
    {
        IEnumerable<TreeNode<T>> InnerGetAncestors(TreeNode<T> node)
        {
            var parent = node.Parent;
            while (parent != null)
            {
                yield return parent;
                parent = parent.Parent;
            }
        }

        return InnerGetAncestors(item)
            .Reverse();
    }
    public static IEnumerable<TreeNode<T>> GetChildren<T>(this TreeNode<T> item)
    {
        return item.Children;
    }
    public static IEnumerable<TreeNode<T>> GetOffspring<T>(this TreeNode<T> item)
    {
        var children = item.GetChildren();
        foreach (var child in children)
        {
            yield return child;
            foreach (var grandChild in child.GetOffspring())
            {
                yield return grandChild;
            }
        }
    }
    public static IEnumerable<TreeNode<T>> GetBrothers<T>(this TreeNode<T> item)
    {
        return item.Parent
                   ?.GetChildren()
                   .Where(i => !item.Equals(i))
               ?? Array.Empty<TreeNode<T>>();
    }
    public static IEnumerable<TreeNode<T>> GetUncles<T>(this TreeNode<T> item)
    {
        return item.Parent
                   ?.GetBrothers()
                   .GetChildren()
               ?? Array.Empty<TreeNode<T>>();
    }
    public static IEnumerable<TreeNode<T>> GetNephews<T>(this TreeNode<T> item)
    {
        return item
            .GetUncles()
            .GetChildren();
    }

    //TreeList extensions
    public static IEnumerable<TreeNode<T>> GetSelf<T>(this IEnumerable<TreeNode<T>> collection, IEnumerable<T> selectedItems)
    {
        return collection.Join(selectedItems, c => c.Value, i => i, (c, _) => c);
    }
    public static IEnumerable<TreeNode<T>> GetSelf<T>(this IEnumerable<TreeNode<T>> collection, T item)
    {
        return collection.GetSelf(new[] { item });
    }
    public static IEnumerable<TreeNode<T>> GetParents<T>(this IEnumerable<TreeNode<T>> collection)
    {
        return collection
            .Where(i => i.Parent != null)
            .Select(i => i.Parent!)
            .Distinct();
    }
    public static IEnumerable<TreeNode<T>> GetAncestors<T>(this IEnumerable<TreeNode<T>> collection)
    {
        return collection
            .SelectMany(i => i.GetAncestors())
            .Distinct();
    }
    public static IEnumerable<TreeNode<T>> GetRoots<T>(this IEnumerable<TreeNode<T>> collection)
    {
        return collection
            .Select(i => i.GetRoot())
            .Distinct();
    }
    public static IEnumerable<TreeNode<T>> GetChildren<T>(this IEnumerable<TreeNode<T>> collection)
    {
        return collection
            .SelectMany(c => c.GetChildren())
            .Distinct();
    }
    public static IEnumerable<TreeNode<T>> GetOffspring<T>(this IEnumerable<TreeNode<T>> collection)
    {
        return collection
            .SelectMany(i => i.GetOffspring())
            .Distinct();
    }
    public static IEnumerable<TreeNode<T>> WithOffspring<T>(this IEnumerable<TreeNode<T>> collection)
    {
        var list = collection as IList<TreeNode<T>> ?? collection.ToList();
        return list
            .Concat(list.GetOffspring())
            .Distinct();
    }
    public static IEnumerable<TreeNode<T>> GetBrothers<T>(this IEnumerable<TreeNode<T>> collection)
    {
        return collection
            .SelectMany(i => i.GetBrothers())
            .Distinct();
    }
    public static IEnumerable<TreeNode<T>> GetUncles<T>(this IEnumerable<TreeNode<T>> collection)
    {
        return collection
            .SelectMany(i => i.GetUncles())
            .Distinct();
    }
}