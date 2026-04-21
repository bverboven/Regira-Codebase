# Regira Payments

Regira Payments provides a unified abstraction for payment processing via Mollie and POM.

## Projects

| Project | Package | Backend |
|---------|---------|---------|
| `Common.Payments` | `Regira.Payments` | Shared abstractions |
| `Payments.Mollie` | `Regira.Payments.Mollie` | Mollie API |
| `Payments.Pom` | `Regira.Payments.Pom` | POM payment gateway |

## Installation

```xml
<PackageReference Include="Regira.Payments.Mollie" Version="5.*" />
<PackageReference Include="Regira.Payments.Pom"    Version="5.*" />
```

---

## Mollie

### MollieConfig

| Property | Type | Description |
|----------|------|-------------|
| `Api` | `string` | Mollie API base URL |
| `Key` | `string` | Mollie API key (`test_...` or `live_...`) |
| `MaxPageSize` | `int` | Default `250` |
| `RedirectFactory` | `Func<IPayment, string>?` | Builds redirect URL per payment |
| `WebhookFactory` | `Func<IPayment, string>?` | Builds webhook URL per payment |

### PaymentService

```csharp
var svc = new Regira.Payments.Mollie.PaymentService(new MollieConfig
{
    Api             = "https://api.mollie.com/v2",
    Key             = configuration["Mollie:Key"]!,
    RedirectFactory = p => $"https://myapp.com/payment/return/{p.Id}",
    WebhookFactory  = p => $"https://myapp.com/payment/webhook/{p.Id}"
});

// CRUD
IPayment?            payment  = await svc.Details(paymentId);
IEnumerable<IPayment> list    = await svc.List();
await svc.Save(newPayment);    // creates payment, sets checkout URL
await svc.Delete(payment);

// Handle webhook
await svc.WebHook(paymentId, async p =>
{
    if (p?.Status == "paid")
        await orderService.MarkPaid(p.Id);
});
```

---

## POM

### PomSettings

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

### PaymentService

```csharp
var svc = new Regira.Payments.Pom.PaymentService(pomSettings, jsonSerializer);

// Get payment status
IPayment? payment = await svc.Details(paymentId);

// Create a payment link
var pom = new PomPayment
{
    Amount         = 49.99m,
    Currency       = "EUR",
    DocumentDate   = DateTime.Today,
    SenderContractNumber = settings.SenderContractNumber
};
await svc.Save(pom);
```

`PomException` is thrown on API errors; check `StatusCode` and `PomResponse` for details.

---

## Notes

- Both services are stateless and can be registered as singletons.
- `IPayment` is the shared interface from `Common.Payments`.
- Mollie uses the official `Mollie.Api` NuGet client; POM uses `RestSharp` with HTTP Basic Auth.
