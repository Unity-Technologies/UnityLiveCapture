using System;
using System.Diagnostics;
using System.IO;
using Unity.LiveCapture.VideoStreaming.Client.RawFrames.Video;
using Unity.LiveCapture.VideoStreaming.Client.Utils;

namespace Unity.LiveCapture.VideoStreaming.Client.MediaParsers
{
    class MJPEGVideoPayloadParser : MediaPayloadParser
    {
        private const int JpegHeaderSize = 8;

        private static readonly ArraySegment<byte> JpegEndMarkerByteSegment =
            new ArraySegment<byte>(RawJpegFrame.EndMarkerBytes);

        private static readonly byte[] DefaultQuantizers =
        {
            16, 11, 12, 14, 12, 10, 16, 14,
            13, 14, 18, 17, 16, 19, 24, 40,
            26, 24, 22, 22, 24, 49, 35, 37,
            29, 40, 58, 51, 61, 60, 57, 51,
            56, 55, 64, 72, 92, 78, 64, 68,
            87, 69, 55, 56, 80, 109, 81, 87,
            95, 98, 103, 104, 103, 62, 77, 113,
            121, 112, 100, 120, 92, 101, 103, 99,
            17, 18, 18, 24, 21, 24, 47, 26,
            26, 47, 99, 66, 56, 66, 99, 99,
            99, 99, 99, 99, 99, 99, 99, 99,
            99, 99, 99, 99, 99, 99, 99, 99,
            99, 99, 99, 99, 99, 99, 99, 99,
            99, 99, 99, 99, 99, 99, 99, 99,
            99, 99, 99, 99, 99, 99, 99, 99,
            99, 99, 99, 99, 99, 99, 99, 99
        };

        private static readonly byte[] LumDcCodelens =
        {
            0, 1, 5, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0
        };

        private static readonly byte[] LumDcSymbols =
        {
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11
        };

        private static readonly byte[] LumAcCodelens =
        {
            0, 2, 1, 3, 3, 2, 4, 3, 5, 5, 4, 4, 0, 0, 1, 0x7d
        };

        private static readonly byte[] LumAcSymbols =
        {
            0x01, 0x02, 0x03, 0x00, 0x04, 0x11, 0x05, 0x12,
            0x21, 0x31, 0x41, 0x06, 0x13, 0x51, 0x61, 0x07,
            0x22, 0x71, 0x14, 0x32, 0x81, 0x91, 0xa1, 0x08,
            0x23, 0x42, 0xb1, 0xc1, 0x15, 0x52, 0xd1, 0xf0,
            0x24, 0x33, 0x62, 0x72, 0x82, 0x09, 0x0a, 0x16,
            0x17, 0x18, 0x19, 0x1a, 0x25, 0x26, 0x27, 0x28,
            0x29, 0x2a, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39,
            0x3a, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48, 0x49,
            0x4a, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59,
            0x5a, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69,
            0x6a, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78, 0x79,
            0x7a, 0x83, 0x84, 0x85, 0x86, 0x87, 0x88, 0x89,
            0x8a, 0x92, 0x93, 0x94, 0x95, 0x96, 0x97, 0x98,
            0x99, 0x9a, 0xa2, 0xa3, 0xa4, 0xa5, 0xa6, 0xa7,
            0xa8, 0xa9, 0xaa, 0xb2, 0xb3, 0xb4, 0xb5, 0xb6,
            0xb7, 0xb8, 0xb9, 0xba, 0xc2, 0xc3, 0xc4, 0xc5,
            0xc6, 0xc7, 0xc8, 0xc9, 0xca, 0xd2, 0xd3, 0xd4,
            0xd5, 0xd6, 0xd7, 0xd8, 0xd9, 0xda, 0xe1, 0xe2,
            0xe3, 0xe4, 0xe5, 0xe6, 0xe7, 0xe8, 0xe9, 0xea,
            0xf1, 0xf2, 0xf3, 0xf4, 0xf5, 0xf6, 0xf7, 0xf8,
            0xf9, 0xfa
        };

        private static readonly byte[] ChmDcCodelens =
        {
            0, 3, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0
        };

        private static readonly byte[] ChmDcSymbols =
        {
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11
        };

        private static readonly byte[] ChmAcCodelens =
        {
            0, 2, 1, 2, 4, 4, 3, 4, 7, 5, 4, 4, 0, 1, 2, 0x77
        };

