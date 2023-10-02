using Regira.IO.Utilities;
using Regira.Utilities;
using System.Text.RegularExpressions;

namespace Regira.Web.Utilities;

public static class UriUtility
{
    public static string ToAbsoluteUri(string relativeUri)
    {
        var baseUri = new UriBuilder().Uri;
        var uri = new Uri(baseUri, relativeUri);

        var urlBuilder = new UriBuilder
        {
            Path = uri.AbsolutePath,
            Query = uri.Query,
            Port = uri.Port
        };

        return urlBuilder.Uri.ToString();
    }
    public static string Slugify(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        // https://stackoverflow.com/questions/2920744/url-slugify-algorithm-in-c#answer-2921135
        var str = input.RemoveDiacritics()!;
        // invalid chars           
        str = Regex.Replace(str, @"[^a-zA-Z0-9\s-]", string.Empty);
        // convert multiple spaces into one space   
        str = Regex.Replace(str, @"\s+", " ").Trim();
        // cut and trim 
        //str = str.Substring(0, str.Length <= 45 ? str.Length : 45).Trim();
        // hyphens
        str = Regex.Replace(str, @"\s", "-");

        str = Regex.Replace(str, "-+", "-");
        return str.Trim('-');
    }

    public static string ToBase64ImageUrl(byte[] bytes, string contentType = "image/png")
    {
        return $"data:{contentType};base64,{FileUtility.GetBase64String(bytes)}";
    }
}