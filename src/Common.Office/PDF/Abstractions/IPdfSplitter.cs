﻿using Regira.IO.Abstractions;
using Regira.Office.PDF.Models;

namespace Regira.Office.PDF.Abstractions;

public interface IPdfSplitter
{
    IEnumerable<IMemoryFile> Split(IBinaryFile pdf, IEnumerable<PdfSplitRange> ranges);
    int GetPageCount(IBinaryFile pdf);
}