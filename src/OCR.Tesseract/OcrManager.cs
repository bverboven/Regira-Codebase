using Regira.IO.Abstractions;
using Regira.IO.Extensions;
using Tesseract;

namespace Regira.Office.OCR.Tesseract;

public class OcrManager(OcrManager.Options? options = null)
{
    public class Options
    {
        /// <summary>
        /// 3 letter code ISO language
        /// </summary>
        public string Language { get; set; } = "eng";
        /// <summary>
        /// Put language models in this directory
        /// Download more models at https://github.com/tesseract-ocr/tessdata/
        /// </summary>
        public string DataDirectory { get; set; } = "./tessdata";
    }

    public Task<string?> Read(IMemoryFile imgFile)
    {
        options ??= new Options();
        using var engine = new TesseractEngine(options.DataDirectory, options.Language.ToLower(), EngineMode.Default);
        using var img = Pix.LoadFromMemory(imgFile.GetBytes());
        var result = engine.Process(img);
        var text = result.GetText();

        return Task.FromResult<string?>(text);
    }
}