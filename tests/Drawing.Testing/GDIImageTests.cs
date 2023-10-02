using Drawing.Testing.Abstractions;
using Regira.Drawing.GDI.Services;


namespace Drawing.Testing;

public class GDIImageTests : ImageTestsBase
{
    public GDIImageTests() : base(new ImageService())
    {
    }
}