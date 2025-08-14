
using Regira.Drawing.GDI.Services;
using Regira.Media.Drawing.Services.Abstractions;

namespace Drawing.Testing;

#pragma warning disable CA1416
[TestFixture]
public class GDIDrawTextTests
{
    readonly IImageService _imageService = new ImageService();

    [Test]
    public Task Add_Text_No_Params() => _imageService.Add_Text_No_Params();

    [Test]
    public Task Add_Text_With_Options() => _imageService.Add_Text_With_Options();

    [Test]
    public Task Add_Text_With_Margin() => _imageService.Add_Text_With_Margin();
}
#pragma warning restore CA1416