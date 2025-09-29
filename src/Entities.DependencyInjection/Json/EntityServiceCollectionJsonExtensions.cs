#if NETCOREAPP3_1_OR_GREATER
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.DependencyInjection.ServiceBuilders.Models;
using Regira.Entities.Web.DependencyInjection;

namespace Regira.Entities.DependencyInjection.Json;

public static class EntityServiceCollectionOptionsJsonExtensions
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
            .ConfigureDefaultJsonOptions(configure);

        return options;
    }
}
#endif