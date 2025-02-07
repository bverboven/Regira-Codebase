﻿using Regira.Entities.Keywords.Abstractions;
using Regira.Normalizing;
using Regira.Normalizing.Abstractions;

namespace Regira.Entities.Keywords;

public class QKeywordHelperOptions
{
    /// <summary>
    /// Wildcard character the consumer is using
    /// </summary>
    public string WildcardInput { get; set; } = "*";
    /// <summary>
    /// Wildcard character the data store is using
    /// </summary>
    public string WildcardOutput { get; set; } = "%";
    public bool ApplyNormalize { get; set; } = true;
}
public class QKeywordHelper(INormalizer? normalizer = null, QKeywordHelperOptions? options = null) : IQKeywordHelper
{
    QKeywordHelperOptions Options => options ?? new QKeywordHelperOptions();
    private INormalizer Normalizer => Options.ApplyNormalize
        ? normalizer ?? NormalizingDefaults.DefaultPropertyNormalizer ?? new DefaultNormalizer()
        : null!;



    public ParsedKeywordCollection Parse(string? input)
    {
        var parsedKeywords = input
#if NETSTANDARD2_0
            ?.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
#else
            ?.Split(' ', StringSplitOptions.RemoveEmptyEntries)
#endif
            .Select(ParseKeyword) 
            ?? [];
        return new ParsedKeywordCollection(parsedKeywords, Options.ApplyNormalize ? Normalizer.Normalize(input) : input);
    }
    public QKeyword ParseKeyword(string? input)
    {
        var isStartingWith = input?.StartsWith(Options.WildcardInput) == true;
        var isEndingWith = input?.EndsWith(Options.WildcardInput) == true;
        var trimmed = input?.Trim(Options.WildcardInput.ToCharArray());
        var normalized = Options.ApplyNormalize ? Normalizer.Normalize(trimmed) : trimmed;

        var startsWith = $"{normalized}{Options.WildcardOutput}";
        var endsWith = $"{Options.WildcardOutput}{normalized}";
        var q = $"{(isStartingWith ? Options.WildcardOutput : "")}{normalized}{(isEndingWith ? Options.WildcardOutput : "")}";
        var qw = $"{Options.WildcardOutput}{normalized}{Options.WildcardOutput}";
        return new QKeyword
        {
            Keyword = input,
            HasWildcardAtStart = isStartingWith,
            HasWildcardAtEnd = isEndingWith,
            Trimmed = trimmed,
            Normalized = normalized,
            StartsWith = startsWith,
            EndsWith = endsWith,
            Q = q,
            QW = qw
        };
    }
}