using CsvHelper;
using CsvHelper.Configuration;
using Regira.IO.Abstractions;
using Regira.IO.Extensions;
using Regira.Office.Csv.Abstractions;
using Regira.Office.MimeTypes;
using Regira.Utilities;
using System.Globalization;

namespace Regira.Office.Csv.CsvHelper;

public class CsvManager : CsvManager<IDictionary<string, object>>, ICsvManager
{
    public CsvManager(Options? options = null)
        : base(options)
    {
    }

    protected internal override async Task<List<IDictionary<string, object>>> Read(TextReader tr)
    {
        using var csvReader = GetReader(tr);

        // headers
        var keys = new List<string>();
        await csvReader.ReadAsync();
        int numberOfKeys = 0;
        while (csvReader.TryGetField<string>(numberOfKeys++, out var header))
        {
            keys.Add(header!);
        }

        var items = new List<IDictionary<string, object>>();

        while (await csvReader.ReadAsync())
        {
            var dic = new Dictionary<string, object>();
            for (var i = 0; i < numberOfKeys; i++)
            {
                if (csvReader.TryGetField<string>(i, out var value))
                {
                    var key = keys[i];
                    dic[key] = value!;
                }
            }
            items.Add(dic);
        }

        return items;
    }
    protected internal override async Task Write(TextWriter tw, IEnumerable<IDictionary<string, object>> items)
    {
#if NETSTANDARD2_0
            using var csvWriter = GetWriter(tw);
#else
        await using var csvWriter = GetWriter(tw);
#endif

        var itemList = items.AsList();
        var keys = itemList.SelectMany(x => x.Keys).Distinct().ToArray();

        foreach (var key in keys)
        {
            csvWriter.WriteField(key);
        }
        foreach (var item in itemList)
        {
            await csvWriter.NextRecordAsync();
            foreach (var key in keys)
            {
                csvWriter.WriteField(item.ContainsKey(key) ? item[key] : string.Empty);
            }
        }
    }
}
public class CsvManager<T> : ICsvManager<T>
{

    // ReSharper disable once StaticMemberInGenericType
    static readonly CultureInfo DEFAULT_CULTURE = CultureInfo.InvariantCulture;
    public class Options
    {
        public string Delimiter = ",";
        public CultureInfo Culture { get; set; } = DEFAULT_CULTURE;
        public bool IgnoreBadData = false;
        public bool PreserveWhitespace = false;
    }

    protected readonly Options CsvOptions;
    public CsvManager(Options? options = null)
    {
        CsvOptions = options ?? new Options();
    }

    public async Task<List<T>> Read(string input)
    {
        using var sr = new StringReader(input);
        return await Read(sr);
    }
    public async Task<List<T>> Read(IBinaryFile input)
    {
        using var ms = input.GetStream();
        if (ms == null)
        {
            throw new Exception("Could not get contents of file");
        }

        using var sr = new StreamReader(ms);
        return await Read(sr);
    }
    protected internal virtual async Task<List<T>> Read(TextReader tr)
    {
        using var csvReader = GetReader(tr);

        var records = csvReader.GetRecordsAsync<T>();
        return await records.ToListAsync();
    }


    public async Task<string> Write(IEnumerable<T> items)
    {
#if NETSTANDARD2_0
            using var sw = new StringWriter();
#else
        await using var sw = new StringWriter();
#endif
        await Write(sw, items);
        return sw.ToString();
    }
    public async Task<IMemoryFile> WriteFile(IEnumerable<T> items)
    {
        // create a MemoryStream to avoid error "Cannot access a closed Stream"
#if NETSTANDARD2_0
            var ms = new MemoryStream();
            var sw = new StreamWriter(ms);
#else
        var ms = new MemoryStream();
        var sw = new StreamWriter(ms);
#endif
        await Write(sw, items);

        ms.Seek(0, SeekOrigin.Begin);
        return ms.ToMemoryFile(ContentTypes.CSV);
    }
    protected internal virtual async Task Write(TextWriter tw, IEnumerable<T> items)
    {
#if NETSTANDARD2_0
            using var csvWriter = GetWriter(tw);
#else
        await using var csvWriter = GetWriter(tw);

#endif
        await csvWriter.WriteRecordsAsync(items);
    }

    protected CsvReader GetReader(TextReader tr) => new(tr, GetConfiguration(), true);
    protected CsvWriter GetWriter(TextWriter tw) => new(tw, GetConfiguration(), true);
    protected CsvConfiguration GetConfiguration()
    {
        var config = new CsvConfiguration(CsvOptions.Culture)
        {
            TrimOptions = CsvOptions.PreserveWhitespace ? TrimOptions.None : TrimOptions.Trim,
            Delimiter = CsvOptions.Delimiter
        };

        if (CsvOptions.IgnoreBadData)
        {
            config.BadDataFound = null;
        }

        return config;
    }
}