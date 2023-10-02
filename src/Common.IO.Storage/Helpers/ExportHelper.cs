using Regira.IO.Storage.Abstractions;

namespace Regira.IO.Storage.Helpers;

public interface IExportHelper
{
    Task Export(FileSearchObject so);
}

public class ExportHelper : IExportHelper
{
    private readonly IFileService _sourceService;
    private readonly IFileService _targetService;
    public ExportHelper(IFileService sourceService, IFileService targetService)
    {
        _sourceService = sourceService;
        _targetService = targetService;
    }


    public async Task Export(FileSearchObject so)
    {
        var fileUris = await _sourceService.List(so);
        foreach (var fileUri in fileUris)
        {
#if NETSTANDARD2_0
                using var stream = await _sourceService.GetStream(fileUri);
#else
            await using var stream = await _sourceService.GetStream(fileUri);
#endif
            if (stream != null)
            {
                await _targetService.Save(fileUri, stream);
            }
        }
    }
}