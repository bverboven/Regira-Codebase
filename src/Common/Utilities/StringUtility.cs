using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Regira.Utilities;

public static class StringUtility
{
    // https://stackoverflow.com/questions/249087/how-do-i-remove-diacritics-accents-from-a-string-in-net#answer-34272324
    private static Dictionary<string, string> ForeignCharacterMappings => new()
    {
        {"äæǽ", "ae"},
        {"öœ", "oe"},
        {"ü", "ue"},
        //{"Ä", "Ae"},// double (see below)
        {"Ü", "Ue"},
        {"Ö", "Oe"},
        {"ÀÁÂÃÄÅǺĀĂĄǍΑΆẢẠẦẪẨẬẰẮẴẲẶА", "A"},
        {"àáâãåǻāăąǎªαάảạầấẫẩậằắẵẳặа", "a"},
        {"Б", "B"},
        {"б", "b"},
        {"ÇĆĈĊČ", "C"},
        {"çćĉċč", "c"},
        {"Д", "D"},
        {"д", "d"},
        {"ÐĎĐΔ", "Dj"},
        {"ðďđδ", "dj"},
        {"ÈÉÊËĒĔĖĘĚΕΈẼẺẸỀẾỄỂỆЕЭ", "E"},
        {"èéêëēĕėęěέεẽẻẹềếễểệеэ", "e"},
        {"Ф", "F"},
        {"ф", "f"},
        {"ĜĞĠĢΓГҐ", "G"},
        {"ĝğġģγгґ", "g"},
        {"ĤĦ", "H"},
        {"ĥħ", "h"},
        {"ÌÍÎÏĨĪĬǏĮİΗΉΊΙΪỈỊИЫ", "I"},
        {"ìíîïĩīĭǐįıηήίιϊỉịиыї", "i"},
        {"Ĵ", "J"},
        {"ĵ", "j"},
        {"ĶΚК", "K"},
        {"ķκк", "k"},
        {"ĹĻĽĿŁΛЛ", "L"},
        {"ĺļľŀłλл", "l"},
        {"М", "M"},
        {"м", "m"},
        {"ÑŃŅŇΝН", "N"},
        {"ñńņňŉνн", "n"},
        {"ÒÓÔÕŌŎǑŐƠØǾΟΌΩΏỎỌỒỐỖỔỘỜỚỠỞỢО", "O"},
        {"òóôõōŏǒőơøǿºοόωώỏọồốỗổộờớỡởợо", "o"},
        {"П", "P"},
        {"п", "p"},
        {"ŔŖŘΡР", "R"},
        {"ŕŗřρр", "r"},
        {"ŚŜŞȘŠΣС", "S"},
        {"śŝşșšſσςс", "s"},
        {"ȚŢŤŦτТ", "T"},
        {"țţťŧт", "t"},
        {"ÙÚÛŨŪŬŮŰŲƯǓǕǗǙǛỦỤỪỨỮỬỰУ", "U"},
        {"ùúûũūŭůűųưǔǖǘǚǜυύϋủụừứữửựу", "u"},
        {"ÝŸŶΥΎΫỲỸỶỴЙ", "Y"},
        {"ýÿŷỳỹỷỵй", "y"},
        {"В", "V"},
        {"в", "v"},
        {"Ŵ", "W"},
        {"ŵ", "w"},
        {"ŹŻŽΖЗ", "Z"},
        {"źżžζз", "z"},
        {"ÆǼ", "AE"},
        {"ß", "ss"},
        {"Ĳ", "IJ"},
        {"ĳ", "ij"},
        {"Œ", "OE"},
        {"ƒ", "f"},
        {"ξ", "ks"},
        {"π", "p"},
        {"β", "v"},
        {"μ", "m"},
        {"ψ", "ps"},
        {"Ё", "Yo"},
        {"ё", "yo"},
        {"Є", "Ye"},
        {"є", "ye"},
        {"Ї", "Yi"},
        {"Ж", "Zh"},
        {"ж", "zh"},
        {"Х", "Kh"},
        {"х", "kh"},
        {"Ц", "Ts"},
        {"ц", "ts"},
        {"Ч", "Ch"},
        {"ч", "ch"},
        {"Ш", "Sh"},
        {"ш", "sh"},
        {"Щ", "Shch"},
        {"щ", "shch"},
        {"ЪъЬь", ""},
        {"Ю", "Yu"},
        {"ю", "yu"},
        {"Я", "Ya"},
        {"я", "ya"},
    };
    private static Dictionary<char, string> ForeignCharacters => new(ForeignCharacterMappings
        .SelectMany(p => p.Key.Select(key => new KeyValuePair<char, string>(key, p.Value)))
        .ToDictionary(x => x.Key, v => v.Value)
    );
    public static string[] SplitOnSpace(string input)
    {
        var segments = Regex.Matches(input, @"[\""].+?[\""]|[^ ]+")
#if NETSTANDARD2_0
            .Cast<Match>()
#endif
            .Select(m => m.Value);

        return segments.ToArray();
    }

