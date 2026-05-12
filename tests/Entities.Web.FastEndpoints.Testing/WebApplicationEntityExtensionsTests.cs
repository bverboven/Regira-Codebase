using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Regira.DAL.Paging;
using Regira.Entities.Models.Abstractions;
using Regira.Entities.Services.Abstractions;
using Regira.Entities.Web.FastEndpoints.Extensions;
using Regira.Entities.Web.FastEndpoints.Models;
using Regira.Entities.Web.Models;
using Xunit;

namespace Entities.Web.FastEndpoints.Testing;

public class WebApplicationEntityExtensionsTests
{
    [Fact]
    public async Task MapEntityCrudEndpoints_Maps_Default_List_Route()
    {
        var service = new InMemoryEntityService<Widget>([
            new Widget { Id = 1, Name = "First" }
        ]);

        await using var host = await TestApplication<Widget>.CreateAsync(service);

        var response = await host.Client.GetAsync("/api/widgets");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ListResult<Widget>>();
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(1, result.Items[0].Id);
        Assert.Equal("First", result.Items[0].Name);
    }

    [Fact]
    public async Task MapEntityCrudEndpoints_Registers_Expected_Default_Crud_Routes()
    {
        await using var host = await TestApplication<Widget>.CreateAsync();

        var routes = GetCrudRoutes(host.App)
            .Where(route => route.Contains("/api/widgets", StringComparison.OrdinalIgnoreCase))
            .OrderBy(route => route)
            .ToArray();

        Assert.Equal([
            "DELETE /api/widgets/{id}",
            "GET /api/widgets",
            "GET /api/widgets/{id}",
            "POST /api/widgets",
            "POST /api/widgets/save",
            "PUT /api/widgets/{id}"
        ], routes);
    }

