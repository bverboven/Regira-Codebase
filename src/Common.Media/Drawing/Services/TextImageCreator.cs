using Regira.Media.Drawing.Models;
using Regira.Media.Drawing.Models.Abstractions;
using Regira.Media.Drawing.Services.Abstractions;

namespace Regira.Media.Drawing.Services;

public class TextImageCreator(IImageService service) : ImageCreatorBase<TextImageOptions>
{
    public override IImageFile Create(TextImageOptions input)
        => service.CreateTextImage(input);
}