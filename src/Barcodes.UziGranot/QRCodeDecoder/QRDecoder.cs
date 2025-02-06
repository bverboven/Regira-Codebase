﻿/////////////////////////////////////////////////////////////////////
//
//	QR Code Library
//
//	QR Code decoder.
//
//	Author: Uzi Granot
//	Original Version: 1.0
//	Date: June 30, 2018
//	Copyright (C) 2018-2019 Uzi Granot. All Rights Reserved
//	For full version history please look at QRDecoder.cs
//
//	QR Code Library C# class library and the attached test/demo
//  applications are free software.
//	Software developed by this author is licensed under CPOL 1.02.
//	Some portions of the QRCodeVideoDecoder are licensed under GNU Lesser
//	General Public License v3.0.
//
//	The solution is made of 3 projects:
//	1. QRCodeDecoderLibrary: QR code decoding.
//	3. QRCodeDecoderDemo: Decode QR code image files.
//	4. QRCodeVideoDecoder: Decode QR code using web camera.
//		This demo program is using some of the source modules of
//		Camera_Net project published at CodeProject.com:
//		https://www.codeproject.com/Articles/671407/Camera_Net-Library
//		and at GitHub: https://github.com/free5lot/Camera_Net.
//		This project is based on DirectShowLib.
//		http://sourceforge.net/projects/directshownet/
//		This project includes a modified subset of the source modules.
//
//	The main points of CPOL 1.02 subject to the terms of the License are:
//
//	Source Code and Executable Files can be used in commercial applications;
//	Source Code and Executable Files can be redistributed; and
//	Source Code can be modified to create derivative works.
//	No claim of suitability, guarantee, or any warranty whatsoever is
//	provided. The software is provided "as-is".
//	The Article accompanying the Work may not be distributed or republished
//	without the Author's consent
//
/////////////////////////////////////////////////////////////////////
//
//	Version History:
//
//	Version 1.0 2018/06/30
//		Original revision
//
//	Version 1.1 2018/07/20
//		Consolidate DirectShowLib into one module removing unused code
//
//	Version 2.0 2019/05/15
//		Split the combined QRCode encoder and decoder to two solutions.
//		Add support for .net standard.
//		Add save image to png file without Bitmap class.
//	Version 2.1 2019/07/22
//		Add support for ECI Assignment Value
/////////////////////////////////////////////////////////////////////

using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;

// ReSharper disable once CheckNamespace
namespace QRCodeDecoderLibrary;

/// <summary>
/// QR Code error correction code enumeration
/// </summary>
public enum ErrorCorrection
{
    /// <summary>
    /// Low (01)
    /// </summary>
    L,

    /// <summary>
    /// Medium (00)
    /// </summary>
    M,

    /// <summary>
    /// Medium-high (11)
    /// </summary>
    Q,

    /// <summary>
    /// High (10)
    /// </summary>
    H,
}

/// <summary>
/// QR Code encoding modes
/// </summary>
public enum EncodingMode
{
    /// <summary>
    /// Terminator
    /// </summary>
    Terminator,

    /// <summary>
    /// Numeric
    /// </summary>
    Numeric,

    /// <summary>
    /// Alpha numeric
    /// </summary>
    AlphaNumeric,

    /// <summary>
    /// Append
    /// </summary>
    Append,

    /// <summary>
    /// byte encoding
    /// </summary>
    Byte,

    /// <summary>
    /// FNC1 first
    /// </summary>
    Fnc1First,

    /// <summary>
    /// Unknown encoding constant
    /// </summary>
    Unknown6,

    /// <summary>
    /// Extended Channel Interpretaion (ECI) mode
    /// </summary>
    Eci,

    /// <summary>
    /// Kanji encoding (not implemented by this software)
    /// </summary>
    Kanji,

    /// <summary>
    /// FNC1 second
    /// </summary>
    Fnc1Second,

    /// <summary>
    /// Unknown encoding constant
    /// </summary>
    Unknown10,

    /// <summary>
    /// Unknown encoding constant
    /// </summary>
    Unknown11,

    /// <summary>
    /// Unknown encoding constant
    /// </summary>
    Unknown12,

    /// <summary>
    /// Unknown encoding constant
    /// </summary>
    Unknown13,

    /// <summary>
    /// Unknown encoding constant
    /// </summary>
    Unknown14,

    /// <summary>
    /// Unknown encoding constant
    /// </summary>
    Unknown15,
}

public class QRDecoder
{
    public const string VersionNumber = "Rev 2.1.0 - 2019-07-22";

    /// <summary>
    /// Gets QR Code matrix version
    /// </summary>
    public int QRCodeVersion { get; internal set; }

    /// <summary>
    /// Gets QR Code matrix dimension in bits
    /// </summary>
    public int QRCodeDimension { get; internal set; }

    /// <summary>
    /// Gets QR Code error correction code (L, M, Q, H)
    /// </summary>
    public ErrorCorrection ErrorCorrection { get; internal set; }

    /// <summary>
    /// Error correction percent (L, M, Q, H)
    /// </summary>
    public int[] ErrCorrPercent = [7, 15, 25, 30];

    /// <summary>
    /// Get mask code (0 to 7)
    /// </summary>
    public int MaskCode { get; internal set; }

    /// <summary>
    /// ECI Assignment Value
    /// </summary>
    public int EciAssignValue { get; internal set; }

    internal int ImageWidth;
    internal int ImageHeight;
    internal bool[,] BlackWhiteImage;
    internal List<Finder> FinderList;
    internal List<Finder> AlignList;
    internal List<byte[]> DataArrayList;
    internal int MaxCodewords;
    internal int MaxDataCodewords;
    internal int MaxDataBits;
    internal int ErrCorrCodewords;
    internal int BlocksGroup1;
    internal int DataCodewordsGroup1;
    internal int BlocksGroup2;
    internal int DataCodewordsGroup2;

    internal byte[] CodewordsArray;
    internal int CodewordsPtr;
    internal uint BitBuffer;
    internal int BitBufferLen;
    internal byte[,] BaseMatrix;
    internal byte[,] MaskMatrix;

    internal bool Trans4Mode;

    // transformation cooefficients from QR modules to image pixels
    internal double Trans3A;
    internal double Trans3B;
    internal double Trans3C;
    internal double Trans3d;
    internal double Trans3E;
    internal double Trans3F;

    // transformation matrix based on three finders plus one more point
    internal double Trans4A;
    internal double Trans4B;
    internal double Trans4C;
    internal double Trans4d;
    internal double Trans4E;
    internal double Trans4F;
    internal double Trans4G;
    internal double Trans4H;

    internal const double SIGNATURE_MAX_DEVIATION = 0.35;// originally 0.25, modified by BV;
    internal const double HOR_VERT_SCAN_MAX_DISTANCE = 2.0;
    internal const double MODULE_SIZE_DEVIATION = 0.5; // 0.75;
    internal const double CORNER_SIDE_LENGTH_DEV = 0.8;
    internal const double CORNER_RIGHT_ANGLE_DEV = 0.25; // about Sin(4 deg)
    internal const double ALIGNMENT_SEARCH_AREA = 0.3;

    /// <summary>
    /// Convert byte array to string using UTF8 encoding
    /// </summary>
    /// <param name="dataArray">Input array</param>
    /// <returns>Output string</returns>
    public static string ByteArrayToStr(byte[] dataArray)
    {
        var decoder = Encoding.UTF8.GetDecoder();
        var charCount = decoder.GetCharCount(dataArray, 0, dataArray.Length);
        var charArray = new char[charCount];
        decoder.GetChars(dataArray, 0, dataArray.Length, charArray, 0);
        return new string(charArray);
    }

    /// <summary>
    /// QRCode image decoder
    /// </summary>
    /// <param name="inputImage">Input image</param>
    /// <returns>Output byte arrays</returns>
    public byte[][] ImageDecoder(Bitmap inputImage)
    {
        try
        {
            // empty data string output
            DataArrayList = [];

            // save image dimension
            ImageWidth = inputImage.Width;
            ImageHeight = inputImage.Height;


            // convert input image to black and white boolean image
            if (!ConvertImageToBlackAndWhite(inputImage))
            {
                return null;
            }


            // horizontal search for finders
            if (!HorizontalFindersSearch())
            {
                return null;
            }


            // vertical search for finders
            VerticalFindersSearch();


            // remove unused finders
            if (!RemoveUnusedFinders())
            {
                return null;
            }

        }
        catch
        {
            return null;
        }

        // look for all possible 3 finder patterns
        var index1End = FinderList.Count - 2;
        var index2End = FinderList.Count - 1;
        var index3End = FinderList.Count;
        for (var index1 = 0; index1 < index1End; index1++)
        {
            for (var index2 = index1 + 1; index2 < index2End; index2++)
            {
                for (var index3 = index2 + 1; index3 < index3End; index3++)
                {
                    try
                    {
                        // find 3 finders arranged in L shape
                        var corner = Corner.CreateCorner(FinderList[index1], FinderList[index2], FinderList[index3]);

                        // not a valid corner
                        if (corner == null) continue;

                        // get corner info (version, error code and mask)
                        // continue if failed
                        if (!GetQRCodeCornerInfo(corner)) continue;


                        // decode corner using three finders
                        // continue if successful
                        if (DecodeQRCodeCorner(corner)) continue;

                        // qr code version 1 has no alignment mark
                        // in other words decode failed 
                        if (QRCodeVersion == 1) continue;

                        // find bottom right alignment mark
                        // continue if failed
                        if (!FindAlignmentMark(corner)) continue;

                        // decode using 4 points
                        foreach (var align in AlignList)
                        {
                            // calculate transformation based on 3 finders and bottom right alignment mark
                            SetTransMatrix(corner, align.Row, align.Col);

                            // decode corner using three finders and one alignment mark
                            if (DecodeQRCodeCorner(corner))
                            {
                                break;
                            }
                        }
                    }
                    catch
                    {
                        // nothing
                    }
                }
            }
        }

        // not found exit
        if (DataArrayList.Count == 0)
        {
            return null;
        }

        // successful exit
        return DataArrayList.ToArray();
    }

