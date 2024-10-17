using Regira.IO.Abstractions;
using Regira.IO.Extensions;
using Regira.IO.Models;
using Regira.Office.Excel;
using Regira.Office.Excel.Abstractions;
using Regira.Utilities;
using System.Text.Json;

[assembly: Parallelizable(ParallelScope.Fixtures)]

namespace Office.Excel.Testing;

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

        var countriesJSON = File.ReadAllText(Path.Combine(AssetsDir, "Input", "countries.json"));
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

        var countriesJSON = File.ReadAllText(Path.Combine(AssetsDir, "Input", "countries-2.json"));
        var countries = JsonSerializer.Deserialize<IList<Dictionary<string, object>>>(countriesJSON)!;
        var sheet = new ExcelSheet
        {
            Name = "Countries",
            Data = countries.Cast<object>().ToList()
        };
        using var excelFile = ExcelManager.Create(new[] { sheet });
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
        var countriesJSON = File.ReadAllText(Path.Combine(AssetsDir, "Input", "countries.json"));
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
        return ExcelManager.Create(sheet);
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