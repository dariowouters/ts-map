using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TsMap2.Exceptions;
using TsMap2.Helper;
using TsMap2.Model;

namespace TsMap2.Scs.FileSystem.Entry {
    public class ScsRoadLooksEntry : AbstractScsEntry< Dictionary< ulong, TsRoadLook > > {
        private readonly Dictionary< ulong, TsRoadLook > _roadLooks = new Dictionary< ulong, TsRoadLook >();

        public Dictionary< ulong, TsRoadLook > List() {
            ScsDirectory worldDirectory = Store.Rfs.GetDirectory( ScsPath.Def.WorldPath );
            if ( worldDirectory == null ) {
                var message = $"[Job][RoadLook] Could not read '{ScsPath.Def.WorldPath}' dir";
                throw new ScsEntryException( message );
            }

            List< ScsFile > roadLookFiles = worldDirectory.GetFiles( ScsPath.Def.RoadLook );
            if ( roadLookFiles == null ) {
                var message = $"[Job][RoadLook] Could not read {ScsPath.Def.RoadLook} files";
                throw new ScsEntryException( message );
            }

            // var roadLooks = new Dictionary< ulong, TsRoadLook >();
            foreach ( ScsFile roadLookFile in roadLookFiles ) {
                if ( !roadLookFile.GetFileName().StartsWith( "road" ) ) continue;
                byte[] data = roadLookFile.Entry.Read();
                Generate( data );
            }

            return _roadLooks;
        }

        public override Dictionary< ulong, TsRoadLook > Generate( byte[] stream ) {
            string[]   lines    = Encoding.UTF8.GetString( stream ).Split( '\n' );
            TsRoadLook roadLook = null;

            foreach ( string line in lines ) {
                ( bool validLine, string key, string value ) = ScsSiiHelper.ParseLine( line );
                if ( validLine ) {
                    if ( key == "road_look" )
                        roadLook =
                            new TsRoadLook( ScsHashHelper.StringToToken( ScsSiiHelper.Trim( value.Split( '.' )[ 1 ]
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
                    AddRoadLook( roadLook );
                //     Store().Def.AddRoadLook( roadLook );
            }

            return _roadLooks;
        }

        private void AddRoadLook( TsRoadLook roadLook ) {
            if ( roadLook.Token != 0 && !_roadLooks.ContainsKey( roadLook.Token ) ) // Log.Debug( "R: {0}", roadLook.Token );
                _roadLooks.Add( roadLook.Token, roadLook );
        }
    }
}