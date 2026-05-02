# Regira Invoicing AI Agent Instructions

> Electronic invoice creation, UBL/Peppol conversion, and document transmission via an AP gateway.

## Projects

| Project | Package | Purpose |
|---------|---------|----------|
| `Common.Invoicing` | `Regira.Invoicing` | Shared abstractions |
| `Invoicing.Billit` | `Regira.Invoicing.Billit` | Create and send invoices via Billit |
| `Invoicing.UblSharp` | `Regira.Invoicing.UblSharp` | Convert invoices to UBL XML (Peppol BIS) |
| `Invoicing.ViaAdValvas` | `Regira.Invoicing.ViaAdValvas` | Transmit UBL documents via AdValVas AP gateway |

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

> Shared setup: see [`shared.setup.md`](./shared.setup.md) — **NuGet feed**.

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

