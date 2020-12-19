// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
//
// Changes:
// DE 12-11-2019 - Refactoring and renaming

using System;
using System.IO;
using System.Text;

namespace TestAutomation.SolutionHandler.Core
{
    public static class FileHandler
    {
	    private const int SizeLimitInBytes = 500000;

	    public static StreamReader OpenStream(Stream stream, Encoding defaultEncoding)
		{
			if (stream == null)
				throw new ArgumentNullException("Stream should not be null");
			if (stream.Position != 0)
				throw new ArgumentException("stream is not positioned at beginning.", "stream");
			if (defaultEncoding == null)
				defaultEncoding = Encoding.UTF8;

			if (stream.Length < 2) return new StreamReader(stream, defaultEncoding);
			
			// the autodetection of StreamReader is not capable of detecting the difference
			// between ISO-8859-1 and UTF-8 without BOM.
			var firstByte = stream.ReadByte();
			var secondByte = stream.ReadByte();
			switch ((firstByte << 8) | secondByte) {
				case 0x0000: // either UTF-32 Big Endian or a binary file; use StreamReader
				case 0xfffe: // Unicode BOM (UTF-16 LE or UTF-32 LE)
				case 0xfeff: // UTF-16 BE BOM
				case 0xefbb: // start of UTF-8 BOM
					// StreamReader autodetection works
					stream.Position = 0;
					return new StreamReader(stream);
				default:
					return AutoDetect(stream, (byte)firstByte, (byte)secondByte, defaultEncoding);
			}
		}

	    private static readonly Encoding Utf8NoBom = new UTF8Encoding(false);

	    public static StreamReader AutoDetect(Stream fs, byte firstByte, byte secondByte, Encoding defaultEncoding)
		{
			var sizeLimit = (int)Math.Min(fs.Length, SizeLimitInBytes); // look at max. 500 KB
			
			const int ascii = 0;
			const int error = 1;
			const int utf8 = 2;
			const int utf8Sequence = 3;
			
			var state = ascii;
			var sequenceLength = 0;
			for (var currentByte = 0; currentByte < sizeLimit; currentByte++) {
				var byteSequence = GetByteSequence(fs, firstByte, secondByte, currentByte);

				if (byteSequence < 0x80) {
					// normal ASCII character
					if (state != utf8Sequence) continue;
					state = error;
					break;
				}

				if (byteSequence < 0xc0) {
					// 10xxxxxx : continues UTF8 byte sequence
					if (state == utf8Sequence) {
						--sequenceLength;
						if (sequenceLength < 0) {
							state = error;
							break;
						}

						if (sequenceLength == 0) {
							state = utf8;
						}
					} else {
						state = error;
						break;
					}
				} else if (byteSequence >= 0xc2 && byteSequence < 0xf5) {
					// beginning of byte sequence
					if (state == utf8 || state == ascii) {
						state = utf8Sequence;
						if (byteSequence < 0xe0) {
							sequenceLength = 1; // one more byte following
						} else if (byteSequence < 0xf0) {
							sequenceLength = 2; // two more bytes following
						} else {
							sequenceLength = 3; // three more bytes following
						}
					} else {
						state = error;
						break;
					}
				} else {
					// 0xc0, 0xc1, 0xf5 to 0xff are invalid in UTF-8 (see RFC 3629)
					state = error;
					break;
				}
			}
			fs.Position = 0;
			switch (state) {
				case ascii:
					return new StreamReader(fs, IsAsciiCompatible(defaultEncoding) ? RemoveBom(defaultEncoding) : Encoding.ASCII);
				case error:
					// When the file seems to be non-UTF8,
					// we read it using the user-specified encoding so it is saved again
					// using that encoding.
					if (IsUnicode(defaultEncoding)) {
						// the file is not Unicode, so don't read it using Unicode even if the
						// user has choosen Unicode as the default encoding.

						defaultEncoding = Encoding.Default; // use system encoding instead
					}
					return new StreamReader(fs, RemoveBom(defaultEncoding));
				default:
					return new StreamReader(fs, Utf8NoBom);
			}
		}

	    private static byte GetByteSequence(Stream fs, byte firstByte, byte secondByte, int i)
	    {
		    byte byteSequence;

		    switch (i)
		    {
			    case 0:
				    byteSequence = firstByte;
				    break;
			    case 1:
				    byteSequence = secondByte;
				    break;
			    default:
				    byteSequence = (byte) fs.ReadByte();
				    break;
		    }

		    return byteSequence;
	    }

	    private static bool IsAsciiCompatible(Encoding encoding)
	    {
		    var bytes = encoding.GetBytes("Az");
		    return bytes.Length == 2 && bytes[0] == 'A' && bytes[1] == 'z';
	    }

	    private static Encoding RemoveBom(Encoding encoding)
	    {
		    switch (encoding.CodePage) {
			    case 65001: // UTF-8
				    return Utf8NoBom;
			    default:
				    return encoding;
		    }
	    }

	    private static bool IsUnicode(Encoding encoding)
	    {
		    if (encoding == null)
			    throw new ArgumentNullException("encoding shouldn't be null.");
		    switch (encoding.CodePage) {
			    case 65000: // UTF-7
			    case 65001: // UTF-8
			    case 1200: // UTF-16 LE
			    case 1201: // UTF-16 BE
			    case 12000: // UTF-32 LE
			    case 12001: // UTF-32 BE
				    return true;
			    default:
				    return false;
		    }
	    }
    }
}