    public static string? ToSnakeCase(this string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        // https://plainenglish.io/blog/convert-string-to-different-case-styles-snake-kebab-camel-and-pascal-case-in-javascript-da724b7220d7
        var matches = Regex.Matches(input, "[A-Z]{2,}(?=[A-Z][a-z]+[0-9]*|\\b)|[A-Z]?[a-z]+[0-9]*|[A-Z]|[0-9]+");
        return string.Join("_", matches).Trim('_');
    }
    public static string? ToPascalCase(this string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        return string.Concat(
            input.ToSnakeCase()!
                .Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => char.ToUpper(x[0]) + x.Substring(1).ToLower(CultureInfo.InvariantCulture))
        );
    }
    public static string? ToCamelCase(this string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        var pascalCase = ToPascalCase(input)!;
        return char.ToLower(pascalCase[0]) + pascalCase.Substring(1);
    }
    public static string? ToKebabCase(this string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        return string.Join("-", ToSnakeCase(input)!.Split('_'));
    }
    public static string? ToProperCase(this string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        var matches = Regex.Matches(input, "[A-Z]{2,}(?=[A-Z][a-z]+[0-9]*|\\b)|[A-Z]?[a-z]+[0-9]*|[A-Z]|[0-9]+")
#if NETSTANDARD2_0
            .Cast<Match>()
#endif
            .Select(m => m.Value)
            .ToArray();
        return string.Join(" ", matches.Select(m => $"{char.ToUpperInvariant(m[0])}{m.Substring(1)}")).Trim();
    }

    public static string? GetInitials(this string? input)
        => string.IsNullOrWhiteSpace(input) ? input : string.Join("", input.Split(' ').Select(s => s.First()));
    public static string? Capitalize(this string? input, CultureInfo? culture = null)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        culture ??= CultureInfo.CurrentCulture;
        return culture.TextInfo.ToTitleCase(input.ToLower(culture));
    }
    //public static string RemoveDiacritics(string input)
    //{
    //    if (string.IsNullOrWhiteSpace(input))
    //    {
    //        return input;
    //    }

    //    //https://stackoverflow.com/questions/249087/how-do-i-remove-diacritics-accents-from-a-string-in-net#answer-249126
    //    var normalizedString = input.Normalize(NormalizationForm.FormD);
    //    var stringBuilder = new StringBuilder();

    //    foreach (var c in normalizedString)
    //    {
    //        var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
    //        if (unicodeCategory != UnicodeCategory.NonSpacingMark)
    //        {
    //            stringBuilder.Append(c);
    //        }
    //    }

    //    return stringBuilder.ToString()
    //        .Normalize(NormalizationForm.FormC);
    //}
    public static string? RemoveDiacritics(this string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        // https://stackoverflow.com/questions/249087/how-do-i-remove-diacritics-accents-from-a-string-in-net#answer-67569854
        var normalizedString = input.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

#if NETSTANDARD2_0
        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }
