using Regira.Normalizing;
using Regira.Normalizing.Abstractions;

namespace Regira.Entities.Keywords;

public class QKeywordHelper
{
    public class Options
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

    public string WildcardInput { get; }
    public string WildcardOutput { get; }
    public bool ApplyNormalize { get; }
    private readonly INormalizer _normalizer;
    public QKeywordHelper(INormalizer? normalizer = null, Options? options = null)
    {
        options ??= new Options();
        WildcardInput = options.WildcardInput;
        WildcardOutput = options.WildcardOutput;
        ApplyNormalize = options.ApplyNormalize;
        _normalizer = ApplyNormalize
            ? normalizer ?? NormalizingDefaults.DefaultPropertyNormalizer ?? new DefaultNormalizer()
            : null!;
    }



    public ParsedKeywordCollection Parse(string? input)
    {
        var parsedKeywords = input
#if NETSTANDARD2_0
            ?.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
#else
                ?.Split(' ', StringSplitOptions.RemoveEmptyEntries)
#endif
            .Select(ParseKeyword) ?? Array.Empty<QKeyword>();
        return new ParsedKeywordCollection(parsedKeywords, ApplyNormalize ? _normalizer.Normalize(input) : input);
    }
    public QKeyword ParseKeyword(string? input)
    {
        var isStartingWith = input?.StartsWith(WildcardInput) == true;
        var isEndingWith = input?.EndsWith(WildcardInput) == true;
        var trimmed = input?.Trim(WildcardInput.ToCharArray());
        var normalized = ApplyNormalize ? _normalizer.Normalize(trimmed) : trimmed;

        var startsWith = $"{normalized}{WildcardOutput}";
        var endsWith = $"{WildcardOutput}{normalized}";
        var q = $"{(isStartingWith ? WildcardOutput : "")}{normalized}{(isEndingWith ? WildcardOutput : "")}";
        var qw = $"{WildcardOutput}{normalized}{WildcardOutput}";
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


    public static QKeywordHelper Create(INormalizer? normalizer = null, Options? options = null) => new(normalizer, options);
}