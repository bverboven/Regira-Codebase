using Microsoft.Extensions.DependencyInjection;
#if NET6_0_OR_GREATER
using System.Text.Json.Serialization;
#endif

namespace Regira.Web.Swagger.Extensions;

public static class ApiControllerExtensions
{
#if NET6_0_OR_GREATER
    public static IMvcBuilder DisplayEnumAsString(this IMvcBuilder builder)
    {
        return builder
            .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
    }
#endif
}
