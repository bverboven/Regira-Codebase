namespace Regira.Office.Excel.Abstractions;

public interface IExcelService;
public interface IExcelManager : IExcelService, IExcelReader, IExcelWriter;
public interface IExcelManager<T> : IExcelService, IExcelReader<T>, IExcelWriter<T>
    where T : class, new();