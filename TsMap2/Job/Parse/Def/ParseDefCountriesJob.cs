using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Serilog;
using TsMap2.Factory;
using TsMap2.Helper;
using TsMap2.Model;
using TsMap2.Scs;

namespace TsMap2.Job.Parse.Def {
    public class ParseDefCountriesJob : ThreadJob {
        private bool _isFirstFileRead;

        protected override void Do() {
            Log.Debug( "[Job][Country] Loading" );

            // --- Check RFS
            if ( this.Store().Rfs == null )
                throw new JobException( "[Job][Country] The root file system was not initialized. Check the game path", this.JobName(), null );

            ScsDirectory defDirectory = this.Store().Rfs.GetDirectory( ScsPath.Def.DefFolderName );
            if ( defDirectory == null ) {
                var message = $"[Job][Country] Could not read '{ScsPath.Def.DefFolderName}' dir";
                throw new JobException( message, this.JobName(), ScsPath.Def.DefFolderName );
                // Log.Error( "[Job][Country] Could not read '{0]' dir", ScsPath.Def.DefFolderName );
                // return;
            }

            List< ScsFile > countryFiles = defDirectory.GetFiles( ScsPath.Def.CountryFileName );
            if ( countryFiles == null ) {
                var message = $"[Job][Country] Could not read {ScsPath.Def.CountryFileName} files";
                throw new JobException( message, this.JobName(), ScsPath.Def.CountryFileName );
                // Log.Error( "[Job][Country] Could not read {0} files", ScsPath.Def.CountryFileName );
                // return;
            }

            foreach ( ScsFile countryFile in countryFiles ) {
                byte[]   data  = countryFile.Entry.Read();
                string[] lines = Encoding.UTF8.GetString( data ).Split( '\n' );

                foreach ( string line in lines ) {
                    if ( line.TrimStart().StartsWith( "#" ) ) continue;
                    if ( !line.Contains( "@include" ) ) continue;

                    string path = ScsHelper.GetFilePath( line.Split( '"' )[ 1 ], ScsPath.Def.DefFolderName );
                    // var    country = new TsCountry();

                    this.Store().Def.AddCountry( this.Parse( path ) );
                    // if ( country.Token != 0 && !this._countriesLookup.ContainsKey( country.Token ) )
                    //     this._countriesLookup.Add( country.Token, country );
                }
            }

            Log.Information( "[Job][Country] Loaded. Found: {0}", this.Store().Def.Countries.Count );
        }

        private TsCountry Parse( string path ) {
            ScsFile file = this.Store().Rfs.GetFileEntry( path );

            if ( file == null ) return null;
            // LocalizedNames = new Dictionary<string, string>();
            byte[] fileContent = file.Entry.Read();

            // -- Raw generation
            if ( !this._isFirstFileRead && file.GetFullName() != ScsPath.Def.CountryBaseName ) {
                RawHelper.SaveRawFile( RawType.COUNTRY, file.GetFullName(), fileContent );
                this._isFirstFileRead = true;
            }
            // -- ./Raw generation

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