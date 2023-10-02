using Drawing.Testing.Abstractions;
using Regira.Drawing.SkiaSharp.Services;

namespace Drawing.Testing;

public class SkiaImageTests : ImageTestsBase
{
    public SkiaImageTests() : base(new ImageService())
    {
    }
}