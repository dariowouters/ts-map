using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Serilog;
using TsMap2.Helper;
using TsMap2.Model;
using TsMap2.Scs;

namespace TsMap2.Job.Parse.Def {
    public class ParseDefRoadLooksJob : ThreadJob {
        protected override void Do() {
            Log.Debug( "[Job][RoadLook] Loading" );

            // --- Check RFS
            if ( this.Store().Rfs == null )
                throw new JobException( "[Job][RoadLook] The root file system was not initialized. Check the game path", this.JobName(), null );

            ScsDirectory worldDirectory = this.Store().Rfs.GetDirectory( ScsPath.Def.WorldPath );
            if ( worldDirectory == null ) {
                var message = $"[Job][RoadLook] Could not read '{ScsPath.Def.WorldPath}' dir";
                throw new JobException( message, this.JobName(), ScsPath.Def.WorldPath );
            }

            List< ScsFile > roadLookFiles = worldDirectory.GetFiles( ScsPath.Def.RoadLook );
            if ( roadLookFiles == null ) {
                var message = $"[Job][RoadLook] Could not read {ScsPath.Def.RoadLook} files";
                throw new JobException( message, this.JobName(), ScsPath.Def.RoadLook );
            }

            foreach ( ScsFile roadLookFile in roadLookFiles ) {
                if ( !roadLookFile.GetFileName().StartsWith( "road" ) ) continue;
                byte[]     data     = roadLookFile.Entry.Read();
                string[]   lines    = Encoding.UTF8.GetString( data ).Split( '\n' );
                TsRoadLook roadLook = null;

                foreach ( string line in lines ) {
                    ( bool validLine, string key, string value ) = ScsSiiHelper.ParseLine( line );
                    if ( validLine ) {
                        if ( key == "road_look" )
                            roadLook =
                                new TsRoadLook( ScsHash.StringToToken( ScsSiiHelper.Trim( value.Split( '.' )[ 1 ]
                                                                                               .Trim( '{' ) ) ) );
                        if ( roadLook == null ) continue;
                        if ( key == "lanes_left[]" )
                            roadLook.LanesLeft.Add( value );
                        else if ( key == "lanes_right[]" )
                            roadLook.LanesRight.Add( value );
                        else if ( key == "road_offset" )
                            roadLook.Offset = float.Parse( value, CultureInfo.InvariantCulture );
                    }

                    if ( line.Contains( "}" ) && roadLook != null )
                        this.Store().Def.AddRoadLook( roadLook );
                }
            }

            Log.Information( "[Job][RoadLook] Loaded. Found: {0}", this.Store().Def.RoadLooks.Count );
        }

        protected override void OnEnd() { }
    }
}