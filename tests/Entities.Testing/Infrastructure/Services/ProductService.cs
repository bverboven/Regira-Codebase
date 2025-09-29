using Entities.Testing.Infrastructure.Data;
using Regira.Entities.EFcore.QueryBuilders;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Regira.Entities.EFcore.Services;
using Regira.Entities.Models;
using Regira.Entities.Services.Abstractions;

namespace Entities.Testing.Infrastructure.Services;

public class ProductService(IEntityReadService<Product, int, SearchObject<int>> readService, IEntityWriteService<Product, int> writeService)
    : EntityRepository<Product>(readService, writeService)
{
}
public class ProductQueryBuilder(IEnumerable<IGlobalFilteredQueryBuilder> globalFilters,
    IEnumerable<IFilteredQueryBuilder<Product, int, SearchObject<int>>>? filters = null)
    : QueryBuilder<Product>(globalFilters, filters);