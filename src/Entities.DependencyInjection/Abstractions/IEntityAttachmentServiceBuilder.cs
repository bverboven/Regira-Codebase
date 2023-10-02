using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.Attachments.Abstractions;
using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.DependencyInjection.Abstractions;

public interface IEntityAttachmentServiceBuilder<TEntity, TEntityAttachment> : IEntityAttachmentServiceBuilder<TEntity, int, TEntityAttachment, int, int>
    where TEntity : class, IEntity<int>, IHasAttachments, IHasAttachments<TEntityAttachment>
    where TEntityAttachment : class, IEntityAttachment
{
}
// ReSharper disable once UnusedTypeParameter
public interface IEntityAttachmentServiceBuilder<TEntity, TKey, TEntityAttachment, TEntityAttachmentKey, TAttachmentKey>
    where TEntity : class, IEntity<TKey>, IHasAttachments, IHasAttachments<TEntityAttachment, TEntityAttachmentKey, TKey, TAttachmentKey>
    where TEntityAttachment : class, IEntityAttachment<TEntityAttachmentKey, TKey, TAttachmentKey>
{
    IServiceCollection Services { get; }
}