using System.Collections.Generic;
using System.Text;
using Serilog;
using TsMap2.Factory.Entity;
using TsMap2.Helper;
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

                    this.Store().Def.AddCountry( TsCountryFactory.Create( path ) );
                    // if ( country.Token != 0 && !this._countriesLookup.ContainsKey( country.Token ) )
                    //     this._countriesLookup.Add( country.Token, country );
                }
            }

            Log.Information( "[Job][Country] Loaded. Found: {0}", this.Store().Def.Countries.Count );
        }
    }
}