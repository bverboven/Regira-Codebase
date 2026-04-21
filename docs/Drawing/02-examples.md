# Regira Drawing — Examples

## Example 1: Thumbnail

Resize an uploaded image to a bounded thumbnail and convert to WebP.

```csharp
public byte[] CreateThumbnail(byte[] input, int maxSize = 200)
{
    using var image   = imageService.Parse(input)!;
    using var resized = imageService.Resize(image, new ImageSize(maxSize, maxSize));
    using var webp    = imageService.ChangeFormat(resized, ImageFormat.Webp);
    return webp.Bytes!;
}
```

---

## Example 2: Watermark

Composite a diagonal text stamp and a logo over an existing photo.

```csharp
public IImageFile AddWatermark(IImageFile photo, string watermarkText)
{
    var stamp = new ImageLayer<LabelImageOptions>
    {
        Source = new LabelImageOptions
        {
            Text            = watermarkText,
            FontSize        = 20,
            TextColor       = "#FFFFFFFF",
            BackgroundColor = "#00000060",
            Padding         = 6
        },
        Options = new ImageLayerOptions
        {
            Position = ImagePosition.HCenter | ImagePosition.VCenter,
            Rotation = -30,
            Opacity  = 0.5f
        }
    };

    var logo = new ImageLayer
    {
        Source  = LoadLogo(),
        Options = new ImageLayerOptions
        {
            Position = ImagePosition.Right | ImagePosition.Bottom,
            Margin   = 10,
            Size     = new ImageSize(80, 80)
        }
    };

    return new ImageBuilder(imageService, imageCreators)
        .SetBaseLayer(photo)
        .Add(stamp, logo)
        .Build();
}
```

---

## Example 3: Badge Builder

Compose a name badge from scratch: a coloured canvas, an avatar photo positioned top-left, and a name label.

```csharp
public IImageFile BuildBadge(string name, IImageFile avatar)
{
    var avatarLayer = new ImageLayer
    {
        Source  = avatar,
        Options = new ImageLayerOptions
        {
            Size     = new ImageSize(120, 120),
            Position = ImagePosition.Absolute,
            Offset   = new ImageEdgeOffset(top: 15, left: 15)
        }
    };

    var nameLabel = new ImageLayer<LabelImageOptions>
    {
        Source = new LabelImageOptions
        {
            Text            = name,
            FontName        = "Arial",
            FontSize        = 22,
            TextColor       = "#FFFFFF",
            BackgroundColor = Color.Transparent
        },
        Options = new ImageLayerOptions
        {
            Position = ImagePosition.Absolute,
            Offset   = new ImageEdgeOffset(top: 50, left: 155)
        }
    };

    return new ImageBuilder(imageService, imageCreators)
        .SetBaseLayer(new CanvasImageOptions { Size = new ImageSize(400, 150), BackgroundColor = "#1E3A5F" })
        .Add(avatarLayer, nameLabel)
        .Build();
}
```

---

## Example 4: RichImageService — API service pattern

Wraps `ImageBuilder` in an application-level service that accepts DTO input and returns either a composed image or a PDF.

```csharp
public interface IRichImageService
{
    IImageFile  Generate(DrawImageLayerDto input);
    IMemoryFile Print(DrawImageLayerDto input);
}

public class RichImageService(
    IImageService imageService,
    IImagesToPdfService pdfService,
    IEnumerable<IImageCreator> imageCreators) : IRichImageService
{
    public IImageFile Generate(DrawImageLayerDto input)
    {
        var builder    = new ImageBuilder(imageService, imageCreators);
        ImageSize targetSize = ImageSize.Empty;

        if (input.TargetImage != null)
        {
            var targetImage = input.TargetImage.ToBinaryFile().ToImageFile();
            builder.SetBaseLayer(targetImage);
            targetSize = imageService.GetDimensions(targetImage);
        }
        else if (input.TargetCanvas != null)
        {
            var targetCanvas = input.TargetCanvas.ToCanvasImageOptions(ImageSize.Empty);
            builder.SetBaseLayer(targetCanvas);
            targetSize = targetCanvas.Size;
        }

        builder.Add(input.Items.ToImageLayers(targetSize, imageService).ToArray());
        return builder.Build();
    }

    public IMemoryFile Print(DrawImageLayerDto input)
    {
        using var img = Generate(input);
        return pdfService.ImagesToPdf(new ImagesInput { Images = [img.GetBytes()!] })!;
    }
}
```

Register alongside the image creators:

```csharp
services.AddSingleton<IRichImageService, RichImageService>();
```

---

## Overview

1. [Index](01-index.md) — Overview, models, and API reference
1. **[Examples](02-examples.md)** — Thumbnail, watermark, badge builder, and API service pattern
