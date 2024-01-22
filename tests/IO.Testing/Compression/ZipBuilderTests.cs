using NUnit.Framework.Legacy;
using Regira.IO.Extensions;
using Regira.IO.Models;
using Regira.IO.Storage.Compression;
using Regira.IO.Utilities;

namespace IO.Testing.Compression;

[TestFixture]
[Parallelizable(ParallelScope.Self)]
public class ZipBuilderTests
{
    private BinaryFileItem[] _sourceFiles = null!;
    private readonly string _assetsDir;
    public ZipBuilderTests()
    {
        _assetsDir = Path.Combine(Directory.GetCurrentDirectory(), "assets", "zip-builder");
        Directory.CreateDirectory(_assetsDir);
    }

    [SetUp]
    public void Setup()
    {
        _sourceFiles = TestFilesCreator.Create(_assetsDir)
            .ToArray();
    }
    [TearDown]
    public void Teardown()
    {
        TestFilesCreator.Clear(_assetsDir);
    }

    [Test]
    public async Task Can_Zip_Files()
    {
        var zipBuilder = new ZipBuilder();
        using var zipFile = await zipBuilder
            .For(_sourceFiles)
            .Build();
        ClassicAssert.IsNotEmpty(zipFile.GetBytes()!);
        ClassicAssert.IsTrue(zipFile.GetLength() > 0);
    }
    [Test]
    public async Task Can_Add_Files_To_Existing_Zip()
    {
        var zipBuilder1 = new ZipBuilder();
        var files1 = _sourceFiles.Take(5).ToArray();
        var files2 = _sourceFiles.Except(files1).ToArray();
        using var zipFile = await zipBuilder1
            .For(files1)
            .Build();
        ClassicAssert.AreEqual(files1.Length, ZipUtility.Unzip(zipFile.ToBinaryFile()).Count);
        var zipStream1Length = zipFile.Stream?.Length;
        var zipBuilder2 = new ZipBuilder();
        await zipBuilder2
            .For(zipFile.Stream)
            .For(files2)
            .Build();
        ClassicAssert.IsTrue(zipFile.Stream?.Length > zipStream1Length);
        ClassicAssert.AreEqual(_sourceFiles.Length, ZipUtility.Unzip(zipFile.ToBinaryFile()).Count);
    }

    [Test]
    public async Task Can_Unzip_Files()
    {
        var zipBuilder = new ZipBuilder();
        using var zipFile = await zipBuilder
            .For(_sourceFiles)
            .Build();
        //FileSystemUtility.SaveStream("zipped.zip", zipStream);
        var unzippedFiles = ZipUtility.Unzip(zipFile.ToBinaryFile());
        // compare count
        ClassicAssert.AreEqual(_sourceFiles.Length, unzippedFiles.Count);
        // compare bytes
        CollectionAssert.AreEqual(_sourceFiles.Select(f => f.Bytes), unzippedFiles.Select(f => FileUtility.GetBytes(f.Stream)));
        unzippedFiles.Dispose();
    }
}