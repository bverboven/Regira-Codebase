using Microsoft.Extensions.Configuration;
using Regira.IO.Storage;
using Regira.IO.Storage.Abstractions;
using Regira.IO.Storage.GitHub;
using Regira.Serializing.Newtonsoft.Json;
using Regira.Utilities;
using System.Reflection;

namespace IO.Testing.GitHub
{
    [TestFixture]
    public class GitHubStorageTests
    {
        [Test]
        public virtual async Task List()
        {
            var fileService = GetFileService();
            var files = (await fileService.List()).AsList();
            CollectionAssert.IsNotEmpty(files);
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
            CollectionAssert.IsNotEmpty(files);
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
            CollectionAssert.IsNotEmpty(files);
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
            CollectionAssert.IsNotEmpty(files);
            // folders
            so = new FileSearchObject
            {
                Recursive = true,
                Type = FileEntryTypes.Directories
            };
            var folders = (await fileService.List(so)).AsList();
            CollectionAssert.IsNotEmpty(folders);
            // both
            so = new FileSearchObject
            {
                Recursive = true,
                Type = FileEntryTypes.All
            };
            var both = (await fileService.List(so)).AsList();
            CollectionAssert.IsNotEmpty(both);
            CollectionAssert.AreEquivalent(folders.Concat(files), both);
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
            CollectionAssert.IsNotEmpty(files);
            foreach (var file in files)
            {
                var pathWithoutQuery = file.Split('?').First();
                Assert.IsTrue(so.Extensions.Any(e => pathWithoutQuery.EndsWith(e, StringComparison.CurrentCultureIgnoreCase)));
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
            CollectionAssert.IsNotEmpty(files);
            foreach (var file in files.Take(3))
            {
                var bytes = await fileService.GetBytes(file);
                Assert.IsNotNull(bytes);
                Assert.IsTrue(bytes!.Length > 0);
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
}
