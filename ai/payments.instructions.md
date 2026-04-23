# Regira Payments AI Agent Instructions

You are an expert .NET developer working with the `Regira.Payments` packages.
Your role is to help integrate payment providers (Mollie and POM) using the exact public API described here.

🚨 CRITICAL RULE — READ BEFORE EVERY METHOD USE:
If the exact signature is not listed in this file, STOP.
DO NOT invent. DO NOT combine patterns. ASK the user.

---

## Installation

```xml
<!-- Mollie payment gateway -->
<PackageReference Include="Regira.Payments.Mollie" Version="5.*" />

<!-- POM payment gateway -->
<PackageReference Include="Regira.Payments.Pom" Version="5.*" />
```

> Add the Regira feed to `NuGet.Config`:
> ```xml
> <add key="Regira" value="https://packages.regira.com/v3/index.json" />
> ```

---

## Shared Abstraction

Both services implement `IPaymentService` and work with `IPayment` from `Common.Payments`.
Both are stateless and can be registered as singletons.

---

## Mollie

### `MollieConfig`

| Property | Type | Description |
|----------|------|-------------|
| `Api` | `string` | Mollie API base URL |
| `Key` | `string` | Mollie API key (`test_...` or `live_...`) |
| `MaxPageSize` | `int` | Default `250` |
| `RedirectFactory` | `Func<IPayment, string>?` | Builds redirect URL per payment |
| `WebhookFactory` | `Func<IPayment, string>?` | Builds webhook URL per payment |

### `PaymentService` (Mollie)

```csharp
var svc = new Regira.Payments.Mollie.PaymentService(new MollieConfig
{
    Api             = "https://api.mollie.com/v2",
    Key             = configuration["Mollie:Key"]!,
    RedirectFactory = p => $"https://myapp.com/payment/return/{p.Id}",
    WebhookFactory  = p => $"https://myapp.com/payment/webhook/{p.Id}"
});

IPayment?              payment = await svc.Details(paymentId);
IEnumerable<IPayment>  list    = await svc.List();
await svc.Save(newPayment);     // creates payment, sets checkout URL on the object
await svc.Delete(payment);
```

### Webhook Handling

```csharp
await svc.WebHook(paymentId, async p =>
{
    if (p?.Status == "paid")
        await orderService.MarkPaid(p.Id);
});
```

---

## POM

### `PomSettings`

| Property | Type | Description |
|----------|------|-------------|
| `SenderId` | `string?` | POM sender ID |
| `SenderContractNumber` | `string?` | Contract number |
| `Username` | `string?` | API username |
| `Password` | `string?` | API password |
| `ExpiresIn` | `int` | Payment link expiry (seconds) |
| `Mode` | `string?` | `"live"` or `"test"` |
| `AuthApi` | `string?` | Authentication endpoint |
| `CreatePaylinkApi` | `string` | Create payment link endpoint |
| `PaylinkStatusApi` | `string` | Payment status endpoint |
| `WebhookKey` | `string?` | Webhook HMAC key |

### `PaymentService` (POM)

```csharp
var svc = new Regira.Payments.Pom.PaymentService(pomSettings, jsonSerializer);

IPayment? payment = await svc.Details(paymentId);

var pom = new PomPayment
{
    Amount               = 49.99m,
    Currency             = "EUR",
    DocumentDate         = DateTime.Today,
    SenderContractNumber = settings.SenderContractNumber
};
await svc.Save(pom);
```

`PomException` is thrown on API errors — check `StatusCode` and `PomResponse` for details.

---

## Provider Comparison

| Feature | Mollie | POM |
|---------|--------|-----|
| CRUD | ✓ | ✓ |
| Webhook handling | `WebHook()` | Via `WebhookKey` HMAC |
| Payment link | ✓ (checkout URL set on Save) | ✓ (`CreatePaylinkApi`) |
| Client library | Official `Mollie.Api` NuGet | `RestSharp` + HTTP Basic Auth |

---

**Load these instructions when** the user asks about payment processing, Mollie integration, POM payment links, or handling payment webhooks.
