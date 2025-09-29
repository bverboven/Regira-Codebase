#if NETCOREAPP3_1_OR_GREATER
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json.Serialization;

namespace Regira.Entities.Web.DependencyInjection;

public static class EntityServiceCollectionJsonExtensions
{
    /// <summary>
    /// <list type="bullet">
    /// <item>Ignore nulls</item>
    /// <item>Ignore reference cycles</item>
    /// <item>Enums as string</item>
    /// </list>
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    public static IServiceCollection ConfigureDefaultJsonOptions(this IServiceCollection services, Action<JsonOptions>? configure = null)
    {
        services
            .Configure<JsonOptions>(o =>
            {
                o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                configure?.Invoke(o);
            });

        return services;
    }
}
#endif