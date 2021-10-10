using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TsMap2.Exceptions;
using TsMap2.Helper;
using TsMap2.Model;

namespace TsMap2.Scs.FileSystem.Entry {
    public class ScsCountryEntry : AbstractScsEntry< TsCountry > {
        public Dictionary< ulong, TsCountry > List() {
            VerifyRfs();

            ScsDirectory defDirectory = Store.Rfs.GetDirectory( ScsPath.Def.DefFolderName );
            if ( defDirectory == null ) {
                var message = $"[Job][Country] Could not read '{ScsPath.Def.DefFolderName}' dir";
                throw new ScsEntryException( message );
            }

            List< ScsFile > countryFiles = defDirectory.GetFiles( ScsPath.Def.CountryFileName );
            if ( countryFiles == null ) {
                var message = $"[Job][Country] Could not read {ScsPath.Def.CountryFileName} files";
                throw new ScsEntryException( message );
            }

            var countries = new Dictionary< ulong, TsCountry >();
            foreach ( ScsFile countryFile in countryFiles ) {
                byte[]   data  = countryFile.Entry.Read();
                string[] lines = Encoding.UTF8.GetString( data ).Split( '\n' );

                foreach ( string line in lines ) {
                    if ( line.TrimStart().StartsWith( "#" ) ) continue;
                    if ( !line.Contains( "@include" ) ) continue;

                    string path = ScsHelper.GetFilePath( line.Split( '"' )[ 1 ], ScsPath.Def.DefFolderName );

                    TsCountry country = Get( path );

                    if ( country.Token != 0 && !countries.ContainsKey( country.Token ) )
                        countries.Add( country.Token, country );
                }
            }

            return countries;
        }


        public override TsCountry Generate( byte[] stream ) {
            string[] fileLines = Encoding.UTF8.GetString( stream ).Split( '\n' );

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
                        token = ScsHashHelper.StringToToken( ScsSiiHelper.Trim( value.Split( '.' )[ 2 ] ) );
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