    ////////////////////////////////////////////////////////////////////
    // Convert image to black and white boolean matrix
    ////////////////////////////////////////////////////////////////////

    internal bool ConvertImageToBlackAndWhite(Bitmap inputImage)
    {
        // lock image bits
        var bitmapData = inputImage.LockBits(new Rectangle(0, 0, ImageWidth, ImageHeight), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);


        // address of first line
        var bitArrayPtr = bitmapData.Scan0;

        // length in bytes of one scan line
        var scanLineWidth = bitmapData.Stride;
        if (scanLineWidth < 0)
        {
            return false;
        }

        // image total bytes
        var totalBytes = scanLineWidth * ImageHeight;
        var bitmapArray = new byte[totalBytes];

        // Copy the RGB values into the array.
        Marshal.Copy(bitArrayPtr, bitmapArray, 0, totalBytes);

        // unlock image
        inputImage.UnlockBits(bitmapData);

        // allocate gray image 
        var grayImage = new byte[ImageHeight, ImageWidth];
        var grayLevel = new int[256];

        // convert to gray
        var delta = scanLineWidth - 3 * ImageWidth;
        var bitmapPtr = 0;
        for (var row = 0; row < ImageHeight; row++)
        {
            for (var col = 0; col < ImageWidth; col++)
            {
                var module = (30 * bitmapArray[bitmapPtr] + 59 * bitmapArray[bitmapPtr + 1] + 11 * bitmapArray[bitmapPtr + 2]) / 100;
                grayLevel[module]++;
                grayImage[row, col] = (byte)module;
                bitmapPtr += 3;
            }
            bitmapPtr += delta;
        }

        // gray level cutoff between black and white
        int levelStart;
        int levelEnd;
        for (levelStart = 0; levelStart < 256 && grayLevel[levelStart] == 0; levelStart++)
        {
            // continue
        }
        for (levelEnd = 255; levelEnd >= levelStart && grayLevel[levelEnd] == 0; levelEnd--)
        {
            // continue
        }
        levelEnd++;
        if (levelEnd - levelStart < 2)
        {
            return false;
        }

        var cutoffLevel = (levelStart + levelEnd) / 2;

        // create boolean image white = false, black = true
        BlackWhiteImage = new bool[ImageHeight, ImageWidth];
        for (var row = 0; row < ImageHeight; row++)
        {
            for (var col = 0; col < ImageWidth; col++)
            {
                BlackWhiteImage[row, col] = grayImage[row, col] < cutoffLevel;
            }
        }

        // save as black white image

        // exit;
        return true;
    }

    ////////////////////////////////////////////////////////////////////
    // search row by row for finders blocks
    ////////////////////////////////////////////////////////////////////

    internal bool HorizontalFindersSearch()
    {
        // create empty finders list
        FinderList = [];

        // look for finder patterns
        var colPos = new int[ImageWidth + 1];

        // scan one row at a time
        for (var row = 0; row < ImageHeight; row++)
        {
            // look for first black pixel
            int col;
            for (col = 0; col < ImageWidth && !BlackWhiteImage[row, col]; col++)
            {
                // continue
            }

            if (col == ImageWidth) continue;

            // first black
            var posPtr = 0;
            colPos[posPtr++] = col;

            // loop for pairs
            for (; ; )
            {
                // look for next white
                // if black is all the way to the edge, set next white after the edge
                for (; col < ImageWidth && BlackWhiteImage[row, col]; col++)
                {
                    // continue
                }
                colPos[posPtr++] = col;
                if (col == ImageWidth)
                {
                    break;
                }

                // look for next black
                for (; col < ImageWidth && !BlackWhiteImage[row, col]; col++)
                {
                    // continue
                }
                if (col == ImageWidth)
                {
                    break;
                }
                colPos[posPtr++] = col;
            }

            // we must have at least 6 positions
            if (posPtr < 6) continue;

            // build length array
            var posLen = posPtr - 1;
            var len = new int[posLen];
            for (var ptr = 0; ptr < posLen; ptr++)
            {
                len[ptr] = colPos[ptr + 1] - colPos[ptr];
            }

            // test signature
            var sigLen = posPtr - 5;
            for (var sigPtr = 0; sigPtr < sigLen; sigPtr += 2)
            {
                if (TestFinderSig(colPos, len, sigPtr, out var moduleSize))
                {
                    FinderList.Add(new Finder(row, colPos[sigPtr + 2], colPos[sigPtr + 3], moduleSize));
                }
            }
        }

        // no finders found
        if (FinderList.Count < 3)
        {
            return false;
        }

        // exit
        return true;
    }

    ////////////////////////////////////////////////////////////////////
    // search row by row for alignment blocks
    ////////////////////////////////////////////////////////////////////

    internal bool HorizontalAlignmentSearch(int areaLeft, int areaTop, int areaWidth, int areaHeight)
    {
        // create empty finders list
        AlignList = [];

        // look for finder patterns
        var colPos = new int[areaWidth + 1];

        // area right and bottom
        var areaRight = areaLeft + areaWidth;
        var areaBottom = areaTop + areaHeight;

        // scan one row at a time
        for (var row = areaTop; row < areaBottom; row++)
        {
            // look for first black pixel
            int col;
            for (col = areaLeft; col < areaRight && !BlackWhiteImage[row, col]; col++)
            {
                ;
            }

            if (col == areaRight)
            {
                continue;
            }

            // first black
            var posPtr = 0;
            colPos[posPtr++] = col;

            // loop for pairs
            for (; ; )
            {
                // look for next white
                // if black is all the way to the edge, set next white after the edge
                for (; col < areaRight && BlackWhiteImage[row, col]; col++)
                {
                    // continue
                }

                colPos[posPtr++] = col;
                if (col == areaRight)
                {
                    break;
                }

                // look for next black
                for (; col < areaRight && !BlackWhiteImage[row, col]; col++)
                {
                    // continue
                }

                if (col == areaRight)
                {
                    break;
                }

                colPos[posPtr++] = col;
            }

            // we must have at least 6 positions
            if (posPtr < 6)
            {
                continue;
            }

            // build length array
            var posLen = posPtr - 1;
            var len = new int[posLen];
            for (var ptr = 0; ptr < posLen; ptr++)
            {
                len[ptr] = colPos[ptr + 1] - colPos[ptr];
            }

            // test signature
            var sigLen = posPtr - 5;
            for (var sigPtr = 0; sigPtr < sigLen; sigPtr += 2)
            {
                if (TestAlignSig(colPos, len, sigPtr, out var moduleSize))
                {
                    AlignList.Add(new Finder(row, colPos[sigPtr + 2], colPos[sigPtr + 3], moduleSize));
                }
            }
        }

        // exit
        return AlignList.Count != 0;
    }

    ////////////////////////////////////////////////////////////////////
    // search column by column for finders blocks
    ////////////////////////////////////////////////////////////////////

    internal void VerticalFindersSearch()
    {
        // active columns
        var activeColumn = new bool[ImageWidth];
        foreach (var hf in FinderList)
        {
            for (var col = hf.Col1; col < hf.Col2; col++)
            {
                activeColumn[col] = true;
            }
        }

        // look for finder patterns
        var rowPos = new int[ImageHeight + 1];

        // scan one column at a time
        for (var col = 0; col < ImageWidth; col++)
        {
            // not active column
            if (!activeColumn[col]) continue;

            // look for first black pixel
            int row;
            for (row = 0; row < ImageHeight && !BlackWhiteImage[row, col]; row++)
            {
                // continue
            }

            if (row == ImageWidth) continue;

            // first black
            var posPtr = 0;
            rowPos[posPtr++] = row;

            // loop for pairs
            for (; ; )
            {
                // look for next white
                // if black is all the way to the edge, set next white after the edge
                for (; row < ImageHeight && BlackWhiteImage[row, col]; row++)
                {
                    // continue
                }

                rowPos[posPtr++] = row;
                if (row == ImageHeight)
                {
                    break;
                }

                // look for next black
                for (; row < ImageHeight && !BlackWhiteImage[row, col]; row++)
                {
                    // continue
                }

                if (row == ImageHeight)
                {
                    break;
                }

                rowPos[posPtr++] = row;
            }

            // we must have at least 6 positions
            if (posPtr < 6)
            {
                continue;
            }

            // build length array
            var posLen = posPtr - 1;
            var len = new int[posLen];
            for (var ptr = 0; ptr < posLen; ptr++)
            {
                len[ptr] = rowPos[ptr + 1] - rowPos[ptr];
            }

            // test signature
            var sigLen = posPtr - 5;
            for (var sigPtr = 0; sigPtr < sigLen; sigPtr += 2)
            {
                if (!TestFinderSig(rowPos, len, sigPtr, out var moduleSize))
                {
                    continue;
                }

                foreach (var hf in FinderList)
                {
                    hf.Match(col, rowPos[sigPtr + 2], rowPos[sigPtr + 3], moduleSize);
                }
            }
        }

        // exit
    }