        private static readonly byte[] ChmAcSymbols =
        {
            0x00, 0x01, 0x02, 0x03, 0x11, 0x04, 0x05, 0x21,
            0x31, 0x06, 0x12, 0x41, 0x51, 0x07, 0x61, 0x71,
            0x13, 0x22, 0x32, 0x81, 0x08, 0x14, 0x42, 0x91,
            0xa1, 0xb1, 0xc1, 0x09, 0x23, 0x33, 0x52, 0xf0,
            0x15, 0x62, 0x72, 0xd1, 0x0a, 0x16, 0x24, 0x34,
            0xe1, 0x25, 0xf1, 0x17, 0x18, 0x19, 0x1a, 0x26,
            0x27, 0x28, 0x29, 0x2a, 0x35, 0x36, 0x37, 0x38,
            0x39, 0x3a, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48,
            0x49, 0x4a, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58,
            0x59, 0x5a, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68,
            0x69, 0x6a, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78,
            0x79, 0x7a, 0x82, 0x83, 0x84, 0x85, 0x86, 0x87,
            0x88, 0x89, 0x8a, 0x92, 0x93, 0x94, 0x95, 0x96,
            0x97, 0x98, 0x99, 0x9a, 0xa2, 0xa3, 0xa4, 0xa5,
            0xa6, 0xa7, 0xa8, 0xa9, 0xaa, 0xb2, 0xb3, 0xb4,
            0xb5, 0xb6, 0xb7, 0xb8, 0xb9, 0xba, 0xc2, 0xc3,
            0xc4, 0xc5, 0xc6, 0xc7, 0xc8, 0xc9, 0xca, 0xd2,
            0xd3, 0xd4, 0xd5, 0xd6, 0xd7, 0xd8, 0xd9, 0xda,
            0xe2, 0xe3, 0xe4, 0xe5, 0xe6, 0xe7, 0xe8, 0xe9,
            0xea, 0xf2, 0xf3, 0xf4, 0xf5, 0xf6, 0xf7, 0xf8,
            0xf9, 0xfa
        };

        private readonly MemoryStream _frameStream;

        private int _currentDri;
        private int _currentQ;
        private int _currentType;
        private int _currentFrameWidth;
        private int _currentFrameHeight;

        private bool _hasExternalQuantizationTable;

        private byte[] _jpegHeaderBytes = new byte[0];
        private ArraySegment<byte> _jpegHeaderBytesSegment;

        private byte[] _quantizationTables = new byte[0];
        private int _quantizationTablesLength;
        private TimeSpan _previousTimeOffset = TimeSpan.MinValue;

        public MJPEGVideoPayloadParser()
        {
            _frameStream = new MemoryStream(64 * 1024);
        }

        public override void Parse(TimeSpan timeOffset, ArraySegment<byte> byteSegment, bool markerBit)
        {
            Debug.Assert(byteSegment.Array != null, "byteSegment.Array != null");

            if (byteSegment.Count < JpegHeaderSize)
                throw new MediaPayloadParserException("Input data size is smaller than JPEG header size");

            if (_previousTimeOffset != timeOffset && _frameStream.Position != 0)
                _frameStream.Position = 0;

            _previousTimeOffset = timeOffset;

            int offset = byteSegment.Offset + 1;

            int fragmentOffset = BigEndianConverter.ReadUInt24(byteSegment.Array, offset);
            offset += 3;

            int type = byteSegment.Array[offset++];
            int q = byteSegment.Array[offset++];
            int width = byteSegment.Array[offset++] * 8;
            int height = byteSegment.Array[offset++] * 8;
            int dri = 0;

            if (type > 63)
            {
                dri = BigEndianConverter.ReadUInt16(byteSegment.Array, offset);
                offset += 4;
            }

            if (fragmentOffset == 0)
            {
                if (_frameStream.Position != 0)
                    GenerateFrame(timeOffset);

                bool quantizationTablesChanged = false;

                if (q > 127)
                {
                    int mbz = byteSegment.Array[offset];

                    if (mbz == 0)
                    {
                        _hasExternalQuantizationTable = true;

                        int quantizationTablesLength = BigEndianConverter.ReadUInt16(byteSegment.Array, offset + 2);
                        offset += 4;

                        if (!ArrayUtils.IsBytesEquals(byteSegment.Array, offset, quantizationTablesLength,
                            _quantizationTables, 0, _quantizationTablesLength))
                        {
                            if (_quantizationTables.Length < quantizationTablesLength)
                                _quantizationTables = new byte[quantizationTablesLength];

                            Buffer.BlockCopy(byteSegment.Array, offset, _quantizationTables, 0,
                                quantizationTablesLength);
                            _quantizationTablesLength = quantizationTablesLength;
                            quantizationTablesChanged = true;
                        }

                        offset += quantizationTablesLength;
                    }
                }

                if (quantizationTablesChanged || _currentType != type || _currentQ != q ||
                    _currentFrameWidth != width || _currentFrameHeight != height || _currentDri != dri)
                {
                    _currentType = type;
                    _currentQ = q;
                    _currentFrameWidth = width;
                    _currentFrameHeight = height;
                    _currentDri = dri;

                    ReInitializeJpegHeader();
                }

                _frameStream.Write(_jpegHeaderBytesSegment.Array, _jpegHeaderBytesSegment.Offset, _jpegHeaderBytesSegment.Count);
            }

            if (fragmentOffset != 0 && _frameStream.Position == 0)
                return;

            int dataSize = byteSegment.Offset + byteSegment.Count - offset;

            if (dataSize < 0)
                throw new MediaPayloadParserException($"Invalid payload size: {dataSize}");

            _frameStream.Write(byteSegment.Array, offset, dataSize);
        }

