﻿namespace Regira.Media.Drawing.Core;

public class TextImageOptions
{
    public const int DEFAULT_FONT_SIZE = 15;
    public const string DEFAULT_FONT_NAME = "Arial";

    public string FontName { get; set; } = DEFAULT_FONT_NAME;
    public int FontSize { get; set; } = DEFAULT_FONT_SIZE;
    public string TextColor { get; set; } = "#000000";
    public string BackgroundColor { get; set; } = "#FFFFFF";
}