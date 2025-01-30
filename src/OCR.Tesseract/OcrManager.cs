using Regira.IO.Abstractions;
using Regira.IO.Extensions;
using Regira.Office.OCR.Abstractions;
using Tesseract;

namespace Regira.Office.OCR.Tesseract;

public class OcrManager : IOcrService
{
    public class Options
    {
        /// <summary>
        /// 3 letter code ISO language
        /// </summary>
        public string Language { get; set; } = "en";
        /// <summary>
        /// Put language models in this directory
        /// Download more models at https://github.com/tesseract-ocr/tessdata/
        /// </summary>
        public string DataDirectory { get; set; } = "./tessdata";
    }

    private readonly string _lang;
    private readonly string _dataDirectory;
    public OcrManager(Options? options = null)
    {
        options ??= new Options();
        _lang = options.Language;
        _dataDirectory = options.DataDirectory;
    }


    public Task<string?> Read(IMemoryFile imgFile, string? lang = null)
    {
        using var engine = new TesseractEngine(_dataDirectory, ConvertLang(lang ?? _lang), EngineMode.Default);
        using var img = Pix.LoadFromMemory(imgFile.GetBytes());
        var result = engine.Process(img);
        var text = result.GetText();

        return Task.FromResult<string?>(text);
    }

    public string ConvertLang(string? lang)
    {
        return (lang?.ToLower()) switch
        {
            "nl" => "nld",
            _ => "eng",
        };
    }
}