        public override void ResetState()
        {
            _frameStream.Position = 0;
        }

        private void ReInitializeJpegHeader()
        {
            if (!_hasExternalQuantizationTable)
                GenerateQuantizationTables(_currentQ);

            var jpegHeaderSize = GetJpegHeaderSize(_currentDri);

            _jpegHeaderBytes = new byte[jpegHeaderSize];
            _jpegHeaderBytesSegment = new ArraySegment<byte>(_jpegHeaderBytes);

            FillJpegHeader(_jpegHeaderBytes, _currentType, _currentFrameWidth, _currentFrameHeight, _currentDri);
        }

        private void GenerateQuantizationTables(int factor)
        {
            _quantizationTablesLength = 128;

            if (_quantizationTables.Length < _quantizationTablesLength)
                _quantizationTables = new byte[_quantizationTablesLength];

            int q;

            if (factor < 1)
                factor = 1;
            else if (factor > 99)
                factor = 99;

            if (factor < 50)
                q = 5000 / factor;
            else
                q = 200 - factor * 2;

            for (var i = 0; i < 128; ++i)
            {
                int newVal = (DefaultQuantizers[i] * q + 50) / 100;

                if (newVal < 1)
                    newVal = 1;
                else if (newVal > 255)
                    newVal = 255;

                _quantizationTables[i] = (byte)newVal;
            }
        }

        private int GetJpegHeaderSize(int dri)
        {
            int qtlen = _quantizationTablesLength;

            int qtlenHalf = qtlen / 2;
            qtlen = qtlenHalf * 2;

            int qtablesCount = qtlen > 64 ? 2 : 1;
            return 485 + qtablesCount * 5 + qtlen + (dri > 0 ? 6 : 0);
        }

