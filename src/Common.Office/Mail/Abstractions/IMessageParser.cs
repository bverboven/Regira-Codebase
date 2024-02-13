using Regira.IO.Abstractions;

namespace Regira.Office.Mail.Abstractions;

public interface IMessageParser
{
    public IMessageObject Parse(IMemoryFile file);
}