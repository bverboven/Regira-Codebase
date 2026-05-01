using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Regira.Office.Clients.DependencyInjection;

namespace Office.Clients.Testing.Abstractions;

public abstract class OfficeClientTestsBase
{
    protected readonly IServiceProvider Services;

    protected OfficeClientTestsBase()
    {
        var config = new ConfigurationBuilder()
            .AddUserSecrets(GetType().Assembly)
            .Build();

        var services = new ServiceCollection();
        services.AddOfficeClients(o =>
        {
            o.BaseUrl = config["ApiServices:BaseUrl"]!;
            o.ApiKey = config["ApiServices:ApiKey"];
        });

        Services = services.BuildServiceProvider();
    }
}
