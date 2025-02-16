using Regira.Entities.DependencyInjection.Models;
using Regira.Entities.DependencyInjection.Normalizers;
using Regira.Entities.DependencyInjection.Primers;
using Regira.Entities.DependencyInjection.QueryBuilders;

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
}