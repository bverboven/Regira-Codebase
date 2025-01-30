using Regira.IO.Storage.Abstractions;

namespace Regira.IO.Storage.Helpers;

public interface IExportHelper
{
    Task Export(FileSearchObject so);
}

public class ExportHelper(IFileService sourceService, IFileService targetService) : IExportHelper
{
    public async Task Export(FileSearchObject so)
    {
        var fileUris = await sourceService.List(so);
        foreach (var fileUri in fileUris)
        {
#if NETSTANDARD2_0
                using var stream = await sourceService.GetStream(fileUri);
#else
            await using var stream = await sourceService.GetStream(fileUri);
#endif
            if (stream != null)
            {
                await targetService.Save(fileUri, stream);
            }
        }
    }
}