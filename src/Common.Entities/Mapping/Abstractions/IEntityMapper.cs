using Regira.Entities.Attachments.Abstractions;
using Regira.Entities.Attachments.Models;
using Regira.Entities.Mapping.Models;
using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.Mapping.Abstractions;

public interface IEntityMapper
{
    TTarget Map<TTarget>(object source);

    TTarget Map<TSource, TTarget>(TSource source);

    TTarget Map<TSource, TTarget>(TSource source, TTarget target);
}

public interface IEntityMapConfigurator
{
    void Configure<TSource, TTarget>();
    void ConfigureAttachment<TEntityAttachment, TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment, TEntityAttachmentDto>()
        where TEntityAttachment : class, IEntityAttachment<TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment>
        where TAttachment : class, IAttachment<TAttachmentKey>, new()
        where TEntityAttachmentDto : EntityAttachmentDto;
}
