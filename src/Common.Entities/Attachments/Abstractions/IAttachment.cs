using Regira.Entities.Models.Abstractions;
using Regira.IO.Abstractions;

namespace Regira.Entities.Attachments.Abstractions;

public interface IAttachment;
public interface IAttachment<TKey> : IEntity<TKey>, IAttachment, IBinaryFile, IHasTimestamps;