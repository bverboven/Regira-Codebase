# Codebase

## Libraries

### IO

Services to read, write or delete files from different storage types in a uniform way:
- FileSystem
- Azure
- SSH (FTP)
- GitHub (fetching only)

[Unit Tests](https://github.com/bverboven/Regira-Codebase/tree/master/tests/IO.Testing)

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
```

### Office

- Csv
  - CsvHelper
- Excel
  - EPPlus (deprecated)
  - NpoiMapper
- Mail 
  - Mailgun
  - SendGrid
- OCR
  - PaddleOCR
  - Tesseract
- PDF
  - DocNET
  - SelectPDF
  - (Free)Spire.PDF
- vCards
    - FolkerKinzel
- Word
  - (Free)Spire.Doc

### Drawing

Functions for cropping, resizing, rotating, flipping

- GDI (System.Drawing.Common)
- SkiaSharp

#### Barcodes

- QRCoder (create only)
- (Free)Spire.Barcode: create & write (limited types) of barcodes (including QR codes)
- ZXing: create & write multiple types of barcodes (including QR Codes)

## Tools

### ProjectFilesProcessor

A console application to synchronize versions of project (*.cproj) files and push them to a custom NuGet server.
1. First it builds a tree of project dependencies
1. If one of the parent projects has an increased version, the child projects' version will be updated
3. Then the versions of the NuGet packages are compared with the local projects versions
4. If needed the project package is pushed to the NuGet server

## Tests

Before synchronizing modified projects, better run the [unit tests](https://github.com/bverboven/Regira-Codebase/tree/master/tests) first.