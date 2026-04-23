# Regira Invoicing AI Agent Instructions

You are an expert .NET developer working with the `Regira.Invoicing` packages.
Your role is to help create electronic invoices, convert them to UBL/Peppol format, and transmit them via an AP gateway using the exact public API described here.

🚨 CRITICAL RULE — READ BEFORE EVERY METHOD USE:
If the exact signature is not listed in this file, STOP.
DO NOT invent. DO NOT combine patterns. ASK the user.

---

## Installation

```xml
<!-- Billit — create and send invoices via Billit -->
<PackageReference Include="Regira.Invoicing.Billit" Version="5.*" />

<!-- UblSharp — convert invoices to UBL XML (Peppol BIS) -->
<PackageReference Include="Regira.Invoicing.UblSharp" Version="5.*" />

<!-- ViaAdValvas — transmit UBL documents via AdValVas AP gateway -->
<PackageReference Include="Regira.Invoicing.ViaAdValvas" Version="5.*" />
```

> Add the Regira feed to `NuGet.Config`:
> ```xml
> <add key="Regira" value="https://packages.regira.com/v3/index.json" />
> ```

---

## Billit

### `BillitConfig`

| Property | Type | Description |
|----------|------|-------------|
| `PartyId` | `string?` | Your Billit party ID |
| `Api.BaseUrl` | `string?` | Billit API base URL |
| `Api.Key` | `string?` | API key |

### DI Registration

```csharp
services.AddBillit(sp => new BillitConfig
{
    PartyId = configuration["Billit:PartyId"],
    Api     = new() { BaseUrl = configuration["Billit:Api:Url"], Key = configuration["Billit:Api:Key"] }
});
// Registers: IInvoiceManager, IFileManager, IPartyManager, IPeppolManager
```

### `IInvoiceManager`

```csharp
Task<ICreateInvoiceResult> Create(IInvoice item);
Task<ISendInvoiceResult>   Send(params string[] ids);   // send by IDs
Task<ISendInvoiceResult>   Send(IInvoice input);         // send by invoice object
```

---

## UblSharp — UBL Conversion

### `IUblConverter`

```csharp
XDocument Convert(UblDocumentInput input);
```

Produces a UBL 2.1 `Invoice` document.

```csharp
var converter = new UblConverter();
XDocument ubl = converter.Convert(new UblDocumentInput
{
    // IInvoice-based input with lines, parties, tax, etc.
});
```

### Supporting Constants

- **`UblConstants`** — Customization ID and Profile ID for Peppol BIS Billing 3.0

- **`InvoiceTypeCode`**
  - `380` — commercial invoice
  - `381` — credit note

- **`PaymentMeansCode`**
  - `31` — bank transfer
  - `58` — SEPA credit transfer

- **`TaxCategoryCode`**
  - `S` — standard rate
  - `Z` — zero-rated
  - `E` — exempt
  - `AE` — reverse charge

---

## ViaAdValvas — Peppol Transmission

### `GatewaySettings`

| Property | Type | Description |
|----------|------|-------------|
| `Uri` | `string` | AdValVas gateway endpoint |
| `SenderID` | `string` | Your Peppol participant ID |
| `SenderName` | `string` | Display name |
| `Token` | `string` | API token |
| `SecretKey` | `string` | HMAC secret for request signing |

### `PeppolService`

```csharp
var service = new PeppolService(gatewaySettings, jsonSerializer);

UblDocumentResponse result = await service.Send(ublDocument);

if (result.Success)
    Console.WriteLine($"Sent. Reference: {result.Reference}");
```

Requests are HMAC-signed internally via `SealUtility.Generate()`.

---

## Typical End-to-End Flow

```csharp
// 1. Build the invoice domain model
IInvoice invoice = BuildInvoice(order);

// 2. Convert to UBL XML
XDocument ubl = new UblConverter().Convert(new UblDocumentInput { /* … */ });

// 3. Transmit via Peppol
var result = await peppolService.Send(ubl);

// 4. (Optional) also create in Billit for accounting
await invoiceManager.Create(invoice);
await invoiceManager.Send(invoice);
```

---

**Load these instructions when** the user asks about creating or sending electronic invoices, UBL/Peppol conversion, AP gateway transmission, or integrating with Billit or AdValVas.