    [Fact]
    public async Task MapEntityCrudEndpoints_Deduplicates_Routes_For_Duplicate_Service_Registrations()
    {
        var duplicate = new InMemoryEntityService<Widget>();

        await using var host = await TestApplication<Widget>.CreateAsync(
            configureServices: services => AddEntityService(services, duplicate));

        var routes = GetCrudRoutes(host.App)
            .Where(route => route.Contains("/api/widgets", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        Assert.Equal(6, routes.Length);
    }

    [Fact]
    public async Task MapEntityCrudEndpoints_Maps_Routes_For_Multiple_Entities()
    {
        var gadgetService = new InMemoryEntityService<Gadget>();

        await using var host = await TestApplication<Widget>.CreateAsync(
            configureServices: services => AddEntityService(services, gadgetService));

        var routes = GetCrudRoutes(host.App);

        Assert.Equal(12, routes.Length);
        Assert.Contains("GET /api/widgets", routes);
        Assert.Contains("GET /api/gadgets", routes);
    }

    [Fact]
    public async Task MapEntityCrudEndpoints_Uses_Custom_RoutePrefix()
    {
        await using var host = await TestApplication<Widget>.CreateAsync(routePrefix: "v2");

        var routes = GetCrudRoutes(host.App);

        Assert.Contains("GET /v2/widgets", routes);
        Assert.DoesNotContain("GET /api/widgets", routes);
    }

    [Fact]
    public async Task MapEntityCrudEndpoints_Uses_Explicit_Route_Override()
    {
        await using var host = await TestApplication<Widget>.CreateAsync(
            routePrefix: "v2",
            configure: options => options.For<Widget>("catalog/widgets"));

        var routes = GetCrudRoutes(host.App);

        Assert.Contains("GET /catalog/widgets", routes);
        Assert.DoesNotContain("GET /v2/widgets", routes);
    }

    [Fact]
    public async Task MapEntityCrudEndpoints_Does_Not_Map_Routes_Without_ServiceCollection_In_DI()
    {
        await using var host = await TestApplication<Widget>.CreateAsync(registerServiceCollection: false);

        Assert.Empty(GetCrudRoutes(host.App));
    }

    [Fact]
    public async Task MapEntityCrudEndpoints_Get_Details_Returns_NotFound_For_Unknown_Id()
    {
        await using var host = await TestApplication<Widget>.CreateAsync();

        var response = await host.Client.GetAsync("/api/widgets/404");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task MapEntityCrudEndpoints_Post_Create_Returns_SaveResult()
    {
        await using var host = await TestApplication<Widget>.CreateAsync();

        var response = await host.Client.PostAsJsonAsync("/api/widgets", new Widget { Name = "Created" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<SaveResult<Widget>>();
        Assert.NotNull(result);
        Assert.True(result.IsNew);
        Assert.Equal(1, result.Affected);
        Assert.NotNull(result.Item);
        Assert.True(result.Item.Id > 0);
        Assert.Equal("Created", result.Item.Name);
    }

    [Fact]
    public async Task MapEntityCrudEndpoints_Delete_Removes_Entity()
    {
        var service = new InMemoryEntityService<Widget>([
            new Widget { Id = 1, Name = "Delete Me" }
        ]);

        await using var host = await TestApplication<Widget>.CreateAsync(service);

        var response = await host.Client.DeleteAsync("/api/widgets/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<DeleteResult<Widget>>();
        Assert.NotNull(result);
        Assert.Equal(1, result.Item.Id);
        Assert.Null(await host.Service.Details(1));
    }

    private static void AddEntityService<TEntity>(IServiceCollection services, InMemoryEntityService<TEntity> service)
        where TEntity : class, IEntity<int>
    {
        services.AddSingleton(service);
        services.AddSingleton<IEntityService<TEntity, int>>(sp => sp.GetRequiredService<InMemoryEntityService<TEntity>>());
    }

    private static string[] GetCrudRoutes(WebApplication app)
    {
        return app.Services.GetServices<EndpointDataSource>()
            .SelectMany(dataSource => dataSource.Endpoints)
            .OfType<RouteEndpoint>()
            .SelectMany(endpoint => GetHttpMethods(endpoint)
                .Select(method => $"{method} {NormalizeRoute(endpoint.RoutePattern.RawText)}"))
            .OrderBy(route => route)
            .ToArray();
    }

    private static IEnumerable<string> GetHttpMethods(Endpoint endpoint)
    {
        var metadata = endpoint.Metadata.FirstOrDefault(x => x.GetType().Name == "HttpMethodMetadata");
        var property = metadata?.GetType().GetProperty("HttpMethods");
        return property?.GetValue(metadata) as IEnumerable<string> ?? [];
    }

    private static string NormalizeRoute(string? route)
    {
        if (string.IsNullOrWhiteSpace(route) || route == "/")
        {
            return "/";
        }

        return "/" + route.Trim('/');
    }

    private sealed class TestApplication<TEntity>(WebApplication app, HttpClient client, InMemoryEntityService<TEntity> service) : IAsyncDisposable
        where TEntity : class, IEntity<int>
    {
        public WebApplication App { get; } = app;
        public HttpClient Client { get; } = client;
        public InMemoryEntityService<TEntity> Service { get; } = service;

        public static async Task<TestApplication<TEntity>> CreateAsync(
            InMemoryEntityService<TEntity>? service = null,
            bool registerServiceCollection = true,
            string routePrefix = "api",
            Action<EntityAutoEndpointsOptions>? configure = null,
            Action<IServiceCollection>? configureServices = null)
        {
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseTestServer();

            if (registerServiceCollection)
            {
                builder.Services.AddSingleton<IServiceCollection>(builder.Services);
            }

            service ??= new InMemoryEntityService<TEntity>();
            AddEntityService(builder.Services, service);
            configureServices?.Invoke(builder.Services);

            var app = builder.Build();
            app.MapEntityEndpoints(routePrefix, configure);
            await app.StartAsync();

            return new TestApplication<TEntity>(app, app.GetTestClient(), service);
        }

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await App.DisposeAsync();
        }
    }

    private sealed class InMemoryEntityService<TEntity>(IEnumerable<TEntity>? seed = null) : IEntityService<TEntity, int>
        where TEntity : class, IEntity<int>
    {
        private readonly Dictionary<int, TEntity> _items = seed?.ToDictionary(item => item.Id) ?? [];
        private int _nextId = seed?.Select(item => item.Id).DefaultIfEmpty().Max() + 1 ?? 1;

        public Task Add(TEntity item, CancellationToken token = default)
        {
            SaveInternal(item);
            return Task.CompletedTask;
        }

        public Task<TEntity?> Modify(TEntity item, CancellationToken token = default)
        {
            SaveInternal(item);
            return Task.FromResult<TEntity?>(item);
        }

        public Task Save(TEntity item, CancellationToken token = default)
        {
            SaveInternal(item);
            return Task.CompletedTask;
        }

        public Task Remove(TEntity item, CancellationToken token = default)
        {
            _items.Remove(item.Id);
            return Task.CompletedTask;
        }

        public Task<int> SaveChanges(CancellationToken token = default)
            => Task.FromResult(1);

        public Task<TEntity?> Details(int id, CancellationToken token = default)
            => Task.FromResult(_items.GetValueOrDefault(id));

        public Task<IList<TEntity>> List(object? so = null, PagingInfo? pagingInfo = null, CancellationToken token = default)
        {
            var items = _items.Values.OrderBy(item => item.Id).ToList();
            if (pagingInfo is { PageSize: > 0 })
            {
                items = items
                    .Skip((pagingInfo.Page - 1) * pagingInfo.PageSize)
                    .Take(pagingInfo.PageSize)
                    .ToList();
            }

            return Task.FromResult<IList<TEntity>>(items);
        }

        public Task<long> Count(object? so, CancellationToken token = default)
        {
            if (TryGetId(so, out var id))
            {
                return Task.FromResult(_items.ContainsKey(id) ? 1L : 0L);
            }

            return Task.FromResult((long)_items.Count);
        }

        private void SaveInternal(TEntity item)
        {
            if (item.Id == 0)
            {
                item.Id = _nextId++;
            }

            _items[item.Id] = item;
        }

        private static bool TryGetId(object? searchObject, out int id)
        {
            if (searchObject != null)
            {
                var property = searchObject.GetType().GetProperties()
                    .FirstOrDefault(x => string.Equals(x.Name, nameof(IEntity<int>.Id), StringComparison.OrdinalIgnoreCase));
                if (property?.GetValue(searchObject) is int value)
                {
                    id = value;
                    return true;
                }
            }

            id = default;
            return false;
        }
    }

    private sealed class Widget : IEntity<int>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private sealed class Gadget : IEntity<int>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}