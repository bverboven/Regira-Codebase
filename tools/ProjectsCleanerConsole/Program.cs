// See https://aka.ms/new-console-template for more information

var srcDir = args.Any() && !string.IsNullOrWhiteSpace(args[0]) ? args[0] : Directory.GetCurrentDirectory();

var slnFiles = Directory.GetFiles(srcDir, "*.sln", SearchOption.AllDirectories);

foreach (var slnFile in slnFiles)
{
    var slnDir = Directory.GetParent(slnFile)!.FullName;

    var files = Directory.GetFiles(slnDir, "*.csproj", SearchOption.AllDirectories);

    CleanUp(files);

}
static void CleanUp(IEnumerable<string> projectPaths)
{
    var projectDirs = projectPaths.Select(Path.GetDirectoryName).Distinct();
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