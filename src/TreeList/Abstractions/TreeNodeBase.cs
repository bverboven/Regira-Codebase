namespace Regira.TreeList.Abstractions;

public abstract class TreeNodeBase<T> : ITreeNode<T>
{
    public T Value { get; protected set; } = default!;
    public int Level { get; protected set; }
    public TreeNode<T>? Root { get; protected set; }
    public TreeNode<T>? Parent { get; protected set; }
    public abstract ICollection<TreeNode<T>> Children { get; }
}