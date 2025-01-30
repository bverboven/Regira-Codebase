using System.Globalization;
using PhoneNumbers;
using Regira.Normalizing.Abstractions;

namespace Regira.Globalization.LibPhoneNumber;

public class PhoneNumberFormatter(CultureInfo? culture = null) : INormalizer, IFormatter
{
    protected CultureInfo Culture { get; } = culture ?? CultureInfo.CurrentCulture;
    protected PhoneNumberUtil PhoneNumberUtil { get; } = PhoneNumberUtil.GetInstance();


    public string? Normalize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        var phoneNumber = ParseInput(input);
        return PhoneNumberUtil.Format(phoneNumber, PhoneNumberFormat.E164);
    }
    public string? Format(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        var phoneNumber = ParseInput(input);
        return PhoneNumberUtil.Format(phoneNumber, PhoneNumberFormat.INTERNATIONAL);
    }

    internal PhoneNumber? ParseInput(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        var countryCode = Culture.GetCountry()?.Iso2Code;
        return PhoneNumberUtil.Parse(input, countryCode);
    }
}