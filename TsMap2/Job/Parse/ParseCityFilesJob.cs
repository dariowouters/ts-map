using System.Collections.Generic;
using System.Text;
using Serilog;
using TsMap2.Helper;
using TsMap2.Model;
using TsMap2.Scs;

namespace TsMap2.Job.Parse {
    public class ParseCityFilesJob : ThreadJob {
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
                    this.Store().AddCity( this.Parse( path ) );
                }
            }

            Log.Information( "[Job][City] Loaded. Found: {0}", this.Store().Cities.Count );
        }

        protected override void OnEnd() { }

        private TsCity Parse( string path ) {
            ScsFile file = this.Store().Rfs.GetFileEntry( path );

            if ( file == null ) return null;
            // LocalizedNames = new Dictionary< string, string >();
            byte[] fileContent = file.Entry.Read();

            string[] lines       = Encoding.UTF8.GetString( fileContent ).Split( '\n' );
            var      offsetCount = 0;
            var      XOffsets    = new List< int >();
            var      YOffsets    = new List< int >();
            ulong    Token       = 0;
            string   Name        = null;
            string   Country     = null;

            foreach ( string line in lines ) {
                ( bool validLine, string key, string value ) = ScsSiiHelper.ParseLine( line );
                if ( !validLine ) continue;

                if ( key == "city_data" )
                    Token = ScsHash.StringToToken( ScsSiiHelper.Trim( value.Split( '.' )[ 1 ] ) );
                else if ( key == "city_name" )
                    Name = line.Split( '"' )[ 1 ];
                // else if (key == "city_name_localized")
                // {
                //     LocalizationToken = value.Split('"')[1];
                //     LocalizationToken = LocalizationToken.Replace("@", "");
                // }
                else if ( key == "country" )
                    Country = value;
                else if ( key.Contains( "map_x_offsets[]" ) ) {
                    if ( ++offsetCount > 4 )
                        if ( int.TryParse( value, out int offset ) )
                            XOffsets.Add( offset );

                    if ( offsetCount == 8 ) offsetCount = 0;
                } else if ( key.Contains( "map_y_offsets[]" ) )
                    if ( ++offsetCount > 4 )
                        if ( int.TryParse( value, out int offset ) )
                            YOffsets.Add( offset );
            }

            return new TsCity( Name, Country, Token, XOffsets, YOffsets );
        }
    }
}