    ////////////////////////////////////////////////////////////////////
    // search column by column for finders blocks
    ////////////////////////////////////////////////////////////////////

    internal void VerticalAlignmentSearch(int areaLeft, int areaTop, int areaWidth, int areaHeight)
    {
        // active columns
        var activeColumn = new bool[areaWidth];
        foreach (var hf in AlignList)
        {
            for (var col = hf.Col1; col < hf.Col2; col++) activeColumn[col - areaLeft] = true;
        }

        // look for finder patterns
        var rowPos = new int[areaHeight + 1];

        // area right and bottom
        var areaRight = areaLeft + areaWidth;
        var areaBottom = areaTop + areaHeight;

        // scan one column at a time
        for (var col = areaLeft; col < areaRight; col++)
        {
            // not active column
            if (!activeColumn[col - areaLeft])
            {
                continue;
            }

            // look for first black pixel
            int row;
            for (row = areaTop; row < areaBottom && !BlackWhiteImage[row, col]; row++)
            {
                // continue
            }

            if (row == areaBottom)
            {
                continue;
            }

            // first black
            var posPtr = 0;
            rowPos[posPtr++] = row;

            // loop for pairs
            for (; ; )
            {
                // look for next white
                // if black is all the way to the edge, set next white after the edge
                for (; row < areaBottom && BlackWhiteImage[row, col]; row++)
                {
                    // continue
                }

                rowPos[posPtr++] = row;
                if (row == areaBottom)
                {
                    break;
                }

                // look for next black
                for (; row < areaBottom && !BlackWhiteImage[row, col]; row++)
                {
                    // continue
                }

                if (row == areaBottom)
                {
                    break;
                }

                rowPos[posPtr++] = row;
            }

            // we must have at least 6 positions
            if (posPtr < 6)
            {
                continue;
            }

            // build length array
            var posLen = posPtr - 1;
            var len = new int[posLen];
            for (var ptr = 0; ptr < posLen; ptr++)
            {
                len[ptr] = rowPos[ptr + 1] - rowPos[ptr];
            }

            // test signature
            var sigLen = posPtr - 5;
            for (var sigPtr = 0; sigPtr < sigLen; sigPtr += 2)
            {
                if (!TestAlignSig(rowPos, len, sigPtr, out var moduleSize))
                {
                    continue;
                }

                foreach (var hf in AlignList)
                {
                    hf.Match(col, rowPos[sigPtr + 2], rowPos[sigPtr + 3], moduleSize);
                }
            }
        }

        // exit
    }

    ////////////////////////////////////////////////////////////////////
    // search column by column for finders blocks
    ////////////////////////////////////////////////////////////////////

    internal bool RemoveUnusedFinders()
    {
        // remove all entries without a match
        for (var index = 0; index < FinderList.Count; index++)
        {
            if (Math.Abs(FinderList[index].Distance - double.MaxValue) < double.Epsilon)
            {
                FinderList.RemoveAt(index);
                index--;
            }
        }

        // list is now empty or has less than three finders
        if (FinderList.Count < 3)
        {
            return false;
        }

        // keep best entry for each overlapping area
        for (var index = 0; index < FinderList.Count; index++)
        {
            var finder = FinderList[index];
            for (var index1 = index + 1; index1 < FinderList.Count; index1++)
            {
                var finder1 = FinderList[index1];
                if (!finder.Overlap(finder1))
                {
                    continue;
                }

                if (finder1.Distance < finder.Distance)
                {
                    finder = finder1;
                    FinderList[index] = finder;
                }
                FinderList.RemoveAt(index1);
                index1--;
            }
        }

        // list is now empty or has less than three finders
        if (FinderList.Count < 3)
        {
            return false;
        }

        // exit
        return true;
    }

    ////////////////////////////////////////////////////////////////////
    // search column by column for finders blocks
    ////////////////////////////////////////////////////////////////////

    internal bool RemoveUnusedAlignMarks()
    {
        // remove all entries without a match
        for (var index = 0; index < AlignList.Count; index++)
        {
            if (Math.Abs(AlignList[index].Distance - double.MaxValue) < double.Epsilon)
            {
                AlignList.RemoveAt(index);
                index--;
            }
        }

        // keep best entry for each overlapping area
        for (var index = 0; index < AlignList.Count; index++)
        {
            var finder = AlignList[index];
            for (var index1 = index + 1; index1 < AlignList.Count; index1++)
            {
                var finder1 = AlignList[index1];
                if (!finder.Overlap(finder1))
                {
                    continue;
                }

                if (finder1.Distance < finder.Distance)
                {
                    finder = finder1;
                    AlignList[index] = finder;
                }
                AlignList.RemoveAt(index1);
                index1--;
            }
        }

        // exit
        return AlignList.Count != 0;
    }

    ////////////////////////////////////////////////////////////////////
    // test finder signature 1 1 3 1 1
    ////////////////////////////////////////////////////////////////////

    internal bool TestFinderSig(int[] pos, int[] len, int index, out double module)
    {
        module = (pos[index + 5] - pos[index]) / 7.0;
        var maxDev = SIGNATURE_MAX_DEVIATION * module;
        if (Math.Abs(len[index] - module) > maxDev) return false;
        if (Math.Abs(len[index + 1] - module) > maxDev) return false;
        if (Math.Abs(len[index + 2] - 3 * module) > maxDev) return false;
        if (Math.Abs(len[index + 3] - module) > maxDev) return false;
        if (Math.Abs(len[index + 4] - module) > maxDev) return false;
        return true;
    }

    ////////////////////////////////////////////////////////////////////
    // test alignment signature n 1 1 1 n
    ////////////////////////////////////////////////////////////////////

    internal bool TestAlignSig(int[] pos, int[] len, int index, out double module)
    {
        module = (pos[index + 4] - pos[index + 1]) / 3.0;
        var maxDev = SIGNATURE_MAX_DEVIATION * module;
        if (len[index] < module - maxDev) return false;
        if (Math.Abs(len[index + 1] - module) > maxDev) return false;
        if (Math.Abs(len[index + 2] - module) > maxDev) return false;
        if (Math.Abs(len[index + 3] - module) > maxDev) return false;
        if (len[index + 4] < module - maxDev) return false;
        return true;
    }

    ////////////////////////////////////////////////////////////////////
    // Build corner list
    ////////////////////////////////////////////////////////////////////

    internal List<Corner> BuildCornerList()
    {
        // empty list
        var corners = new List<Corner>();

        // look for all possible 3 finder patterns
        var index1End = FinderList.Count - 2;
        var index2End = FinderList.Count - 1;
        var index3End = FinderList.Count;
        for (var index1 = 0; index1 < index1End; index1++)
        {
            for (var index2 = index1 + 1; index2 < index2End; index2++)
            {
                for (var index3 = index2 + 1; index3 < index3End; index3++)
                {
                    // find 3 finders arranged in L shape
                    var corner = Corner.CreateCorner(FinderList[index1], FinderList[index2], FinderList[index3]);

                    // add corner to list
                    if (corner != null)
                    {
                        corners.Add(corner);
                    }
                }
            }
        }

        // exit
        return corners.Count == 0 ? null : corners;
    }

    ////////////////////////////////////////////////////////////////////
    // Get QR Code corner info
    ////////////////////////////////////////////////////////////////////

    internal bool GetQRCodeCornerInfo(Corner corner)
    {
        try
        {
            // initial version number
            QRCodeVersion = corner.InitialVersionNumber();

            // qr code dimension
            QRCodeDimension = 17 + 4 * QRCodeVersion;

            // set transformation matrix
            SetTransMatrix(corner);

            // if version number is 7 or more, get version code
            if (QRCodeVersion >= 7)
            {
                var version = GetVersionOne();
                if (version == 0)
                {
                    version = GetVersionTwo();
                    if (version == 0) return false;
                }

                // QR Code version number is different than initial version
                if (version != QRCodeVersion)
                {
                    // initial version number and dimension
                    QRCodeVersion = version;

                    // qr code dimension
                    QRCodeDimension = 17 + 4 * QRCodeVersion;

                    // set transformation matrix
                    SetTransMatrix(corner);
                }
            }

            // get format info arrays
            var formatInfo = GetFormatInfoOne();
            if (formatInfo < 0)
            {
                formatInfo = GetFormatInfoTwo();
                if (formatInfo < 0)
                {
                    return false;
                }
            }

            // set error correction code and mask code
            ErrorCorrection = FormatInfoToErrCode(formatInfo >> 3);
            MaskCode = formatInfo & 7;

            // successful exit
            return true;
        }
        catch
        {
            // failed exit
            return false;
        }
    }

    ////////////////////////////////////////////////////////////////////
    // Search for QR Code version
    ////////////////////////////////////////////////////////////////////

