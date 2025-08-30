using Regira.Media.Drawing.Models.Abstractions;
using Regira.Media.Drawing.Services.Abstractions;

namespace Regira.Media.Drawing.Utilities;

public static class DrawImageUtility
{
    public static IImageFile? Create<T>(this IEnumerable<IImageCreator> services, T input)
        where T : class
        => services.FirstOrDefault(s => s.CanCreate(input))?.Create(input);
    
    public static IImageFile? GetImageFile(this IEnumerable<IImageCreator> services, IImageToAdd item)
        => item.Source as IImageFile ?? services.Create(item.Source);
}