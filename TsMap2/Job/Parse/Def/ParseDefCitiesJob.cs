using System.Collections.Generic;
using System.Text;
using Serilog;
using TsMap2.Factory;
using TsMap2.Helper;
using TsMap2.Model;
using TsMap2.Scs;

namespace TsMap2.Job.Parse.Def {
    public class ParseDefCitiesJob : ThreadJob {
        private bool _isFirstFileRead;

        protected override void Do() {
            Log.Debug( "[Job][City] Loading" );

            // --- Check RFS
            if ( this.Store().Rfs == null )
                throw new JobException( "[Job][City] The root file system was not initialized. Check the game path", this.JobName(), null );

            ScsDirectory defDirectory = this.Store().Rfs.GetDirectory( ScsPath.Def.DefFolderName );
            if ( defDirectory == null ) {
                var message = $"[Job][City] Could not read '{ScsPath.Def.DefFolderName}' dir";
                throw new JobException( message, this.JobName(), ScsPath.Def.DefFolderName );
            }

            List< ScsFile > cityFiles = defDirectory.GetFiles( ScsPath.Def.CityFileName );
            if ( cityFiles == null ) {
                var message = $"[Job][City] Could not read {ScsPath.Def.CityFileName} files";
                throw new JobException( message, this.JobName(), ScsPath.Def.CityFileName );
            }

            foreach ( ScsFile cityFile in cityFiles ) {
                byte[]   data  = cityFile.Entry.Read();
                string[] lines = Encoding.UTF8.GetString( data ).Split( '\n' );
                foreach ( string line in lines ) {
                    if ( line.TrimStart().StartsWith( "#" ) ) continue;
                    if ( !line.Contains( "@include" ) ) continue;

                    string path = ScsHelper.GetFilePath( line.Split( '"' )[ 1 ], "def" );
                    this.Store().Def.AddCity( this.Parse( path ) );
                }
            }

            Log.Information( "[Job][City] Loaded. Found: {0}", this.Store().Def.Cities.Count );
        }

        private TsCity Parse( string path ) {
            ( string name, string country, ulong token, string localizationToken, List< int > xOffsets, List< int > yOffsets ) =
                ScsParseHelper.CityParse( path );

            // -- Raw generation
            if ( !this._isFirstFileRead ) {
                RawHelper.SaveRawFile( RawType.CITY, path, null );
                this._isFirstFileRead = true;
            }
            // -- ./Raw generation

            return new TsCity( name, country, token, localizationToken, xOffsets, yOffsets );
        }
    }
}