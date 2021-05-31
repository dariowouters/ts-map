using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using Serilog;
using TsMap2.Model;

namespace TsMap2.Helper {
    public static class ScsHelper {
        internal static ushort ReadUInt16( BinaryReader br, long offset, SeekOrigin so = SeekOrigin.Begin ) {
            br.BaseStream.Seek( offset, so );
            return br.ReadUInt16();
        }

        internal static uint ReadUInt32( BinaryReader br, long offset, SeekOrigin so = SeekOrigin.Begin ) {
            br.BaseStream.Seek( offset, so );
            return br.ReadUInt32();
        }

        internal static int ReadInt32( BinaryReader br, long offset, SeekOrigin so = SeekOrigin.Begin ) {
            br.BaseStream.Seek( offset, so );
            return br.ReadInt32();
        }

        internal static ulong ReadUInt64( BinaryReader br, long offset, SeekOrigin so = SeekOrigin.Begin ) {
            br.BaseStream.Seek( offset, so );
            return br.ReadUInt64();
        }

        internal static long ReadInt64( BinaryReader br, long offset, SeekOrigin so = SeekOrigin.Begin ) {
            br.BaseStream.Seek( offset, so );
            return br.ReadInt64();
        }

        internal static string CombinePath( string firstPath, string secondPath ) {
            string fullPath = Path.Combine( firstPath, secondPath );

            fullPath = fullPath.Replace( '\\', '/' );

            if ( fullPath.StartsWith( "/" ) ) // absolute path
                fullPath = fullPath.Substring( 1 );

            return fullPath;
        }

        internal static string GetFilePath( string path, string currentPath = "" ) {
            if ( path.StartsWith( "/" ) ) // absolute path
            {
                path = path.Substring( 1 );
                return path;
            }

            return CombinePath( currentPath, path );
        }

        internal static string Decrypt3Nk( byte[] src ) // from quickbms scsgames.bms script
        {
            if ( src.Length < 0x05 || src[ 0 ] != 0x33 && src[ 1 ] != 0x6E && src[ 2 ] != 0x4B ) return null;
            var  decrypted = new byte[ src.Length - 6 ];
            byte key       = src[ 5 ];

            for ( var i = 6; i < src.Length; i++ ) {
                decrypted[ i - 6 ] = (byte) ( ( ( ( key << 2 ) ^ key ^ 0xff ) << 3 ) ^ key ^ src[ i ] );
                key++;
            }

            return Encoding.UTF8.GetString( decrypted );
        }

        // https: //stackoverflow.com/a/3677960
        public static FileStream WaitForFile( string fullPath, FileMode mode, FileAccess access, FileShare share ) {
            for ( var numTries = 0; numTries < 10; numTries++ ) {
                FileStream fs = null;
                try {
                    fs = new FileStream( fullPath, mode, access, share );
                    return fs;
                } catch ( IOException ) {
                    Log.Warning( "Reading try {0} for {1}", numTries, fullPath );
                    if ( fs != null )
                        fs.Dispose();

                    Thread.Sleep( 50 );
                }
            }

            return null;
        }
    }

    public static class ScsRenderHelper {
        public static PointF RotatePoint( float x, float z, float angle, float rotX, float rotZ ) {
            double s    = Math.Sin( angle );
            double c    = Math.Cos( angle );
            double newX = x - rotX;
            double newZ = z - rotZ;
            return new PointF( (float) ( newX * c - newZ * s + rotX ), (float) ( newX * s + newZ * c + rotZ ) );
        }

        public static PointF GetCornerCoords( float x, float z, float width, double angle ) =>
            new PointF(
                       (float) ( x + width * Math.Cos( angle ) ),
                       (float) ( z + width * Math.Sin( angle ) )
                      );

        public static double Hypotenuse( float x, float y ) => Math.Sqrt( Math.Pow( x, 2 ) + Math.Pow( y, 2 ) );

        // https://stackoverflow.com/a/45881662
        public static Tuple< PointF, PointF > GetBezierControlNodes( float startX, float startZ, double startRot, float endX, float endZ, double endRot ) {
            double len = Hypotenuse( endX - startX, endZ - startZ );
            var    ax1 = (float) ( Math.Cos( startRot ) * len * ( 1 / 3f ) );
            var    az1 = (float) ( Math.Sin( startRot ) * len * ( 1 / 3f ) );
            var    ax2 = (float) ( Math.Cos( endRot )   * len * ( 1 / 3f ) );
            var    az2 = (float) ( Math.Sin( endRot )   * len * ( 1 / 3f ) );
            return new Tuple< PointF, PointF >( new PointF( ax1, az1 ), new PointF( ax2, az2 ) );
        }

        public static int GetZoomIndex( Rectangle clip, float scale ) {
            float smallestSize = clip.Width > clip.Height
                                     ? clip.Height / scale
                                     : clip.Width  / scale;
            if ( smallestSize < 1000 ) return 0;
            if ( smallestSize < 5000 ) return 1;
            if ( smallestSize < 18500 ) return 2;
            return 3;
        }
    }

    public static class ScsSiiHelper {
        private const string CharNotToTrim = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~";

