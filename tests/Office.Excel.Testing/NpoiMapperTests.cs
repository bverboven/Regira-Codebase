using Regira.Office.Excel.NpoiMapper;

namespace Office.Excel.Testing;

[TestFixture]
[Parallelizable(ParallelScope.Self)]
public class NpoiMapperTests : ExcelTestsBase
{
    public NpoiMapperTests()
    {
        ExcelManager = new ExcelManager();
    }
}