#else
        foreach (var c in normalizedString.EnumerateRunes())
        {
            var unicodeCategory = Rune.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }
#endif

        var normalizedC = stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        var chars = normalizedC.ToCharArray();
        var sb = new StringBuilder();
        foreach (var c in chars)
        {
            if (ForeignCharacters.ContainsKey(c))
            {
                sb.Append(ForeignCharacters[c]);
            }
            else
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }
    /// <summary>
    /// Truncates a <see cref="string">input</see> to a <see cref="int">maxLength</see>.
    /// Returns null if <see cref="string">input</see> is null.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="maxLength"></param>
    /// <returns>Truncated <see cref="string">input</see> or null</returns>
    public static string? Truncate(this string? input, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(input) || input.Length <= maxLength)
        {
            return input;
        }

        return input.Substring(0, maxLength);
    }
    /// <summary>
    /// Cleans up input by trimming white space (and returning null when empty)
    /// </summary>
    /// <param name="input"></param>
    /// <param name="nullWhenEmpty"></param>
    /// <returns>Clean input string</returns>
    public static string? CleanUp(this string? input, bool nullWhenEmpty = true)
    {
        if (input == null || nullWhenEmpty && input == string.Empty)
        {
            return null;
        }

        return input.Trim();
    }

    // Base64
    public static string Base64Encode(this string plainText, Encoding? encoding = null)
    {
        encoding ??= Encoding.Default;
        var plainTextBytes = encoding.GetBytes(plainText);
        return Convert.ToBase64String(plainTextBytes);
    }
    public static string Base64Decode(this string base64EncodedData, Encoding? encoding = null)
    {
        encoding ??= Encoding.Default;
        var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
        return encoding.GetString(base64EncodedBytes);
    }


    //Source: http://mo.notono.us/2008/07/c-stringinject-format-strings-by-key.html
    public static string? Inject(this string? formatString, object injectionObject, CultureInfo? ci = null)
    {
        return injectionObject is IDictionary dic
            ? formatString.Inject(dic, ci)
            : formatString.Inject(GetPropertyHash(injectionObject), ci);
    }
    public static string? Inject(this string? formatString, IDictionary dictionary, CultureInfo? ci = null)
    {
        return formatString.Inject(new Hashtable(dictionary), ci);
    }
    public static string? Inject(this string? formatString, Hashtable? attributes, CultureInfo? ci = null)
    {
        var result = formatString;
        if (attributes == null || formatString == null)
            return result;

        return attributes.Keys.Cast<string>().Aggregate(result, (current, attributeKey) => current?.InjectSingleValue(attributeKey, attributes[attributeKey], ci));
    }
    public static string? InjectSingleValue(this string? formatString, string key, object? replacementValue, CultureInfo? ci = null)
    {
        if (formatString == null)
        {
            return null;
        }

        var result = formatString;
        Regex attributeRegex = new Regex("{(" + key + ")(?:}|(?::(.[^}]*)}))");

        foreach (Match m in attributeRegex.Matches(formatString))
        {
            string? replacement;
            if (m.Groups[2].Length > 0)
            {
                var attributeFormatString = string.Format(CultureInfo.InvariantCulture, "{{0:{0}}}", m.Groups[2]);
                replacement = string.Format(ci ?? CultureInfo.CurrentCulture, attributeFormatString, replacementValue);
            }
            else
            {
                replacement = (replacementValue ?? string.Empty).ToString();
            }
            result = result.Replace(m.ToString(), replacement);
        }
        return result;
    }
    private static Hashtable? GetPropertyHash(object? properties)
    {
        Hashtable? values = null;
        if (properties != null)
        {
            values = new Hashtable();
            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(properties);
            foreach (PropertyDescriptor prop in props)
            {
                values.Add(prop.Name, prop.GetValue(properties));
            }
        }
        return values;
    }
}