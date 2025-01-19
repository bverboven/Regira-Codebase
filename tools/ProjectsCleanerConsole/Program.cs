// See https://aka.ms/new-console-template for more information

var srcDir = Directory.GetCurrentDirectory();
string? slnFile;
do
{
    if (string.IsNullOrWhiteSpace(srcDir))
    {
        break;
    }
    slnFile = Directory.GetFiles(srcDir, "*.sln", SearchOption.TopDirectoryOnly).FirstOrDefault();
    if (string.IsNullOrWhiteSpace(slnFile))
    {
        srcDir = Directory.GetParent(srcDir)?.FullName;
    }
} while (string.IsNullOrWhiteSpace(slnFile));

var files = Directory.GetFiles(srcDir, "*.csproj", SearchOption.AllDirectories)
    .Where(f => f.Contains("\\src\\") || f.Contains("\\tests\\"))
    .ToArray();

CleanUp(files);

static void CleanUp(IEnumerable<string> projectPaths)
{
    var projectDirs = projectPaths.Select(Path.GetDirectoryName!).Distinct();
    foreach (var dir in projectDirs)
    {
        Console.WriteLine($"Processing '{dir}'");
        var binDir = Path.Combine(dir!, "bin");
        if (Directory.Exists(binDir))
        {
            Directory.Delete(binDir, true);
        }
        var objDir = Path.Combine(dir!, "obj");
        if (Directory.Exists(objDir))
        {
            Directory.Delete(objDir, true);
        }
    }
}