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

            ScsDirectory defDirectory = this.Store().Rfs.GetDirectory( "def" );
            if ( defDirectory == null ) {
                Log.Error( "[Job][Country] Could not read 'def' dir" );
                return;
            }

            List< ScsFile > countryFiles = defDirectory.GetFiles( "country" );
            if ( countryFiles == null ) {
                Log.Error( "[Job][Country] Could not read country files" );
                return;
            }

            foreach ( ScsFile countryFile in countryFiles ) {
                byte[]   data  = countryFile.Entry.Read();
                string[] lines = Encoding.UTF8.GetString( data ).Split( '\n' );

                foreach ( string line in lines ) {
                    if ( line.TrimStart().StartsWith( "#" ) ) continue;

                    if ( line.Contains( "@include" ) ) {
                        string path    = ScsHelper.GetFilePath( line.Split( '"' )[ 1 ], "def" );
                        var    country = new TsCountry();

                        ScsFile file = this.Store().Rfs.GetFileEntry( path );

                        if ( file == null ) return;
                        // LocalizedNames = new Dictionary<string, string>();
                        byte[] fileContent = file.Entry.Read();

                        string[] fileLines = Encoding.UTF8.GetString( fileContent ).Split( '\n' );

                        foreach ( string fileLine in fileLines ) {
                            ( bool validLine, string key, string value ) = ScsSiiHelper.ParseLine( fileLine );

                            if ( !validLine ) continue;

                            if ( key == "country_data" )
                                country.Token = ScsHash.StringToToken( ScsSiiHelper.Trim( value.Split( '.' )[ 2 ] ) );
                            else if ( key == "country_id" )
                                country.CountryId = int.Parse( value );
                            else if ( key == "name" )
                                country.Name = value.Split( '"' )[ 1 ];
                            // else if (key == "name_localized")
                            // {
                            //     LocalizationToken = value.Split('"')[1];
                            //     LocalizationToken = LocalizationToken.Replace("@", "");
                            // }
                            else if ( key == "country_code" )
                                country.CountryCode = value.Split( '"' )[ 1 ];
                            else if ( key == "pos" ) {
                                string   vector = value.Split( '(' )[ 1 ].Split( ')' )[ 0 ];
                                string[] values = vector.Split( ',' );

                                country.X = float.Parse( values[ 0 ], CultureInfo.InvariantCulture );
                                country.Y = float.Parse( values[ 2 ], CultureInfo.InvariantCulture );
                            }
                        }

                        this.Store().AddCountry( country );
                        // if ( country.Token != 0 && !this._countriesLookup.ContainsKey( country.Token ) )
                        //     this._countriesLookup.Add( country.Token, country );
                    }
                }
            }

            Log.Information( "[Job][Country] Loaded. Found: {0}", this.Store().Countries.Count );
        }

        public override string JobName() => "ParseCountryFilesJob";

        protected override void OnEnd() { }
    }
}