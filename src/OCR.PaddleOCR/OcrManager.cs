using OpenCvSharp;
using Regira.IO.Abstractions;
using Regira.IO.Extensions;
using Regira.Office.OCR.Abstractions;
using Sdcb.PaddleInference;
using Sdcb.PaddleOCR;
using Sdcb.PaddleOCR.Models;
using Sdcb.PaddleOCR.Models.LocalV3;

namespace Regira.Office.OCR.PaddleOCR;

public class OcrManager : IOcrService
{
    public Task<string?> Read(IMemoryFile imgFile, string? lang = null)
    {
        FullOcrModel model = ConvertLang(lang);
        using PaddleOcrAll all = new PaddleOcrAll(model, PaddleDevice.Mkldnn())
        {
            AllowRotateDetection = false,//true,
            Enable180Classification = false,
        };
        var bytes = imgFile.GetBytes();
        using Mat src = Cv2.ImDecode(bytes!, ImreadModes.Color);
        PaddleOcrResult result = all.Run(src);
        var content = result.Text;
        return Task.FromResult<string?>(content);
    }


    public FullOcrModel ConvertLang(string? lang = null)
    {
        return (lang?.ToLower()) switch
        {
            //"nl" => LocalFullModels.LatinV3,
            "cn" => LocalFullModels.ChineseV3,
            _ => LocalFullModels.EnglishV3,
        };
    }
}