        private void FillJpegHeader(byte[] buffer, int type, int width, int height, int dri)
        {
            int qtablesCount = _quantizationTablesLength > 64 ? 2 : 1;
            int offset = 0;

            buffer[offset++] = 0xFF;
            buffer[offset++] = 0xD8;
            buffer[offset++] = 0xFF;
            buffer[offset++] = 0xe0;
            buffer[offset++] = 0x00;
            buffer[offset++] = 0x10;
            buffer[offset++] = (byte)'J';
            buffer[offset++] = (byte)'F';
            buffer[offset++] = (byte)'I';
            buffer[offset++] = (byte)'F';
            buffer[offset++] = 0x00;
            buffer[offset++] = 0x01;
            buffer[offset++] = 0x01;
            buffer[offset++] = 0x00;
            buffer[offset++] = 0x00;
            buffer[offset++] = 0x01;
            buffer[offset++] = 0x00;
            buffer[offset++] = 0x01;
            buffer[offset++] = 0x00;
            buffer[offset++] = 0x00;

            if (dri > 0)
            {
                buffer[offset++] = 0xFF;
                buffer[offset++] = 0xdd;
                buffer[offset++] = 0x00;
                buffer[offset++] = 0x04;
                buffer[offset++] = (byte)(dri >> 8);
                buffer[offset++] = (byte)dri;
            }

            int tableSize = qtablesCount == 1 ? _quantizationTablesLength : _quantizationTablesLength / 2;
            buffer[offset++] = 0xFF;
            buffer[offset++] = 0xdb;
            buffer[offset++] = 0x00;
            buffer[offset++] = (byte)(tableSize + 3);
            buffer[offset++] = 0x00;

            int qtablesOffset = 0;
            Buffer.BlockCopy(_quantizationTables, qtablesOffset, buffer, offset, tableSize);
            qtablesOffset += tableSize;
            offset += tableSize;

            if (qtablesCount > 1)
            {
                tableSize = _quantizationTablesLength - _quantizationTablesLength / 2;

                buffer[offset++] = 0xFF;
                buffer[offset++] = 0xdb;
                buffer[offset++] = 0x00;
                buffer[offset++] = (byte)(tableSize + 3);
                buffer[offset++] = 0x01;
                Buffer.BlockCopy(_quantizationTables, qtablesOffset, buffer, offset, tableSize);
                offset += tableSize;
            }

            buffer[offset++] = 0xFF;
            buffer[offset++] = 0xc0;
            buffer[offset++] = 0x00;
            buffer[offset++] = 0x11;
            buffer[offset++] = 0x08;
            buffer[offset++] = (byte)(height >> 8);
            buffer[offset++] = (byte)height;
            buffer[offset++] = (byte)(width >> 8);
            buffer[offset++] = (byte)width;
            buffer[offset++] = 0x03;
            buffer[offset++] = 0x01;
            buffer[offset++] = (type & 1) != 0 ? (byte)0x22 : (byte)0x21;
            buffer[offset++] = 0x00;
            buffer[offset++] = 0x02;
            buffer[offset++] = 0x11;
            buffer[offset++] = qtablesCount == 1 ? (byte)0x00 : (byte)0x01;
            buffer[offset++] = 0x03;
            buffer[offset++] = 0x11;
            buffer[offset++] = qtablesCount == 1 ? (byte)0x00 : (byte)0x01;

            CreateHuffmanHeader(buffer, offset, LumDcCodelens, LumDcCodelens.Length, LumDcSymbols, LumDcSymbols.Length,
                0, 0);
            offset += 5 + LumDcCodelens.Length + LumDcSymbols.Length;

            CreateHuffmanHeader(buffer, offset, LumAcCodelens, LumAcCodelens.Length, LumAcSymbols, LumAcSymbols.Length,
                0, 1);
            offset += 5 + LumAcCodelens.Length + LumAcSymbols.Length;

            CreateHuffmanHeader(buffer, offset, ChmDcCodelens, ChmDcCodelens.Length, ChmDcSymbols, ChmDcSymbols.Length,
                1, 0);
            offset += 5 + ChmDcCodelens.Length + ChmDcSymbols.Length;

            CreateHuffmanHeader(buffer, offset, ChmAcCodelens, ChmAcCodelens.Length, ChmAcSymbols, ChmAcSymbols.Length,
                1, 1);
            offset += 5 + ChmAcCodelens.Length + ChmAcSymbols.Length;

            buffer[offset++] = 0xFF;
            buffer[offset++] = 0xda;
            buffer[offset++] = 0x00;
            buffer[offset++] = 0x0C;
            buffer[offset++] = 0x03;
            buffer[offset++] = 0x01;
            buffer[offset++] = 0x00;
            buffer[offset++] = 0x02;
            buffer[offset++] = 0x11;
            buffer[offset++] = 0x03;
            buffer[offset++] = 0x11;
            buffer[offset++] = 0x00;
            buffer[offset++] = 0x3F;
            buffer[offset] = 0x00;
        }

        private static void CreateHuffmanHeader(byte[] buffer, int offset, byte[] codelens, int ncodes, byte[] symbols,
            int nsymbols, int tableNo, int tableClass)
        {
            buffer[offset++] = 0xff;
            buffer[offset++] = 0xc4;
            buffer[offset++] = 0;
            buffer[offset++] = (byte)(3 + ncodes + nsymbols);
            buffer[offset++] = (byte)((tableClass << 4) | tableNo);
            Buffer.BlockCopy(codelens, 0, buffer, offset, ncodes);
            offset += ncodes;
            Buffer.BlockCopy(symbols, 0, buffer, offset, nsymbols);
        }

        private void GenerateFrame(TimeSpan timeOffset)
        {
            int frameSize = (int)_frameStream.Position;

            if (!ArrayUtils.EndsWith(_frameStream.GetBuffer(), 0,
                frameSize, RawJpegFrame.EndMarkerBytes))
                _frameStream.Write(JpegEndMarkerByteSegment.Array, JpegEndMarkerByteSegment.Offset, JpegEndMarkerByteSegment.Count);

            DateTime timestamp = GetFrameTimestamp(timeOffset);
            var frameBytes = new ArraySegment<byte>(_frameStream.GetBuffer(), 0, frameSize);
            _frameStream.Position = 0;

            var frame = new RawJpegFrame(timestamp, frameBytes);
            OnFrameGenerated(frame);
        }
    }
}
