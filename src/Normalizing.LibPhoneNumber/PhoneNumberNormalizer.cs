using PhoneNumbers;
using Regira.Globalization;
using Regira.Normalizing.Abstractions;
using System.Globalization;

namespace Regira.Normalizing.LibPhoneNumber;

[Obsolete("Use Regira.Globalization.LibPhoneNumber.PhoneNumberFormatter instead")]
public class PhoneNumberNormalizer : INormalizer
{
    private readonly CultureInfo _culture;
    private readonly PhoneNumberUtil _phoneNumberUtil;
    public PhoneNumberNormalizer(CultureInfo? culture = null)
    {
        _culture = culture ?? CultureInfo.CurrentCulture;
        _phoneNumberUtil = PhoneNumberUtil.GetInstance();
    }
    public string? Normalize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        var phoneNumber = ParseInput(input);
        return _phoneNumberUtil.Format(phoneNumber, PhoneNumberFormat.E164);
    }

    public string? Format(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        var phoneNumber = ParseInput(input);
        return _phoneNumberUtil.Format(phoneNumber, PhoneNumberFormat.INTERNATIONAL);
    }

    internal PhoneNumber? ParseInput(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        var countryCode = _culture.GetCountry()?.Iso2Code;
        return _phoneNumberUtil.Parse(input, countryCode);
    }
}