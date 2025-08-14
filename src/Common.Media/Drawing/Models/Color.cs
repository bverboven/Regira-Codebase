namespace Regira.Media.Drawing.Models;

public class Color(byte red = byte.MaxValue, byte green = byte.MaxValue, byte blue = byte.MaxValue, byte alpha = ImageConstants.DEFAULT_ALPHA)
{
    public byte Red { get; } = red;
    public byte Green { get; } = green;
    public byte Blue { get; } = blue;
    public byte Alpha { get; } = alpha;

    /// <summary>
    /// Gets the hexadecimal color code representation of the RGB color (without alpha).
    /// </summary>
    public string Hex => $"#{Red:X2}{Green:X2}{Blue:X2}";

    // ReSharper disable RedundantArgumentDefaultValue
    public static Color Transparent => new(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MinValue);
    public static Color White => new(byte.MaxValue, byte.MaxValue, byte.MaxValue);
    public static Color Black => new(byte.MinValue, byte.MinValue, byte.MinValue);
    // ReSharper restore RedundantArgumentDefaultValue

    public override string ToString()
        => $"#{Red:X2}{Green:X2}{Blue:X2}{Alpha:X2}";
    public override bool Equals(object? obj)
    {
        if (!(obj is Color color))
        {
            return false;
        }
        return Red == color.Red && Green == color.Green && Blue == color.Blue && Alpha == color.Alpha;
    }

    public static implicit operator Color(byte[] array)
    {
        if (array == null || (array.Length != 3 && array.Length != 4))
        {
            throw new ArgumentException("Array must have 3 (RGB) or 4 (RGBA) elements.");
        }

        var r = array[0];
        var g = array[1];
        var b = array[2];
        var a = array.Length == 4 ? array[3] : ImageConstants.DEFAULT_ALPHA;

        return new Color(r, g, b, a);
    }
    public static implicit operator Color(string? hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
        {
            return new Color();
        }

        hex = hex.TrimStart('#');

        if (hex.Length == 3 || hex.Length == 4)
        {
            var expanded = "";
            foreach (var c in hex)
            {
                expanded += new string(c, 2);
            }
            hex = expanded;
        }

        if (hex.Length != 6 && hex.Length != 8)
        {
            throw new ArgumentException("Hex color must be RGB, RGBA, RRGGBB, or RRGGBBAA format.", nameof(hex));
        }

        var r = Convert.ToByte(hex.Substring(0, 2), 16);
        var g = Convert.ToByte(hex.Substring(2, 2), 16);
        var b = Convert.ToByte(hex.Substring(4, 2), 16);
        var a = hex.Length == 8 ? Convert.ToByte(hex.Substring(6, 2), 16) : ImageConstants.DEFAULT_ALPHA;

        return new Color(r, g, b, a);
    }
    public static implicit operator byte[](Color? rgba)
        => [rgba?.Red ?? byte.MaxValue, rgba?.Green ?? byte.MaxValue, rgba?.Blue ?? byte.MaxValue, rgba?.Alpha ?? ImageConstants.DEFAULT_ALPHA];
    public static implicit operator string(Color? rgba)
        => (rgba ?? new Color()).ToString();
}