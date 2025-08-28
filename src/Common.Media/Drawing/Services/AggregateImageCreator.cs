using Regira.Media.Drawing.Models;
using Regira.Media.Drawing.Models.Abstractions;
using Regira.Media.Drawing.Services.Abstractions;

namespace Regira.Media.Drawing.Services;

public class AggregateImageCreator(IImageService service, IEnumerable<IImageCreator> imageCreators) : ImageCreatorBase<AggregateImageOptions>
{
    public override IImageFile Create(AggregateImageOptions input)
    {
        var builder = new ImageBuilder(service, imageCreators);
        
        if (input.Target != null)
        {
            builder.SetTargetObject(input.Target);
        }

        builder.Add(input.ImagesToAdd.ToArray());
        return builder.Build();
    }
}