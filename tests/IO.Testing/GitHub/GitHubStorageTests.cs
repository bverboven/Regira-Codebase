using Microsoft.Extensions.Configuration;
using NUnit.Framework.Legacy;
using Regira.IO.Storage;
using Regira.IO.Storage.Abstractions;
using Regira.IO.Storage.GitHub;
using Regira.Serializing.Newtonsoft.Json;
using Regira.Utilities;
using System.Reflection;

namespace IO.Testing.GitHub;

[TestFixture]
public class GitHubStorageTests
{
    [Test]
    public virtual async Task List()
    {
        var fileService = GetFileService();
        var files = (await fileService.List()).AsList();
        Assert.That(files, Is.Not.Empty);
    }
    [Test]
    public virtual async Task Filter_By_Folder()
    {
        var so = new FileSearchObject
        {
            FolderUri = "img"
        };
        var fileService = GetFileService();
        var files = (await fileService.List(so)).AsList();
        Assert.That(files, Is.Not.Empty);
    }
    [Test]
    public virtual async Task Filter_Recursive()
    {
        var so = new FileSearchObject
        {
            Recursive = true,
        };
        var fileService = GetFileService();
        var files = (await fileService.List(so)).AsList();
        Assert.That(files, Is.Not.Empty);
    }
    [Test]
    public virtual async Task Filter_By_EntryType()
    {
        // files
        var so = new FileSearchObject
        {
            Recursive = true,
            Type = FileEntryTypes.Files
        };
        var fileService = GetFileService();
        var files = (await fileService.List(so)).AsList();
        Assert.That(files, Is.Not.Empty);
        // folders
        so = new FileSearchObject
        {
            Recursive = true,
            Type = FileEntryTypes.Directories
        };
        var folders = (await fileService.List(so)).AsList();
        Assert.That(folders, Is.Not.Empty);
        // both
        so = new FileSearchObject
        {
            Recursive = true,
            Type = FileEntryTypes.All
        };
        var both = (await fileService.List(so)).AsList();
        Assert.That(both, Is.Not.Empty);
        Assert.That(both, Is.EquivalentTo(folders.Concat(files)));
    }
    [Test]
    public virtual async Task Filter_By_Extension()
    {
        var so = new FileSearchObject
        {
            Recursive = true,
            Extensions = new[] { ".png", ".txt", ".sql" }
        };
        var fileService = GetFileService();
        var files = (await fileService.List(so)).AsList();
        Assert.That(files, Is.Not.Empty);
        foreach (var file in files)
        {
            var pathWithoutQuery = file.Split('?').First();
            Assert.That(so.Extensions.Any(e => pathWithoutQuery.EndsWith(e, StringComparison.CurrentCultureIgnoreCase)), Is.True);
        }
    }
    [Test]
    public virtual async Task GetBytes()
    {
        var fileService = GetFileService();
        var fso = new FileSearchObject
        {
            Type = FileEntryTypes.Files,
            FolderUri = "img",
            Recursive = true
        };
        var files = (await fileService.List(fso)).AsList();
        Assert.That(files, Is.Not.Empty);
        // Getting file contents is sometimes very slow (penalty for testing too much?)
        foreach (var file in files.Take(1))
        {
            var bytes = await fileService.GetBytes(file);
            ClassicAssert.IsNotNull(bytes);
            Assert.That(bytes!.Length > 0, Is.True);
        }
    }


    IFileService GetFileService()
    {
        var config = new ConfigurationBuilder()
            .AddUserSecrets(Assembly.GetExecutingAssembly())
            .Build();
        return new GitHubService(new GitHubOptions
        {
            Uri = config["Storage:GitHub:Uri"]!,
            Key = config["Storage:GitHub:Key"]
        }, new JsonSerializer());
    }
}
