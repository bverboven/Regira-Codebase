using PuppeteerSharp;
using Regira.IO.Abstractions;
using Regira.IO.Extensions;
using Regira.Office.PDF.Abstractions;
using Regira.Office.PDF.Models;

namespace Regira.Office.PDF.Puppeteer;

public class PdfManager : IHtmlToPdfService
{
    public async Task<IMemoryFile> Create(HtmlInput template)
    {
        await new BrowserFetcher().DownloadAsync();
        var browser = await PuppeteerSharp.Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true
        });
        var page = await browser.NewPageAsync();
        await page.SetContentAsync(template.HtmlContent);

        var pdfStream = await page.PdfStreamAsync(new PdfOptions { });

        return pdfStream.ToMemoryFile();
    }
}