    internal bool DecodeQRCodeCorner(Corner corner)
    {
        try
        {
            // create base matrix
            BuildBaseMatrix();

            // create data matrix and test fixed modules
            ConvertImageToMatrix();

            // based on version and format information
            // set number of data and error correction codewords length  
            SetDataCodewordsLength();

            // apply mask as per get format information step
            ApplyMask(MaskCode);

            // unload data from binary matrix to byte format
            UnloadDataFromMatrix();

            // restore blocks (undo interleave)
            RestoreBlocks();

            // calculate error correction
            // in case of error try to correct it
            CalculateErrorCorrection();

            // decode data
            var dataArray = DecodeData();
            DataArrayList.Add(dataArray);

            // successful exit
            return true;
        }

        catch
        {
            // failed exit
            return false;
        }
    }

    internal void SetTransMatrix(Corner corner)
    {
        // save
        var bottomRightPos = QRCodeDimension - 4;

        // transformation matrix based on three finders
        var matrix1 = new double[3, 4];
        var matrix2 = new double[3, 4];

        // build matrix 1 for horizontal X direction
        matrix1[0, 0] = 3;
        matrix1[0, 1] = 3;
        matrix1[0, 2] = 1;
        matrix1[0, 3] = corner.TopLeftFinder.Col;

        matrix1[1, 0] = bottomRightPos;
        matrix1[1, 1] = 3;
        matrix1[1, 2] = 1;
        matrix1[1, 3] = corner.TopRightFinder.Col;

        matrix1[2, 0] = 3;
        matrix1[2, 1] = bottomRightPos;
        matrix1[2, 2] = 1;
        matrix1[2, 3] = corner.BottomLeftFinder.Col;

        // build matrix 2 for Vertical Y direction
        matrix2[0, 0] = 3;
        matrix2[0, 1] = 3;
        matrix2[0, 2] = 1;
        matrix2[0, 3] = corner.TopLeftFinder.Row;

        matrix2[1, 0] = bottomRightPos;
        matrix2[1, 1] = 3;
        matrix2[1, 2] = 1;
        matrix2[1, 3] = corner.TopRightFinder.Row;

        matrix2[2, 0] = 3;
        matrix2[2, 1] = bottomRightPos;
        matrix2[2, 2] = 1;
        matrix2[2, 3] = corner.BottomLeftFinder.Row;

        // solve matrix1
        SolveMatrixOne(matrix1);
        Trans3A = matrix1[0, 3];
        Trans3C = matrix1[1, 3];
        Trans3E = matrix1[2, 3];

        // solve matrix2
        SolveMatrixOne(matrix2);
        Trans3B = matrix2[0, 3];
        Trans3d = matrix2[1, 3];
        Trans3F = matrix2[2, 3];

        // reset trans 4 mode
        Trans4Mode = false;
    }

    internal void SolveMatrixOne(double[,] matrix)
    {
        for (var row = 0; row < 3; row++)
        {
            // If the element is zero, make it non zero by adding another row
            if (matrix[row, row] == 0)
            {
                int row1;
                for (row1 = row + 1; row1 < 3 && matrix[row1, row] == 0; row1++)
                {
                    // continue
                }

                if (row1 == 3) throw new ApplicationException("Solve linear equations failed");

                for (var col = row; col < 4; col++)
                {
                    matrix[row, col] += matrix[row1, col];
                }
            }

            // make the diagonal element 1.0
            for (var col = 3; col > row; col--)
            {
                matrix[row, col] /= matrix[row, row];
            }

            // subtract current row from next rows to eliminate one value
            for (var row1 = row + 1; row1 < 3; row1++)
            {
                for (var col = 3; col > row; col--)
                {
                    matrix[row1, col] -= matrix[row, col] * matrix[row1, row];
                }
            }
        }

        // go up from last row and eliminate all solved values
        matrix[1, 3] -= matrix[1, 2] * matrix[2, 3];
        matrix[0, 3] -= matrix[0, 2] * matrix[2, 3];
        matrix[0, 3] -= matrix[0, 1] * matrix[1, 3];
    }

    ////////////////////////////////////////////////////////////////////
    // Get image pixel color
    ////////////////////////////////////////////////////////////////////

    internal bool GetModule(int row, int col)
    {
        // get module based on three finders
        if (!Trans4Mode)
        {
            var trans3Col = (int)Math.Round(Trans3A * col + Trans3C * row + Trans3E, 0, MidpointRounding.AwayFromZero);
            var trans3Row = (int)Math.Round(Trans3B * col + Trans3d * row + Trans3F, 0, MidpointRounding.AwayFromZero);
            return BlackWhiteImage[trans3Row, trans3Col];
        }

        // get module based on three finders plus one alignment mark
        var w = Trans4G * col + Trans4H * row + 1.0;
        var trans4Col = (int)Math.Round((Trans4A * col + Trans4B * row + Trans4C) / w, 0, MidpointRounding.AwayFromZero);
        var trans4Row = (int)Math.Round((Trans4d * col + Trans4E * row + Trans4F) / w, 0, MidpointRounding.AwayFromZero);
        return BlackWhiteImage[trans4Row, trans4Col];
    }

    ////////////////////////////////////////////////////////////////////
    // search row by row for finders blocks
    ////////////////////////////////////////////////////////////////////

    internal bool FindAlignmentMark(Corner corner)
    {
        // alignment mark estimated position
        var alignRow = QRCodeDimension - 7;
        var alignCol = QRCodeDimension - 7;
        var imageCol = (int)Math.Round(Trans3A * alignCol + Trans3C * alignRow + Trans3E, 0, MidpointRounding.AwayFromZero);
        var imageRow = (int)Math.Round(Trans3B * alignCol + Trans3d * alignRow + Trans3F, 0, MidpointRounding.AwayFromZero);

        // search area
        var side = (int)Math.Round(ALIGNMENT_SEARCH_AREA * (corner.TopLineLength + corner.LeftLineLength), 0, MidpointRounding.AwayFromZero);

        var areaLeft = imageCol - side / 2;
        var areaTop = imageRow - side / 2;
        var areaWidth = side;
        var areaHeight = side;

        // horizontal search for finders
        if (!HorizontalAlignmentSearch(areaLeft, areaTop, areaWidth, areaHeight))
        {
            return false;
        }

        // vertical search for finders
        VerticalAlignmentSearch(areaLeft, areaTop, areaWidth, areaHeight);

        // remove unused alignment entries
        if (!RemoveUnusedAlignMarks())
        {
            return false;
        }

        // successful exit
        return true;
    }

    internal void SetTransMatrix(Corner corner, double imageAlignRow, double imageAlignCol)
    {
        // top right and bottom left QR code position
        var farFinder = QRCodeDimension - 4;
        var farAlign = QRCodeDimension - 7;

        var matrix = new double[8, 9];

        matrix[0, 0] = 3.0;
        matrix[0, 1] = 3.0;
        matrix[0, 2] = 1.0;
        matrix[0, 6] = -3.0 * corner.TopLeftFinder.Col;
        matrix[0, 7] = -3.0 * corner.TopLeftFinder.Col;
        matrix[0, 8] = corner.TopLeftFinder.Col;

        matrix[1, 0] = farFinder;
        matrix[1, 1] = 3.0;
        matrix[1, 2] = 1.0;
        matrix[1, 6] = -farFinder * corner.TopRightFinder.Col;
        matrix[1, 7] = -3.0 * corner.TopRightFinder.Col;
        matrix[1, 8] = corner.TopRightFinder.Col;

        matrix[2, 0] = 3.0;
        matrix[2, 1] = farFinder;
        matrix[2, 2] = 1.0;
        matrix[2, 6] = -3.0 * corner.BottomLeftFinder.Col;
        matrix[2, 7] = -farFinder * corner.BottomLeftFinder.Col;
        matrix[2, 8] = corner.BottomLeftFinder.Col;

        matrix[3, 0] = farAlign;
        matrix[3, 1] = farAlign;
        matrix[3, 2] = 1.0;
        matrix[3, 6] = -farAlign * imageAlignCol;
        matrix[3, 7] = -farAlign * imageAlignCol;
        matrix[3, 8] = imageAlignCol;

        matrix[4, 3] = 3.0;
        matrix[4, 4] = 3.0;
        matrix[4, 5] = 1.0;
        matrix[4, 6] = -3.0 * corner.TopLeftFinder.Row;
        matrix[4, 7] = -3.0 * corner.TopLeftFinder.Row;
        matrix[4, 8] = corner.TopLeftFinder.Row;

        matrix[5, 3] = farFinder;
        matrix[5, 4] = 3.0;
        matrix[5, 5] = 1.0;
        matrix[5, 6] = -farFinder * corner.TopRightFinder.Row;
        matrix[5, 7] = -3.0 * corner.TopRightFinder.Row;
        matrix[5, 8] = corner.TopRightFinder.Row;

        matrix[6, 3] = 3.0;
        matrix[6, 4] = farFinder;
        matrix[6, 5] = 1.0;
        matrix[6, 6] = -3.0 * corner.BottomLeftFinder.Row;
        matrix[6, 7] = -farFinder * corner.BottomLeftFinder.Row;
        matrix[6, 8] = corner.BottomLeftFinder.Row;

        matrix[7, 3] = farAlign;
        matrix[7, 4] = farAlign;
        matrix[7, 5] = 1.0;
        matrix[7, 6] = -farAlign * imageAlignRow;
        matrix[7, 7] = -farAlign * imageAlignRow;
        matrix[7, 8] = imageAlignRow;

        for (var row = 0; row < 8; row++)
        {
            // If the element is zero, make it non zero by adding another row
            if (matrix[row, row] == 0)
            {
                int row1;
                for (row1 = row + 1; row1 < 8 && matrix[row1, row] == 0; row1++)
                {
                    // continue
                }

                if (row1 == 8)
                {
                    throw new ApplicationException("Solve linear equations failed");
                }

                for (var col = row; col < 9; col++)
                {
                    matrix[row, col] += matrix[row1, col];
                }
            }

            // make the diagonal element 1.0
            for (var col = 8; col > row; col--)
            {
                matrix[row, col] /= matrix[row, row];
            }

            // subtract current row from next rows to eliminate one value
            for (var row1 = row + 1; row1 < 8; row1++)
            {
                for (var col = 8; col > row; col--)
                {
                    matrix[row1, col] -= matrix[row, col] * matrix[row1, row];
                }
            }
        }

        // go up from last row and eliminate all solved values
        for (var col = 7; col > 0; col--)
        {
            for (var row = col - 1; row >= 0; row--)
            {
                matrix[row, 8] -= matrix[row, col] * matrix[col, 8];
            }
        }

        Trans4A = matrix[0, 8];
        Trans4B = matrix[1, 8];
        Trans4C = matrix[2, 8];
        Trans4d = matrix[3, 8];
        Trans4E = matrix[4, 8];
        Trans4F = matrix[5, 8];
        Trans4G = matrix[6, 8];
        Trans4H = matrix[7, 8];

        // set trans 4 mode
        Trans4Mode = true;
    }

