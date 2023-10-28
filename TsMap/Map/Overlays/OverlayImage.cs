using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using TsMap.FileSystem;
using TsMap.Helpers;
using TsMap.Helpers.Logger;

namespace TsMap.Map.Overlays
{
    internal class OverlayImage
    {
        private Bitmap _bitmap;

        private UberFile _file;

        private Color8888[] _pixelData;
        private byte[] _stream;

        public OverlayImage(string filePath)
        {
            FilePath = filePath;
        }

        internal bool Valid { get; private set; }
        internal uint Width { get; private set; }
        internal uint Height { get; private set; }

        internal string FilePath { get; }

        public Bitmap GetBitmap()
        {
            if (_bitmap == null)
            {
                _bitmap = new Bitmap((int)Width, (int)Height, PixelFormat.Format32bppArgb);

                var bd = _bitmap.LockBits(new Rectangle(0, 0, _bitmap.Width, _bitmap.Height), ImageLockMode.WriteOnly,
                    PixelFormat.Format32bppArgb);

                var ptr = bd.Scan0;

                Marshal.Copy(GetData(), 0, ptr, bd.Width * bd.Height * 0x4);

                _bitmap.UnlockBits(bd);
            }

            return _bitmap;
        }

        private byte[] GetData()
        {
            var bytes = new byte[Width * Height * 4];
            for (var i = 0; i < _pixelData.Length; ++i)
            {
                var pixel = _pixelData[i];
                bytes[i * 4 + 3] = pixel.A;
                bytes[i * 4] = pixel.B;
                bytes[i * 4 + 1] = pixel.G;
                bytes[i * 4 + 2] = pixel.R;
            }

            return bytes;
        }

        internal void Parse()
        {
            Valid = true;
            _file = UberFileSystem.Instance.GetFile(FilePath);
            if (_file == null)
            {
                Valid = false;
                Logger.Instance.Error($"Could not find DDS file ({FilePath})");
                return;
            }

            _stream = _file.Entry.Read();

            if (_stream.Length < 128 ||
                MemoryHelper.ReadUInt32(_stream, 0x00) != 0x20534444 ||
                MemoryHelper.ReadUInt32(_stream, 0x04) != 0x7C)
            {
                Valid = false;
                Logger.Instance.Error($"Invalid DDS file ({FilePath})");
                return;
            }

            Height = MemoryHelper.ReadUInt32(_stream, 0x0C);
            Width = MemoryHelper.ReadUInt32(_stream, 0x10);

            var fourCc = MemoryHelper.ReadUInt32(_stream, 0x54);

            if (fourCc == 861165636) ParseDxt3();
            else if (fourCc == 894720068) ParseDxt5();
            else ParseUncompressed();
        }

        private void ParseUncompressed()
        {
            if ((_stream.Length - 128) / 4 < Width * Height)
            {
                Valid = false;
                Logger.Instance.Error($"Invalid DDS file (size), '{FilePath}'");
                return;
            }

            var fileOffset = 0x7C;

            _pixelData = new Color8888[Width * Height];

            for (var i = 0; i < Width * Height; ++i)
            {
                var rgba = MemoryHelper.ReadUInt32(_stream, fileOffset += 0x04);
                _pixelData[i] = new Color8888((byte)((rgba >> 0x18) & 0xFF), (byte)((rgba >> 0x10) & 0xFF),
                    (byte)((rgba >> 0x08) & 0xFF), (byte)(rgba & 0xFF));
            }
        }

        private void ParseDxt3() // https://msdn.microsoft.com/en-us/library/windows/desktop/bb694531
        {
            var fileOffset = 0x80;
            _pixelData = new Color8888[Width * Height];
            for (var y = 0; y < Height; y += 4)
                for (var x = 0; x < Width; x += 4)
                {
                    var baseOffset = fileOffset;

                    var color0 = new Color565(BitConverter.ToUInt16(_stream, fileOffset += 0x08));
                    var color1 = new Color565(BitConverter.ToUInt16(_stream, fileOffset += 0x02));

                    var color2 = (double)2 / 3 * color0 + (double)1 / 3 * color1;
                    var color3 = (double)1 / 3 * color0 + (double)2 / 3 * color1;

                    var colors = new[]
                    {
                    new Color8888(color0, 0xFF), // bit code 00
                    new Color8888(color1, 0xFF), // bit code 01
                    new Color8888(color2, 0xFF), // bit code 10
                    new Color8888(color3, 0xFF) // bit code 11
                };

                    fileOffset += 0x02;
                    for (var i = 0; i < 4; ++i)
                    {
                        var colorRow = _stream[fileOffset + i];
                        var alphaRow = BitConverter.ToUInt16(_stream, baseOffset + i * 2);

                        for (var j = 0; j < 4; ++j)
                        {
                            var colorIndex = (colorRow >> (j * 2)) & 3;
                            var alpha = (alphaRow >> (j * 4)) & 15;
                            var pos = y * Width + i * Width + x + j;
                            _pixelData[pos] = colors[colorIndex];
                            _pixelData[pos].SetAlpha((byte)(alpha / 15f * 255));
                        }
                    }

                    fileOffset += 0x04;
                }
        }

