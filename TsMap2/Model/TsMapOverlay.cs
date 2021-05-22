using System.Drawing;

namespace TsMap2.Model {
    public struct Color8888 {
        public byte A { get; private set; }
        public byte R { get; }
        public byte G { get; }
        public byte B { get; }

        public Color8888( byte a, byte r, byte g, byte b ) {
            this.A = a;
            this.R = r;
            this.G = g;
            this.B = b;
        }

        public Color8888( Color565 color565, byte a ) {
            this.A = a;
            this.R = (byte) ( color565.R << 3 );
            this.G = (byte) ( color565.G << 2 );
            this.B = (byte) ( color565.B << 3 );
        }

        public void SetAlpha( byte a ) {
            this.A = a;
        }
    }

    public readonly struct Color565 {
        public byte R { get; }
        public byte G { get; }
        public byte B { get; }

        private Color565( byte r, byte g, byte b ) {
            this.R = r;
            this.G = g;
            this.B = b;
        }

        public Color565( ushort color ) : this( (byte) ( ( color >> 11 ) & 0x1F ), (byte) ( ( color >> 5 ) & 0x3F ), (byte) ( color & 0x1F ) ) { }

        public static Color565 operator +( Color565 col1, Color565 col2 ) =>
            new Color565( (byte) ( col1.R + col2.R ), (byte) ( col1.G + col2.G ), (byte) ( col1.B + col2.B ) );

        public static Color565 operator *( Color565 col1, double val ) => new Color565( (byte) ( col1.R * val ), (byte) ( col1.G * val ), (byte) ( col1.B * val ) );

        public static Color565 operator *( double val, Color565 col1 ) => col1 * val;
    }

    public class OverlayIcon {
        private readonly Color8888[] _pixelData;

        public OverlayIcon( Color8888[] pixelData, uint width, uint height ) {
            this._pixelData = pixelData;
            this.Width      = width;
            this.Height     = height;
            this.Valid      = true;
        }

        public OverlayIcon() { }
        public bool Valid  { get; }
        public uint Width  { get; }
        public uint Height { get; }

        public byte[] GetData() {
            var bytes = new byte[ this.Width * this.Height * 4 ];
            for ( var i = 0; i < this._pixelData.Length; i++ ) {
                Color8888 pixel = this._pixelData[ i ];
                bytes[ i * 4 + 3 ] = pixel.A;
                bytes[ i * 4 ]     = pixel.B;
                bytes[ i * 4 + 1 ] = pixel.G;
                bytes[ i * 4 + 2 ] = pixel.R;
            }

            return bytes;
        }
    }

    public class TsMapOverlay {
        private readonly Bitmap _overlayBitmap;

        public TsMapOverlay( Bitmap overlayBitmap, ulong token ) {
            this._overlayBitmap = overlayBitmap;
            this.Token          = token;
        }

        public ulong  Token       { get; }
        public Bitmap GetBitmap() => this._overlayBitmap;
    }
}