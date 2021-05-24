using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TsMap2.Scs;

namespace TsMap2.Helper {
    public static class ScsParseHelper {
        private static StoreHelper Store => StoreHelper.Instance;

        public static (string name, string country, ulong token, string localizationToken, List< int > xOffsets, List< int > yOffsets)
            CityParse( string path ) {
            ScsFile file = Store.Rfs.GetFileEntry( path );

            if ( file == null ) return ( null, null, 0, null, null, null );
            byte[] fileContent = file.Entry.Read();

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

        public static (string code, int id, string name, ulong token, float x, float y, string localizationToken)
            CountryParse( string path ) {
            ScsFile file = Store.Rfs.GetFileEntry( path );

            if ( file == null ) return ( null, 0, null, 0, 0, 0, null );
            byte[] fileContent = file.Entry.Read();

            string[] fileLines = Encoding.UTF8.GetString( fileContent ).Split( '\n' );

            var   id                = 0;
            var   name              = "";
            var   code              = "";
            ulong token             = 0;
            float x                 = 0;
            float y                 = 0;
            var   localizationToken = "";

            foreach ( string fileLine in fileLines ) {
                ( bool validLine, string key, string value ) = ScsSiiHelper.ParseLine( fileLine );

                if ( !validLine ) continue;

                switch ( key ) {
                    case "country_data":
                        token = ScsHash.StringToToken( ScsSiiHelper.Trim( value.Split( '.' )[ 2 ] ) );
                        break;
                    case "country_id":
                        id = int.Parse( value );
                        break;
                    case "name_localized":
                        localizationToken = value.Split( '"' )[ 1 ];
                        localizationToken = localizationToken.Replace( "@", "" );
                        break;
                    case "name":
                        name = value.Split( '"' )[ 1 ];
                        break;
                    case "country_code":
                        code = value.Split( '"' )[ 1 ];
                        break;
                    case "pos": {
                        string   vector = value.Split( '(' )[ 1 ].Split( ')' )[ 0 ];
                        string[] values = vector.Split( ',' );

                        x = float.Parse( values[ 0 ], CultureInfo.InvariantCulture );
                        y = float.Parse( values[ 2 ], CultureInfo.InvariantCulture );
                        break;
                    }
                }
            }

            return ( code, id, name, token, x, y, localizationToken );
        }
    }
}