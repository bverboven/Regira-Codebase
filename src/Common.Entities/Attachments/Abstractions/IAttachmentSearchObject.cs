namespace Regira.Entities.Attachments.Abstractions;

public interface IAttachmentSearchObject
{
    string? FileName { get; set; }
    string? Extension { get; set; }
    long? MinSize { get; set; }
    long? MaxSize { get; set; }
}