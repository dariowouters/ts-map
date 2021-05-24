using System;
using System.IO;
using TsMap2.Factory;

namespace TsMap2.Helper {
    public static class RawHelper {
        public static void SaveFile( string fileName, string path, byte[] data ) {
            string fullPath = Path.Combine( path, fileName );

            Directory.CreateDirectory( path );
            File.WriteAllBytes( fullPath, data );
        }

        public static byte[] LoadFile( string fileName, string path ) {
            string fullPath = Path.Combine( path, fileName );

            return !File.Exists( fullPath )
                       ? null
                       : File.ReadAllBytes( fullPath );
        }

        public static void SaveRawFile( RawType type, string fileName, byte[] stream ) {
            var rawFactory = new RawFactory( stream );
            rawFactory.Save( Path.Combine( RawTypeToString( type ), fileName ) );
        }

        public static void InitRawFolder() {
            foreach ( RawType type in Enum.GetValues( typeof( RawType ) ) ) {
                string fullPath = Path.Combine( AppPath.RawFolder, RawTypeToString( type ) );

                Directory.CreateDirectory( fullPath );
            }
        }

        private static string RawTypeToString( RawType type ) => Enum.GetName( typeof( RawType ), type );
    }
}