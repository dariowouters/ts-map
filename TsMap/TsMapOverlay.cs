using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace TsMap
{
    public struct Color8888
    {
        public byte A { get; private set; }
        public byte R { get; }
        public byte G { get; }
        public byte B { get; }

        public Color8888(byte a, byte r, byte g, byte b)
        {
            A = a;
            R = r;
            G = g;
            B = b;
        }

        public Color8888(Color565 color565, byte a)
        {
            A = a;
            R = (byte)(color565.R << 3);
            G = (byte)(color565.G << 2);
            B = (byte)(color565.B << 3);
        }

        public void SetAlpha(byte a)
        {
            A = a;
        }
    }

    public struct Color565
    {
        public byte R { get; }
        public byte G { get; }
        public byte B { get; }

        private Color565(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }

        public Color565(ushort color) : this((byte)((color >> 11) & 0x1F), (byte)((color >> 5) & 0x3F), (byte)(color & 0x1F)) { }

        public static Color565 operator +(Color565 col1, Color565 col2)
        {
            return new Color565((byte)(col1.R + col2.R), (byte)(col1.G + col2.G), (byte)(col1.B + col2.B));
        }

        public static Color565 operator *(Color565 col1, double val)
        {
            return new Color565((byte)(col1.R * val), (byte)(col1.G * val), (byte)(col1.B * val));
        }

        public static Color565 operator *(double val, Color565 col1)
        {
            return col1 * val;
        }
    }

    public class OverlayIcon
    {
        private readonly byte[] _stream;
        public bool Valid { get; private set; }

        public uint Width { get; private set; }
        public uint Height { get; private set; }

        private Color8888[] _pixelData;

        public OverlayIcon(string filePath)
        {
            _stream = File.ReadAllBytes(filePath);
            Parse();
        }

        public byte[] GetData()
        {
            var bytes = new byte[Width * Height * 4];
            for (var i = 0; i < _pixelData.Length; i++)
            {
                var pixel = _pixelData[i];
                bytes[i * 4 + 3] = pixel.A;
                bytes[i * 4] = pixel.B;
                bytes[i * 4 + 1] = pixel.G;
                bytes[i * 4 + 2] = pixel.R;
            }
            return bytes;
        }

        private void Parse()
        {
            Valid = true;
            if (_stream.Length < 128 ||
                BitConverter.ToUInt32(_stream, 0x00) != 0x20534444 ||
                BitConverter.ToUInt32(_stream, 0x04) != 0x7C)
            {
                Valid = false;
                Console.WriteLine("Invalid DDS file.");
                return;
            }
            Height = BitConverter.ToUInt32(_stream, 0x0C);
            Width = BitConverter.ToUInt32(_stream, 0x10);

            var fourCc = BitConverter.ToUInt32(_stream, 0x54);

            if (fourCc == 0x35545844) ParseDXT5();
            else ParseUncompressed();

        }

        private void ParseUncompressed()
        {
            if ((_stream.Length - 128) / 4 < Width * Height)
            {
                Valid = false;
                Log.Msg("Invalid DDS file (size).");
                return;
            }

            var fileOffset = 0x7C;

            _pixelData = new Color8888[Width * Height];

            for (var i = 0; i < Width * Height; i++)
            {
                var rgba = BitConverter.ToUInt32(_stream, fileOffset += 0x04);
                _pixelData[i] = new Color8888((byte)((rgba >> 0x18) & 0xFF), (byte)((rgba >> 0x10) & 0xFF), (byte)((rgba >> 0x08) & 0xFF), (byte)(rgba & 0xFF));
            }
        }

        private void ParseDXT5() // https://msdn.microsoft.com/en-us/library/windows/desktop/bb694531
        {
            var fileOffset = 0x80;
            _pixelData = new Color8888[Width * Height];
            for (var y = 0; y < Height; y += 4)
            {

                for (var x = 0; x < Width; x += 4)
                {
                    var alphas = new byte[8];
                    alphas[0] = _stream[fileOffset];
                    alphas[1] = _stream[fileOffset += 0x01];

                    if (alphas[0] > alphas[1])
                    {
                        // 6 interpolated alpha values.
                        alphas[2] = (byte) ((double) 6 / 7 * alphas[0] + (double) 1 / 7 * alphas[1]); // bit code 010
                        alphas[3] = (byte) ((double) 5 / 7 * alphas[0] + (double) 2 / 7 * alphas[1]); // bit code 011
                        alphas[4] = (byte) ((double) 4 / 7 * alphas[0] + (double) 3 / 7 * alphas[1]); // bit code 100
                        alphas[5] = (byte) ((double) 3 / 7 * alphas[0] + (double) 4 / 7 * alphas[1]); // bit code 101
                        alphas[6] = (byte) ((double) 2 / 7 * alphas[0] + (double) 5 / 7 * alphas[1]); // bit code 110
                        alphas[7] = (byte) ((double) 1 / 7 * alphas[0] + (double) 6 / 7 * alphas[1]); // bit code 111
                    }
                    else
                    {
                        // 4 interpolated alpha values.
                        alphas[2] = (byte) ((double) 4 / 5 * alphas[0] + (double) 1 / 5 * alphas[1]); // bit code 010
                        alphas[3] = (byte) ((double) 3 / 5 * alphas[0] + (double) 2 / 5 * alphas[1]); // bit code 011
                        alphas[4] = (byte) ((double) 2 / 5 * alphas[0] + (double) 3 / 5 * alphas[1]); // bit code 100
                        alphas[5] = (byte) ((double) 1 / 5 * alphas[0] + (double) 4 / 5 * alphas[1]); // bit code 101
                        alphas[6] = 0; // bit code 110
                        alphas[7] = 255; // bit code 111
                    }

                    var alphaTexelUlongData = BitConverter.ToUInt64(_stream, fileOffset += 0x01);

                    var alphaTexelData =
                        alphaTexelUlongData & 0xFFFFFFFFFFFF; // remove 2 excess bytes (read 8 bytes only need 6)

                    var alphaTexels = new byte[16];
                    for (var j = 0; j < 2; j++)
                    {
                        var alphaTexelRowData = (alphaTexelData >> (j * 0x18)) & 0xFFFFFF;
                        for (var i = 0; i < 8; i++)
                        {
                            var index = (alphaTexelRowData >> (i * 0x03)) & 0x07;
                            alphaTexels[i + j * 8] = alphas[index];
                        }
                    }

                    var color0 = new Color565(BitConverter.ToUInt16(_stream, fileOffset += 0x06));
                    var color1 = new Color565(BitConverter.ToUInt16(_stream, fileOffset += 0x02));

                    var color2 = (double) 2 / 3 * color0 + (double) 1 / 3 * color1;
                    var color3 = (double) 1 / 3 * color0 + (double) 2 / 3 * color1;

                    var colors = new[]
                    {
                        new Color8888(color0, 0xFF), // bit code 00
                        new Color8888(color1, 0xFF), // bit code 01
                        new Color8888(color2, 0xFF), // bit code 10
                        new Color8888(color3, 0xFF) // bit code 11
                    };

                    var colorTexelData = BitConverter.ToUInt32(_stream, fileOffset += 0x02);
                    for (var j = 3; j >= 0; j--)
                    {
                        var colorTexelRowData = (colorTexelData >> (j * 0x08)) & 0xFF;
                        for (var i = 0; i < 4; i++)
                        {
                            var index = (colorTexelRowData >> (i * 0x02)) & 0x03;
                            var pos = (uint) (y * Width + j * Width + x + i);
                            _pixelData[pos] = colors[index];
                            _pixelData[pos].SetAlpha(alphaTexels[j * 4 + i]);
                        }
                    }
                    fileOffset += 0x04;
                }
            }
        }
    }

    public class TsMapOverlay
    {
        private readonly OverlayIcon _icon;

        public TsMapOverlay(string filePath)
        {
            if (File.Exists(filePath))
            {
                _icon = new OverlayIcon(filePath);
            }
            else
            {
                Log.Msg($"Map Overlay file not found, {filePath}");
            }
        }

        public Bitmap GetBitmap()
        {
            if (_icon == null || !_icon.Valid) return null;
            var b = new Bitmap((int)_icon.Width, (int)_icon.Height, PixelFormat.Format32bppArgb);

            var bd = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            var ptr = bd.Scan0;

            Marshal.Copy(_icon.GetData(), 0, ptr, (bd.Width * bd.Height * 0x4));

            b.UnlockBits(bd);

            return b;

        }
    }
}
