using Regira.IO.Abstractions;

namespace Regira.DAL.Abstractions;

public interface IDbRestoreService
{
    Task Restore(IMemoryFile file);
}