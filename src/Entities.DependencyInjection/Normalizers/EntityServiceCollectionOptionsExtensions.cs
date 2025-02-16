using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.DependencyInjection.Models;
using Regira.Entities.Keywords;
using Regira.Entities.Keywords.Abstractions;
using Regira.Normalizing.Abstractions;

namespace Regira.Entities.DependencyInjection.Normalizers;

public static class EntityServiceCollectionOptionsExtensions
{
    public static EntityServiceCollectionOptions AddDefaultQKeywordHelper(this EntityServiceCollectionOptions options, Func<IServiceProvider, INormalizer>? normalizerFactory = null, QKeywordHelperOptions? qOptions = null)
    {
        options.Services.AddTransient<IQKeywordHelper>(p => new QKeywordHelper(normalizerFactory?.Invoke(p), qOptions));
        return options;
    }
}