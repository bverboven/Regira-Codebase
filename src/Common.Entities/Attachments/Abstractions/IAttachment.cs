using Regira.Entities.Models.Abstractions;
using Regira.IO.Abstractions;

namespace Regira.Entities.Attachments.Abstractions;

public interface IAttachment : IAttachment<int>;
public interface IAttachment<TKey> : IEntity<TKey>, IBinaryFile, IHasTimestamps;