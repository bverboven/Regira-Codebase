#if NETCOREAPP3_1_OR_GREATER
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.DependencyInjection.ServiceBuilders.Models;

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
    /// <param name="options"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    public static EntityServiceCollectionOptions ConfigureDefaultJsonOptions(this EntityServiceCollectionOptions options, Action<JsonOptions>? configure = null)
    {
        options.Services
            .Configure<JsonOptions>(o =>
            {
                o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                configure?.Invoke(o);
            });

        return options;
    }
}
#endif