using System.Drawing;
using System.Globalization;

namespace Regira.Utilities;

public static class ColorUtility
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="hex"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static Color FromHex(string hex)
    {
        // https://stackoverflow.com/questions/48173375/get-color-from-hex-in-asp-net-core#48182388
        FromHex(hex, out var a, out var r, out var g, out var b);

        return Color.FromArgb(a, r, g, b);
    }

    private static void FromHex(string hex, out byte a, out byte r, out byte g, out byte b)
    {
        var rgbaHex = ToRgbaHex(hex);
        if (rgbaHex == null || !uint.TryParse(rgbaHex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var packedValue))
        {
            throw new ArgumentException("Hexadecimal string is not in the correct format.", nameof(hex));
        }

        a = (byte)(packedValue >> 0);
        r = (byte)(packedValue >> 24);
        g = (byte)(packedValue >> 16);
        b = (byte)(packedValue >> 8);
    }
    private static string? ToRgbaHex(string hex)
    {
        hex = hex.StartsWith("#") ? hex.Substring(1) : hex;

        if (hex.Length == 8)
        {
            return hex;
        }

        if (hex.Length == 6)
        {
            return hex + "FF";
        }

        if (hex.Length < 3 || hex.Length > 4)
        {
            return null;
        }

        //Handle values like #3B2
        var red = char.ToString(hex[0]);
        var green = char.ToString(hex[1]);
        var blue = char.ToString(hex[2]);
        var alpha = hex.Length == 3 ? "F" : char.ToString(hex[3]);


        return string.Concat(red, red, green, green, blue, blue, alpha, alpha);
    }
}