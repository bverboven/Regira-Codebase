using Regira.IO.Abstractions;

namespace Regira.DAL.Abstractions;

public interface IDbBackupService
{
    Task<IMemoryFile> Backup();
}