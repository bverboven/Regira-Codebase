using Regira.TreeList;
using System.Diagnostics;
[assembly: Parallelizable(ParallelScope.Fixtures)]

namespace TreeList.Testing;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class CreationTests
{
    public class TestData<T>
    {
        public TreeList<T> Tree { get; set; } = null!;
        public T[] Roots { get; set; } = null!;
        public T[] Values { get; set; } = null!;
    }
    public class FsItem
    {
        public string Path { get; set; } = null!;
        public string ParentDirectory { get; set; } = null!;
    }

    private readonly string _testDirectory;
    public CreationTests()
    {
        _testDirectory = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "../../../../")).FullName;
    }

    [Test]
    public void Create_Directories_Tree()
    {
        var data = GetDirectoryData();
        var tree = data.Tree;
        var directories = data.Values;
        var roots = data.Roots;

        Assert.AreEqual(directories.Length, tree.Count);
        CollectionAssert.AreEquivalent(roots, tree.Roots.Select(n => n.Value));
        foreach (var node in tree)
        {
            if (node.Parent == null)
            {
                CollectionAssert.Contains(roots, node.Value);
            }
            else
            {
                Assert.AreEqual(node.Parent.Value, Path.GetDirectoryName(node.Value));
            }

            var subDirectories = Directory.GetDirectories(node.Value).ToArray();
            var childDirectories = node.Children.Select(n => n.Value).ToArray();
            CollectionAssert.AreEquivalent(subDirectories, childDirectories);
        }
    }
    [Test]
    public void Offspring()
    {
        var data = GetDirectoryData();
        var tree = data.Tree;

        foreach (var root in tree.Roots)
        {
            var subDirectories = Directory.GetDirectories(root.Value, string.Empty, SearchOption.AllDirectories);
            var offspring = root.GetOffspring().Select(o => o.Value);
            CollectionAssert.AreEquivalent(subDirectories, offspring);
        }
    }
    [Test]
    public void Ancestors()
    {
        var data = GetDirectoryData();
        var tree = data.Tree;
        var directories = data.Values;

        var lowestLevelChild = tree.OrderByDescending(n => n.Level)
            .ThenBy(n => n.Value)
            .First();
        var highestSegmentCountDirectory = directories
            .OrderByDescending(d => d.Split('\\').Length)
            .ThenBy(d => d)
            .First();
        Assert.AreEqual(highestSegmentCountDirectory, lowestLevelChild.Value);

        var ancestors = lowestLevelChild.GetAncestors()
            .Select(a => a.Value)
            .ToArray();
        var parentDirectories = Path.GetDirectoryName(highestSegmentCountDirectory)!
            .Substring(_testDirectory.Length)
            .Trim('\\')
            .Split('\\', StringSplitOptions.RemoveEmptyEntries)
            .Aggregate(new List<string>(), (r, s) =>
            {
                r.Add(Path.Combine(_testDirectory, r.LastOrDefault() ?? string.Empty, s));
                return r;
            });
        CollectionAssert.AreEquivalent(parentDirectories, ancestors);
    }

    [Test]
    public void Create_Files_And_Directories_Tree()
    {
        var data = GetFsItemData();
        var tree = data.Tree;
        var roots = data.Roots;
        var items = data.Values;

        Assert.AreEqual(items.Length, tree.Count);
        foreach (var node in tree)
        {
            if (node.Parent == null)
            {
                CollectionAssert.Contains(roots, node.Value);
            }
            else
            {
                Assert.AreEqual(node.Parent!.Value.Path, node.Value.ParentDirectory);
            }

            // only directories can have children
            if (Directory.Exists(node.Value.Path))
            {
                var subPaths = Directory.GetDirectories(node.Value.Path).Concat(Directory.GetFiles(node.Value.Path)).ToArray();
                var childPaths = node.Children.Select(n => n.Value.Path).ToArray();
                CollectionAssert.AreEquivalent(subPaths, childPaths);
            }
        }
    }

    [Test]
    public void OrderBy_Hierarchy()
    {
        var data = GetFsItemData();
        var tree = data.Tree;
        var items = data.Values;

        var sortedItems = items
            .OrderBy(x => x.Path)
            .ToArray();
        var treeItems = tree.Select(n => n.Value).ToArray();
        var sw = new Stopwatch();
        sw.Start();
        var sortedNodes = tree.OrderByHierarchy(n => n.Value.Path)
            .ToArray();
        sw.Stop();
        Debug.Print($"OrderBy_Hierarchy tree.OrderByHierarchy: {sw.ElapsedMilliseconds} ms");

        var sortedNodeValues = sortedNodes
            .Select(n => n.Value)
            .ToArray();

        CollectionAssert.AreNotEqual(sortedItems, treeItems);
        CollectionAssert.AreEqual(sortedItems, sortedNodeValues);
        // slow
        CollectionAssert.AreEquivalent(sortedItems, treeItems);
    }

    [Test]
    public void TreeView()
    {
        var data = GetFsItemData();
        var tree = data.Tree;

        var sortedValues = tree.OrderByHierarchy()
            .Select(n => n.Value)
            .ToArray();
        var treeView = tree.ToTreeView();
        CollectionAssert.AreEqual(sortedValues, treeView);
    }

    private TestData<string> GetDirectoryData()
    {
        var directories = Directory.GetDirectories(_testDirectory, string.Empty, SearchOption.AllDirectories)
            // shuffle
            .OrderBy(_ => Guid.NewGuid())
            .ToArray();
        var roots = directories.Where(r => !directories.Contains(Path.GetDirectoryName(r)))
            .ToArray();
        var tree = new TreeList<string>(directories.Length);
        tree.Fill(roots, parentNode => directories.Where(path => Path.GetDirectoryName(path) == parentNode.Value));
        return new TestData<string>
        {
            Tree = tree,
            Values = directories,
            Roots = roots
        };
    }
    private TestData<FsItem> GetFsItemData()
    {
        var directories = Directory.GetDirectories(_testDirectory, string.Empty, SearchOption.AllDirectories)
            .Select(d => new FsItem { Path = d, ParentDirectory = Path.GetDirectoryName(d)! });
        var files = Directory.GetFiles(_testDirectory, string.Empty, SearchOption.AllDirectories)
            .Select(f => new FsItem { Path = f, ParentDirectory = Path.GetDirectoryName(f)! });
        var items = directories.Concat(files)
            // shuffle
            .OrderBy(_ => Guid.NewGuid())
            .ToArray();

        var sw = new Stopwatch();
        sw.Start();
        var roots = items.Where(x => x.ParentDirectory == _testDirectory.TrimEnd('\\'))
            .ToArray();
        var tree = new TreeList<FsItem>(items.Length);
        // using FsItem with already calculated ParentDirectory to prevent overhead of requesting Path.GetDirectoryName (up to 6 sec longer)
        tree.Fill(roots, parentNode => items.Where(item => item.ParentDirectory == parentNode.Value.Path));
        sw.Stop();
        Debug.Print($"GetFsItemData tree.Fill: {sw.ElapsedMilliseconds} ms");

        return new TestData<FsItem>
        {
            Tree = tree,
            Roots = roots,
            Values = items
        };
    }
}