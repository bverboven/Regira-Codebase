using Regira.Office.Excel.EPPlus;

namespace Office.Excel.Testing;

[TestFixture]
[Parallelizable(ParallelScope.Self)]
public class EPPlustTests : ExcelTestsBase
{
    public EPPlustTests()
    {
        ExcelManager = new ExcelManager();
    }
}