using System.Globalization;
using System.Text;
using TsMap2.Helper;
using TsMap2.Model;
using TsMap2.Scs;

namespace TsMap2.Factory.Entity {
    public static class TsCountryFactory {
        private static StoreHelper Store() => StoreHelper.Instance;

        public static TsCountry Create( string path ) {
            ScsFile file = Store().Rfs.GetFileEntry( path );

            if ( file == null ) return null;
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

            return new TsCountry( code, id, name, token, x, y, localizationToken );
        }
    }
}