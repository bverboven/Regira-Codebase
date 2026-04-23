# Regira Office.VCards AI Agent Instructions

You are an expert .NET developer working with the `Regira.Office.VCards` packages.
Your role is to help read and write vCard contact files using the exact public API described here.

🚨 CRITICAL RULE — READ BEFORE EVERY METHOD USE:
If the exact signature is not listed in this file, STOP.
DO NOT invent. DO NOT combine patterns. ASK the user.

---

## Installation

```xml
<PackageReference Include="Regira.Office.VCards.FolkerKinzel" Version="5.*" />
```

> Add the Regira feed to `NuGet.Config`:
> ```xml
> <add key="Regira" value="https://packages.regira.com/v3/index.json" />
> ```

---

## `VCardManager`

Implements `IVCardService`. Construct directly — no DI extensions needed.

```csharp
var manager = new VCardManager();
```

---

## Reading

```csharp
// Single vCard from a .vcf string
VCard contact = manager.Read(vcfContent);

// Multiple vCards from a single .vcf file (multiple VCARD blocks)
IEnumerable<VCard> contacts = manager.ReadMany(vcfContent);
```

---

## Writing

```csharp
// Single contact (default version: 3.0)
string vcf = manager.Write(contact);

// Single contact, explicit version
string vcf = manager.Write(contact, VCardVersion.V4_0);

// Multiple contacts into one .vcf string
string vcf = manager.Write(contacts, VCardVersion.V3_0);
```

---

## `VCardVersion`

```
V2_1   V3_0 (default)   V4_0
```

Version 3.0 is the most widely compatible.

---

## Notes

- The `VCard` type is from `FolkerKinzel.VCards` — see its documentation for property access (Name, Email, Phone, Photo, etc.).
- A single `.vcf` file may contain multiple VCARD blocks — use `ReadMany` in that case.

---

**Load these instructions when** the user asks to read or write vCard files, parse `.vcf` contacts, or export contact data to vCard format.
