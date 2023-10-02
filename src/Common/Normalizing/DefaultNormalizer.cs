using Regira.Normalizing.Abstractions;
using Regira.Normalizing.Models;
using Regira.Utilities;
using System.Text.RegularExpressions;

namespace Regira.Normalizing;

public class DefaultNormalizer : INormalizer
{
    private readonly NormalizeOptions _options;
    // Parameter-less constructor needed for Activator.CreateInstance
    public DefaultNormalizer() : this(new NormalizeOptions()) { }
    public DefaultNormalizer(NormalizeOptions? options)
    {
        _options = options ?? new NormalizeOptions();
    }

    public string? Normalize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        var output = input;
        if (_options.RemoveDiacritics)
        {
            output = output.RemoveDiacritics()!;
        }
        output = Regex.Replace(output, @"[^a-z0-9\s\-_,!;&']", string.Empty, RegexOptions.IgnoreCase);
        output = Regex.Replace(output, $@"[{_options.CharsToSpace}]", " ").Trim();
        output = Regex.Replace(output, @"\s+", " ").Trim();
        output = Regex.Replace(output, @"\s", " ");
        switch (_options.Transform)
        {
            case TextTransform.ToUpperCase:
                output = output.ToUpper();
                break;
            case TextTransform.ToLowerCase:
                output = output.ToLower();
                break;
        }
        return output;
    }
}