    ////////////////////////////////////////////////////////////////////
    // Get version code bits top right
    ////////////////////////////////////////////////////////////////////

    internal int GetVersionOne()
    {
        var versionCode = 0;
        for (var index = 0; index < 18; index++)
        {
            if (GetModule(index / 3, QRCodeDimension - 11 + index % 3))
            {
                versionCode |= 1 << index;
            }
        }
        return TestVersionCode(versionCode);
    }

    ////////////////////////////////////////////////////////////////////
    // Get version code bits bottom left
    ////////////////////////////////////////////////////////////////////

    internal int GetVersionTwo()
    {
        var versionCode = 0;
        for (var index = 0; index < 18; index++)
        {
            if (GetModule(QRCodeDimension - 11 + index % 3, index / 3))
            {
                versionCode |= 1 << index;
            }
        }
        return TestVersionCode(versionCode);
    }

    ////////////////////////////////////////////////////////////////////
    // Test version code bits
    ////////////////////////////////////////////////////////////////////

    internal int TestVersionCode(int versionCode)
    {
        // format info
        var code = versionCode >> 12;

        // test for exact match
        if (code >= 7 && code <= 40 && StaticTables.VersionCodeArray[code - 7] == versionCode)
        {
            return code;
        }

        // look for a match
        var bestInfo = 0;
        var error = int.MaxValue;
        for (var index = 0; index < 34; index++)
        {
            // test for exact match
            var errorBits = StaticTables.VersionCodeArray[index] ^ versionCode;
            if (errorBits == 0)
            {
                return versionCode >> 12;
            }

            // count errors
            var errorCount = CountBits(errorBits);

            // save best result
            if (errorCount < error)
            {
                error = errorCount;
                bestInfo = index;
            }
        }

        return error <= 3 ? bestInfo + 7 : 0;
    }

    ////////////////////////////////////////////////////////////////////
    // Get format info around top left corner
    ////////////////////////////////////////////////////////////////////

    public int GetFormatInfoOne()
    {
        var info = 0;
        for (var index = 0; index < 15; index++)
        {
            if (GetModule(StaticTables.FormatInfoOne[index, 0], StaticTables.FormatInfoOne[index, 1]))
            {
                info |= 1 << index;
            }
        }
        return TestFormatInfo(info);
    }

    ////////////////////////////////////////////////////////////////////
    // Get format info around top right and bottom left corners
    ////////////////////////////////////////////////////////////////////

    internal int GetFormatInfoTwo()
    {
        var info = 0;
        for (var index = 0; index < 15; index++)
        {
            var row = StaticTables.FormatInfoTwo[index, 0];
            if (row < 0)
            {
                row += QRCodeDimension;
            }

            var col = StaticTables.FormatInfoTwo[index, 1];
            if (col < 0)
            {
                col += QRCodeDimension;
            }

            if (GetModule(row, col))
            {
                info |= 1 << index;
            }
        }
        return TestFormatInfo(info);
    }

    ////////////////////////////////////////////////////////////////////
    // Test format info bits
    ////////////////////////////////////////////////////////////////////

    internal int TestFormatInfo(int formatInfo)
    {
        // format info
        var info = (formatInfo ^ 0x5412) >> 10;

        // test for exact match
        if (StaticTables.FormatInfoArray[info] == formatInfo)
        {
            return info;
        }

        // look for a match
        var bestInfo = 0;
        var error = int.MaxValue;
        for (var index = 0; index < 32; index++)
        {
            var errorCount = CountBits(StaticTables.FormatInfoArray[index] ^ formatInfo);
            if (errorCount < error)
            {
                error = errorCount;
                bestInfo = index;
            }
        }

        return error <= 3 ? bestInfo : -1;
    }

    ////////////////////////////////////////////////////////////////////
    // Count Bits
    ////////////////////////////////////////////////////////////////////

    internal int CountBits(int value)
    {
        var count = 0;
        for (var mask = 0x4000; mask != 0; mask >>= 1)
        {
            if ((value & mask) != 0)
            {
                count++;
            }
        }
        return count;
    }

    ////////////////////////////////////////////////////////////////////
    // Convert image to qr code matrix and test fixed modules
    ////////////////////////////////////////////////////////////////////

    internal void ConvertImageToMatrix()
    {
        // loop for all modules
        var fixedCount = 0;
        var errorCount = 0;
        for (var row = 0; row < QRCodeDimension; row++)
        {
            for (var col = 0; col < QRCodeDimension; col++)
            {
                // the module (Row, Col) is not a fixed module 
                if ((BaseMatrix[row, col] & StaticTables.Fixed) == 0)
                {
                    if (GetModule(row, col))
                    {
                        BaseMatrix[row, col] |= StaticTables.Black;
                    }
                }

                // fixed module
                else
                {
                    // total fixed modules
                    fixedCount++;

                    // test for error
                    if ((GetModule(row, col) ? StaticTables.Black : StaticTables.White) != (BaseMatrix[row, col] & 1))
                    {
                        errorCount++;
                    }
                }
            }
        }

        if (errorCount > fixedCount * ErrCorrPercent[(int)ErrorCorrection] / 100)
        {
            throw new ApplicationException("Fixed modules error");
        }
    }

    ////////////////////////////////////////////////////////////////////
    // Unload matrix data from base matrix
    ////////////////////////////////////////////////////////////////////

    internal void UnloadDataFromMatrix()
    {
        // input array pointer initialization
        var ptr = 0;
        var ptrEnd = 8 * MaxCodewords;
        CodewordsArray = new byte[MaxCodewords];

        // bottom right corner of output matrix
        var row = QRCodeDimension - 1;
        var col = QRCodeDimension - 1;

        // step state
        var state = 0;
        for (; ; )
        {
            // current module is data
            if ((MaskMatrix[row, col] & StaticTables.NonData) == 0)
            {
                // unload current module with
                if ((MaskMatrix[row, col] & 1) != 0)
                {
                    CodewordsArray[ptr >> 3] |= (byte)(1 << (7 - (ptr & 7)));
                }

                if (++ptr == ptrEnd)
                {
                    break;
                }
            }

            // current module is non data and vertical timing line condition is on
            else if (col == 6)
            {
                col--;
            }

            // update matrix position to next module
            switch (state)
            {
                // going up: step one to the left
                case 0:
                    col--;
                    state = 1;
                    continue;

                // going up: step one row up and one column to the right
                case 1:
                    col++;
                    row--;
                    // we are not at the top, go to state 0
                    if (row >= 0)
                    {
                        state = 0;
                        continue;
                    }
                    // we are at the top, step two columns to the left and start going down
                    col -= 2;
                    row = 0;
                    state = 2;
                    continue;

                // going down: step one to the left
                case 2:
                    col--;
                    state = 3;
                    continue;

                // going down: step one row down and one column to the right
                case 3:
                    col++;
                    row++;
                    // we are not at the bottom, go to state 2
                    if (row < QRCodeDimension)
                    {
                        state = 2;
                        continue;
                    }
                    // we are at the bottom, step two columns to the left and start going up
                    col -= 2;
                    row = QRCodeDimension - 1;
                    state = 0;
                    continue;
            }
        }
    }

    ////////////////////////////////////////////////////////////////////
    // Restore interleave data and error correction blocks
    ////////////////////////////////////////////////////////////////////

