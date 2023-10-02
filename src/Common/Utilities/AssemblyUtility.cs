using System.Reflection;

namespace Regira.Utilities;

public static class AssemblyUtility
{
    // https://stackoverflow.com/questions/52797/how-do-i-get-the-path-of-the-assembly-the-code-is-in#answer-283917
    /// <summary>
    /// Uses executing assembly when no parameter is given
    /// </summary>
    /// <param name="assembly"></param>
    /// <returns></returns>
    public static string? GetAssemblyDirectory(Assembly? assembly = null)
    {
        var codeBase = (assembly ?? Assembly.GetExecutingAssembly()).Location;
        var uri = new UriBuilder(codeBase);
        var path = Uri.UnescapeDataString(uri.Path);
        return Path.GetDirectoryName(path);
    }
}