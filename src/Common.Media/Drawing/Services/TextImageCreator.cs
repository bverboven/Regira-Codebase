using Regira.Media.Drawing.Models;
using Regira.Media.Drawing.Models.Abstractions;
using Regira.Media.Drawing.Services.Abstractions;

namespace Regira.Media.Drawing.Services;

public class TextImageCreator(IImageService service) : ImageCreatorBase<LabelImageOptions>
{
    public override IImageFile Create(LabelImageOptions input)
        => service.CreateTextImage(input);
}