    internal void RestoreBlocks()
    {
        // allocate temp codewords array
        var tempArray = new byte[MaxCodewords];

        // total blocks
        var totalBlocks = BlocksGroup1 + BlocksGroup2;

        // create array of data blocks starting point
        var start = new int[totalBlocks];
        for (var index = 1; index < totalBlocks; index++)
        {
            start[index] = start[index - 1] + (index <= BlocksGroup1 ? DataCodewordsGroup1 : DataCodewordsGroup2);
        }

        // step one. interleave base on group one length
        var ptrEnd = DataCodewordsGroup1 * totalBlocks;

        // restore group one and two
        int ptr;
        var block = 0;
        for (ptr = 0; ptr < ptrEnd; ptr++)
        {
            tempArray[start[block]] = CodewordsArray[ptr];
            start[block]++;
            block++;
            if (block == totalBlocks) block = 0;
        }

        // restore group two
        if (DataCodewordsGroup2 > DataCodewordsGroup1)
        {
            // step one. iterleave base on group one length
            ptrEnd = MaxDataCodewords;

            block = BlocksGroup1;
            for (; ptr < ptrEnd; ptr++)
            {
                tempArray[start[block]] = CodewordsArray[ptr];
                start[block]++;
                block++;
                if (block == totalBlocks) block = BlocksGroup1;
            }
        }

        // create array of error correction blocks starting point
        start[0] = MaxDataCodewords;
        for (var index = 1; index < totalBlocks; index++)
        {
            start[index] = start[index - 1] + ErrCorrCodewords;
        }

        // restore all groups
        ptrEnd = MaxCodewords;
        block = 0;
        for (; ptr < ptrEnd; ptr++)
        {
            tempArray[start[block]] = CodewordsArray[ptr];
            start[block]++;
            block++;
            if (block == totalBlocks) block = 0;
        }

        // save result
        CodewordsArray = tempArray;
    }

    ////////////////////////////////////////////////////////////////////
    // Calculate Error Correction
    ////////////////////////////////////////////////////////////////////

    protected void CalculateErrorCorrection()
    {
        // set generator polynomial array
        var generator = StaticTables.GenArray[ErrCorrCodewords - 7];

        // error correction calculation buffer
        var bufSize = Math.Max(DataCodewordsGroup1, DataCodewordsGroup2) + ErrCorrCodewords;
        var errCorrBuff = new byte[bufSize];

        // initial number of data codewords
        var dataCodewords = DataCodewordsGroup1;
        var buffLen = dataCodewords + ErrCorrCodewords;

        // codewords pointer
        var dataCodewordsPtr = 0;

        // codewords buffer error correction pointer
        var codewordsArrayErrCorrPtr = MaxDataCodewords;

        // loop one block at a time
        var totalBlocks = BlocksGroup1 + BlocksGroup2;
        for (var blockNumber = 0; blockNumber < totalBlocks; blockNumber++)
        {
            // switch to group2 data codewords
            if (blockNumber == BlocksGroup1)
            {
                dataCodewords = DataCodewordsGroup2;
                buffLen = dataCodewords + ErrCorrCodewords;
            }

            // copy next block of codewords to the buffer and clear the remaining part
            Array.Copy(CodewordsArray, dataCodewordsPtr, errCorrBuff, 0, dataCodewords);
            Array.Copy(CodewordsArray, codewordsArrayErrCorrPtr, errCorrBuff, dataCodewords, ErrCorrCodewords);

            // make a duplicate
            var correctionBuffer = (byte[])errCorrBuff.Clone();

            // error correction polynomial division
            ReedSolomon.PolynominalDivision(errCorrBuff, buffLen, generator, ErrCorrCodewords);

            // test for error
            int index;
            for (index = 0; index < ErrCorrCodewords && errCorrBuff[dataCodewords + index] == 0; index++)
            {
                // continue
            }

            if (index < ErrCorrCodewords)
            {
                // correct the error
                var errorCount = ReedSolomon.CorrectData(correctionBuffer, buffLen, ErrCorrCodewords);
                if (errorCount <= 0)
                {
                    throw new ApplicationException("Data is damaged. Error correction failed");
                }

                // fix the data
                Array.Copy(correctionBuffer, 0, CodewordsArray, dataCodewordsPtr, dataCodewords);
            }

            // update codewords array to next buffer
            dataCodewordsPtr += dataCodewords;

            // update pointer				
            codewordsArrayErrCorrPtr += ErrCorrCodewords;
        }
    }

    ////////////////////////////////////////////////////////////////////
    // Convert bit array to byte array
    ////////////////////////////////////////////////////////////////////

    internal byte[] DecodeData()
    {
        // bit buffer initial condition
        BitBuffer = (uint)((CodewordsArray[0] << 24) | (CodewordsArray[1] << 16) | (CodewordsArray[2] << 8) | CodewordsArray[3]);
        BitBufferLen = 32;
        CodewordsPtr = 4;

        // allocate data byte list
        var dataSeg = new List<byte>();

        // reset ECI assignment value
        EciAssignValue = -1;

        // data might be made of blocks
        for (; ; )
        {
            // first 4 bits is mode indicator
            var encodingMode = (EncodingMode)ReadBitsFromCodewordsArray(4);

            // end of data
            if (encodingMode <= 0)
            {
                break;
            }

            // test for encoding ECI assignment number
            if (encodingMode == EncodingMode.Eci)
            {
                // one byte assinment value
                EciAssignValue = ReadBitsFromCodewordsArray(8);
                if ((EciAssignValue & 0x80) == 0)
                {
                    continue;
                }

                // two bytes assinment value
                EciAssignValue = (EciAssignValue << 8) | ReadBitsFromCodewordsArray(8);
                if ((EciAssignValue & 0x4000) == 0)
                {
                    EciAssignValue &= 0x3fff;
                    continue;
                }

                // three bytes assinment value
                EciAssignValue = (EciAssignValue << 8) | ReadBitsFromCodewordsArray(8);
                if ((EciAssignValue & 0x200000) == 0)
                {
                    EciAssignValue &= 0x1fffff;
                    continue;
                }

                throw new ApplicationException("ECI encoding assinment number in error");
            }

            // read data length
            var dataLength = ReadBitsFromCodewordsArray(DataLengthBits(encodingMode));
            if (dataLength < 0)
            {
                throw new ApplicationException("Premature end of data (DataLengh)");
            }

            // save start of segment
            var segStart = dataSeg.Count;

            // switch based on encode mode
            // numeric code indicator is 0001, alpha numeric 0010, byte 0100
            switch (encodingMode)
            {
                // numeric mode
                case EncodingMode.Numeric:
                    // encode digits in groups of 2
                    var numericEnd = dataLength / 3 * 3;
                    for (var index = 0; index < numericEnd; index += 3)
                    {
                        var temp = ReadBitsFromCodewordsArray(10);
                        if (temp < 0)
                        {
                            throw new ApplicationException("Premature end of data (Numeric 1)");
                        }
                        dataSeg.Add(StaticTables.DecodingTable[temp / 100]);
                        dataSeg.Add(StaticTables.DecodingTable[temp % 100 / 10]);
                        dataSeg.Add(StaticTables.DecodingTable[temp % 10]);
                    }

                    // we have one character remaining
                    if (dataLength - numericEnd == 1)
                    {
                        var temp = ReadBitsFromCodewordsArray(4);
                        if (temp < 0)
                        {
                            throw new ApplicationException("Premature end of data (Numeric 2)");
                        }
                        dataSeg.Add(StaticTables.DecodingTable[temp]);
                    }

                    // we have two character remaining
                    else if (dataLength - numericEnd == 2)
                    {
                        var temp = ReadBitsFromCodewordsArray(7);
                        if (temp < 0)
                        {
                            throw new ApplicationException("Premature end of data (Numeric 3)");
                        }
                        dataSeg.Add(StaticTables.DecodingTable[temp / 10]);
                        dataSeg.Add(StaticTables.DecodingTable[temp % 10]);
                    }
                    break;

                // alphanumeric mode
                case EncodingMode.AlphaNumeric:
                    // encode digits in groups of 2
                    var alphaNumEnd = dataLength / 2 * 2;
                    for (var index = 0; index < alphaNumEnd; index += 2)
                    {
                        var temp = ReadBitsFromCodewordsArray(11);
                        if (temp < 0)
                        {
                            throw new ApplicationException("Premature end of data (Alpha Numeric 1)");
                        }
                        dataSeg.Add(StaticTables.DecodingTable[temp / 45]);
                        dataSeg.Add(StaticTables.DecodingTable[temp % 45]);
                    }

                    // we have one character remaining
                    if (dataLength - alphaNumEnd == 1)
                    {
                        var temp = ReadBitsFromCodewordsArray(6);
                        if (temp < 0)
                        {
                            throw new ApplicationException("Premature end of data (Alpha Numeric 2)");
                        }
                        dataSeg.Add(StaticTables.DecodingTable[temp]);
                    }
                    break;

                // byte mode					
                case EncodingMode.Byte:
                    // append the data after mode and character count
                    for (var index = 0; index < dataLength; index++)
                    {
                        var temp = ReadBitsFromCodewordsArray(8);
                        if (temp < 0)
                        {
                            throw new ApplicationException("Premature end of data (byte mode)");
                        }
                        dataSeg.Add((byte)temp);
                    }
                    break;

                default:
                    throw new ApplicationException($"Encoding mode not supported {encodingMode.ToString()}");
            }

            if (dataLength != dataSeg.Count - segStart) throw new ApplicationException("Data encoding length in error");
        }

        // save data
        return dataSeg.ToArray();
    }

    ////////////////////////////////////////////////////////////////////
    // Read data from codeword array
    ////////////////////////////////////////////////////////////////////

