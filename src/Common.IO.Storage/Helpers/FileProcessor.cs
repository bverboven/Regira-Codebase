using Regira.IO.Storage.Abstractions;

namespace Regira.IO.Storage.Helpers;

public class FileProcessor(IFileService fileService) : IFileProcessor
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="so"></param>
    /// <param name="handleFile"></param>
    /// <param name="processRecursively">
    ///     Set true when all subfolders should be processed, but not at once.
    ///     When true, overwrites the Recursive property to false in the FileSearchObject, but calls this function recursively for each subfolder.
    /// </param>
    /// <returns></returns>
    public async Task ProcessFiles(FileSearchObject so, Func<string, IFileService, Task> handleFile, bool? processRecursively = null)
    {
        var recursive = processRecursively ?? false ? !processRecursively.Value : so.Recursive;
        // files
        var filesSo = new FileSearchObject
        {
            FolderUri = so.FolderUri,
            Extensions = so.Extensions,
            Recursive = recursive,
            Type = FileEntryTypes.Files
        };
        var fileUris = await fileService.List(filesSo);
        // handle file
        var handleFilesFuncs = fileUris
            .Select(fileUri =>
            {
                var absoluteUri = fileService.GetAbsoluteUri(fileUri);
                return handleFile(absoluteUri, fileService);
            })
            .ToArray();
        Task.WaitAll(handleFilesFuncs);

        // recursive
        if (processRecursively ?? so.Recursive)
        {
            // directories
            var dirSo = new FileSearchObject
            {
                FolderUri = so.FolderUri,
                Type = FileEntryTypes.Directories,
                Recursive = recursive
            };
            var directoryUris = await fileService.List(dirSo);
            var dirTasks = directoryUris
                .Select(dirUri => ProcessFiles(new FileSearchObject { FolderUri = dirUri, Extensions = so.Extensions, Recursive = so.Recursive }, handleFile, processRecursively))
                .ToArray();
            Task.WaitAll(dirTasks);
        }
    }
}