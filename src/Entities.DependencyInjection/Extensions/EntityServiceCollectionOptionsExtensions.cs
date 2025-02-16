using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.DependencyInjection.Normalizers;
using Regira.Entities.DependencyInjection.Primers;
using Regira.Entities.DependencyInjection.QueryBuilders;
using Regira.Entities.Keywords;
using Regira.Entities.Keywords.Abstractions;
using Regira.Normalizing.Abstractions;

namespace Regira.Entities.DependencyInjection.Extensions;

public static class EntityServiceCollectionOptionsExtensions
{
    public static EntityServiceCollectionOptions UseDefaults(this EntityServiceCollectionOptions options)
    {
        options.AddDefaultQKeywordHelper();
        options.AddDefaultPrimers();
        options.AddDefaultEntityNormalizer();
        options.AddDefaultGlobalQueryFilters();
        return options;
    }

    public static EntityServiceCollectionOptions AddDefaultQKeywordHelper(this EntityServiceCollectionOptions options, Func<IServiceProvider, INormalizer>? normalizerFactory = null, QKeywordHelperOptions? qOptions = null)
    {
        options.Services.AddTransient<IQKeywordHelper>(p => new QKeywordHelper(normalizerFactory?.Invoke(p), qOptions));
        return options;
    }
}