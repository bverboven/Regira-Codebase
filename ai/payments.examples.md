# Payments — Example: Webshop Checkout

> Context: A webshop creates a Mollie payment when a customer checks out and handles the webhook to mark the order as paid.

## DI Registration

```csharp
services.AddSingleton<IPaymentService>(_ => new Regira.Payments.Mollie.PaymentService(
    new MollieConfig
    {
        Api             = "https://api.mollie.com/v2",
        Key             = configuration["Mollie:Key"]!,
        RedirectFactory = p => $"https://myshop.com/checkout/return/{p.Id}",
        WebhookFactory  = p => $"https://myshop.com/checkout/webhook/{p.Id}"
    }));
```

## Create a payment at checkout

```csharp
public async Task<string> StartCheckout(Order order)
{
    var payment = new MolliePayment
    {
        Id          = order.Id.ToString(),
        Amount      = order.Total,
        Currency    = "EUR",
        Description = $"Order #{order.Number}"
    };

    await _paymentService.Save(payment);   // sets checkout URL on the object
    return payment.CheckoutUrl!;           // redirect customer here
}
```

## Handle the Mollie webhook

```csharp
[HttpPost("checkout/webhook/{orderId}")]
public async Task<IActionResult> Webhook(string orderId)
{
    await _paymentService.WebHook(orderId, async p =>
    {
        if (p?.Status == "paid")
            await _orderService.MarkPaid(orderId);
    });
    return Ok();
}
```

## Check payment status

```csharp
public async Task<bool> IsPaid(string paymentId)
{
    var payment = await _paymentService.Details(paymentId);
    return payment?.Status == "paid";
}
```