    internal int ReadBitsFromCodewordsArray(int bits)
    {
        if (bits > BitBufferLen)
        {
            return -1;
        }

        var data = (int)(BitBuffer >> (32 - bits));
        BitBuffer <<= bits;
        BitBufferLen -= bits;
        while (BitBufferLen <= 24 && CodewordsPtr < MaxDataCodewords)
        {
            BitBuffer |= (uint)(CodewordsArray[CodewordsPtr++] << (24 - BitBufferLen));
            BitBufferLen += 8;
        }
        return data;
    }
    ////////////////////////////////////////////////////////////////////
    // Set encoded data bits length
    ////////////////////////////////////////////////////////////////////

    internal int DataLengthBits(EncodingMode encodingMode)
    {
        // Data length bits
        switch (encodingMode)
        {
            // numeric mode
            case EncodingMode.Numeric:
                return QRCodeVersion < 10 ? 10 : QRCodeVersion < 27 ? 12 : 14;

            // alpha numeric mode
            case EncodingMode.AlphaNumeric:
                return QRCodeVersion < 10 ? 9 : QRCodeVersion < 27 ? 11 : 13;

            // byte mode
            case EncodingMode.Byte:
                return QRCodeVersion < 10 ? 8 : 16;
        }
        throw new ApplicationException("Unsupported encoding mode " + encodingMode.ToString());
    }

    ////////////////////////////////////////////////////////////////////
    // Set data and error correction codewords length
    ////////////////////////////////////////////////////////////////////

    internal void SetDataCodewordsLength()
    {
        // index shortcut
        var blockInfoIndex = (QRCodeVersion - 1) * 4 + (int)ErrorCorrection;

        // Number of blocks in group 1
        BlocksGroup1 = StaticTables.EcBlockInfo[blockInfoIndex, StaticTables.BLOCKS_GROUP1];

        // Number of data codewords in blocks of group 1
        DataCodewordsGroup1 = StaticTables.EcBlockInfo[blockInfoIndex, StaticTables.DATA_CODEWORDS_GROUP1];

        // Number of blocks in group 2
        BlocksGroup2 = StaticTables.EcBlockInfo[blockInfoIndex, StaticTables.BLOCKS_GROUP2];

        // Number of data codewords in blocks of group 2
        DataCodewordsGroup2 = StaticTables.EcBlockInfo[blockInfoIndex, StaticTables.DATA_CODEWORDS_GROUP2];

        // Total number of data codewords for this version and EC level
        MaxDataCodewords = BlocksGroup1 * DataCodewordsGroup1 + BlocksGroup2 * DataCodewordsGroup2;
        MaxDataBits = 8 * MaxDataCodewords;

        // total data plus error correction bits
        MaxCodewords = StaticTables.MaxCodewordsArray[QRCodeVersion];

        // Error correction codewords per block
        ErrCorrCodewords = (MaxCodewords - MaxDataCodewords) / (BlocksGroup1 + BlocksGroup2);

        // exit
    }

    ////////////////////////////////////////////////////////////////////
    // Format info to error correction code
    ////////////////////////////////////////////////////////////////////

    internal ErrorCorrection FormatInfoToErrCode(int info)
    {
        return (ErrorCorrection)(info ^ 1);
    }

    ////////////////////////////////////////////////////////////////////
    // Build Base Matrix
    ////////////////////////////////////////////////////////////////////

    internal void BuildBaseMatrix()
    {
        // allocate base matrix
        BaseMatrix = new byte[QRCodeDimension + 5, QRCodeDimension + 5];

        // top left finder patterns
        for (var row = 0; row < 9; row++)
        {
            for (var col = 0; col < 9; col++) BaseMatrix[row, col] = StaticTables.FinderPatternTopLeft[row, col];
        }

        // top right finder patterns
        var pos = QRCodeDimension - 8;
        for (var row = 0; row < 9; row++)
        {
            for (var col = 0; col < 8; col++) BaseMatrix[row, pos + col] = StaticTables.FinderPatternTopRight[row, col];
        }

        // bottom left finder patterns
        for (var row = 0; row < 8; row++)
        {
            for (var col = 0; col < 9; col++) BaseMatrix[pos + row, col] = StaticTables.FinderPatternBottomLeft[row, col];
        }

        // Timing pattern
        for (var z = 8; z < QRCodeDimension - 8; z++)
        {
            BaseMatrix[z, 6] = BaseMatrix[6, z] = (z & 1) == 0 ? StaticTables.FixedBlack : StaticTables.FixedWhite;
        }

        // alignment pattern
        if (QRCodeVersion > 1)
        {
            var alignPos = StaticTables.AlignmentPositionArray[QRCodeVersion];
            var alignmentDimension = alignPos.Length;
            for (var row = 0; row < alignmentDimension; row++)
            {
                for (var col = 0; col < alignmentDimension; col++)
                {
                    if (col == 0 && row == 0 || col == alignmentDimension - 1 && row == 0 || col == 0 && row == alignmentDimension - 1)
                    {
                        continue;
                    }

                    int posRow = alignPos[row];
                    int posCol = alignPos[col];
                    for (var aRow = -2; aRow < 3; aRow++)
                    {
                        for (var aCol = -2; aCol < 3; aCol++)
                        {
                            BaseMatrix[posRow + aRow, posCol + aCol] = StaticTables.AlignmentPattern[aRow + 2, aCol + 2];
                        }
                    }
                }
            }
        }

        // reserve version information
        if (QRCodeVersion >= 7)
        {
            // position of 3 by 6 rectangles
            pos = QRCodeDimension - 11;

            // top right
            for (var row = 0; row < 6; row++)
            {
                for (var col = 0; col < 3; col++) BaseMatrix[row, pos + col] = StaticTables.FormatWhite;
            }

            // bottom right
            for (var col = 0; col < 6; col++)
            {
                for (var row = 0; row < 3; row++) BaseMatrix[pos + row, col] = StaticTables.FormatWhite;
            }
        }
    }

    ////////////////////////////////////////////////////////////////////
    // Apply Mask
    ////////////////////////////////////////////////////////////////////

    internal void ApplyMask(int mask)
    {
        MaskMatrix = (byte[,])BaseMatrix.Clone();
        switch (mask)
        {
            case 0:
                ApplyMask0();
                break;

            case 1:
                ApplyMask1();
                break;

            case 2:
                ApplyMask2();
                break;

            case 3:
                ApplyMask3();
                break;

            case 4:
                ApplyMask4();
                break;

            case 5:
                ApplyMask5();
                break;

            case 6:
                ApplyMask6();
                break;

            case 7:
                ApplyMask7();
                break;
        }
    }

    ////////////////////////////////////////////////////////////////////
    // Apply Mask 0
    // (row + column) % 2 == 0
    ////////////////////////////////////////////////////////////////////

    internal void ApplyMask0()
    {
        for (var row = 0; row < QRCodeDimension; row += 2)
        {
            for (var col = 0; col < QRCodeDimension; col += 2)
            {
                if ((MaskMatrix[row, col] & StaticTables.NonData) == 0)
                {
                    MaskMatrix[row, col] ^= 1;
                }

                if ((MaskMatrix[row + 1, col + 1] & StaticTables.NonData) == 0)
                {
                    MaskMatrix[row + 1, col + 1] ^= 1;
                }
            }
        }
    }

    ////////////////////////////////////////////////////////////////////
    // Apply Mask 1
    // row % 2 == 0
    ////////////////////////////////////////////////////////////////////

    internal void ApplyMask1()
    {
        for (var row = 0; row < QRCodeDimension; row += 2)
        {
            for (var col = 0; col < QRCodeDimension; col++)
            {
                if ((MaskMatrix[row, col] & StaticTables.NonData) == 0)
                {
                    MaskMatrix[row, col] ^= 1;
                }
            }
        }
    }

    ////////////////////////////////////////////////////////////////////
    // Apply Mask 2
    // column % 3 == 0
    ////////////////////////////////////////////////////////////////////

    internal void ApplyMask2()
    {
        for (var row = 0; row < QRCodeDimension; row++)
        {
            for (var col = 0; col < QRCodeDimension; col += 3)
            {
                if ((MaskMatrix[row, col] & StaticTables.NonData) == 0)
                {
                    MaskMatrix[row, col] ^= 1;
                }
            }
        }
    }

    ////////////////////////////////////////////////////////////////////
    // Apply Mask 3
    // (row + column) % 3 == 0
    ////////////////////////////////////////////////////////////////////

    internal void ApplyMask3()
    {
        for (var row = 0; row < QRCodeDimension; row += 3)
        {
            for (var col = 0; col < QRCodeDimension; col += 3)
            {
                if ((MaskMatrix[row, col] & StaticTables.NonData) == 0)
                {
                    MaskMatrix[row, col] ^= 1;
                }
                if ((MaskMatrix[row + 1, col + 2] & StaticTables.NonData) == 0)
                {
                    MaskMatrix[row + 1, col + 2] ^= 1;
                }
                if ((MaskMatrix[row + 2, col + 1] & StaticTables.NonData) == 0)
                {
                    MaskMatrix[row + 2, col + 1] ^= 1;
                }
            }
        }
    }

    ////////////////////////////////////////////////////////////////////
    // Apply Mask 4
    // ((row / 2) + (column / 3)) % 2 == 0
    ////////////////////////////////////////////////////////////////////

