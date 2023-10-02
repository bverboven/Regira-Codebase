using System.Globalization;
using System.Text.RegularExpressions;

namespace Regira.Utilities;

public static class RegexUtility
{
    // https://docs.microsoft.com/en-us/dotnet/standard/base-types/how-to-verify-that-strings-are-in-valid-email-format
    private static string EmailPattern => @"(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))";
    // https://www.freecodecamp.org/news/how-to-validate-urls-in-javascript/
    private static string UrlPattern => @"([a-zA-Z]+:\/\/)?((([a-z\d]([a-z\d-]*[a-z\d])*)\.)+[a-z]{2,}|((\d{1,3}\.){3}\d{1,3}))(\:\d+)?(\/[-a-z\d%_.~+]*)*(\?[;&a-z\d%_.~+=-]*)?(\#[-a-z\d_]*)?";
    // https://uibakery.io/regex-library/phone-number (added '/' to allowed chars and allowed (0) in second segment)
    private static string PhoneNumberPattern => @"\+?\d{1,4}?[-./\s]?(\(0\))?\(?\d{1,3}?\)?[-.\s]?\d{1,4}[-.\s]?\d{1,4}[-.\s]?\d{1,9}";
    private static string IPv4AddressPattern => @"(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)";
    private static string IPv6AddressPattern => @"(([0-9a-fA-F]{1,4}:){7,7}[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,7}:|([0-9a-fA-F]{1,4}:){1,6}:[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,5}(:[0-9a-fA-F]{1,4}){1,2}|([0-9a-fA-F]{1,4}:){1,4}(:[0-9a-fA-F]{1,4}){1,3}|([0-9a-fA-F]{1,4}:){1,3}(:[0-9a-fA-F]{1,4}){1,4}|([0-9a-fA-F]{1,4}:){1,2}(:[0-9a-fA-F]{1,4}){1,5}|[0-9a-fA-F]{1,4}:((:[0-9a-fA-F]{1,4}){1,6})|:((:[0-9a-fA-F]{1,4}){1,7}|:)|fe80:(:[0-9a-fA-F]{0,4}){0,4}%[0-9a-zA-Z]{1,}|::(ffff(:0{1,4}){0,1}:){0,1}((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])|([0-9a-fA-F]{1,4}:){1,4}:((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9]))";

    public static bool IsValidEmail(string? input)
    {
        // https://docs.microsoft.com/en-us/dotnet/standard/base-types/how-to-verify-that-strings-are-in-valid-email-format

        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        try
        {
            // Normalize the domain
            input = Regex.Replace(input, @"(@)(.+)$", DomainMapper, RegexOptions.None, TimeSpan.FromMilliseconds(200));

            // Examines the domain part of the email and normalizes it.
            string DomainMapper(Match match)
            {
                // Use IdnMapping class to convert Unicode domain names.
                var idn = new IdnMapping();

                // Pull out and process domain name (throws ArgumentException on invalid)
                var domainName = idn.GetAscii(match.Groups[2].Value);

                return match.Groups[1].Value + domainName;
            }
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }

        try
        {
            // modified original regex -> https://emailregex.com/
            return Regex.IsMatch(input, PatternToExactMatch(EmailPattern), RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }
    public static bool IsValidUrl(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        return Regex.IsMatch(input, PatternToExactMatch(UrlPattern), RegexOptions.IgnoreCase);
    }
    public static bool IsValidPhoneNumber(string? input, bool allowShortNumber = false)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        if (!allowShortNumber && input.Length <= 6)
        {
            return false;
        }

        // short number (sms)
        if (allowShortNumber && Regex.IsMatch(input, @"\d{4,6}"))
        {
            return true;
        }

        return Regex.IsMatch(input, PatternToExactMatch(PhoneNumberPattern));
    }
    public static bool IsValidIPv4Address(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }
        return Regex.IsMatch(input, IPv4AddressPattern);
    }
    public static bool IsValidIPv6Address(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }
        return Regex.IsMatch(input, IPv6AddressPattern, RegexOptions.IgnoreCase);
    }
    public static bool IsValidIPAddress(string? input)
        => IsValidIPv4Address(input) || IsValidIPv6Address(input);

    static string PatternToExactMatch(string input) => $"^{input}$";
    static string PatternToExtract(string input) => $@"\b{input}\b";

    public static string[] ExtractEmails(string input)
    {
        if (!string.IsNullOrWhiteSpace(input))
        {
            return Regex.Matches(input, PatternToExtract(EmailPattern), RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250))
#if NETSTANDARD2_0
                .Cast<Match>()
#endif
                .Select(m => m.Value)
                .Distinct()
                .ToArray();
        }

        return Array.Empty<string>();
    }
    public static string[] ExtractUrls(string input)
    {
        if (!string.IsNullOrWhiteSpace(input))
        {
            return Regex.Matches(input, PatternToExtract(UrlPattern), RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250))
#if NETSTANDARD2_0
                .Cast<Match>()
#endif
                .Select(m => m.Value)
                .Distinct()
                .ToArray();
        }

        return Array.Empty<string>();
    }
    public static string[] ExtractPhoneNumbers(string input, bool allowShortNumbers = false)
    {
        if (!string.IsNullOrWhiteSpace(input))
        {
            var pattern = allowShortNumbers ? $@"(\d{{4,6}})|{PhoneNumberPattern})" : PhoneNumberPattern;
            return Regex.Matches(input, pattern, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250))
#if NETSTANDARD2_0
                .Cast<Match>()
#endif
                .Select(m => m.Value)
                .Distinct()
                .ToArray();
        }

        return Array.Empty<string>();
    }
    public static string[] ExtractIPAddresses(string input)
    {
        if (!string.IsNullOrWhiteSpace(input))
        {
            var pattern = $"({IPv4AddressPattern})|({IPv6AddressPattern})";
            return Regex.Matches(input, PatternToExtract(pattern), RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250))
#if NETSTANDARD2_0
                .Cast<Match>()
#endif
                .Select(m => m.Value)
                .Distinct()
                .ToArray();
        }

        return Array.Empty<string>();
    }
}