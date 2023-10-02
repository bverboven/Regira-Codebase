namespace IO.Testing.Abstractions;

public static class FileConstants
{
    public static IEnumerable<string> FileUris = new[] {
        @"file1.txt",
        @"file2.txt",
        @"dir1\file1.1.txt",
        @"dir1\file1.2.txt",
        @"dir2\dir2.1\dir2.1.1\file2.1.1.1.xxx",
        @"dir2\dir2.1\dir2.1.1\file2.1.1.2.xxx",
        @"dir2\dir2.1\dir2.1.1\file2.1.1.3.xxx",
        @"dir2\dir2.1\file2.1.1.txt",
        @"dir2\dir2.1\file2.1.2.txt",
        @"dir2\dir2.2\file2.2.1.text",
        @"dir2\dir2.2\file2.2.2.text",
        @"dir3\file3.1.txt"
    };
}