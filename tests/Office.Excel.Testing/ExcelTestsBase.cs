using Office.Excel.Testing.Models;
using Regira.IO.Abstractions;
using Regira.IO.Extensions;
using Regira.IO.Models;
using Regira.Office.Excel;
using Regira.Office.Excel.Abstractions;
using Regira.Utilities;
using System.Text.Json;

[assembly: Parallelizable(ParallelScope.Fixtures)]

namespace Office.Excel.Testing;

public interface IExcelTest
{
    string AssetsDir { get; }
}
public abstract class ExcelTestsBase
{
    public class TestObject
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public decimal Value { get; set; }
        public DateTime Created { get; set; }
    }

    protected readonly string AssetsDir;
    protected TestObject[] TestData = null!;
    protected IExcelManager ExcelManager { get; set; } = null!;
    protected ExcelTestsBase()
    {
        var assemblyDir = AssemblyUtility.GetAssemblyDirectory()!;
        AssetsDir = new DirectoryInfo(Path.Combine(assemblyDir, "../../../", "Assets")).FullName;
        Directory.CreateDirectory(AssetsDir);
    }

    [SetUp]
    public void Setup()
    {
        TestData = Enumerable.Range(0, 100)
            .Select((_, i) => new TestObject
            {
                Id = i + 1,
                Title = $"Item #{i + 1}",
                Value = 100 / (decimal)(i + 1),
                Created = DateTime.Now.AddDays(-(100 - i))
            })
            .ToArray();
        Directory.CreateDirectory(Path.Combine(AssetsDir, "Output"));
    }

    protected async Task Run_List_To_Excel()
    {
        using var excelFile = CreateExcel(TestData);
        Assert.Multiple(() =>
        {
            Assert.That(excelFile.GetBytes(), Is.Not.Null);
            Assert.That(excelFile.GetLength(), Is.GreaterThan(0));
        });
        var file = await SaveOutput("TestData.xlsx", excelFile);

        Assert.That(file.Exists, Is.True);
    }
    protected void Run_Compare_DictionaryCollection_Input_With_Output()
    {
        var dicList = TestData.Select(x => DictionaryUtility.ToDictionary(x)).ToList();
        using var excelFile = CreateExcel(dicList)
            .ToBinaryFile();
        _ = SaveOutput("DicList.xlsx", excelFile);
        var sheets = ExcelManager.Read(excelFile).ToList();
        var data = sheets[0].Data!.ToList();
        for (var i = 0; i < TestData.Length; i++)
        {
            var sourceItem = TestData[i];
            var excelItem = DictionaryUtility.ToDictionary(data[i]);
            Assert.Multiple(() =>
            {
                Assert.That(excelItem["Title"], Is.EqualTo(sourceItem.Title));
                Assert.That(Convert.ToDecimal(excelItem["Value"]) - sourceItem.Value, Is.LessThan(.000000001m));
            });
            var excelDate = (DateTime)excelItem["Created"]!;
            Assert.That((sourceItem.Created - excelDate), Is.LessThan(TimeSpan.FromMilliseconds(1)));
        }
    }
    protected void Run_Compare_UnTyped_Input_With_Output()
    {
        using var excelFile = CreateExcel(TestData)
            .ToBinaryFile();
        var sheets = ExcelManager.Read(excelFile).ToList();
        var data = sheets[0].Data!.ToList();
        for (var i = 0; i < TestData.Length; i++)
        {
            var sourceItem = TestData[i];
            var excelItem = DictionaryUtility.ToDictionary(data[i]);
            Assert.Multiple(() =>
            {
                Assert.That(excelItem["Title"], Is.EqualTo(sourceItem.Title));
                Assert.That(Convert.ToDecimal(excelItem["Value"]) - sourceItem.Value, Is.LessThan(.000000001m));
            });
            var excelDate = (DateTime)excelItem["Created"]!;
            Assert.That((sourceItem.Created - excelDate), Is.LessThan(TimeSpan.FromMilliseconds(1)));
        }
    }
    protected void Run_Compare_Typed_Input_With_Output()
    {
        using var excelFile = CreateExcel(TestData)
            .ToBinaryFile();

        var sheets = ExcelManager.Read(excelFile);
        var data = sheets.First().Data;
        var excelItems = JsonSerializer.Deserialize<TestObject[]>(JsonSerializer.Serialize(data))!.ToList();

        for (var i = 0; i < TestData.Length; i++)
        {
            var sourceItem = TestData[i];
            var excelItem = excelItems[i];
            Assert.Multiple(() =>
            {
                Assert.That(excelItem.Title, Is.EqualTo(sourceItem.Title));
                Assert.That(Math.Abs(sourceItem.Value - excelItem.Value), Is.LessThan(.000000001m));
            });
            var excelDate = excelItem.Created;
            Assert.That((sourceItem.Created - excelDate), Is.LessThan(TimeSpan.FromMilliseconds(1)));
        }
    }
    protected void Run_Read_With_Duplicate_Headers()
    {
        var inputPath = Path.Combine(AssetsDir, "Input", "input-with-duplicates.xlsx");
        using var inputFile = new BinaryFileItem { Path = inputPath };
        var sheets = ExcelManager.Read(inputFile).ToList();
        Assert.That(sheets, Is.Not.Empty);
        var data = sheets.First().Data!.ToList();
        Assert.That(data, Is.Not.Empty);
    }
    protected async Task Run_Export_Countries()
    {
        var outputPath = Path.Combine(AssetsDir, "Output", "countries-output.xlsx");
        if (File.Exists(outputPath))
        {
            File.Delete(outputPath);
        }

        var countriesJSON = await File.ReadAllTextAsync(Path.Combine(AssetsDir, "Input", "countries.json"));
        var countries = JsonSerializer.Deserialize<IList<Dictionary<string, object>>>(countriesJSON)!;
        using var excelFile = CreateExcel(countries);
        Assert.Multiple(() =>
        {
            Assert.That(excelFile.GetBytes(), Is.Not.Null);
            Assert.That(excelFile.GetLength() > 0, Is.True);
        });

        var file = await excelFile.SaveAs(outputPath);
        Assert.That(file.Exists, Is.True);
    }
    protected async Task Run_Export_Countries_As_Sheet()
    {
        var outputFile = Path.Combine(AssetsDir, "Output", "countries-as-sheet-output.xlsx");
        if (File.Exists(outputFile))
        {
            File.Delete(outputFile);
        }

        var countriesJSON = await File.ReadAllTextAsync(Path.Combine(AssetsDir, "Input", "countries-2.json"));
        var countries = JsonSerializer.Deserialize<IList<Dictionary<string, object>>>(countriesJSON)!;
        var sheet = new ExcelSheet
        {
            Name = "Countries",
            Data = countries.Cast<object>().ToList()
        };
        using var excelFile = ExcelManager.Create([sheet]);
        Assert.Multiple(() =>
        {
            Assert.That(excelFile.GetBytes(), Is.Not.Null);
            Assert.That(excelFile.GetLength() > 0, Is.True);
        });

        var file = await excelFile.SaveAs(outputFile);
        Assert.That(file.Exists, Is.True);
    }

    protected async Task Run_From_Json()
    {
        var countriesJSON = await File.ReadAllTextAsync(Path.Combine(AssetsDir, "Input", "countries.json"));
        var outputFile = Path.Combine(AssetsDir, "Output", "from_json.xlsx");

        var data = JsonSerializer.Deserialize<IList<Dictionary<string, object>>>(countriesJSON)!
            .Cast<object>()
            .ToList();
        using var excelFile = ExcelManager.Create(new List<ExcelSheet>
        {
            new () {Name = "Countries", Data = data}
        });
        Assert.Multiple(() =>
        {
            Assert.That(excelFile.GetBytes(), Is.Not.Null);
            Assert.That(excelFile.GetLength() > 0, Is.True);
        });

        var file = await excelFile.SaveAs(outputFile);
        Assert.That(file.Exists, Is.True);
    }

    protected IMemoryFile CreateExcel<T>(IEnumerable<T> input)
        where T : class
    {
        var sheet = new ExcelSheet { Data = input.Cast<object>().ToList() };
        return ExcelManager.Create([sheet]);
    }

    async Task<FileInfo> SaveOutput(string fileName, IMemoryFile file)
    {
        var path = Path.Combine(AssetsDir, "Output", fileName);
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        await file.SaveAs(path);

        return new FileInfo(path);
    }
}

