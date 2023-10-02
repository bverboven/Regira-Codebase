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

    [Test]
    public virtual async Task List_To_Excel()
    {
        var outputFile = Path.Combine(AssetsDir, "Output", "output.xlsx");
        if (File.Exists(outputFile))
        {
            File.Delete(outputFile);
        }

        using var excelFile = CreateExcel(TestData);
        Assert.IsNotNull(excelFile.GetBytes());
        Assert.IsTrue(excelFile.GetLength() > 0);
        var file = await excelFile.SaveAs(outputFile);

        Assert.IsTrue(file.Exists);
    }
    [Test]
    public virtual void Compare_DictionaryCollection_Input_With_Output()
    {
        var dicList = TestData.Select(x => DictionaryUtility.ToDictionary(x)).ToList();
        using var excelFile = CreateExcel(dicList)
            .ToBinaryFile();
        var sheets = ExcelManager.Read(excelFile).ToList();
        var data = sheets[0].Data!.ToList();
        for (var i = 0; i < TestData.Length; i++)
        {
            var sourceItem = TestData[i];
            var excelItem = DictionaryUtility.ToDictionary(data[i]);
            Assert.That(excelItem["Title"], Is.EqualTo(sourceItem.Title));
            Assert.That(excelItem["Value"], Is.EqualTo(sourceItem.Value));
            var excelDate = (DateTime)excelItem["Created"]!;
            Assert.IsTrue((sourceItem.Created - excelDate) < TimeSpan.FromMilliseconds(1));
        }
    }
    [Test]
    public virtual void Compare_UnTyped_Input_With_Output()
    {
        using var excelFile = CreateExcel(TestData)
            .ToBinaryFile();
        var sheets = ExcelManager.Read(excelFile).ToList();
        var data = sheets[0].Data!.ToList();
        for (var i = 0; i < TestData.Length; i++)
        {
            var sourceItem = TestData[i];
            var excelItem = DictionaryUtility.ToDictionary(data[i]);
            Assert.That(excelItem["Title"], Is.EqualTo(sourceItem.Title));
            Assert.That(excelItem["Value"], Is.EqualTo(sourceItem.Value));
            var excelDate = (DateTime)excelItem["Created"]!;
            Assert.IsTrue((sourceItem.Created - excelDate) < TimeSpan.FromMilliseconds(1));
        }
    }
    [Test]
    public virtual void Compare_Typed_Input_With_Output()
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
            Assert.That(excelItem.Title, Is.EqualTo(sourceItem.Title));
            Assert.IsTrue(Math.Abs(sourceItem.Value - excelItem.Value) < .000000001m);
            var excelDate = excelItem.Created;
            Assert.IsTrue((sourceItem.Created - excelDate) < TimeSpan.FromMilliseconds(1));
        }
    }
    [Test]
    public virtual void Read_With_Duplicate_Headers()
    {
        var inputPath = Path.Combine(AssetsDir, "Input", "input-with-duplicates.xlsx");
        using var inputFile = new BinaryFileItem { Path = inputPath };
        var sheets = ExcelManager.Read(inputFile).ToList();
        CollectionAssert.IsNotEmpty(sheets);
        var data = sheets.First().Data!.ToList();
        CollectionAssert.IsNotEmpty(data);
    }
    [Test]
    public virtual async Task Export_Countries()
    {
        var outputPath = Path.Combine(AssetsDir, "Output", "countries-output.xlsx");
        if (File.Exists(outputPath))
        {
            File.Delete(outputPath);
        }

        var countriesJSON = File.ReadAllText(Path.Combine(AssetsDir, "Input", "countries.json"));
        var countries = JsonSerializer.Deserialize<IList<Dictionary<string, object>>>(countriesJSON)!;
        using var excelFile = CreateExcel(countries);
        Assert.IsNotNull(excelFile.GetBytes());
        Assert.IsTrue(excelFile.GetLength() > 0);

        var file = await excelFile.SaveAs(outputPath);
        Assert.IsTrue(file.Exists);
    }
    [Test]
    public virtual async Task Export_Countries_As_Sheet()
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
        Assert.IsNotNull(excelFile.GetBytes());
        Assert.IsTrue(excelFile.GetLength() > 0);

        var file = await excelFile.SaveAs(outputFile);
        Assert.IsTrue(file.Exists);
    }

    [Test]
    public virtual async Task From_Json()
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
        Assert.IsNotNull(excelFile.GetBytes());
        Assert.IsTrue(excelFile.GetLength() > 0);

        var file = await excelFile.SaveAs(outputFile);
        Assert.IsTrue(file.Exists);
    }

    public IMemoryFile CreateExcel<T>(IEnumerable<T> input)
        where T : class
    {
        var sheet = new ExcelSheet { Data = input.Cast<object>().ToList() };
        return ExcelManager.Create(sheet);
    }
}