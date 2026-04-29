namespace Regira.Office.PDF.Models.DTO;

public static class DtoExtensions
{
    public static HtmlToPdfInputDto ToHtmlToPdfInput(this HtmlInput input)
        => new()
        {
            HtmlContent = input.HtmlContent ?? string.Empty,
            Orientation = input.Orientation,
            Format = input.Format,
            Margins = [input.Margins.Top, input.Margins.Right, input.Margins.Bottom, input.Margins.Left],
            HeaderHtmlContent = input.HeaderHtmlContent,
            HeaderHeight = input.HeaderHeight ?? 0,
            FooterHtmlContent = input.FooterHtmlContent,
            FooterHeight = input.FooterHeight ?? 0
        };

    public static PdfToImagesOptionsDto ToPdfImagesInput(this PdfToImagesOptions input)
    {
        return new PdfToImagesOptionsDto
        {
            Width = input.Size?.Width,
            Height = input.Size?.Height,
            Format = input.Format
        };
    }
}