    internal void ApplyMask4()
    {
        for (var row = 0; row < QRCodeDimension; row += 4)
        {
            for (var col = 0; col < QRCodeDimension; col += 6)
            {
                if ((MaskMatrix[row, col] & StaticTables.NonData) == 0) MaskMatrix[row, col] ^= 1;
                if ((MaskMatrix[row, col + 1] & StaticTables.NonData) == 0) MaskMatrix[row, col + 1] ^= 1;
                if ((MaskMatrix[row, col + 2] & StaticTables.NonData) == 0) MaskMatrix[row, col + 2] ^= 1;

                if ((MaskMatrix[row + 1, col] & StaticTables.NonData) == 0) MaskMatrix[row + 1, col] ^= 1;
                if ((MaskMatrix[row + 1, col + 1] & StaticTables.NonData) == 0) MaskMatrix[row + 1, col + 1] ^= 1;
                if ((MaskMatrix[row + 1, col + 2] & StaticTables.NonData) == 0) MaskMatrix[row + 1, col + 2] ^= 1;

                if ((MaskMatrix[row + 2, col + 3] & StaticTables.NonData) == 0) MaskMatrix[row + 2, col + 3] ^= 1;
                if ((MaskMatrix[row + 2, col + 4] & StaticTables.NonData) == 0) MaskMatrix[row + 2, col + 4] ^= 1;
                if ((MaskMatrix[row + 2, col + 5] & StaticTables.NonData) == 0) MaskMatrix[row + 2, col + 5] ^= 1;

                if ((MaskMatrix[row + 3, col + 3] & StaticTables.NonData) == 0) MaskMatrix[row + 3, col + 3] ^= 1;
                if ((MaskMatrix[row + 3, col + 4] & StaticTables.NonData) == 0) MaskMatrix[row + 3, col + 4] ^= 1;
                if ((MaskMatrix[row + 3, col + 5] & StaticTables.NonData) == 0) MaskMatrix[row + 3, col + 5] ^= 1;
            }
        }
    }

    ////////////////////////////////////////////////////////////////////
    // Apply Mask 5
    // ((row * column) % 2) + ((row * column) % 3) == 0
    ////////////////////////////////////////////////////////////////////

    internal void ApplyMask5()
    {
        for (var row = 0; row < QRCodeDimension; row += 6)
        {
            for (var col = 0; col < QRCodeDimension; col += 6)
            {
                for (var delta = 0; delta < 6; delta++)
                {
                    if ((MaskMatrix[row, col + delta] & StaticTables.NonData) == 0)
                    {
                        MaskMatrix[row, col + delta] ^= 1;
                    }
                }
                for (var delta = 1; delta < 6; delta++)
                {
                    if ((MaskMatrix[row + delta, col] & StaticTables.NonData) == 0)
                    {
                        MaskMatrix[row + delta, col] ^= 1;
                    }
                }
                if ((MaskMatrix[row + 2, col + 3] & StaticTables.NonData) == 0) MaskMatrix[row + 2, col + 3] ^= 1;
                if ((MaskMatrix[row + 3, col + 2] & StaticTables.NonData) == 0) MaskMatrix[row + 3, col + 2] ^= 1;
                if ((MaskMatrix[row + 3, col + 4] & StaticTables.NonData) == 0) MaskMatrix[row + 3, col + 4] ^= 1;
                if ((MaskMatrix[row + 4, col + 3] & StaticTables.NonData) == 0) MaskMatrix[row + 4, col + 3] ^= 1;
            }
        }
    }

    ////////////////////////////////////////////////////////////////////
    // Apply Mask 6
    // (((row * column) % 2) + ((row * column) mod 3)) mod 2 == 0
    ////////////////////////////////////////////////////////////////////

    internal void ApplyMask6()
    {
        for (var row = 0; row < QRCodeDimension; row += 6)
        {
            for (var col = 0; col < QRCodeDimension; col += 6)
            {
                for (var delta = 0; delta < 6; delta++)
                {
                    if ((MaskMatrix[row, col + delta] & StaticTables.NonData) == 0)
                    {
                        MaskMatrix[row, col + delta] ^= 1;
                    }
                }
                for (var delta = 1; delta < 6; delta++)
                {
                    if ((MaskMatrix[row + delta, col] & StaticTables.NonData) == 0)
                    {
                        MaskMatrix[row + delta, col] ^= 1;
                    }
                }
                if ((MaskMatrix[row + 1, col + 1] & StaticTables.NonData) == 0) MaskMatrix[row + 1, col + 1] ^= 1;
                if ((MaskMatrix[row + 1, col + 2] & StaticTables.NonData) == 0) MaskMatrix[row + 1, col + 2] ^= 1;
                if ((MaskMatrix[row + 2, col + 1] & StaticTables.NonData) == 0) MaskMatrix[row + 2, col + 1] ^= 1;
                if ((MaskMatrix[row + 2, col + 3] & StaticTables.NonData) == 0) MaskMatrix[row + 2, col + 3] ^= 1;
                if ((MaskMatrix[row + 2, col + 4] & StaticTables.NonData) == 0) MaskMatrix[row + 2, col + 4] ^= 1;
                if ((MaskMatrix[row + 3, col + 2] & StaticTables.NonData) == 0) MaskMatrix[row + 3, col + 2] ^= 1;
                if ((MaskMatrix[row + 3, col + 4] & StaticTables.NonData) == 0) MaskMatrix[row + 3, col + 4] ^= 1;
                if ((MaskMatrix[row + 4, col + 2] & StaticTables.NonData) == 0) MaskMatrix[row + 4, col + 2] ^= 1;
                if ((MaskMatrix[row + 4, col + 3] & StaticTables.NonData) == 0) MaskMatrix[row + 4, col + 3] ^= 1;
                if ((MaskMatrix[row + 4, col + 5] & StaticTables.NonData) == 0) MaskMatrix[row + 4, col + 5] ^= 1;
                if ((MaskMatrix[row + 5, col + 4] & StaticTables.NonData) == 0) MaskMatrix[row + 5, col + 4] ^= 1;
                if ((MaskMatrix[row + 5, col + 5] & StaticTables.NonData) == 0) MaskMatrix[row + 5, col + 5] ^= 1;
            }
        }
    }

    ////////////////////////////////////////////////////////////////////
    // Apply Mask 7
    // (((row + column) % 2) + ((row * column) mod 3)) mod 2 == 0
    ////////////////////////////////////////////////////////////////////

    internal void ApplyMask7()
    {
        for (var row = 0; row < QRCodeDimension; row += 6)
        {
            for (var col = 0; col < QRCodeDimension; col += 6)
            {
                if ((MaskMatrix[row, col] & StaticTables.NonData) == 0) MaskMatrix[row, col] ^= 1;
                if ((MaskMatrix[row, col + 2] & StaticTables.NonData) == 0) MaskMatrix[row, col + 2] ^= 1;
                if ((MaskMatrix[row, col + 4] & StaticTables.NonData) == 0) MaskMatrix[row, col + 4] ^= 1;

                if ((MaskMatrix[row + 1, col + 3] & StaticTables.NonData) == 0) MaskMatrix[row + 1, col + 3] ^= 1;
                if ((MaskMatrix[row + 1, col + 4] & StaticTables.NonData) == 0) MaskMatrix[row + 1, col + 4] ^= 1;
                if ((MaskMatrix[row + 1, col + 5] & StaticTables.NonData) == 0) MaskMatrix[row + 1, col + 5] ^= 1;

                if ((MaskMatrix[row + 2, col] & StaticTables.NonData) == 0) MaskMatrix[row + 2, col] ^= 1;
                if ((MaskMatrix[row + 2, col + 4] & StaticTables.NonData) == 0) MaskMatrix[row + 2, col + 4] ^= 1;
                if ((MaskMatrix[row + 2, col + 5] & StaticTables.NonData) == 0) MaskMatrix[row + 2, col + 5] ^= 1;

                if ((MaskMatrix[row + 3, col + 1] & StaticTables.NonData) == 0) MaskMatrix[row + 3, col + 1] ^= 1;
                if ((MaskMatrix[row + 3, col + 3] & StaticTables.NonData) == 0) MaskMatrix[row + 3, col + 3] ^= 1;
                if ((MaskMatrix[row + 3, col + 5] & StaticTables.NonData) == 0) MaskMatrix[row + 3, col + 5] ^= 1;

                if ((MaskMatrix[row + 4, col] & StaticTables.NonData) == 0) MaskMatrix[row + 4, col] ^= 1;
                if ((MaskMatrix[row + 4, col + 1] & StaticTables.NonData) == 0) MaskMatrix[row + 4, col + 1] ^= 1;
                if ((MaskMatrix[row + 4, col + 2] & StaticTables.NonData) == 0) MaskMatrix[row + 4, col + 2] ^= 1;

                if ((MaskMatrix[row + 5, col + 1] & StaticTables.NonData) == 0) MaskMatrix[row + 5, col + 1] ^= 1;
                if ((MaskMatrix[row + 5, col + 2] & StaticTables.NonData) == 0) MaskMatrix[row + 5, col + 2] ^= 1;
                if ((MaskMatrix[row + 5, col + 3] & StaticTables.NonData) == 0) MaskMatrix[row + 5, col + 3] ^= 1;
            }
        }
    }
}