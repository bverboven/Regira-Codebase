using Microsoft.AspNetCore.Mvc;
using Regira.IO.Abstractions;
using Regira.IO.Extensions;
using Regira.IO.Utilities;
using System.Net.Mime;

namespace Regira.Web.Extensions;

public static class ControllerExtensions
{
    public static FileContentResult File(this ControllerBase ctrl, INamedFile file, bool inline = true)
    {
        var disposition = new ContentDisposition
        {
            FileName = file.FileName,
            Inline = inline
        };
        ctrl.Response.Headers.Add("Content-Disposition", disposition.ToString());
        // make content-disposition available for axios client
        ctrl.Response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");

        var bytes = file.GetBytes()!;
        var contentType = !string.IsNullOrWhiteSpace(file.ContentType)
            ? file.ContentType
            : ContentTypeUtility.GetContentType(file.FileName) ?? ContentTypeUtility.GetContentType(bytes);
        return ctrl.File(bytes!, contentType, inline ? null : file.FileName);
    }
}