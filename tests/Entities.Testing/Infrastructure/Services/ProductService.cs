using Entities.Testing.Infrastructure.Data;
using Regira.Entities.EFcore.QueryBuilders;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Regira.Entities.EFcore.Services;
using Regira.Entities.Models;

namespace Entities.Testing.Infrastructure.Services;

public class ProductService(ProductContext dbContext, IQueryBuilder<Product> queryBuilder)
    : EntityRepository<ProductContext, Product>(dbContext, queryBuilder)
{
}
public class ProductQueryBuilder(IEnumerable<IGlobalFilteredQueryBuilder> globalFilters,
    IEnumerable<IFilteredQueryBuilder<Product, SearchObject<int>>>? filters = null)
    : QueryBuilder<Product>(globalFilters, filters);