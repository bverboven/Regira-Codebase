using Regira.Office.Excel.Abstractions;
using Regira.Office.Excel.EPPlus;

namespace Office.Excel.Testing;

[TestFixture]
[Parallelizable(ParallelScope.Self)]
public class EPPlustTests
{
    IExcelManager CreateExcelManager() => new ExcelManager();
    //IExcelManager<ExcelCountry> CreateTypedExcelManager() => new ExcelManager<ExcelCountry>();

    //[Test]
    //public Task List_To_Excel() => CreateTypedExcelManager()
    //    .Run_List_To_Excel();
    [Test]
    public Task Compare_DictionaryCollection_Input_With_Output() => CreateExcelManager()
        .Run_Compare_DictionaryCollection_Input_With_Output();
    [Test]
    public void Compare_UnTyped_Input_With_Output() => CreateExcelManager()
        .Run_Compare_UnTyped_Input_With_Output();
    [Test]
    public void Compare_Typed_Input_With_Output() => CreateExcelManager()
        .Run_Compare_Typed_Input_With_Output();
    [Test]
    public void Read_With_Duplicate_Headers() => CreateExcelManager()
        .Run_Read_With_Duplicate_Headers();
    [Test]
    public Task Export_Countries_As_Dictionary() => CreateExcelManager()
        .Run_Export_Countries_As_Dictionary();

    //[Test]
    //public Task Export_Countries() => CreateTypedExcelManager()
    //    .Run_Export_Countries();

    [Test]
    public Task Export_Countries_As_Sheet() => CreateExcelManager()
        .Run_Export_Countries_As_Sheet();

    [Test]
    public Task From_Json() => CreateExcelManager()
        .Run_From_Json();
}