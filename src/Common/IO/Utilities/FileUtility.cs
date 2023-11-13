using Regira.Utilities;
using System.Text;

namespace Regira.IO.Utilities;

public static class FileUtility
{
    //https://stackoverflow.com/questions/3825390/effective-way-to-find-any-files-encoding#19283954
    /// <summary>
    /// Reads the first 4 bytes (bom) to analyze for the Encoding<br />
    /// Possible encodings:<br />
    /// <list type="bullet">
    ///     <item>UTF7</item>
    ///     <item>UTF8</item>
    ///     <item>Unicode</item>
    ///     <item>BigEndianUnicode</item>
    ///     <item>UTF32</item>
    ///     <item>ASCII</item>
    /// </list>
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns>Encoding (ASCII when not match is found)</returns>
    public static Encoding GetEncoding(this byte[] bytes)
    {
        if (bytes.Length >= 2)
        {
            // Read the BOM
            var bom = bytes.Take(4).ToArray();

            if (bytes.Length >= 3)
            {
                // Analyze the BOM
#pragma warning disable SYSLIB0001
                if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) return Encoding.UTF7;
#pragma warning restore SYSLIB0001
                if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8;
            }

            if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode; //UTF-16LE
            if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode; //UTF-16BE


            if (bytes.Length >= 4)
            {
                if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) return Encoding.UTF32;
            }
        }

        return Encoding.Default;
    }

    public static byte[]? GetBytes(this Stream? stream)
    {
        if (stream == null)
        {
            return null;
        }

        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        {
            if (stream is MemoryStream ms)
            {
                return ms.ToArray();
            }
        }

        using (var ms = new MemoryStream())
        {
            stream.CopyTo(ms);
            return ms.ToArray();
        }
    }
    public static byte[] GetBytes(string base64String)
    {
        var cleanBase64String = CleanBase64String(base64String);
        return Convert.FromBase64String(cleanBase64String);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="contents"></param>
    /// <param name="encoding">Using Encoding.Default when null</param>
    /// <returns></returns>
    public static byte[] GetBytesFromString(this string contents, Encoding? encoding = null)
    {
        encoding ??= Encoding.Default;
        return encoding.GetBytes(contents);
    }

    public static Stream? GetStream(this byte[]? bytes)
    {
        if (bytes == null)
        {
            return null;
        }

        return new MemoryStream(bytes);
    }
    public static Stream? GetStream(string base64String) 
        => GetStream(GetBytes(base64String));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="contents"></param>
    /// <param name="encoding">Using Encoding.Default when null</param>
    /// <returns></returns>
    public static Stream GetStreamFromString(this string contents, Encoding? encoding = null)
    {
        encoding ??= Encoding.Default;
        return new MemoryStream(encoding.GetBytes(contents));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="encoding">When null, try to detect encoding</param>
    /// <returns></returns>
    public static string? GetString(this Stream? stream, Encoding? encoding = null)
    {
        if (stream == null)
        {
            return null;
        }

        using var reader = encoding == null
            ? new StreamReader(stream, true)
            : new StreamReader(stream, encoding);
        return reader.ReadToEnd();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="encoding">Will try to retrieve from BOM, using ASCII as last resort</param>
    /// <returns></returns>
    public static string? GetString(this byte[]? bytes, Encoding? encoding = null)
    {
        if (bytes == null)
        {
            return null;
        }

        encoding ??= GetEncoding(bytes);
        return encoding.GetString(bytes);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="clean">Removes content-type, spaces and newlines when true</param>
    /// <returns></returns>
    public static string GetBase64String(this byte[] bytes, bool clean = true)
    {
        var base64 = Convert.ToBase64String(bytes);
        return clean ? CleanBase64String(base64) : base64;
    }
    public static string CleanBase64String(string base64String)
    {
        var sb = new StringBuilder(base64String, base64String.Length)
            .Replace(Environment.NewLine, string.Empty)
            .Replace(" ", string.Empty);
        var cleanedBase64 = sb.ToString();
        var firstChars = StringUtility.Truncate(cleanedBase64, 64);
        if (firstChars?.Contains(',') == true)
        {
            return cleanedBase64.Substring(cleanedBase64.IndexOf(',') + 1);
        }

        return cleanedBase64;
    }
}