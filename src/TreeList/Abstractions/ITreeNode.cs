namespace Regira.TreeList.Abstractions;

public interface ITreeNode<T>
{
    T Value { get; }
    int Level { get; }
    TreeNode<T>? Root { get; }
    TreeNode<T>? Parent { get; }
    ICollection<TreeNode<T>> Children { get; }
}