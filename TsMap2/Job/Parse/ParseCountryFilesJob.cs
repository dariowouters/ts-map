using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Serilog;
using TsMap2.Helper;
using TsMap2.Model;
using TsMap2.Scs;

namespace TsMap2.Job.Parse {
    public class ParseCountryFilesJob : ThreadJob {
        protected override void Do() {
            Log.Debug( "[Job][Country] Loading" );

            ScsDirectory defDirectory = this.Store().Rfs.GetDirectory( ScsPath.Def.DefFolderName );
            if ( defDirectory == null ) {
                Log.Error( "[Job][Country] Could not read '{0]' dir", ScsPath.Def.DefFolderName );
                return;
            }

            List< ScsFile > countryFiles = defDirectory.GetFiles( ScsPath.Def.CountryFileName );
            if ( countryFiles == null ) {
                Log.Error( "[Job][Country] Could not read {0} files", ScsPath.Def.CountryFileName );
                return;
            }

            foreach ( ScsFile countryFile in countryFiles ) {
                byte[]   data  = countryFile.Entry.Read();
                string[] lines = Encoding.UTF8.GetString( data ).Split( '\n' );

                foreach ( string line in lines ) {
                    if ( line.TrimStart().StartsWith( "#" ) ) continue;
                    if ( !line.Contains( "@include" ) ) continue;

                    string path = ScsHelper.GetFilePath( line.Split( '"' )[ 1 ], ScsPath.Def.DefFolderName );
                    // var    country = new TsCountry();

                    this.Store().AddCountry( this.Parse( path ) );
                    // if ( country.Token != 0 && !this._countriesLookup.ContainsKey( country.Token ) )
                    //     this._countriesLookup.Add( country.Token, country );
                }
            }

            Log.Information( "[Job][Country] Loaded. Found: {0}", this.Store().Countries.Count );
        }

        protected override void OnEnd() { }

        private TsCountry Parse( string path ) {
            ScsFile file = this.Store().Rfs.GetFileEntry( path );

            if ( file == null ) return null;
            // LocalizedNames = new Dictionary<string, string>();
            byte[] fileContent = file.Entry.Read();

            string[] fileLines = Encoding.UTF8.GetString( fileContent ).Split( '\n' );

            var   id    = 0;
            var   name  = "";
            var   code  = "";
            ulong token = 0;
            float x     = 0;
            float y     = 0;

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
                    // else if (key == "name_localized")
                    // {
                    //     LocalizationToken = value.Split('"')[1];
                    //     LocalizationToken = LocalizationToken.Replace("@", "");
                    // }
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

            return new TsCountry( code, id, name, token, x, y );
        }
    }
}