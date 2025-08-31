namespace Regira.Globalization;

/// <summary>
/// Represents a country with its associated ISO code, title, and localized names.
/// </summary>
/// <remarks>
/// This class provides functionality to manage country-related information, including
/// retrieving localized names based on language codes. It is designed to support
/// globalization and localization scenarios.
/// </remarks>
public class Country
{
    /// <summary>
    /// 2-letter ISO code
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

    /// <summary>
    /// Retrieves the localized name of the country based on the specified language.
    /// </summary>
    /// <param name="language">
    /// The language code (e.g., "en", "fr-BE") used to determine the localized name.
    /// </param>
    /// <returns>
    /// The localized name of the country if available; otherwise, <c>null</c>.
    /// </returns>
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