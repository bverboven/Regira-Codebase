using Regira.IO.Abstractions;
using Regira.IO.Extensions;
using Tesseract;

namespace Regira.Office.OCR.Tesseract;

public class OcrManager
{
    public class Options
    {
        /// <summary>
        /// 3 letter code ISO language
        /// </summary>
        public string Language { get; set; } = "eng";

        public string DataDirectory { get; set; } = @"./tessdata";
    }


    private readonly Options _options;
    public OcrManager(Options? options = null)
    {
        _options = options ?? new Options();
    }


    public Task<string?> Read(IMemoryFile imgFile)
    {
        using var engine = new TesseractEngine(_options.DataDirectory, _options.Language.ToLower(), EngineMode.Default);
        using var img = Pix.LoadFromMemory(imgFile.GetBytes());
        var result = engine.Process(img);
        var text = result.GetText();

        return Task.FromResult<string?>(text);
    }
}