using Regira.IO.Storage.Abstractions;
using Regira.IO.Utilities;

namespace Regira.IO.Storage.SimpleTCP;

public class TCPService : ITextFileService
{
    public string RootFolder => throw new NotSupportedException();

    private readonly TCPCommunicator _communicator;
    public TCPService(TCPCommunicator communicator)
    {
        _communicator = communicator;
    }


    public Task<string> Save(string identifier, byte[] bytes, string? contentType = null)
    {
        _communicator.Open().Write(bytes);
        return Task.FromResult(string.Empty);
    }
    public Task<string> Save(string identifier, Stream stream, string? contentType = null)
    {
        var bytes = FileUtility.GetBytes(stream)!;
        return Save(identifier, bytes, contentType);
    }
    public Task<string> Save(string identifier, string contents, string? contentType = null)
    {
        var client = _communicator.Open();
        var msg = client.WriteAndGetReply(contents);
        return Task.FromResult(msg.MessageString);
    }


    #region NotSupported
    public Task<bool> Exists(string identifier) => throw new NotSupportedException();
    public string GetAbsoluteUri(string identifier) => throw new NotSupportedException();
    public string GetIdentifier(string uri) => throw new NotImplementedException();
    public string GetRelativeFolder(string identifier) => throw new NotImplementedException();

    public Task<byte[]?> GetBytes(string identifier) => throw new NotSupportedException();
    public Task<Stream?> GetStream(string identifier) => throw new NotSupportedException();
    public Task<IEnumerable<string>> List(FileSearchObject? so = null) => throw new NotSupportedException();
    public Task Move(string sourceIdentifier, string targetIdentifier) => throw new NotSupportedException();
    public Task Delete(string identifier) => throw new NotSupportedException();
    public Task<string?> GetContents(string identifier) => throw new NotSupportedException();
    #endregion
}