using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.Attachments.Abstractions;
using Regira.Entities.Attachments.Models;
using Regira.Entities.Web.Attachments.Models;
using Regira.Entities.Web.Mapping.Abstractions;

namespace Regira.Entities.Mapping.AutoMapper;

public class MappedEntityServiceCollectionOptions<TEntity, TEntityDto, TEntityInputDto>(IMappedEntityServiceCollection mappedServices) : IMappedEntityServiceCollectionOptions<TEntity, TEntityDto, TEntityInputDto>
{
    public IMappedEntityServiceCollection MappedServices => mappedServices;

    public IMappedEntityServiceCollectionOptions<TEntity, TEntityDto, TEntityInputDto> HasAttachments<TEntityAttachment, TEntityAttachmentDto, TEntityAttachmentInputDto>()
        where TEntityAttachment : IEntityAttachment<int, int, int, Attachment>
        where TEntityAttachmentDto : EntityAttachmentDto<int, int, int>
    {
        MappedServices.Services
            .AddTransient<AttachmentUriResolver<TEntityAttachment, TEntityAttachmentDto>>()
            .AddAutoMapper(e =>
            {
                e.CreateMap<TEntityAttachment, TEntityAttachmentDto>()
                    .ForMember(
                        x => x.Uri,
                        opt => opt.MapFrom<AttachmentUriResolver<TEntityAttachment, TEntityAttachmentDto>>()
                    );
                e.CreateMap<TEntityAttachmentInputDto, TEntityAttachment>();
            });
        return this;
    }
}