public static class ExcelTestExtensions
{
    public static async Task Run_List_To_Excel<TService>(this TService service)
        where TService : IExcelManager<ExcelCountry>
    {
        var assetsDir = service.GetAssetsDir();
        var items = await ParseCountriesJson(assetsDir);
        var countries = items.ToExcelCountries();

        using var excelFile = service.CreateExcel(countries);

        Assert.Multiple(() =>
        {
            Assert.That(excelFile.GetBytes(), Is.Not.Null);
            Assert.That(excelFile.GetLength(), Is.GreaterThan(0));
        });
        var outputPath = Path.Combine(assetsDir, "Output", "TestData.xlsx");
        var file = await excelFile.SaveAs(outputPath);

        Assert.That(file.Exists, Is.True);
    }
    public static async Task Run_Compare_DictionaryCollection_Input_With_Output<TService>(this TService service)
        where TService : IExcelManager
    {
        var testData = CreateTestData();
        var dicList = testData.Select(x => DictionaryUtility.ToDictionary(x)).ToList();
        using var excelFile = service.CreateExcel(dicList)
            .ToBinaryFile();
        _ = await service.SaveExcel(excelFile, "DicList.xlsx");

        var sheets = service.Read(excelFile).ToList();
        var data = sheets[0].Data!.ToList();
        for (var i = 0; i < testData.Length; i++)
        {
            var sourceItem = testData[i];
            var excelItem = DictionaryUtility.ToDictionary(data[i]);
            Assert.Multiple(() =>
            {
                Assert.That(excelItem["Title"], Is.EqualTo(sourceItem.Title));
                Assert.That(Convert.ToDecimal(excelItem["Value"]) - sourceItem.Value, Is.LessThan(.000000001m));
            });
            var excelDate = (DateTime)excelItem["Created"]!;
            Assert.That((sourceItem.Created - excelDate), Is.LessThan(TimeSpan.FromMilliseconds(1)));
        }
    }
    public static void Run_Compare_UnTyped_Input_With_Output<TService>(this TService service)
        where TService : IExcelManager
    {
        var testData = CreateTestData();
        using var excelFile = service.CreateExcel(testData)
            .ToBinaryFile();
        var sheets = service.Read(excelFile).ToList();
        var data = sheets[0].Data!.ToList();
        for (var i = 0; i < testData.Length; i++)
        {
            var sourceItem = testData[i];
            var excelItem = DictionaryUtility.ToDictionary(data[i]);
            Assert.Multiple(() =>
            {
                Assert.That(excelItem["Title"], Is.EqualTo(sourceItem.Title));
                Assert.That(Convert.ToDecimal(excelItem["Value"]) - sourceItem.Value, Is.LessThan(.000000001m));
            });
            var excelDate = (DateTime)excelItem["Created"]!;
            Assert.That((sourceItem.Created - excelDate), Is.LessThan(TimeSpan.FromMilliseconds(1)));
        }
    }
    public static void Run_Compare_Typed_Input_With_Output<TService>(this TService service)
        where TService : IExcelManager
    {
        var testData = CreateTestData();
        using var excelFile = service.CreateExcel(testData)
            .ToBinaryFile();

        var sheets = service.Read(excelFile);
        var data = sheets.First().Data;
        var excelItems = JsonSerializer.Deserialize<TestObject[]>(JsonSerializer.Serialize(data))!.ToList();

        for (var i = 0; i < testData.Length; i++)
        {
            var sourceItem = testData[i];
            var excelItem = excelItems[i];
            Assert.Multiple(() =>
            {
                Assert.That(excelItem.Title, Is.EqualTo(sourceItem.Title));
                Assert.That(Math.Abs(sourceItem.Value - excelItem.Value), Is.LessThan(.000000001m));
            });
            var excelDate = excelItem.Created;
            Assert.That((sourceItem.Created - excelDate), Is.LessThan(TimeSpan.FromMilliseconds(1)));
        }
    }
    public static void Run_Read_With_Duplicate_Headers<TService>(this TService service)
        where TService : IExcelManager
    {
        var assetsDir = service.GetAssetsDir();
        var inputPath = Path.Combine(assetsDir, "Input", "input-with-duplicates.xlsx");
        using var inputFile = new BinaryFileItem { Path = inputPath };
        var sheets = service.Read(inputFile).ToList();
        Assert.That(sheets, Is.Not.Empty);
        var data = sheets.First().Data!.ToList();
        Assert.That(data, Is.Not.Empty);
    }

