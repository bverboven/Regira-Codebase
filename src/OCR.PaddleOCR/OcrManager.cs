using OpenCvSharp;
using Regira.IO.Abstractions;
using Regira.IO.Extensions;
using Sdcb.PaddleInference;
using Sdcb.PaddleOCR;
using Sdcb.PaddleOCR.Models;
using Sdcb.PaddleOCR.Models.LocalV3;

namespace Regira.Office.OCR.PaddleOCR;

public class OcrManager
{
    public Task<string?> Read(IMemoryFile imgFile)
    {
        FullOcrModel model = LocalFullModels.EnglishV3;
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
}