﻿/////////////////////////////////////////////////////////////////////
//
//	QR Code Encoder Library
//
//	ZLib compression for PNG files
//
//	Author: Uzi Granot
//	Original Version: 1.0
//	Date: June 30, 2018
//	Copyright (C) 2018-2019 Uzi Granot. All Rights Reserved
//	For full version history please look at QREncode.cs
//
//	QR Code Library C# class library and the attached test/demo
//  applications are free software.
//	Software developed by this author is licensed under CPOL 1.02.
//	Some portions of the QRCodeVideoDecoder are licensed under GNU Lesser
//	General Public License v3.0.
//
//	The solution is made of 3 projects:
//	1. QRCodeEncoderLibrary: QR code encoding.
//	2. QRCodeEncoderDemo: Create QR Code images.
//	3. QRCodeConsoleDemo: Demo app for net standard
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

using System.IO.Compression;

// ReSharper disable once CheckNamespace
namespace QRCodeEncoderLibrary;

internal static class ZLibCompression
{
    internal static byte[] Compress(byte[] inputBuf)
    {
        // input length
        var inputLen = inputBuf.Length;

        // create output memory stream to receive the compressed buffer
        var outputStream = new MemoryStream();

        // deflate compression object
        var deflate = new DeflateStream(outputStream, CompressionMode.Compress, true);

        // load input buffer into the compression class
        deflate.Write(inputBuf, 0, inputLen);

        // compress, flush and close
        deflate.Close();

        // compressed file length
        var outputLen = (int)outputStream.Length;

        // create empty output buffer
        var outputBuf = new byte[outputLen + 18];

        // Header is made out of 16 bits [iiiicccclldxxxxx]
        // iiii is compression information. It is WindowBit - 8 in this case 7. iiii = 0111
        // cccc is compression method. Deflate (8 dec) or Store (0 dec)
        // The first byte is 0x78 for deflate and 0x70 for store
        // ll is compression level 2
        // d is preset dictionary. The preset dictionary is not supported by this program. d is always 0
        // xxx is 5 bit check sum (31 - header % 31)
        // write two bytes in most significant byte first
        outputBuf[8] = 0x78;
        outputBuf[9] = 0x9c;

        // copy the compressed result
        outputStream.Seek(0, SeekOrigin.Begin);
        outputStream.Read(outputBuf, 10, outputLen);
        outputStream.Close();

        // successful exit
        return outputBuf;
    }
}