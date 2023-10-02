using Microsoft.Extensions.DependencyInjection;
#if NETSTANDARD2_0
using Newtonsoft.Json.Converters;
#else
using System.Text.Json.Serialization;
#endif

namespace Regira.Web.Swagger.Extensions
{
    public static class ApiControllerExtensions
    {
#if NETSTANDARD2_0
        public static IMvcCoreBuilder DisplayEnumAsString(this IMvcCoreBuilder builder)
        {
            var converter = new StringEnumConverter();
            return builder
                .AddJsonOptions(o => o.SerializerSettings.Converters.Add(converter));
        }
#else
        public static IMvcBuilder DisplayEnumAsString(this IMvcBuilder builder)
        {
            return builder
                .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        }
#endif
    }
}
