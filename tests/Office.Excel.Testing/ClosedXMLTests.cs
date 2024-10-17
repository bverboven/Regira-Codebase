using Regira.Office.Excel.ClosedXML;

namespace Office.Excel.Testing;

[TestFixture]
[Parallelizable(ParallelScope.Self)]
public class ClosedXMLTests : ExcelTestsBase
{
    public ClosedXMLTests()
    {
        ExcelManager = new ExcelManager();
    }


    [Test]
    public Task List_To_Excel() => base.Run_List_To_Excel();
    [Test]
    public void Compare_DictionaryCollection_Input_With_Output() => base.Run_Compare_DictionaryCollection_Input_With_Output();
    [Test]
    public void Compare_UnTyped_Input_With_Output() => base.Run_Compare_UnTyped_Input_With_Output();
    [Test]
    public void Compare_Typed_Input_With_Output() => base.Run_Compare_Typed_Input_With_Output();
    [Test]
    public void Read_With_Duplicate_Headers() => base.Run_Read_With_Duplicate_Headers();
    [Test]
    public Task Export_Countries() => base.Run_Export_Countries();
    [Test]
    public Task Export_Countries_As_Sheet() => base.Run_Export_Countries_As_Sheet();

    [Test]
    public Task From_Json() => base.Run_From_Json();
}