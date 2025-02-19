﻿using Regira.Entities.Attachments.Abstractions;
using Regira.Entities.Models;

namespace Regira.Entities.Attachments.Models;

public class AttachmentSearchObject : AttachmentSearchObject<int>;
public class AttachmentSearchObject<TKey> : SearchObject<TKey>, IAttachmentSearchObject
{
    public string? FileName { get; set; }
    public string? Extension { get; set; }
    public long? MinSize { get; set; }
    public long? MaxSize { get; set; }
}