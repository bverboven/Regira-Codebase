using Regira.Media.Drawing.Models.DTO;
using Regira.Office.Barcodes.Drawing;
using Regira.Office.PDF.Drawing;

namespace Regira.Office.Clients.Models;

public class DrawImageLayerDto
{
    public class ImageLayerSelectorDto
    {
        public ImageLayerDto? Image { get; set; }
        public CanvasImageLayerDto? Canvas { get; set; }
        public LabelImageLayerDto? Label { get; set; }
        public BarcodeImageLayerDto? Barcode { get; set; }
        public PdfImageLayerDto? Pdf { get; set; }
        public AggregateInputDto? Aggregate { get; set; }
    }

    public CanvasImageLayerDto? TargetCanvas { get; set; }
    public byte[]? TargetImage { get; set; }
    public ImageLayerSelectorDto[] Items { get; set; } = null!;
}