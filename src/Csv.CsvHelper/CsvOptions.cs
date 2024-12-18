using System.Globalization;

namespace Regira.Office.Csv.CsvHelper;

public class CsvOptions
{
    // ReSharper disable once StaticMemberInGenericType
    static readonly CultureInfo DEFAULT_CULTURE = CultureInfo.InvariantCulture;

    public string Delimiter = ",";
    public CultureInfo Culture { get; set; } = DEFAULT_CULTURE;
    public bool IgnoreBadData = false;
    public bool PreserveWhitespace = false;
}
