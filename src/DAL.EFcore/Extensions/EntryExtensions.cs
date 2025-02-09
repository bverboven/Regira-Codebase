using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Regira.Utilities;

namespace Regira.DAL.EFcore.Extensions;

#if NETCOREAPP3_1_OR_GREATER
#endif
public static class EntryExtensions
{
    /// <summary> 
    /// Truncates all string properties with a <see cref="MaxLengthAttribute"/> for <see cref="EntityEntry">Entries</see><br />
    /// Credits: https://gist.github.com/abrari/dfe772db172f950e9f0d8acdd3982fbb
    /// </summary>
    /// <param name="entry"></param>
    public static void AutoTruncate(this EntityEntry entry)
    {
        var propertiesWithMaxLength = entry.GetPropertyAttributes()
            .Where(d => d.Value.Any(a => a is MaxLengthAttribute || a is StringLengthAttribute))
            .ToArray();

        foreach (var propWithMaxLength in propertiesWithMaxLength)
        {
            var propName = propWithMaxLength.Key.Name;
            var value = entry.CurrentValues[propName]?.ToString();
            if (value != null)
            {
                var maxLengthAnnotation = propWithMaxLength.Value.FirstOrDefault(x => x is MaxLengthAttribute) as MaxLengthAttribute;
                var stringLengthAnnotation = propWithMaxLength.Value.FirstOrDefault(x => x is StringLengthAttribute) as StringLengthAttribute;
                var maxLength = Convert.ToInt32(maxLengthAnnotation?.Length ?? stringLengthAnnotation!.MaximumLength);
                entry.CurrentValues[propName] = value.Truncate(maxLength);
            }
        }
    }
}