    public static async Task Run_Export_Countries_As_Dictionary<TService>(this TService service)
        where TService : IExcelManager
    {
        var assetsDir = service.GetAssetsDir();
        var countriesJSON = await File.ReadAllTextAsync(Path.Combine(assetsDir, "Input", "countries.json"));
        var countries = JsonSerializer.Deserialize<IList<Dictionary<string, object>>>(countriesJSON)!;
        using var excelFile = service.CreateExcel(countries);
        Assert.Multiple(() =>
        {
            Assert.That(excelFile.GetBytes(), Is.Not.Null);
            Assert.That(excelFile.GetLength() > 0, Is.True);
        });

        var file = await service.SaveExcel(excelFile, "countries-output.xlsx");
        Assert.That(file.Exists, Is.True);
    }
    public static async Task Run_Export_Countries<TService>(this TService service)
        where TService : IExcelManager<ExcelCountry>
    {
        var assetsDir = service.GetAssetsDir();
        var items = await ParseCountriesJson(assetsDir);
        var countries = items.ToExcelCountries();

        using var excelFile = service.CreateExcel(countries);
        Assert.Multiple(() =>
        {
            Assert.That(excelFile.GetBytes(), Is.Not.Null);
            Assert.That(excelFile.GetLength() > 0, Is.True);
        });

        var file = await service.SaveExcel(excelFile, "countries-output.xlsx");
        Assert.That(file.Exists, Is.True);
    }
    public static async Task Run_Export_Countries_As_Sheet<TService>(this TService service)
        where TService : IExcelManager
    {
        var assetsDir = service.GetAssetsDir();
        var countriesJSON = await File.ReadAllTextAsync(Path.Combine(assetsDir, "Input", "countries-2.json"));
        var countries = JsonSerializer.Deserialize<IList<Dictionary<string, object>>>(countriesJSON)!;
        var sheet = new ExcelSheet
        {
            Name = "Countries",
            Data = countries.Cast<object>().ToList()
        };
        using var excelFile = service.Create([sheet]);
        Assert.Multiple(() =>
        {
            Assert.That(excelFile.GetBytes(), Is.Not.Null);
            Assert.That(excelFile.GetLength() > 0, Is.True);
        });

        var file = await service.SaveExcel(excelFile, "countries-as-sheet-output.xlsx");
        Assert.That(file.Exists, Is.True);
    }
    
