namespace Regira.Utilities;

public static class EnumUtility
{
    public static IEnumerable<T> FindBitFields<T>(T enumValue)
        where T : struct, Enum, IConvertible
    {
        if (!typeof(T).IsEnum)
        {
            throw new ArgumentException("T must be an enumerated type");
        }

        return FindBitFields<T>((int)Enum.ToObject(typeof(T), enumValue));
    }
    public static IEnumerable<T> FindBitFields<T>(int bitmapValue)
        where T : struct, Enum, IConvertible
    {
        if (bitmapValue == 0)
        {
            yield break;
        }

        var values = Enum.GetValues(typeof(T));
        foreach (var value in values)
        {
            var intValue = (int)value;
            if (intValue != 0 && (bitmapValue & intValue) == intValue)// exclude 0 enums
            {
                // ReSharper disable once PossibleInvalidCastException
                yield return (T)value;
            }
        }
    }
    public static IEnumerable<object> FindBitFields(object enumValue)
    {
        if (enumValue == null)
        {
            throw new ArgumentNullException(nameof(enumValue));
        }
        if (!enumValue.GetType().IsEnum)
        {
            throw new ArgumentException("T must be an enumerated type");
        }

        var values = Enum.GetValues(enumValue.GetType());
        var bitmapValue = (int)enumValue;
        foreach (var value in values)
        {
            var intValue = (int)value;
            if (intValue != 0 && (bitmapValue & intValue) == intValue)
            {
                yield return value;
            }
        }
    }
    public static bool HasFlag<T>(T enumValue, T bitfield)
        where T : struct, Enum, IConvertible
    {
        return FindBitFields(enumValue).Contains(bitfield);
    }

    public static IEnumerable<TEnum> ListValidFlagValues<TEnum>()
        where TEnum : struct, Enum
    {
        return ListValidFlagNumericValues<TEnum>()
            .Select(v => (TEnum)(object)v);
    }
    static IEnumerable<int> ListValidFlagNumericValues<TEnum>()
        where TEnum : struct, Enum
    {
#if NETSTANDARD2_0
        var values = Enum.GetValues(typeof(TEnum)).Cast<TEnum>();
#else
        var values = Enum.GetValues<TEnum>();
#endif
        return values
            .Select(v => (int)(object)v)
            // exclude non-bitmask values
            .Where(v => (v & (v - 1)) == 0);
    }
    public static IEnumerable<TEnum> ListFlagValues<TEnum>(bool showAllCombinations = false, bool onlyDefined = true)
        where TEnum : struct, Enum
    {
        if (!showAllCombinations)
        {
            return ListValidFlagValues<TEnum>()
                .OrderBy(v => (int)(object)v);
        }

        var definedFlagValues = Enum
#if NETSTANDARD2_0
            .GetValues(typeof(TEnum)).Cast<TEnum>()
#else
            .GetValues<TEnum>()
#endif
            .Select(v => (int)(object)v)
            .Where(v => v > 0 && (v & (v - 1)) == 0)
            .ToArray();

        var values = ListValidFlagNumericValues<TEnum>()
            .ToArray();
        var minValue = values.Min(v => v);
        var maxValue = values.Sum(v => v);

        var allValues = Enumerable.Range(minValue, (maxValue - minValue) + 1).ToArray();

        var undefinedValues = allValues
            .Where(v => v > 0 && (v & (v - 1)) == 0)
            .Where(x => !definedFlagValues.Contains(x))
            .ToArray();

        var combos = allValues
            .Where(v => !onlyDefined || undefinedValues.All(uv => (uv & v) != uv));

        return combos
            .Distinct()
            .OrderBy(x => x)
            .Select(x => (TEnum)(object)x);
    }
    public static TEnum GetMaxFlagValue<TEnum>()
        where TEnum : struct, Enum
    {
        var values = ListValidFlagNumericValues<TEnum>()
            .ToArray();
        return (TEnum)(object)values.Sum();
    }

    public static TEnum ToBitmask<TEnum>(this IEnumerable<TEnum> values)
        => (TEnum)(object)(values.Cast<int>().Sum());
}