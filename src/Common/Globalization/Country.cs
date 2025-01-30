namespace Regira.Globalization;

public class Country
{
    /// <summary>
    /// 2 letter ISO code
    /// </summary>
    public string Iso2Code { get; set; } = null!;

    /// <summary>
    /// Name in english
    /// </summary>
    public string Title { get; set; } = null!;
    /// <summary>
    /// Native Names per language code
    /// </summary>
    public IDictionary<string, string>? NamesByLanguage { get; set; }

    public string? GetName(string language)
    {
        var lang = language
            .Split("-".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
            .First();

        if (lang == "en")
        {
            return Title;
        }

        var key = $"{lang}-{Iso2Code}";
        if (NamesByLanguage?.TryGetValue(key, out var name) is true)
        {
            return name;
        }

        return null;
    }
}