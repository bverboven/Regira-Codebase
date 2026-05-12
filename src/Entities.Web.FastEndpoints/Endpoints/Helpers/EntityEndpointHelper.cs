using Microsoft.Extensions.DependencyInjection;
using Regira.DAL.Paging;
using Regira.Entities.Extensions;
using Regira.Entities.Mapping.Abstractions;
using Regira.Entities.Models;
using Regira.Entities.Models.Abstractions;
using Regira.Entities.Services.Abstractions;
using Regira.Entities.Web.FastEndpoints.Extensions;
using Regira.Entities.Web.Models;
using System.Diagnostics;

namespace Regira.Entities.Web.FastEndpoints.Endpoints.Helpers;

internal static class EntityEndpointHelper
{
    /// <summary>
    /// Shared save logic used by Save, Create and Modify endpoints.
    /// Returns <c>(result, null)</c> on success, <c>(null, errors)</c> on validation failure,
    /// or <c>(null, null)</c> when the entity was not found (for update scenarios).
    /// </summary>
    internal static async Task<(SaveResult<TDto>? result, IDictionary<string, string>? errors)> SaveAsync<TEntity, TKey, TDto, TInputDto>(
        IServiceProvider services, TInputDto model, TKey? id, CancellationToken ct)
        where TEntity : class, IEntity<TKey>
    {
        var sw = Stopwatch.StartNew();

        try
        {
            var mapper = services.GetRequiredService<IEntityMapper>();
            var item = mapper.Map<TEntity>(model!);
            if (id != null && !id.Equals(default(TKey)))
            {
                item.Id = id;
            }
            var isNew = item.IsNew();

            var service = services.GetRequiredEntityService<IEntityService<TEntity, TKey>>();
            if (!isNew)
            {
                var exists = await service.Count(new { item.Id }, ct) == 1;
                if (!exists)
                {
                    return (null, null);
                }
            }

            await service.Save(item, ct);
            var affected = await service.SaveChanges(ct);

            var savedItem = await service.Details(item.Id, ct);
            var savedModel = mapper.Map<TDto>(savedItem!);

            sw.Stop();

            return (new SaveResult<TDto> { Item = savedModel, Affected = affected, IsNew = isNew, Duration = sw.ElapsedMilliseconds }, null);
        }
        catch (EntityInputException<TEntity> ex)
        {
            return (null, ex.InputErrors);
        }
    }

    /// <summary>
    /// Shared delete logic used by Delete endpoints.
    /// Returns <c>null</c> when the entity was not found.
    /// </summary>
    internal static async Task<DeleteResult<TDto>?> DeleteAsync<TEntity, TKey, TDto>(
        IServiceProvider services, TKey id, CancellationToken ct)
        where TEntity : class, IEntity<TKey>
    {
        var sw = Stopwatch.StartNew();

        var service = services.GetRequiredEntityService<IEntityService<TEntity, TKey>>();
        var item = (await service.List(new { id }, token: ct)).SingleOrDefault();
        if (item == null)
        {
            return null;
        }

        await service.Remove(item, ct);
        await service.SaveChanges(ct);

        var mapper = services.GetRequiredService<IEntityMapper>();
        var model = mapper.Map<TDto>(item);

        sw.Stop();

        return new DeleteResult<TDto> { Item = model, Duration = sw.ElapsedMilliseconds };
    }

    /// <summary>Extracts <see cref="PagingInfo"/> from the current query string.</summary>
    internal static PagingInfo? ExtractPagingInfo(int? page, int? pageSize)
    {
        if (!page.HasValue && !pageSize.HasValue)
        {
            return null;
        }

        return new PagingInfo
        {
            Page = page.GetValueOrDefault(1),
            PageSize = pageSize.GetValueOrDefault(0)
        };
    }

    /// <summary>Parses an array of enum values from a comma-separated or repeated query parameter.</summary>
    internal static TEnum[] ParseEnumValues<TEnum>(IEnumerable<string?> rawValues)
        where TEnum : struct, Enum
    {
        var result = new List<TEnum>();
        foreach (var value in rawValues)
        {
            if (!string.IsNullOrWhiteSpace(value) && Enum.TryParse<TEnum>(value, ignoreCase: true, out var enumValue))
            {
                result.Add(enumValue);
            }
        }
        return result.ToArray();
    }
}
