﻿using IO.Testing.Helpers;
using Regira.IO.Abstractions;
using Regira.IO.Storage.Compression;

namespace IO.Testing.Compression;

[TestFixture]
[Parallelizable(ParallelScope.Self)]
public class ZipStorageTests
{
    protected IMemoryFile SourceZip { get; set; } = null!;
    public StorageTestHelper.IStorageTestContext StorageTestContext { get; set; }

    [SetUp]
    public void Setup()
    {
        StorageTestContext = StorageTestHelper.CreateDecoratedFileService((files, _) =>
        {
            SourceZip = files.Zip();
            return new ZipFileService(new ZipFileCommunicator { SourceFile = SourceZip });
        });
    }
    [TearDown]
    public async Task TearDown()
    {
        await StorageTestContext.DisposeAsync();
        SourceZip.Dispose();
    }

    [Test]
    public async Task List() => await StorageTestContext.Test_List();
    [Test]
    public async Task GetBytes() => await StorageTestContext.Test_GetBytes();
    [Test]
    public async Task Filter_By_Folder() => await StorageTestContext.Test_Filter_By_Folder();
    [Test]
    public async Task Filter_By_Extension() => await StorageTestContext.Test_Filter_By_Extension();
    [Test]
    public async Task Filter_Recursive() => await StorageTestContext.Test_Filter_Recursive();
    //[Test]
    //public async Task Filter_By_EntryType() => await StorageTestContext.Test_Filter_By_EntryType();

    [Test]
    public async Task Add_File() => await StorageTestContext.Test_Add_File();
    [Test]
    public async Task Update_File() => await StorageTestContext.Test_Update_File();
    [Test]
    public async Task Remove_File() => await StorageTestContext.Test_Remove_File();
}