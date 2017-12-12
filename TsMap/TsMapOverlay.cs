using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace TsMap
{
    public struct Color8888
    {
        public byte A { get; }
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
                BitConverter.ToUInt32(_stream, 0) != 0x20534444 ||
                BitConverter.ToUInt32(_stream, 4) != 0x7C)
            {
                Valid = false;
                Console.WriteLine("Invalid DDS file.");
                return;
            }
            var fileOffset = 0x0C;
            Height = BitConverter.ToUInt32(_stream, fileOffset);
            Width = BitConverter.ToUInt32(_stream, fileOffset += 0x04);
            fileOffset += 0x6C;

            _pixelData = new Color8888[Width * Height];

            for (var i = 0; i < Width * Height; i++)
            {
                uint rgba = BitConverter.ToUInt32(_stream, fileOffset += 0x04);
                _pixelData[i] = new Color8888((byte)((rgba >> 0x18) & 0xFF), (byte)((rgba >> 0x10) & 0xFF), (byte)((rgba >> 0x08) & 0xFF), (byte)(rgba & 0xFF));
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
