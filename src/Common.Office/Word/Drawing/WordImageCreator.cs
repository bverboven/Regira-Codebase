using Regira.Media.Drawing.Models.Abstractions;
using Regira.Media.Drawing.Services.Abstractions;
using Regira.Office.Word.Abstractions;

namespace Regira.Office.Word.Drawing;

public class WordImageCreator(IWordToImagesService service) : ImageCreatorBase<WordToImageLayerOptions>
{
    public override IImageFile? Create(WordToImageLayerOptions input)
    {
        return service.ToImages(input.ToWordTemplateInput())
            .Skip((input.Page ?? 1) - 1)
            .FirstOrDefault();
    }
}