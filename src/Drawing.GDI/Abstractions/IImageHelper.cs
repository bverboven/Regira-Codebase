using System.Drawing;
using Regira.Media.Drawing.Core;

namespace Regira.Drawing.GDI.Abstractions;

public interface IImageHelper
{
    Image Draw(IEnumerable<ImageToAdd> images, Image? target = null, int dpi = ImageConstants.DEFAULT_DPI);
}