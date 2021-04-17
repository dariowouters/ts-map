using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TsMap.TsTileMapInfo;

namespace TsMap {
    public class JsonHelper {
        private static readonly string _settingsPath =
            Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ), "ts-map" );

        public static void SaveTileMapInfo( string path,
                                            string tileMapInfoStructure,
                                            TsGame game,
                                            int    mapPadding,
                                            int    tileSize,
                                            float  x1,
                                            float  x2,
                                            float  y1,
                                            float  y2,
                                            int    minZoom,
                                            int    maxZoom,
                                            string gameVersion ) {
            JObject tileMapInfo = new TsTileMapDefault()
                .TileMapInfo( game, mapPadding, tileSize, x1, x2, y1, y2, minZoom, maxZoom, gameVersion );

            if ( tileMapInfoStructure == TsTileMapJagfxDash.Name )
                tileMapInfo = new TsTileMapJagfxDash()
                    .TileMapInfo( game, mapPadding, tileSize, x1, x2, y1, y2, minZoom, maxZoom, gameVersion );

            Directory.CreateDirectory( path );
            File.WriteAllText( Path.Combine( path, "TileMapInfo.json" ), tileMapInfo.ToString( Formatting.Indented ) );
        }

        public static void SaveSettings( Settings settings ) {
            Directory.CreateDirectory( _settingsPath );
            File.WriteAllText( Path.Combine( _settingsPath, "Settings.json" ),
                               JsonConvert.SerializeObject( settings, Formatting.Indented ) );
        }

        public static Settings LoadSettings() {
            if ( !File.Exists( Path.Combine( _settingsPath, "Settings.json" ) ) ) return new Settings();
            return JsonConvert.DeserializeObject< Settings >( File.ReadAllText( Path.Combine( _settingsPath,
                                                                                   "Settings.json" ) ) );
        }

        public static void SaveRoadPoints( List< dynamic > points ) {
            Directory.CreateDirectory( _settingsPath );
            var serializer = new JavaScriptSerializer();

            serializer.MaxJsonLength = int.MaxValue;
            File.WriteAllText( Path.Combine( _settingsPath, "RoadPoints.json" ), serializer.Serialize( points ) );
        }
    }
}