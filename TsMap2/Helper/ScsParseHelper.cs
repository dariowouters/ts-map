using System.Collections.Generic;
using System.Text;
using TsMap2.Factory;
using TsMap2.Scs;

namespace TsMap2.Helper {
    public abstract class ScsParseHelper {
        private static StoreHelper Store => StoreHelper.Instance;

        public static (string name, string country, ulong token, string localizationToken, List< int > xOffsets, List< int > yOffsets)
            CityParse( string path, bool exportRaw = true ) {
            ScsFile file = Store.Rfs.GetFileEntry( path );

            if ( file == null ) return ( null, null, 0, null, null, null );
            byte[] fileContent = file.Entry.Read();

            // -- Raw generation
            if ( !exportRaw ) RawHelper.SaveRawFile( RawType.CITY, file.GetFullName(), fileContent );
            // -- ./Raw generation

            string[] lines             = Encoding.UTF8.GetString( fileContent ).Split( '\n' );
            var      offsetCount       = 0;
            var      xOffsets          = new List< int >();
            var      yOffsets          = new List< int >();
            ulong    token             = 0;
            string   name              = null;
            string   country           = null;
            string   localizationToken = null;

            foreach ( string line in lines ) {
                ( bool validLine, string key, string value ) = ScsSiiHelper.ParseLine( line );
                if ( !validLine ) continue;

                if ( key == "city_data" )
                    token = ScsHash.StringToToken( ScsSiiHelper.Trim( value.Split( '.' )[ 1 ] ) );
                else if ( key == "city_name" )
                    name = line.Split( '"' )[ 1 ];
                else if ( key == "city_name_localized" )
                    localizationToken = value.Split( '"' )[ 1 ].Replace( "@", "" );
                else if ( key == "country" )
                    country = value;
                else if ( key.Contains( "map_x_offsets[]" ) ) {
                    if ( ++offsetCount > 4 )
                        if ( int.TryParse( value, out int offset ) )
                            xOffsets.Add( offset );

                    if ( offsetCount == 8 ) offsetCount = 0;
                } else if ( key.Contains( "map_y_offsets[]" ) )
                    if ( ++offsetCount > 4 )
                        if ( int.TryParse( value, out int offset ) )
                            yOffsets.Add( offset );
            }

            return ( name, country, token, localizationToken, xOffsets, yOffsets );
        }
    }
}