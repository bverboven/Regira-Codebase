﻿using Regira.Utilities;

namespace Regira.Office.VCards.Utilities;

public static class ConvertUtility
{
    public static IEnumerable<TOutEnum> ConvertBitFields<TInEnum, TOutEnum>(TInEnum srcBitmap)
        where TInEnum : struct, Enum, IConvertible
        where TOutEnum : struct, Enum, IConvertible
    {
        var srcValues = EnumUtility.FindBitFields(srcBitmap)
            .ToArray();
        var targetValues =
#if NETSTANDARD2_0
                Enum.GetValues(typeof(TOutEnum)).Cast<TOutEnum>()
#else
            Enum.GetValues<TOutEnum>()
#endif
                .Join(srcValues, target => target.ToString().ToLower(), src => src.ToString().ToLower(), (target, _) => target);

        return targetValues;
    }
}