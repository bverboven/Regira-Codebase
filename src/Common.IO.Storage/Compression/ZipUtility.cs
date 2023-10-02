using Regira.IO.Abstractions;
using Regira.IO.Extensions;
using Regira.IO.Models;
using Regira.IO.Storage.Helpers;
using System.IO.Compression;

namespace Regira.IO.Storage.Compression;

public static class ZipUtility
{
    public static ZipArchive ToZipArchive(IBinaryFile file)
        => new(file.GetStream() ?? throw new Exception("Could not retrieve stream"), ZipArchiveMode.Update, true);

    public static IMemoryFile Zip(IEnumerable<IBinaryFile> files)
    {
        var zipStream = new MemoryStream();

        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
        {
            AddFiles(archive, files);
        }
        zipStream.Position = 0;

        return zipStream.ToMemoryFile();
    }
    public static IMemoryFile Zip(IEnumerable<string> paths, string? baseFolder = null)
    {
        var zipFiles = GetFiles(paths, baseFolder);
        return Zip(zipFiles);
    }


    public static BinaryFileCollection Unzip(IBinaryFile zip)
    {
        using var zipStream = zip.GetStream() ?? throw new Exception("Could not retrieve stream");
        using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read, true);
        var files = ExtractFiles(archive);
        return new BinaryFileCollection(files);
    }
    public static BinaryFileCollection Unzip(string sourceDirectory)
    {
        using var archive = ZipFile.OpenRead(sourceDirectory);
        var files = ExtractFiles(archive);
        return new BinaryFileCollection(files);
    }

    public static string[] Unzip(IBinaryFile zip, string targetDirectory)
    {
        using var zipStream = zip.GetStream() ?? throw new Exception("Could not retrieve stream");
        using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read, true);
        return ExtractFiles(targetDirectory, archive.Entries);
    }
    public static string[] Unzip(string sourceDirectory, string targetDirectory)
    {
        using var archive = ZipFile.OpenRead(sourceDirectory);
        return ExtractFiles(targetDirectory, archive.Entries);
    }


    public static void AddFiles(Stream zipArchive, IEnumerable<IBinaryFile> files)
    {
        using var archive = new ZipArchive(zipArchive, ZipArchiveMode.Update, true);
        AddFiles(archive, files);
    }
    public static void AddFiles(ZipArchive archive, IEnumerable<IBinaryFile> files)
    {
        foreach (var zipFile in files)
        {
            AddFile(archive, zipFile);
        }
    }
    public static void AddFile(ZipArchive archive, IBinaryFile file)
    {
        using var fileStream = file.GetStream();
        if (fileStream != null)
        {
            var filename = file.Identifier ?? file.FileName;
            if (string.IsNullOrWhiteSpace(filename))
            {
                throw new Exception($"{nameof(file.Identifier)} and {nameof(file.FileName)} of file should not be empty");
            }
            var entry = archive.CreateEntry(filename);
            using var entryStream = entry.Open();
            fileStream.Seek(0, SeekOrigin.Begin);
            fileStream.CopyTo(entryStream);
        }
        else
        {
            throw new Exception("Could not add file to zip archive. File has no stream, no bytes and no path defined.");
        }
    }

    public static void RemoveFile(Stream zipArchive, IBinaryFile file)
    {
        RemoveFiles(zipArchive, new[] { file });
    }
    public static void RemoveFiles(Stream zipArchive, IEnumerable<IBinaryFile> files)
    {
        using var archive = new ZipArchive(zipArchive, ZipArchiveMode.Update, true);
        RemoveFiles(archive, files);
    }
    public static void RemoveFiles(ZipArchive zipArchive, IEnumerable<IBinaryFile> files)
    {
        var entries = zipArchive.Entries.Where(e => files.Any(f => (f.Identifier ?? f.FileName) == e.FullName)).ToArray();
        foreach (var entry in entries)
        {
            entry.Delete();
        }
    }

    private static string[] ExtractFiles(string targetDirectory, IEnumerable<ZipArchiveEntry> entries)
    {
        var files = new List<string>();

        foreach (var entry in entries)
        {
            using var zipStream = entry.Open();
            var fullPath = Path.Combine(targetDirectory, entry.FullName.TrimEnd('/'));
            if (IsDirectory(entry))
            {
                Directory.CreateDirectory(fullPath);
            }
            else
            {
                var directory = Path.GetDirectoryName(fullPath);
                Directory.CreateDirectory(directory!);
                using (var fileStream = File.Create(fullPath))
                {
                    zipStream.CopyTo(fileStream);
                }

                files.Add(fullPath);
            }
        }

        return files.ToArray();
    }
    private static IBinaryFile[] ExtractFiles(ZipArchive archive)
    {
        return archive.Entries
            .Select(entry =>
            {
                var ms = new MemoryStream();
                using var entryStream = entry.Open();
                entryStream.CopyTo(ms);
                IBinaryFile item = new BinaryFileItem
                {
                    Identifier = entry.FullName,
                    FileName = entry.Name,
                    Length = entry.Length,
                    Stream = ms
                };
                return item;
            })
            .ToArray();
    }

    private static IEnumerable<IBinaryFile> GetFiles(IEnumerable<string> paths, string? baseFolder = null)
    {
        var fileCollection = paths.ToArray();
        baseFolder ??= FileNameHelper.GetBaseFolder(fileCollection);

        return fileCollection
            .Select(p => new BinaryFileItem
            {
                // ReSharper disable once PossibleNullReferenceException
                Identifier = p.Substring(baseFolder?.Length ?? 0).TrimStart('\\', '/'),
                FileName = Path.GetFileName(p),
                Path = p
            });
    }

    private static bool IsDirectory(ZipArchiveEntry entry)
    {
        return entry.FullName.EndsWith("/");
    }
}