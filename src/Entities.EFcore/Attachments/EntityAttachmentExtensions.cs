using Microsoft.EntityFrameworkCore;
using Regira.Entities.Attachments.Abstractions;

namespace Regira.Entities.EFcore.Attachments
{
    public static class EntityAttachmentExtensions
    {
        public static void ModifyEntityAttachments<TEntityAttachment>(this DbContext dbContext, IHasAttachments<TEntityAttachment> original, IHasAttachments<TEntityAttachment> item)
            where TEntityAttachment : class, IEntityAttachment
        {
            if (item.Attachments == null || original.Attachments == null)
            {
                return;
            }

            var originalAttachments = original.Attachments.ToArray();
            foreach (var originalItem in originalAttachments)
            {
                var modifiedItem = item.Attachments.FirstOrDefault(p => p.Id == originalItem.Id);
                if (modifiedItem == null)
                {
                    dbContext.Entry(originalItem).State = EntityState.Deleted;
                }
                else
                {
                    modifiedItem.Attachment = originalItem.Attachment;
                    if (!string.IsNullOrWhiteSpace(modifiedItem.NewFileName))
                    {
                        originalItem.Attachment!.FileName = modifiedItem.NewFileName;
                    }

                    if (!string.IsNullOrWhiteSpace(modifiedItem.NewContentType))
                    {
                        originalItem.Attachment!.ContentType = modifiedItem.NewContentType;
                    }

                    dbContext.Entry(originalItem).State = EntityState.Detached;
                    dbContext.Entry(modifiedItem).OriginalValues.SetValues(originalItem);
                    dbContext.Update(modifiedItem);
                }
            }

            original.Attachments = item.Attachments;
        }
    }
}
