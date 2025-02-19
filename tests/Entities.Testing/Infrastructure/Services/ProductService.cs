using Entities.Testing.Infrastructure.Data;
using Regira.Entities.Abstractions;
using Regira.Entities.EFcore.QueryBuilders;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Regira.Entities.EFcore.Services;
using Regira.Entities.Models;

namespace Entities.Testing.Infrastructure.Services;

public class ProductService(IEntityReadService<Product, int, SearchObject<int>> readService, IEntityWriteService<Product> writeService)
    : EntityRepository<Product>(readService, writeService)
{
}
public class ProductQueryBuilder(IEnumerable<IGlobalFilteredQueryBuilder> globalFilters,
    IEnumerable<IFilteredQueryBuilder<Product, SearchObject<int>>>? filters = null)
    : QueryBuilder<Product>(globalFilters, filters);