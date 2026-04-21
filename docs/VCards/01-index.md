# Regira VCards

Regira VCards provides reading and writing of vCard contact files in versions 2.1, 3.0, and 4.0.

## Projects

| Project | Package | Backend |
|---------|---------|---------|
| `VCards.FolkerKinzel` | `Regira.Office.VCards.FolkerKinzel` | FolkerKinzel.VCards |

## Installation

```xml
<PackageReference Include="Regira.Office.VCards.FolkerKinzel" Version="5.*" />
```

## VCardManager

Implements `IVCardService`.

```csharp
var manager = new VCardManager();
```

### Read

```csharp
// Single vCard from a .vcf string
VCard contact = manager.Read(vcfContent);

// Multiple vCards from a single .vcf file (multiple VCARD blocks)
IEnumerable<VCard> contacts = manager.ReadMany(vcfContent);
```

### Write

```csharp
// Single contact
string vcf = manager.Write(contact);

// Single contact, explicit version
string vcf = manager.Write(contact, VCardVersion.V4_0);

// Multiple contacts into one .vcf string
string vcf = manager.Write(contacts, VCardVersion.V3_0);
```

### VCardVersion

```
V2_1   V3_0 (default)   V4_0
```

## Notes

- The `VCard` type is from `FolkerKinzel.VCards` — see its documentation for property access (Name, Email, Phone, Photo, etc.).
- Version 3.0 is the most widely compatible and is used as the default.
