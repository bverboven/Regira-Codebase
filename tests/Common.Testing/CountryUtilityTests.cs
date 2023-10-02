using Regira.Globalization;

namespace Common.Testing;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class CountryUtilityTests
{
    [Test]
    public void Test_Get_All()
    {
        var countries = CountryUtility.GetAllCountries()
            .ToArray();
        CollectionAssert.IsNotEmpty(countries);
        CollectionAssert.AllItemsAreUnique(countries);
    }

    [TestCase("BE", "Belgium")]
    [TestCase("DE", "Germany")]
    [TestCase("PL", "Poland")]
    [TestCase("CD", "Congo (DRC)")]
    [TestCase("TR", "Turkey")]
    [TestCase("MN", "Mongolia")]
    [TestCase("XX", null)]
    [TestCase(null, null)]
    public void Test_Get_By_Code(string? code, string? expectedTitle)
    {
        var country = CountryUtility.GetCountry(code);
        Assert.That(country?.Title, Is.EqualTo(expectedTitle));
    }

    [TestCase("Belgium", "BE")]
    [TestCase("Belgie", "BE")]
    [TestCase("Belgique", "BE")]
    [TestCase("Antwerpen is een provincie in België", "BE")]
    [TestCase("En Belgique il y a une ville Liège", "BE")]
    [TestCase("Antwerp is a province in Flanders", null)]
    [TestCase(null, null)]
    public void Test_Find_By_Name(string? input, string? expectedCode)
    {
        var country = CountryUtility.FindCountryByName(input);
        Assert.That(country?.Iso2Code, Is.EqualTo(expectedCode));
    }

    [TestCase("nl-BE", "België")]
    [TestCase("fr-BE", "Belgique")]
    [TestCase("fr-CD", "Congo (République démocratique du)")]
    //[TestCase("de-BE", "Belgien")] // not possible to convert CultureInfo to RegionInfo
    [TestCase("xx-BE", null)]
    public void Test_Get_Country_Name(string cultureName, string expectedCountryName)
    {
        var countryCode = cultureName.Split('-').Last();
        var country = CountryUtility.GetCountry(countryCode);
        Assert.That(country?.GetName(cultureName), Is.EqualTo(expectedCountryName));
    }

    [TestCase("nl", new[] { "NL", "BE" })]
    [TestCase("de", new[] { "DE", "AT", "CH", "LI", "LU" })]
    [TestCase("xx", new string[0])]
    public void Test_Get_By_Language(string lang, string[] expectedCodes)
    {
        var countries = CountryUtility.GetCountriesByLanguage(lang);
        CollectionAssert.AreEquivalent(expectedCodes, countries.Select(c => c.Iso2Code));
    }
}