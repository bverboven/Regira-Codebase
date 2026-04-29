using Regira.IO.Extensions;

namespace Regira.Office.Word.Models.DTO;

public static class DtoExtensions
{
    public static WordDocumentInputDto ToWordDocumentInputDto(this WordTemplateInput input) => new()
    {
        TemplateBytes = input.Template.GetBytes() ?? throw new ArgumentException("Template has no content.", nameof(input)),
        GlobalParameters = input.GlobalParameters,
        CollectionParameters = input.CollectionParameters,
        Images = input.Images?.Select(img => new WordImageModel
        {
            Name = img.Name,
            Bytes = img.File?.GetBytes() ?? []
        }).ToList(),
        Headers = input.Headers?.Select(h => new WordHeaderFooterModel { Template = ToWordDocumentInputDto(h.Template), Type = h.Type }).ToList(),
        Footers = input.Footers?.Select(f => new WordHeaderFooterModel { Template = ToWordDocumentInputDto(f.Template), Type = f.Type }).ToList()
    };
}