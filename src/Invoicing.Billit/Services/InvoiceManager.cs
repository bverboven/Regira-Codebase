using Regira.Invoicing.Billit.Config;
using Regira.Invoicing.Billit.Models.Contacts;
using Regira.Invoicing.Billit.Models.Extensions;
using Regira.Invoicing.Billit.Models.Orders;
using Regira.Invoicing.Billit.Models.Orders.Results;
using Regira.Invoicing.Invoices.Models.Abstractions;
using Regira.IO.Extensions;
using Regira.Serializing.Abstractions;
using System.Text;

namespace Regira.Invoicing.Billit.Services;

public interface IInvoiceManager
{
    Task<CreateOrderResult> Create(IInvoice item);
    Task<SendOrderResult> Send(params int[] ids);
}

public class InvoiceManager(IHttpClientFactory clientFactory, ISerializer serializer)
{
    private const string Path = "/v1/orders";
    private readonly HttpClient _client = clientFactory.CreateClient(BillitConstants.HttpClientName);

    public async Task<CreateOrderResult> Create(IInvoice item)
    {
        var model = Map(item);
        var json = serializer.Serialize(model);
        var body = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync(Path, body);

        try
        {
            response.EnsureSuccessStatusCode();
            return await response.ToApiResult<CreateOrderResult>() with
            {
                OrderId = await response.Content.ReadAsStringAsync()
            };
        }
        catch (Exception ex)
        {
            return await response.ToApiResult<CreateOrderResult>(ex);
        }
    }

    private OrderInput Map(IInvoice item)
    {
        FileInput? pdf = null;
        if (item.Attachments?.Any() == true)
        {
            var file = item.Attachments.First();
            pdf = new FileInput
            {
                FileContent = file.GetBytes()!,
                FileName = file.FileName!,
                MimeType = file.ContentType!
            };
        }

        return new OrderInput
        {
            // Map properties from IInvoice to OrderInput

            OrderType = item.InvoiceType == InvoiceType.Invoice ? OrderTypes.Invoice : OrderTypes.CreditNote,
            OrderDirection = OrderDirections.Income,
            OrderNumber = item.Code,
            OrderDate = item.IssueDate,
            ExpiryDate = item.DueDate ?? item.IssueDate.AddDays(30),
            PaymentReference = item.RemittanceInfo,
            OrderPDF = pdf,
            Customer = new CustomerInput
            {
                Name = item.Customer.Title,
                VATNumber = item.Customer.Code
            },
            OrderLines = item.InvoiceLines
                .Select(line => new OrderInput.OrderLineInput
                {
                    Reference = line.Code,
                    Description = line.Title,
                    DescriptionExtended = line.Description,
                    Quantity = line.Quantity,
                    UnitPriceExcl = line.UnitPriceExcl,
                    VATPercentage = line.VatPercentage,
                })
                .ToList()
        };
    }

    public async Task<SendOrderResult> Send(params int[] ids)
    {
        var url = $"{Path}/commands/send";
        var input = new SendOrderInput
        {
            OrderIDs = ids
        };
        var json = serializer.Serialize(input);
        var body = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync(url, body);
        try
        {
            response.EnsureSuccessStatusCode();
            return await response.ToApiResult<SendOrderResult>();
        }
        catch (Exception ex)
        {
            return await response.ToApiResult<SendOrderResult>(ex);
        }
    }
}