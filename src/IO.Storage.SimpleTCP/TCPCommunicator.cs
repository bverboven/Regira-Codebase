using SimpleTCP;

namespace Regira.IO.Storage.SimpleTCP;

public class TCPCommunicator : IDisposable
{
    private readonly TCPConfig _config;
    private readonly SimpleTcpClient _client;
    private bool _isOpen;
    public TCPCommunicator(TCPConfig config)
    {
        _config = config;
        _client = new SimpleTcpClient();
    }

    protected internal SimpleTcpClient Open()
    {
        if (!_isOpen)
        {
            _client.Connect(_config.Host, _config.Port);
            _isOpen = true;
        }
        return _client;
    }
    protected internal SimpleTcpClient Close()
    {
        _isOpen = false;
        return _client.Disconnect();
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}