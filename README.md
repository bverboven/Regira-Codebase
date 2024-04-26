# Codebase

## Libraries

### IO

Services to read, write or delete files from different storage types in a uniform way:
- FileSystem
- Azure
- SSH (FTP)
- GitHub (fetching only)

<img src="https://nunit.org/img/nunit.svg" height="16" /> [Unit Tests](https://github.com/bverboven/Regira-Codebase/tree/master/tests/IO.Testing)

Sample
```c#
services.AddTransient<IFileService>(_ => {
    // FileSystem
    return new BinaryFileService(
        new BinaryFileService.FileServiceOptions { 
            RootFolder = ROOT_FOLDER 
        }
    );
    // Azure
    return new BinaryBlobService(
        new AzureCommunicator(
            new AzureConfig
            {
                ConnectionString = AZURE_CONNECTION_STRING,
                ContainerName = BLOB_CONTAINER
            }
        )
    );
    // GitHub
    return new GitHubService(
        new GitHubOptions {
                Uri = GITHUB_URI_,
                // Key = TOKEN_IF_REQUIRED
        },
        new Regira.Serializing.Newtonsoft.Json.JsonSerializer()
    );
})

IFileService fileService = sp.GetRequiredService<IFileService>();
IEnumerable<string> files = await fileService.List();
byte[]? bytes = await fileService.GetBytes(identifier);
```

### Office

- Csv - [Tests](https://github.com/bverboven/Regira-Codebase/blob/master/tests/Office.Csv.Testing)
  - [CsvHelper](https://github.com/bverboven/Regira-Codebase/tree/master/src/Csv.CsvHelper)
- Barcodes - [Tests](https://github.com/bverboven/Regira-Codebase/tree/master/tests/Office.Barcodes.Testing)
  - [QRCoder](https://github.com/bverboven/Regira-Codebase/tree/master/src/Barcodes.QRCoder) (create only)
  - [(Free)Spire.Barcode](https://github.com/bverboven/Regira-Codebase/tree/master/src/Barcodes.Spire): create & write (limited types) of barcodes (including QR codes)
  - [UziGranot](https://github.com/bverboven/Regira-Codebase/tree/master/src/Barcodes.UziGranot)
  - [ZXing.NET](https://github.com/bverboven/Regira-Codebase/tree/master/src/Barcodes.ZXing): create & write multiple types of barcodes (including QR Codes)
- Excel - [Tests](https://github.com/bverboven/Regira-Codebase/blob/master/tests/Office.Excel.Testing)
  - [EPPlus](https://github.com/bverboven/Regira-Codebase/tree/master/src/Excel.EPPlus)
  - [NpoiMapper](https://github.com/bverboven/Regira-Codebase/tree/master/src/Excel.NpoiMapper)
- Mail - [Tests](https://github.com/bverboven/Regira-Codebase/tree/master/tests/Office.Mail.Testing)
  - [Mailgun](https://github.com/bverboven/Regira-Codebase/tree/master/src/Mail.MailGun)
  - [SendGrid](https://github.com/bverboven/Regira-Codebase/tree/master/src/Mail.SendGrid)
- OCR - [Tests](https://github.com/bverboven/Regira-Codebase/tree/master/tests/Office.OCR.Testing)
  - [PaddleOCR](https://github.com/bverboven/Regira-Codebase/tree/master/src/OCR.PaddleOCR)
  - [Tesseract](https://github.com/bverboven/Regira-Codebase/tree/master/src/OCR.Tesseract)
- PDF - [Tests](https://github.com/bverboven/Regira-Codebase/tree/master/tests/Office.PDF.Testing)
  - [DocNET](https://github.com/bverboven/Regira-Codebase/tree/master/src/PDF.DocNET)
  - [SelectPDF](https://github.com/bverboven/Regira-Codebase/tree/master/src/PDF.SelectPdf)
  - [(Free)Spire.PDF](https://github.com/bverboven/Regira-Codebase/tree/master/src/PDF.Spire)
- vCards - [Tests](https://github.com/bverboven/Regira-Codebase/tree/master/tests/Office.VCards.Testing)
    - [FolkerKinzel](https://github.com/bverboven/Regira-Codebase/tree/master/src/VCards.FolkerKinzel)
- Word - [Tests](https://github.com/bverboven/Regira-Codebase/tree/master/tests/Office.Word.testing)
  - [(Free)Spire.Doc](https://github.com/bverboven/Regira-Codebase/tree/master/src/Word.Spire)

### Drawing

Functions for cropping, resizing, rotating, flipping

- [GDI](https://github.com/bverboven/Regira-Codebase/tree/master/src/Drawing.GDI) (System.Drawing.Common)
- [SkiaSharp](https://github.com/bverboven/Regira-Codebase/tree/master/src/Drawing.SkiaSharp)

<img src="https://nunit.org/img/nunit.svg" height="16" /> [Unit Tests](https://github.com/bverboven/Regira-Codebase/tree/master/tests/Drawing.Testing)

## Tools

### ProjectFilesProcessor

A console application to synchronize versions of project (*.cproj) files and push them to a custom NuGet server.
1. A tree of project dependencies is built
1. If one of the parent projects has an increased version, the child projects' version will be updated
3. Then the versions of the NuGet packages are compared with the local projects versions
4. When needed, the project package is pushed to the NuGet server

## Tests

Before synchronizing modified projects, better run the <img src="https://nunit.org/img/nunit.svg" height="16" /> [Unit Tests](https://github.com/bverboven/Regira-Codebase/tree/master/tests) first.