        public static string Trim( string src ) {
            var startTrimIndex = 0;
            int endTrimIndex   = src.Length;
            for ( var i = 0; i < src.Length; i++ )
                if ( !CharNotToTrim.Contains( src[ i ].ToString() ) )
                    startTrimIndex = i + 1;
                else break;

            for ( int i = src.Length - 1; i >= 0; i-- )
                if ( !CharNotToTrim.Contains( src[ i ].ToString() ) )
                    endTrimIndex = i;
                else break;

            if ( startTrimIndex == src.Length || startTrimIndex >= endTrimIndex ) return "";
            return src.Substring( startTrimIndex, endTrimIndex - startTrimIndex );
        }

        public static (bool Valid, string Key, string Value) ParseLine( string line ) {
            line = Trim( line );
            if ( !line.Contains( ":" ) || line.StartsWith( "#" ) || line.StartsWith( "//" ) ) return ( false, line, line );
            string key = Trim( line.Split( ':' )[ 0 ] );
            string val = line.Split( ':' )[ 1 ];
            if ( val.Contains( "//" ) ) {
                int commentIndex = val.LastIndexOf( "//", StringComparison.OrdinalIgnoreCase );
                val = val.Substring( 0, commentIndex );
            }

            val = Trim( val );
            return ( true, key, val );
        }
    }

    public static class ScsOverlayHelper {
        public static Color8888[] ParseUncompressed( byte[] stream, uint width, uint height ) {
            if ( ( stream.Length - 128 ) / 4 < width * height ) {
                Log.Warning( "Invalid DDS file (size)" );
                return null;
            }

            var fileOffset = 0x7C;

            var pixelData = new Color8888[ width * height ];

            for ( var i = 0; i < width * height; i++ ) {
                uint rgba = MemoryHelper.ReadUInt32( stream, fileOffset += 0x04 );
                pixelData[ i ] = new Color8888( (byte) ( ( rgba >> 0x18 ) & 0xFF ), (byte) ( ( rgba >> 0x10 ) & 0xFF ), (byte) ( ( rgba >> 0x08 ) & 0xFF ),
                                                (byte) ( rgba             & 0xFF ) );
            }

            return pixelData;
        }

        public static Color8888[] ParseDxt3( byte[] stream, uint width, uint height ) // https://msdn.microsoft.com/en-us/library/windows/desktop/bb694531
        {
            var fileOffset = 0x80;
            var pixelData  = new Color8888[ width * height ];
            for ( var y = 0; y < height; y += 4 )
            for ( var x = 0; x < width; x += 4 ) {
                int baseOffset = fileOffset;

                var color0 = new Color565( BitConverter.ToUInt16( stream, fileOffset += 0x08 ) );
                var color1 = new Color565( BitConverter.ToUInt16( stream, fileOffset += 0x02 ) );

                Color565 color2 = (double) 2 / 3 * color0 + (double) 1 / 3 * color1;
                Color565 color3 = (double) 1 / 3 * color0 + (double) 2 / 3 * color1;

                var colors = new[] {
                    new Color8888( color0, 0xFF ), // bit code 00
                    new Color8888( color1, 0xFF ), // bit code 01
                    new Color8888( color2, 0xFF ), // bit code 10
                    new Color8888( color3, 0xFF )  // bit code 11
                };

                fileOffset += 0x02;
                for ( var i = 0; i < 4; i++ ) {
                    byte colorRow = stream[ fileOffset                        + i ];
                    var  alphaRow = BitConverter.ToUInt16( stream, baseOffset + i * 2 );

                    for ( var j = 0; j < 4; j++ ) {
                        int  colorIndex = ( colorRow >> ( j * 2 ) ) & 3;
                        int  alpha      = ( alphaRow >> ( j * 4 ) ) & 15;
                        long pos        = y * width + i * width + x + j;
                        pixelData[ pos ] = colors[ colorIndex ];
                        pixelData[ pos ].SetAlpha( (byte) ( alpha / 15f * 255 ) );
                    }
                }

                fileOffset += 0x04;
            }

            return pixelData;
        }

