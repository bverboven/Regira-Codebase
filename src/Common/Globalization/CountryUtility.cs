using Regira.Normalizing;
using System.Globalization;

namespace Regira.Globalization;

public static class CountryUtility
{
    public static IEnumerable<Country> GetAllCountries()
        => GetCountries(null);
    public static IEnumerable<Country> GetCountries(this IEnumerable<CultureInfo>? cultures)
        => GetCulturesWithRegion(cultures)
            .ToCountries();
    public static IEnumerable<Country> GetCountriesByLanguage(string langIso2Code)
    {
        return GetCountryCultures()
            .Where(c => c.TwoLetterISOLanguageName.StartsWith(langIso2Code))
            .GetCountries();
    }
    public static Country? GetCountry(string? iso2Code)
        => string.IsNullOrWhiteSpace(iso2Code)
            ? null
            : GetCountryCultures(iso2Code)
                .GetCountries()
                .FirstOrDefault();
    public static Country? GetCountry(this CultureInfo culture)
        => new[] { culture }.GetCountries().FirstOrDefault();
    public static Country? FindCountryByName(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        var culturesWithRegion = GetCulturesWithRegion()
            .ToArray();
        var country = culturesWithRegion
            .Where(x => x.region.TwoLetterISORegionName.Equals(input, StringComparison.InvariantCultureIgnoreCase)
                        || x.region.NativeName.Equals(input, StringComparison.InvariantCultureIgnoreCase)
                        || x.region.EnglishName.Equals(input, StringComparison.InvariantCultureIgnoreCase))
            .ToCountries()
            .FirstOrDefault();
        if (country != null)
        {
            return country;
        }

        // more tolerance
        country = culturesWithRegion
#if NETSTANDARD2_0
                .Where(x => (!string.IsNullOrWhiteSpace(x.region.NativeName) && input!.ToLowerInvariant().Contains(x.region.NativeName.ToLowerInvariant()))
                            || (!string.IsNullOrWhiteSpace(x.region.EnglishName) && input!.ToLowerInvariant().Contains(x.region.EnglishName.ToLowerInvariant())))
#else
            .Where(x => (!string.IsNullOrWhiteSpace(x.region.NativeName) && input.Contains(x.region.NativeName, StringComparison.InvariantCultureIgnoreCase))
                        || (!string.IsNullOrWhiteSpace(x.region.EnglishName) && input.Contains(x.region.EnglishName, StringComparison.InvariantCultureIgnoreCase)))
#endif
            .ToCountries()
            .FirstOrDefault();
        if (country != null)
        {
            return country;
        }

        // even more tolerance
        var normalizer = NormalizingDefaults.DefaultPropertyNormalizer ?? new DefaultNormalizer();
        var normalizedInput = normalizer.Normalize(input)!;

        var countries = culturesWithRegion
            .ToCountries();
        foreach (var c in countries)
        {
            var names = c.NamesByLanguage!.Values.Concat(new[] { c.Title })
                .Select(n => n.Trim("-?".ToCharArray()))
                .Select(normalizer.Normalize)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct()
                .ToArray();

#if NETSTANDARD2_0
                if (names.Any(n => normalizedInput.ToLowerInvariant().Contains(n!.ToLowerInvariant())))
#else
            if (names.Any(n => normalizedInput.Contains(n!, StringComparison.InvariantCultureIgnoreCase)))
#endif
            {
                return c;
            }
        }

        return null;
    }

    static IEnumerable<CultureInfo> GetCountryCultures(params string[] iso2Codes)
        => CultureInfo.GetCultures(CultureTypes.SpecificCultures & ~CultureTypes.NeutralCultures)
            .Where(c => c.LCID != 4096)
            .Where(c => !iso2Codes.Any() || iso2Codes.Any(iso2Code => c.Name.EndsWith($"-{iso2Code}")));
    static IEnumerable<(CultureInfo culture, RegionInfo region)> GetCulturesWithRegion(IEnumerable<CultureInfo>? cultures = null)
        => (cultures ?? GetCountryCultures()).Select(c => (c, new RegionInfo(c.LCID)));
    static IEnumerable<Country> ToCountries(this IEnumerable<(CultureInfo culture, RegionInfo region)> culturesWithRegion)
        => culturesWithRegion
            .GroupBy(x => x.region.TwoLetterISORegionName)
            .Select(g =>
            {
                var firstItem = g.Any(x => x.culture.Name == CultureInfo.CurrentCulture.Name)
                    ? g.First(x => x.culture.Name == CultureInfo.CurrentCulture.Name)
                    : g.Any(x => x.culture.Name == CultureInfo.CurrentUICulture.Name)
                        ? g.First(x => x.culture.Name == CultureInfo.CurrentUICulture.Name)
                        : g.First();
                var firstRegion = firstItem.region;
                var names = g
                    .ToDictionary(k => k.culture.Name, v => v.region.NativeName);
                if (!names.ContainsKey("en-US"))
                {
                    names["en-US"] = firstRegion.EnglishName;
                }
                if (!names.ContainsKey(CultureInfo.CurrentUICulture.Name))
                {
                    names[CultureInfo.CurrentUICulture.Name] = firstRegion.DisplayName;
                }
                return new Country
                {
                    Iso2Code = g.Key.ToUpperInvariant(),
                    Title = firstRegion.EnglishName,
                    NamesByLanguage = names
                };
            });
}