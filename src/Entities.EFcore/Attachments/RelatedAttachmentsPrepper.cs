﻿using Microsoft.EntityFrameworkCore;
using Regira.Entities.Attachments.Abstractions;
using Regira.Entities.EFcore.Preppers.Abstractions;
using Regira.Entities.Models.Abstractions;
using Regira.IO.Extensions;
using System.Linq.Expressions;

namespace Regira.Entities.EFcore.Attachments;

public class RelatedAttachmentsPrepper<TContext, TEntity, TEntityAttachment, TEntityKey, TEntityAttachmentKey, TAttachmentKey, TAttachment>(
        TContext dbContext,
        Expression<Func<TEntity, ICollection<TEntityAttachment>?>> navigationExpression,
        RelatedAttachmentsPrepper<TContext, TEntity, TEntityAttachment, TEntityKey, TEntityAttachmentKey, TAttachmentKey, TAttachment>.Options? options = null
    ) : EntityPrepperBase<TEntity>
    where TContext : DbContext
    where TEntity : class, IEntity<TEntityKey>
    where TAttachment : class, IAttachment<TAttachmentKey>, new()
    where TEntityAttachment : class, IEntityAttachment<TEntityAttachmentKey, TEntityKey, TAttachmentKey, TAttachment>
{
    public class Options
    {
        public bool IsStrictRelation { get; set; } = true;
    }
    private readonly Options _options = options ?? new Options();

    public override Task Prepare(TEntity modified, TEntity? original)
    {
        if (original != null)
        {
            var selectorFunc = navigationExpression.Compile();
            var originalItems = selectorFunc(original);
            var modifiedItems = selectorFunc(modified);

            if (modifiedItems == null || originalItems == null)
            {
                return Task.CompletedTask;
            }

            var relatedItemsToAdd = modifiedItems.Where(m => m.Id == null || m.Id.Equals(default(TEntityAttachmentKey)) || originalItems.All(o => m.Id.Equals(o.Id) != true)).ToArray();
            var relatedItemsToDelete = originalItems.Where(o => modifiedItems.All(m => m.Id != null && m.Id.Equals(o.Id) != true)).ToArray();
            foreach (var entity in relatedItemsToAdd)
            {
                // Only add when attachment has content
                if (entity.Attachment?.HasContent() == true)
                {
                    dbContext.Entry(entity.Attachment).State = EntityState.Added;
                    dbContext.Entry(entity).State = EntityState.Added;
                }
            }
            foreach (var entity in relatedItemsToDelete)
            {
                dbContext.Entry(entity).State = EntityState.Deleted;
                // also delete Attachment entity in DB
                if (_options.IsStrictRelation)
                {
                    dbContext.Entry(entity.Attachment ?? new TAttachment { Id = entity.AttachmentId }).State = EntityState.Deleted;
                }
            }
            var relatedItemsToModify = modifiedItems.Except(relatedItemsToAdd).Where(m => m.Id != null && !m.Id.Equals(default(TEntityAttachmentKey)));
            foreach (var entity in relatedItemsToModify)
            {
                var originalEntity = originalItems.Single(p => p.Id!.Equals(entity.Id));
                dbContext.Entry(originalEntity).State = EntityState.Detached;
                dbContext.Attach(entity);
                dbContext.Entry(entity).OriginalValues.SetValues(originalEntity);
                dbContext.Update(entity);
            }
        }

        return Task.CompletedTask;
    }
}