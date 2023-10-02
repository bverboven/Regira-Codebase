using System.Collections.Concurrent;

namespace Regira.IO.Utilities;

public static class ContentTypeUtility
{
    public static IDictionary<string, byte[]> MimeTypeByteSequences => new ConcurrentDictionary<string, byte[]>(new Dictionary<string, byte[]>
    {
        // https://stackoverflow.com/questions/58510/using-net-how-can-you-find-the-mime-type-of-a-file-based-on-the-file-signature#answer-13614746
        { "avi", new byte[]{ 82, 73, 70, 70 } },
        { "bmp", new byte[]{ 66, 77 } },
        { "dll", new byte[]{ 77, 90 } },
        { "doc", new byte[]{ 208, 207, 17, 224, 161, 177, 26, 225 } },
        { "docx", new byte[]{ 80, 75, 3, 4 } },
        { "exe", new byte[]{ 77, 90 } },
        { "gif", new byte[]{ 71, 73, 70, 56 } },
        { "ico", new byte[]{ 0, 0, 1, 0 } },
        { "jpeg", new byte[]{ 255, 216, 255 } },
        { "jpg", new byte[]{ 255, 216, 255 } },
        { "mp3", new byte[]{ 255, 251, 48 } },
        { "ogg", new byte[]{ 79, 103, 103, 83, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0 } },
        { "pdf", new byte[]{ 37, 80, 68, 70, 45, 49, 46 } },
        { "png", new byte[]{ 137, 80, 78, 71, 13, 10, 26, 10, 0, 0, 0, 13, 73, 72, 68, 82 } },
        { "rar", new byte[]{ 82, 97, 114, 33, 26, 7, 0 } },
        { "swf", new byte[]{ 70, 87, 83 } },
        { "tiff", new byte[]{ 73, 73, 42, 0 } },
        { "ttf", new byte[]{ 0, 1, 0, 0, 0 } },
        { "wav", new byte[]{ 82, 73, 70, 70 } },
        { "wma", new byte[]{ 48, 38, 178, 117, 142, 102, 207, 17, 166, 217, 0, 170, 0, 98, 206, 108 } },
        { "wmv", new byte[]{ 48, 38, 178, 117, 142, 102, 207, 17, 166, 217, 0, 170, 0, 98, 206, 108 } },
        { "zip", new byte[]{ 80, 75, 3, 4 } }
    });
    public static IDictionary<string, string[]> MimeTypesDictionary => new ConcurrentDictionary<string, string[]>(new Dictionary<string, string[]>
    {
        { "ai", new[] { "application/postscript" } },
        { "aif", new[] { "audio/x-aiff" } },
        { "aifc", new[] { "audio/x-aiff" } },
        { "aiff", new[] { "audio/x-aiff" } },
        { "asc", new[] { "text/plain" } },
        { "atom", new[] { "application/atom+xml" } },
        { "au", new[] { "audio/basic" } },
        { "avi", new[] { "video/x-msvideo" } },
        { "bcpio", new[] { "application/x-bcpio" } },
        { "bin", new[] { "application/octet-stream" } },
        { "bmp", new[] { "image/bmp" } },
        { "cdf", new[] { "application/x-netcdf" } },
        { "cgm", new[] { "image/cgm" } },
        { "class", new[] { "application/octet-stream" } },
        { "cpio", new[] { "application/x-cpio" } },
        { "cpt", new[] { "application/mac-compactpro" } },
        { "csh", new[] { "application/x-csh" } },
        { "css", new[] { "text/css" } },
        { "dcr", new[] { "application/x-director" } },
        { "dif", new[] { "video/x-dv" } },
        { "dir", new[] { "application/x-director" } },
        { "djv", new[] { "image/vnd.djvu" } },
        { "djvu", new[] { "image/vnd.djvu" } },
        { "dll", new[] { "application/octet-stream" } },
        { "dmg", new[] { "application/octet-stream" } },
        { "dms", new[] { "application/octet-stream" } },
        { "doc", new[] { "application/msword" } },
        { "docx", new[] { "application/vnd.openxmlformats-officedocument.wordprocessingml.document" } },
        { "dotx", new[] { "application/vnd.openxmlformats-officedocument.wordprocessingml.template" } },
        { "docm", new[] { "application/vnd.ms-word.document.macroEnabled.12" } },
        { "dotm", new[] { "application/vnd.ms-word.template.macroEnabled.12" } },
        { "dtd", new[] { "application/xml-dtd" } },
        { "dv", new[] { "video/x-dv" } },
        { "dvi", new[] { "application/x-dvi" } },
        { "dxr", new[] { "application/x-director" } },
        { "eps", new[] { "application/postscript" } },
        { "etx", new[] { "text/x-setext" } },
        { "exe", new[] { "application/octet-stream" } },
        { "ez", new[] { "application/andrew-inset" } },
        { "gif", new[] { "image/gif" } },
        { "gram", new[] { "application/srgs" } },
        { "grxml", new[] { "application/srgs+xml" } },
        { "gtar", new[] { "application/x-gtar" } },
        { "hdf", new[] { "application/x-hdf" } },
        { "hqx", new[] { "application/mac-binhex40" } },
        { "htm", new[] { "text/html" } },
        { "html", new[] { "text/html" } },
        { "ice", new[] { "x-conference/x-cooltalk" } },
        { "ico", new[] { "image/x-icon" } },
        { "ics", new[] { "text/calendar" } },
        { "ief", new[] { "image/ief" } },
        { "ifb", new[] { "text/calendar" } },
        { "iges", new[] { "model/iges" } },
        { "igs", new[] { "model/iges" } },
        { "jnlp", new[] { "application/x-java-jnlp-file" } },
        { "jp2", new[] { "image/jp2" } },
        { "jpe", new[] { "image/jpeg" } },
        { "jpeg", new[] { "image/jpeg" } },
        { "jpg", new[] { "image/jpeg" } },
        { "js", new[] { "application/x-javascript" } },
        { "kar", new[] { "audio/midi" } },
        { "latex", new[] { "application/x-latex" } },
        { "lha", new[] { "application/octet-stream" } },
        { "lzh", new[] { "application/octet-stream" } },
        { "m3u", new[] { "audio/x-mpegurl" } },
        { "m4a", new[] { "audio/mp4a-latm" } },
        { "m4b", new[] { "audio/mp4a-latm" } },
        { "m4p", new[] { "audio/mp4a-latm" } },
        { "m4u", new[] { "video/vnd.mpegurl" } },
        { "m4v", new[] { "video/x-m4v" } },
        { "mac", new[] { "image/x-macpaint" } },
        { "man", new[] { "application/x-troff-man" } },
        { "mathml", new[] { "application/mathml+xml" } },
        { "me", new[] { "application/x-troff-me" } },
        { "mesh", new[] { "model/mesh" } },
        { "mid", new[] { "audio/midi" } },
        { "midi", new[] { "audio/midi" } },
        { "mif", new[] { "application/vnd.mif" } },
        { "mov", new[] { "video/quicktime" } },
        { "movie", new[] { "video/x-sgi-movie" } },
        { "mp2", new[] { "audio/mpeg" } },
        { "mp3", new[] { "audio/mpeg" } },
        { "mp4", new[] { "video/mp4" } },
        { "mpe", new[] { "video/mpeg" } },
        { "mpeg", new[] { "video/mpeg" } },
        { "mpg", new[] { "video/mpeg" } },
        { "mpga", new[] { "audio/mpeg" } },
        { "ms", new[] { "application/x-troff-ms" } },
        { "msh", new[] { "model/mesh" } },
        { "mxu", new[] { "video/vnd.mpegurl" } },
        { "nc", new[] { "application/x-netcdf" } },
        { "oda", new[] { "application/oda" } },
        { "ogg", new[] { "application/ogg" } },
        { "pbm", new[] { "image/x-portable-bitmap" } },
        { "pct", new[] { "image/pict" } },
        { "pdb", new[] { "chemical/x-pdb" } },
        { "pdf", new[] { "application/pdf" } },
        { "pgm", new[] { "image/x-portable-graymap" } },
        { "pgn", new[] { "application/x-chess-pgn" } },
        { "pic", new[] { "image/pict" } },
        { "pict", new[] { "image/pict" } },
        { "png", new[] { "image/png", "image/x-png" } },
        { "pnm", new[] { "image/x-portable-anymap" } },
        { "pnt", new[] { "image/x-macpaint" } },
        { "pntg", new[] { "image/x-macpaint" } },
        { "ppm", new[] { "image/x-portable-pixmap" } },
        { "ppt", new[] { "application/vnd.ms-powerpoint" } },
        { "pptx", new[] { "application/vnd.openxmlformats-officedocument.presentationml.presentation" } },
        { "potx", new[] { "application/vnd.openxmlformats-officedocument.presentationml.template" } },
        { "ppsx", new[] { "application/vnd.openxmlformats-officedocument.presentationml.slideshow" } },
        { "ppam", new[] { "application/vnd.ms-powerpoint.addin.macroEnabled.12" } },
        { "pptm", new[] { "application/vnd.ms-powerpoint.presentation.macroEnabled.12" } },
        { "potm", new[] { "application/vnd.ms-powerpoint.template.macroEnabled.12" } },
        { "ppsm", new[] { "application/vnd.ms-powerpoint.slideshow.macroEnabled.12" } },
        { "ps", new[] { "application/postscript" } },
        { "qt", new[] { "video/quicktime" } },
        { "qti", new[] { "image/x-quicktime" } },
        { "qtif", new[] { "image/x-quicktime" } },
        { "ra", new[] { "audio/x-pn-realaudio" } },
        { "ram", new[] { "audio/x-pn-realaudio" } },
        { "ras", new[] { "image/x-cmu-raster" } },
        { "rdf", new[] { "application/rdf+xml" } },
        { "rgb", new[] { "image/x-rgb" } },
        { "rm", new[] { "application/vnd.rn-realmedia" } },
        { "roff", new[] { "application/x-troff" } },
        { "rtf", new[] { "text/rtf" } },
        { "rtx", new[] { "text/richtext" } },
        { "sgm", new[] { "text/sgml" } },
        { "sgml", new[] { "text/sgml" } },
        { "sh", new[] { "application/x-sh" } },
        { "shar", new[] { "application/x-shar" } },
        { "silo", new[] { "model/mesh" } },
        { "sit", new[] { "application/x-stuffit" } },
        { "skd", new[] { "application/x-koan" } },
        { "skm", new[] { "application/x-koan" } },
        { "skp", new[] { "application/x-koan" } },
        { "skt", new[] { "application/x-koan" } },
        { "smi", new[] { "application/smil" } },
        { "smil", new[] { "application/smil" } },
        { "snd", new[] { "audio/basic" } },
        { "so", new[] { "application/octet-stream" } },
        { "spl", new[] { "application/x-futuresplash" } },
        { "src", new[] { "application/x-wais-source" } },
        { "sv4cpio", new[] { "application/x-sv4cpio" } },
        { "sv4crc", new[] { "application/x-sv4crc" } },
        { "svg", new[] { "image/svg+xml" } },
        { "swf", new[] { "application/x-shockwave-flash" } },
        { "t", new[] { "application/x-troff" } },
        { "tar", new[] { "application/x-tar" } },
        { "tcl", new[] { "application/x-tcl" } },
        { "tex", new[] { "application/x-tex" } },
        { "texi", new[] { "application/x-texinfo" } },
        { "texinfo", new[] { "application/x-texinfo" } },
        { "tif", new[] { "image/tiff" } },
        { "tiff", new[] { "image/tiff" } },
        { "tr", new[] { "application/x-troff" } },
        { "tsv", new[] { "text/tab-separated-values" } },
        { "txt", new[] { "text/plain" } },
        { "ustar", new[] { "application/x-ustar" } },
        { "vcd", new[] { "application/x-cdlink" } },
        { "vcf", new[] { "text/vcard" } },
        { "vrml", new[] { "model/vrml" } },
        { "vxml", new[] { "application/voicexml+xml" } },
        { "wav", new[] { "audio/x-wav" } },
        { "wbmp", new[] { "image/vnd.wap.wbmp" } },
        { "wbmxl", new[] { "application/vnd.wap.wbxml" } },
        { "wml", new[] { "text/vnd.wap.wml" } },
        { "wmlc", new[] { "application/vnd.wap.wmlc" } },
        { "wmls", new[] { "text/vnd.wap.wmlscript" } },
        { "wmlsc", new[] { "application/vnd.wap.wmlscriptc" } },
        { "wrl", new[] { "model/vrml" } },
        { "xbm", new[] { "image/x-xbitmap" } },
        { "xht", new[] { "application/xhtml+xml" } },
        { "xhtml", new[] { "application/xhtml+xml" } },
        { "xls", new[] { "application/vnd.ms-excel" } },
        { "xml", new[] { "application/xml" } },
        { "xpm", new[] { "image/x-xpixmap" } },
        { "xsl", new[] { "application/xml" } },
        { "xlsx", new[] { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" } },
        { "xltx", new[] { "application/vnd.openxmlformats-officedocument.spreadsheetml.template" } },
        { "xlsm", new[] { "application/vnd.ms-excel.sheet.macroEnabled.12" } },
        { "xltm", new[] { "application/vnd.ms-excel.template.macroEnabled.12" } },
        { "xlam", new[] { "application/vnd.ms-excel.addin.macroEnabled.12" } },
        { "xlsb", new[] { "application/vnd.ms-excel.sheet.binary.macroEnabled.12" } },
        { "xslt", new[] { "application/xslt+xml" } },
        { "xul", new[] { "application/vnd.mozilla.xul+xml" } },
        { "xwd", new[] { "image/x-xwindowdump" } },
        { "xyz", new[] { "chemical/x-xyz" } },
        { "zip", new[] { "application/zip", "application/x-zip-compressed" } }
    });

    /// <summary>
    /// Extends the mapping dictionary with new values
    /// </summary>
    /// <param name="mimeTypes">e.g. { "zip": ["application/zip", "application/x-zip-compressed"] }</param>
    public static void Extend(IEnumerable<KeyValuePair<string, string[]>> mimeTypes)
    {
        foreach (var entry in mimeTypes)
        {
            var newValue = MimeTypesDictionary.ContainsKey(entry.Key)
                ? MimeTypesDictionary[entry.Key].Concat(entry.Value).Distinct().ToArray()
                : entry.Value;
            MimeTypesDictionary[entry.Key] = newValue;
        }
    }
    /// <summary>
    /// Extends the mapping dictionary with new values (replaces existing values)
    /// </summary>
    /// <param name="byteSequences"></param>
    public static void Extend(IEnumerable<KeyValuePair<string, byte[]>> byteSequences)
    {
        foreach (var entry in byteSequences)
        {
            MimeTypeByteSequences[entry.Key] = entry.Value;
        }
    }

    public static string GetContentType(string? fileName)
    {
        if (!string.IsNullOrWhiteSpace(fileName))
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant().TrimStart('.');
            if (extension.Length > 0 && MimeTypesDictionary.ContainsKey(extension))
            {
                return MimeTypesDictionary[extension].First();
            }
        }

        return "application/octet-stream";
    }
    public static string GetContentType(byte[] bytes, string? filename = null)
    {
        var matches = MimeTypeByteSequences
            .Where(x => bytes.Take(x.Value.Length).SequenceEqual(x.Value))
            .ToList();
        if (!matches.Any())
        {
            // no matches, try to find by filename (if provided)
            return GetContentType(filename!);
        }

        // multiple matches
        if (matches.Count > 1)
        {
            if (!string.IsNullOrWhiteSpace(filename))
            {
                var preferredExtension = matches
                    .Where(m => m.Key.Equals(Path.GetExtension(filename).TrimStart('.'), StringComparison.InvariantCultureIgnoreCase))
                    .Select(x => x.Key)
                    .FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(preferredExtension) && MimeTypesDictionary.ContainsKey(preferredExtension))
                {
                    return MimeTypesDictionary[preferredExtension].First();
                }
            }
            // otherwise: continue and take first match
        }

        // 1 match
        return MimeTypesDictionary[matches.First().Key].First();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="mimetype">Content type</param>
    /// <returns></returns>
    public static string? GetExtension(string mimetype)
    {
        var extension = MimeTypesDictionary
            .Where(x => x.Value.Contains(mimetype))
            .Select(x => x.Key)
            .FirstOrDefault();
        return extension ?? mimetype.Split('/').LastOrDefault();
    }
}