        private void ParseDxt5() // https://msdn.microsoft.com/en-us/library/windows/desktop/bb694531
        {
            var fileOffset = 0x80;
            _pixelData = new Color8888[Width * Height];
            for (var y = 0; y < Height; y += 4)
                for (var x = 0; x < Width; x += 4)
                {
                    var alphas = new byte[8];
                    alphas[0] = _stream[fileOffset];
                    alphas[1] = _stream[fileOffset += 0x01];

                    if (alphas[0] > alphas[1])
                    {
                        // 6 interpolated alpha values.
                        alphas[2] = (byte)((double)6 / 7 * alphas[0] + (double)1 / 7 * alphas[1]); // bit code 010
                        alphas[3] = (byte)((double)5 / 7 * alphas[0] + (double)2 / 7 * alphas[1]); // bit code 011
                        alphas[4] = (byte)((double)4 / 7 * alphas[0] + (double)3 / 7 * alphas[1]); // bit code 100
                        alphas[5] = (byte)((double)3 / 7 * alphas[0] + (double)4 / 7 * alphas[1]); // bit code 101
                        alphas[6] = (byte)((double)2 / 7 * alphas[0] + (double)5 / 7 * alphas[1]); // bit code 110
                        alphas[7] = (byte)((double)1 / 7 * alphas[0] + (double)6 / 7 * alphas[1]); // bit code 111
                    }
                    else
                    {
                        // 4 interpolated alpha values.
                        alphas[2] = (byte)((double)4 / 5 * alphas[0] + (double)1 / 5 * alphas[1]); // bit code 010
                        alphas[3] = (byte)((double)3 / 5 * alphas[0] + (double)2 / 5 * alphas[1]); // bit code 011
                        alphas[4] = (byte)((double)2 / 5 * alphas[0] + (double)3 / 5 * alphas[1]); // bit code 100
                        alphas[5] = (byte)((double)1 / 5 * alphas[0] + (double)4 / 5 * alphas[1]); // bit code 101
                        alphas[6] = 0; // bit code 110
                        alphas[7] = 255; // bit code 111
                    }

                    var alphaTexelUlongData = MemoryHelper.ReadUInt64(_stream, fileOffset += 0x01);

                    var alphaTexelData =
                        alphaTexelUlongData & 0xFFFFFFFFFFFF; // remove 2 excess bytes (read 8 bytes only need 6)

                    var alphaTexels = new byte[16];
                    for (var j = 0; j < 2; ++j)
                    {
                        var alphaTexelRowData = (alphaTexelData >> (j * 0x18)) & 0xFFFFFF;
                        for (var i = 0; i < 8; ++i)
                        {
                            var index = (alphaTexelRowData >> (i * 0x03)) & 0x07;
                            alphaTexels[i + j * 8] = alphas[index];
                        }
                    }

                    var color0 = new Color565(MemoryHelper.ReadUInt16(_stream, fileOffset += 0x06));
                    var color1 = new Color565(MemoryHelper.ReadUInt16(_stream, fileOffset += 0x02));

                    var color2 = (double)2 / 3 * color0 + (double)1 / 3 * color1;
                    var color3 = (double)1 / 3 * color0 + (double)2 / 3 * color1;

                    var colors = new[]
                    {
                    new Color8888(color0, 0xFF), // bit code 00
                    new Color8888(color1, 0xFF), // bit code 01
                    new Color8888(color2, 0xFF), // bit code 10
                    new Color8888(color3, 0xFF) // bit code 11
                };

                    var colorTexelData = MemoryHelper.ReadUInt32(_stream, fileOffset += 0x02);
                    for (var j = 3; j >= 0; --j)
                    {
                        var colorTexelRowData = (colorTexelData >> (j * 0x08)) & 0xFF;
                        for (var i = 0; i < 4; ++i)
                        {
                            var index = (colorTexelRowData >> (i * 0x02)) & 0x03;
                            var pos = (uint)(y * Width + j * Width + x + i);
                            _pixelData[pos] = colors[index];
                            _pixelData[pos].SetAlpha(alphaTexels[j * 4 + i]);
                        }
                    }

                    fileOffset += 0x04;
                }
        }
    }
}