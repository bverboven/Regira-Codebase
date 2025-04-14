namespace Regira.Office.Excel.Abstractions;

public interface IExcelManager : IExcelReader, IExcelWriter;
public interface IExcelManager<T> : IExcelReader<T>, IExcelWriter<T>
    where T : class;