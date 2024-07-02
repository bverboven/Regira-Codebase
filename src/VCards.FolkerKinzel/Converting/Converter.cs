using FolkerKinzel.VCards.Enums;
using FolkerKinzel.VCards.Models;
using Regira.Office.VCards.Abstractions;
using Regira.Office.VCards.Models;
using Regira.Office.VCards.Utilities;
using FKvCard = FolkerKinzel.VCards.VCard;

namespace Regira.Office.VCards.FolkerKinzel.Converting;

internal static class Converter
{
    public static VCard Convert(FKvCard item)
    {
        var fkName = item.NameViews?.FirstOrDefault()?.Value;
        var name = new VCardName
        {
            GivenName = fkName?.GivenNames.FirstOrDefault(),
            SurName = fkName?.FamilyNames.FirstOrDefault(),
            Prefix = fkName?.Prefixes.FirstOrDefault(),
            Suffix = fkName?.Suffixes.FirstOrDefault()
        };
        var tels = item.Phones
            ?.Select(phone =>
            {
                VCardTel tel = phone?.Value;
                if (phone?.Parameters.PhoneType.HasValue ?? false)
                {
                    var types = ConvertUtility
                        .ConvertBitFields<Tel, VCardTelType>(phone.Parameters.PhoneType.Value)
                        .ToArray();
                    if (types.Any())
                    {
                        tel.Type = types.Aggregate((r, v) => r | v);
                    }
                }

                if (phone?.Parameters.PropertyClass.HasValue ?? false)
                {
                    var types = ConvertUtility
                        .ConvertBitFields<PCl, VCardTelType>(phone.Parameters.PropertyClass.Value)
                        .ToArray();
                    if (types.Any())
                    {
                        var telType = types.Aggregate((r, v) => r | v);
                        tel.Type = tel.Type.HasValue
                            ? tel.Type | telType
                            : telType;
                    }
                }

                return tel;
            })
            .ToList();
        var emails = item.EMails
            ?.Select(e =>
            {
                var email = new VCardEmail { Text = e?.Value };
                if (e?.Parameters.PropertyClass.HasValue ?? false)
                {
                    var types = ConvertUtility
                        .ConvertBitFields<PCl, VCardPropertyType>(e.Parameters.PropertyClass.Value)
                        .ToArray();
                    if (types.Any())
                    {
                        email.Type = types.Aggregate((r, v) => r | v);
                    }
                }

                return email;
            })
            .ToList();
        var addresses = item.Addresses
            ?.Select(x =>
            {
                var address = new VCardAddress
                {
                    Label = x?.Parameters.Label,
                    Country = x?.Value.Country.FirstOrDefault(),
                    Region = x?.Value.Region.FirstOrDefault(),
                    Locality = x?.Value.Locality.FirstOrDefault(),
                    PostalCode = x?.Value.PostalCode.FirstOrDefault(),
                    StreetAndNumber = x?.Value.Street.FirstOrDefault(),
                    //Extension = x?.Value.ExtendedAddress.FirstOrDefault(),// deprecated
                    //PostBox = x?.Value.PostOfficeBox.FirstOrDefault(),// deprecated
                };
                if (!string.IsNullOrWhiteSpace(x?.Parameters.Label))
                {
                    address.Label = x?.Parameters.Label;
                }
                if (x?.Parameters.PropertyClass.HasValue ?? false)
                {
                    var types = ConvertUtility
                        .ConvertBitFields<PCl, VCardPropertyType>(x.Parameters.PropertyClass.Value)
                        .ToArray();
                    if (types.Any())
                    {
                        address.Type = types.Aggregate((r, v) => r | v);
                    }
                }

                return address;
            })
            .ToList();
        var organization = item.Organizations
            ?.Select(o => new VCardOrganization
            {
                Name = o?.Value.OrganizationName,
            })
            .FirstOrDefault();
        return new VCard
        {
            Name = name,
            FormattedName = item.DisplayNames?.FirstOrDefault()?.Value,
            Tels = tels,
            Emails = emails,
            Addresses = addresses,
            Organization = organization
        };
    }

    public static FKvCard Convert(VCard item)
    {
        var name = new NameProperty(
            item.Name?.SurName,
            item.Name?.GivenName,
            null,
            item.Name?.Prefix,
            item.Name?.Suffix
        );
        var phoneNumbers = item.Tels
            ?.Select(tel =>
            {
                var phone = new TextProperty(tel);
                if (tel.Type.HasValue)
                {
                    var telTypes = ConvertUtility
                        .ConvertBitFields<VCardTelType, Tel>(tel.Type.Value)
                        .ToArray();
                    if (telTypes.Any())
                    {
                        phone.Parameters.PhoneType = telTypes.Aggregate((r, v) => r | v);
                    }

                    var propTypes = ConvertUtility
                        .ConvertBitFields<VCardTelType, PCl>(tel.Type.Value)
                        .ToArray();
                    if (propTypes.Any())
                    {
                        phone.Parameters.PropertyClass = propTypes.Aggregate((r, v) => r | v);
                    }
                }

                return phone;
            });
        var emailAddresses = item.Emails
            ?.Select(email =>
            {
                var emailAddress = new TextProperty(email);
                if (email.Type.HasValue)
                {
                    var types = ConvertUtility
                        .ConvertBitFields<VCardPropertyType, PCl>(email.Type.Value)
                        .ToArray();
                    if (types.Any())
                    {
                        emailAddress.Parameters.PropertyClass = types.Aggregate((r, v) => r | v);
                    }
                }

                return emailAddress;
            });
        var addresses = item.Addresses
            ?.Select(x =>
                {
                    var address = new AddressProperty(
                        x.StreetAndNumber,
                        x.Locality,
                        null,
                        x.PostalCode,
                        x.Country
                    )
                    {
                        Parameters =
                        {
                            Label = x.Label
                        },
                        //Value =
                        //{
                        //    PostOfficeBox = { x.PostBox },
                        //    ExtendedAddress = { x.Extension }
                        //}
                    };
                    if (x.Type.HasValue)
                    {
                        var types = ConvertUtility
                            .ConvertBitFields<VCardPropertyType, PCl>(x.Type.Value)
                            .ToArray();
                        if (types.Any())
                        {
                            address.Parameters.PropertyClass = types.Aggregate((r, v) => r | v);
                        }
                    }

                    return address;
                }
            );
        var orgs = item.Organization != null ? new[] { new OrgProperty(item.Organization?.Name) } : null;
        return new FKvCard
        {
            DisplayNames = new List<TextProperty>
            {
                new (item.FormattedName)
            },
            NameViews = name,
            Phones = phoneNumbers,
            EMails = emailAddresses,
            Addresses = addresses,
            Organizations = orgs
        };
    }
}