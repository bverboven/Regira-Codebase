# Regira Globalization

Regira Globalization extends the phone number formatting and country utilities built into [Common](../Common).

## Projects

| Project | Package | Backend |
|---------|---------|---------|
| `Globalization.LibPhoneNumber` | `Regira.Globalization.LibPhoneNumber` | libphonenumber-csharp |

## Installation

```xml
<PackageReference Include="Regira.Globalization.LibPhoneNumber" Version="5.*" />
```

## PhoneNumberFormatter

Implements both `INormalizer` and `IFormatter`.

```csharp
// Use the system culture to infer the default country code
var fmt = new PhoneNumberFormatter();

string? e164  = fmt.Normalize("+32 471 12 34 56");   // "+32471123456"
string? intl  = fmt.Format("+32 471 12 34 56");       // "+32 471 12 34 56"

// Supply a specific culture for regional number resolution
var be = new PhoneNumberFormatter(new CultureInfo("nl-BE"));
string? local = be.Normalize("0471 12 34 56");        // "+32471123456"
```

| Method | Output format |
|--------|---------------|
| `Normalize` | E.164 (e.g. `+32471123456`) — suitable for storage |
| `Format` | International display (e.g. `+32 471 12 34 56`) |

Returns `null` when the input cannot be parsed as a valid phone number.

## Country Utilities (Common)

`CountryUtility` and the `Country` model are in `Regira.Common`:

```csharp
var countries = CountryUtility.GetAllCountries();
var be        = CountryUtility.GetCountry("BE");
string name   = be.GetName("fr");   // "Belgique"

// Search by local name
var found = CountryUtility.FindCountryByName("Belgique", "fr");
```
