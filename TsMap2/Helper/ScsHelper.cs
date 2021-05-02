using System;
using System.Drawing;
using System.IO;
using System.Text;

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
}