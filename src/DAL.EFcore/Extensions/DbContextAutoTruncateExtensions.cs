using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Regira.DAL.EFcore.Extensions;

public static class DbContextAutoTruncateExtensions
{

    /// <summary>
    /// Truncates all string properties with a <see cref="MaxLengthAttribute"/> for <see cref="EntityEntry">Entries</see> that have pending changes<br />
    /// Credits: https://gist.github.com/abrari/dfe772db172f950e9f0d8acdd3982fbb
    /// </summary>
    /// <param name="dbContext"></param>
    public static void AutoTruncateStringsToMaxLengthForEntries(this DbContext dbContext)
    {
        foreach (var entry in dbContext.GetPendingEntries())
        {
            if (entry.State != EntityState.Deleted)
            {
                entry.AutoTruncate();
            }
        }
    }
}