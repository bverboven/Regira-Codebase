using System.Net;
using Microsoft.AspNetCore.Http;

namespace Regira.Web.Utilities;

public static class RequestUtility
{
    public static string CurrentUrl(this HttpRequest request)
    {
        return $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";
    }
    public static Uri GetBaseUrl(this HttpRequest request)
    {
        return request.GetAbsoluteUrl();
    }
    public static Uri GetAbsoluteUrl(this HttpRequest request, string? relativeUrl = null)
    {
        var baseUri = new Uri($"{request.Scheme}://{request.Host.Value}");
        return string.IsNullOrEmpty(relativeUrl)
            ? new Uri(baseUri.ToString())
            : new Uri(baseUri, relativeUrl);
    }

    public static Uri GetReferrer(this HttpRequest request)
    {
        return request.GetTypedHeaders().Referer;
    }

    public static IPAddress GetIPAddress(this HttpRequest request)
    {
        return request.HttpContext.Connection.RemoteIpAddress;
    }
}