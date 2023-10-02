using Microsoft.EntityFrameworkCore;

namespace Regira.DAL.EFcore.Extensions;

public static class ModelBuilderExtensions
{
    /// <summary>
    /// Sets the given precision and scale for all decimal properties<br />
    /// From EF core 6 possible by overriding <see cref="DbContext.ConfigureConventions"/> method
    /// </summary>
    /// <param name="modelBuilder"></param>
    /// <param name="precision"></param>
    /// <param name="scale"></param>
    public static void SetDecimalPrecisionConvention(this ModelBuilder modelBuilder, int precision = 18, int scale = 4)
    {
        // https://stackoverflow.com/questions/43277154/entity-framework-core-setting-the-decimal-precision-and-scale-to-all-decimal-p#answer-43282620
        var entityTypes = modelBuilder.Model
            .GetEntityTypes()
            .ToArray();

        var decimalColumnType = $"decimal({precision}, {scale})";
        var properties = entityTypes
            .SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?));

        foreach (var property in properties)
        {
            property.SetColumnType(decimalColumnType);
        }
    }

#if NET6_0_OR_GREATER
    /// <summary>
    /// Sets the given precision and scale for all decimal properties<br />
    /// </summary>
    /// <param name="configurationBuilder"></param>
    /// <param name="precision"></param>
    /// <param name="scale"></param>
    public static void SetDecimalPrecisionConvention(this ModelConfigurationBuilder configurationBuilder, int precision = 18, int scale = 4)
        => configurationBuilder.Properties<decimal>().HavePrecision(precision, scale);
#endif
}