        public static Color8888[]
            ParseDxt5( byte[] stream, uint width, uint height ) // https://msdn.microsoft.com/en-us/library/windows/desktop/bb694531
        {
            var fileOffset = 0x80;
            var pixelData  = new Color8888[ width * height ];
            for ( var y = 0; y < height; y += 4 )
            for ( var x = 0; x < width; x += 4 ) {
                var alphas = new byte[ 8 ];
                alphas[ 0 ] = stream[ fileOffset ];
                alphas[ 1 ] = stream[ fileOffset += 0x01 ];

                if ( alphas[ 0 ] > alphas[ 1 ] ) {
                    // 6 interpolated alpha values.
                    alphas[ 2 ] = (byte) ( (double) 6 / 7 * alphas[ 0 ] + (double) 1 / 7 * alphas[ 1 ] ); // bit code 010
                    alphas[ 3 ] = (byte) ( (double) 5 / 7 * alphas[ 0 ] + (double) 2 / 7 * alphas[ 1 ] ); // bit code 011
                    alphas[ 4 ] = (byte) ( (double) 4 / 7 * alphas[ 0 ] + (double) 3 / 7 * alphas[ 1 ] ); // bit code 100
                    alphas[ 5 ] = (byte) ( (double) 3 / 7 * alphas[ 0 ] + (double) 4 / 7 * alphas[ 1 ] ); // bit code 101
                    alphas[ 6 ] = (byte) ( (double) 2 / 7 * alphas[ 0 ] + (double) 5 / 7 * alphas[ 1 ] ); // bit code 110
                    alphas[ 7 ] = (byte) ( (double) 1 / 7 * alphas[ 0 ] + (double) 6 / 7 * alphas[ 1 ] ); // bit code 111
                } else {
                    // 4 interpolated alpha values.
                    alphas[ 2 ] = (byte) ( (double) 4 / 5 * alphas[ 0 ] + (double) 1 / 5 * alphas[ 1 ] ); // bit code 010
                    alphas[ 3 ] = (byte) ( (double) 3 / 5 * alphas[ 0 ] + (double) 2 / 5 * alphas[ 1 ] ); // bit code 011
                    alphas[ 4 ] = (byte) ( (double) 2 / 5 * alphas[ 0 ] + (double) 3 / 5 * alphas[ 1 ] ); // bit code 100
                    alphas[ 5 ] = (byte) ( (double) 1 / 5 * alphas[ 0 ] + (double) 4 / 5 * alphas[ 1 ] ); // bit code 101
                    alphas[ 6 ] = 0;                                                                      // bit code 110
                    alphas[ 7 ] = 255;                                                                    // bit code 111
                }

                ulong alphaTexelUlongData = MemoryHelper.ReadUInt64( stream, fileOffset += 0x01 );

                ulong alphaTexelData =
                    alphaTexelUlongData & 0xFFFFFFFFFFFF; // remove 2 excess bytes (read 8 bytes only need 6)

                var alphaTexels = new byte[ 16 ];
                for ( var j = 0; j < 2; j++ ) {
                    ulong alphaTexelRowData = ( alphaTexelData >> ( j * 0x18 ) ) & 0xFFFFFF;
                    for ( var i = 0; i < 8; i++ ) {
                        ulong index = ( alphaTexelRowData >> ( i * 0x03 ) ) & 0x07;
                        alphaTexels[ i + j * 8 ] = alphas[ index ];
                    }
                }

                var color0 = new Color565( MemoryHelper.ReadUInt16( stream, fileOffset += 0x06 ) );
                var color1 = new Color565( MemoryHelper.ReadUInt16( stream, fileOffset += 0x02 ) );

                Color565 color2 = (double) 2 / 3 * color0 + (double) 1 / 3 * color1;
                Color565 color3 = (double) 1 / 3 * color0 + (double) 2 / 3 * color1;

                var colors = new[] {
                    new Color8888( color0, 0xFF ), // bit code 00
                    new Color8888( color1, 0xFF ), // bit code 01
                    new Color8888( color2, 0xFF ), // bit code 10
                    new Color8888( color3, 0xFF )  // bit code 11
                };

                uint colorTexelData = MemoryHelper.ReadUInt32( stream, fileOffset += 0x02 );
                for ( var j = 3; j >= 0; j-- ) {
                    uint colorTexelRowData = ( colorTexelData >> ( j * 0x08 ) ) & 0xFF;
                    for ( var i = 0; i < 4; i++ ) {
                        uint index = ( colorTexelRowData >> ( i * 0x02 ) ) & 0x03;
                        var  pos   = (uint) ( y * width + j * width + x + i );
                        pixelData[ pos ] = colors[ index ];
                        pixelData[ pos ].SetAlpha( alphaTexels[ j * 4 + i ] );
                    }
                }

                fileOffset += 0x04;
            }

            return pixelData;
        }
    }

    internal struct UlDiv {
        public ulong Quot;
        public ulong Rem;
    }

    public static class ScsHashHelper {
        private static readonly char[] Letters = {
            '\0', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b',
            'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o',
            'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '_'
        };


        private static ulong PowUl( int num ) {
            ulong res                           = 1;
            for ( var i = 0; num > i; i++ ) res *= (ulong) Letters.Length;

            return res;
        }

        private static int GetIdChar( char letter ) {
            for ( var i = 0; i < Letters.Length; i++ )
                if ( letter == Letters[ i ] )
                    return i;

            return 0;
        }

        private static UlDiv Div( ulong num, ulong divider ) {
            var res = new UlDiv { Rem = num % divider, Quot = num / divider };
            return res;
        }

        public static ulong StringToToken( string text ) {
            ulong res                           = 0;
            int   len                           = text.Length;
            for ( var i = 0; i < len; i++ ) res += PowUl( i ) * (ulong) GetIdChar( text.ToLower()[ i ] );

            return res;
        }

        public static string TokenToString( ulong token ) {
            var sb = new StringBuilder();
            for ( var i = 0; token != 0; i++ ) {
                UlDiv res = Div( token, 38 );
                token = res.Quot;
                sb.Append( Letters[ res.Rem ] );
            }

            return sb.ToString();
        }
    }
}