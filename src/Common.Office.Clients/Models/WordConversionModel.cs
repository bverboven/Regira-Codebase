using Regira.Office.Models;

namespace Regira.Office.Clients.Models;

public class WordConversionModel
{
    public FileFormat OutputFormat { get; set; }
    public PageSize? PageSize { get; set; }
    public bool? AutoScaleTables { get; set; }
    public bool? AutoScalePictures { get; set; }
}