    public static async Task Run_From_Json<TService>(this TService service)
        where TService : IExcelManager
    {
        var assetsDir = service.GetAssetsDir();
        var countriesJSON = await File.ReadAllTextAsync(Path.Combine(assetsDir, "Input", "countries.json"));

        var data = JsonSerializer.Deserialize<IList<Dictionary<string, object>>>(countriesJSON)!
            .Cast<object>()
            .ToList();
        using var excelFile = service.Create(new List<ExcelSheet>
        {
            new () {Name = "Countries", Data = data}
        });
        Assert.Multiple(() =>
        {
            Assert.That(excelFile.GetBytes(), Is.Not.Null);
            Assert.That(excelFile.GetLength() > 0, Is.True);
        });
        
        var file = await service.SaveExcel(excelFile, "from_json.xlsx");
        Assert.That(file.Exists, Is.True);
    }

    internal static string CountriesJsonInputPath(string assetsDir) => Path.Combine(assetsDir, "Input", "countries.json");
    internal static async Task<IList<Country>> ParseCountriesJson(string assetsDir)
    {
        var jsonPath = CountriesJsonInputPath(assetsDir);
        var json = await File.ReadAllTextAsync(jsonPath);
        return JsonSerializer.Deserialize<List<Country>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }
    internal static IMemoryFile CreateExcel<TService>(this TService service, IEnumerable<object> input)
        where TService : IExcelManager
    {
        var sheet = new ExcelSheet { Data = input.ToList() };
        return service.Create([sheet]);
    }
    internal static IMemoryFile CreateExcel<TService>(this TService service, IEnumerable<ExcelCountry> input)
        where TService : IExcelManager<ExcelCountry>
    {
        var sheet = new ExcelSheet<ExcelCountry> { Data = input.ToList() };
        return service.Create([sheet]);
    }

    internal static string GetAssetsDir<TService>(this TService service)
        where TService : IExcelService
    {
        var assemblyDir = AssemblyUtility.GetAssemblyDirectory()!;
        var assetsDir = new DirectoryInfo(Path.Combine(assemblyDir, "../../../", "Assets")).FullName;
        Directory.CreateDirectory(assetsDir);

        return assetsDir;
    }
    internal static Task<FileInfo> SaveExcel<TService>(this TService service, IMemoryFile excelFile, string fileName)
        where TService : IExcelService
    {
        var assetsDir = service.GetAssetsDir();
        var serviceNamespace = service.GetType().Namespace!.Split(".").Last();
        var outputDir = Path.Combine(assetsDir, "Output", serviceNamespace);
        var outputPath = Path.Combine(outputDir, fileName);
        Directory.CreateDirectory(outputDir);
        return excelFile.SaveAs(outputPath);
    }
    internal static TestObject[] CreateTestData() => Enumerable.Range(0, 100)
        .Select((_, i) => new TestObject
        {
            Id = i + 1,
            Title = $"Item #{i + 1}",
            Value = 100 / (decimal)(i + 1),
            Created = DateTime.Now.AddDays(-(100 - i))
        })
        .ToArray();
}