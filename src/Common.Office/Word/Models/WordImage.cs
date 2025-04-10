using Regira.Dimensions;

namespace Regira.Office.Word.Models;

public class WordImage
{
    public string Name { get; set; } = null!;
    public Size2D? Size { get; set; }
    public byte[]? Bytes { get; set; }
    public HorizontalAlignment? HorizontalAlignment { get; set; }
}