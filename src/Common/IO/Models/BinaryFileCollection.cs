using Regira.IO.Abstractions;

namespace Regira.IO.Models;

public class BinaryFileCollection : List<IBinaryFile>, IDisposable
{
    public BinaryFileCollection(IEnumerable<IBinaryFile> items)
        : base(items)
    {
    }

    public void Dispose()
    {
        foreach (var fileItem in this)
        {
            fileItem.Dispose();
        }
    }
}