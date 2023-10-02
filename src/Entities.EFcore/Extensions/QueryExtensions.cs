using Microsoft.EntityFrameworkCore;
using Regira.Entities.Attachments.Abstractions;
using Regira.Entities.Keywords;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing.Abstractions;
using Regira.Utilities;

namespace Regira.Entities.EFcore.Extensions;

public static class QueryExtensions
{
    public static IQueryable<T> Filter<T, TKey>(this IQueryable<T> query, ISearchObject<TKey>? so, INormalizer? normalizer = null)
        where T : IEntity<TKey>
    {
        if (so == null)
        {
            return query;
        }

        var qHelper = QKeywordHelper.Create(normalizer);

        query = query.FilterId(so.Id);
        query = query.FilterIds(so.Ids);

        if (TypeUtility.ImplementsInterface<IHasNormalizedContent>(typeof(T)))
        {
            query = query.Cast<IHasNormalizedContent>().FilterQ(qHelper.Parse(so.Q)).Cast<T>();
        }

        if (TypeUtility.ImplementsInterface<IHasCreated>(typeof(T)))
        {
            query = query.Cast<IHasCreated>().FilterCreated(so.MinCreated, so.MaxCreated).Cast<T>();
        }
        if (TypeUtility.ImplementsInterface<IHasLastModified>(typeof(T)))
        {
            query = query.Cast<IHasLastModified>().FilterLastModified(so.MinLastModified, so.MaxLastModified).Cast<T>();
        }

        if (TypeUtility.ImplementsInterface<IArchivable>(typeof(T)))
        {
            query = query.Cast<IArchivable>().FilterArchivable(so.IsArchived).Cast<T>();
        }

        return query;
    }

    public static IQueryable<TEntity> FilterId<TEntity, TKey>(this IQueryable<TEntity> query, TKey? id)
        where TEntity : IEntity<TKey>
    {
        if (default(TKey)?.Equals(id) != true)
        {
            query = query.Where(x => x.Id!.Equals(id));
        }

        return query;
    }
    public static IQueryable<TEntity> FilterIds<TEntity, TKey>(this IQueryable<TEntity> query, ICollection<TKey?>? ids)
        where TEntity : IEntity<TKey>
    {
        if (ids?.Any() == true)
        {
            query = query.Where(x => ids.Contains(x.Id));
        }

        return query;
    }

    public static IQueryable<TEntity> FilterCode<TEntity>(this IQueryable<TEntity> query, string? code)
        where TEntity : IHasCode
    {
        if (code != null)
        {
            query = query.Where(x => x.Code == code);
        }

        return query;
    }

    public static IQueryable<TEntity> FilterTitle<TEntity>(this IQueryable<TEntity> query, ParsedKeywordCollection? keywords)
        where TEntity : IHasTitle
    {
        if (keywords?.Any() == true)
        {
            foreach (var q in keywords)
            {
                query = q.HasWildcard
                    ? query.Where(x => EF.Functions.Like(x.Title!, q.Q!))
                    : query.Where(x => x.Title == q.Keyword);
            }
        }

        return query;
    }
    public static IQueryable<TEntity> FilterNormalizedTitle<TEntity>(this IQueryable<TEntity> query, ParsedKeywordCollection? keywords)
        where TEntity : IHasNormalizedTitle
    {
        if (keywords?.Any() == true)
        {
            foreach (var q in keywords)
            {
                query = q.HasWildcard
                    ? query.Where(x => EF.Functions.Like(x.NormalizedTitle!, q.Q!))
                    : query.Where(x =>
                        x.NormalizedTitle == q.Normalized || x.NormalizedTitle!.Contains($" {q.Normalized}") ||
                        x.NormalizedTitle!.Contains($"{q.Normalized} "));
            }
        }

        return query;
    }

    public static IQueryable<TEntity> FilterCreated<TEntity>(this IQueryable<TEntity> query, DateTime? minDate, DateTime? maxDate)
        where TEntity : IHasCreated
    {
        if (minDate.HasValue)
        {
            query = query.Where(x => x.Created >= minDate);
        }
        if (maxDate.HasValue)
        {
            query = query.Where(x => x.Created <= maxDate);
        }

        return query;
    }
    public static IQueryable<TEntity> FilterLastModified<TEntity>(this IQueryable<TEntity> query, DateTime? minDate, DateTime? maxDate)
        where TEntity : IHasLastModified
    {
        if (minDate.HasValue)
        {
            query = query.Where(x => x.LastModified >= minDate);
        }
        if (maxDate.HasValue)
        {
            query = query.Where(x => x.LastModified <= maxDate);
        }

        return query;
    }
    public static IQueryable<TEntity> FilterTimestamps<TEntity>(this IQueryable<TEntity> query, DateTime? minCreated, DateTime? maxCreated, DateTime? minModified, DateTime? maxModified)
        where TEntity : IHasTimestamps
        => query
            .FilterCreated(minCreated, maxCreated)
            .FilterLastModified(minModified, maxModified);

    public static IQueryable<TEntity> FilterQ<TEntity>(this IQueryable<TEntity> query, ParsedKeywordCollection? keywords)
        where TEntity : IHasNormalizedContent
    {
        if (keywords?.Any() == true)
        {
            foreach (var q in keywords)
            {
                query = query.Where(x => EF.Functions.Like(x.NormalizedContent!, q.QW!));
            }
        }

        return query;
    }
    public static IQueryable<TEntity> FilterArchivable<TEntity>(this IQueryable<TEntity> query, bool? isArchived)
        where TEntity : IArchivable
    {
        if (isArchived.HasValue)
        {
            query = query.Where(x => x.IsArchived == isArchived);
        }

        return query;
    }

    public static IQueryable<TEntity> FilterHasAttachment<TEntity>(this IQueryable<TEntity> query, bool? hasAttachment)
        where TEntity : IHasAttachments
    {
        if (hasAttachment.HasValue)
        {
            query = query.Where(x => hasAttachment == x.Attachments!.Any());
        }

        return query;
    }

    public static IQueryable<TEntity> SortQuery<TEntity, TKey>(this IQueryable<TEntity> query)
        where TEntity : IEntity<TKey>
    {
        if (TypeUtility.ImplementsInterface<IHasNormalizedTitle>(typeof(TEntity)))
        {
            return query
                .Cast<IHasNormalizedTitle>()
                .OrderBy(x => x.NormalizedTitle)
                .Cast<TEntity>();
        }
        if (TypeUtility.ImplementsInterface<IHasTitle>(typeof(TEntity)))
        {
            return query
                .Cast<IHasTitle>()
                .OrderBy(x => x.Title)
                .Cast<TEntity>();
        }

        return query.OrderBy(x => x.Id);
    }
}