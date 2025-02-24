using Entities.Testing.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.EFcore.Processing;

namespace Entities.Testing.Infrastructure.Processors;

public class CategoryProcessor(ProductContext dbContext) : EntityProcessor<Category>
{
    public override async Task Process(IList<Category> items)
    {
        var categoryIds = items
            .Select(x => x.Id)
            .Distinct()
            .ToArray();
        var countPerCategory = await dbContext.Products
            .Where(p => categoryIds.Contains(p.CategoryId!.Value))
            .GroupBy(p => p.CategoryId!.Value)
            .ToDictionaryAsync(k => k.Key, v => v.Count());
        foreach (var item in items)
        {
            item.NumberOfProducts = countPerCategory[item.Id];
        }
    }
}