using System.Collections.Generic;
using System.Text;
using Serilog;
using TsMap2.Helper;
using TsMap2.Model;
using TsMap2.Scs;

namespace TsMap2.Job.Parse.Def {
    public class ParseDefCitiesJob : ThreadJob {
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
            ScsFile file = this.Store().Rfs.GetFileEntry( path );

            if ( file == null ) return null;
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

            return new TsCity( name, country, token, localizationToken, xOffsets, yOffsets );
        }
    }
}