using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TsMap {
    public class JsonHelper {
        private static readonly string _settingsPath =
            Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ), "ts-map" );

        public static void SaveTileMapInfo( string path,
                                            TsGame game,
                                            float  width,
                                            float  height,
                                            int    tileSize,
                                            float  x1,
                                            float  x2,
                                            float  y1,
                                            float  y2,
                                            int    minZoom,
                                            int    maxZoom,
                                            string gameVersion ) {
            // TODO Set transposition factor as const or use a dynamic value
            var tileMapInfo = new JObject {
                [ "map" ] = new JObject {
                    [ "maxX" ]     = width,
                    [ "maxY" ]     = height,
                    [ "tileSize" ] = tileSize,
                    [ "minZoom" ]  = minZoom,
                    [ "maxZoom" ]  = maxZoom
                },
                [ "transposition" ] = new JObject {
                    [ "x" ] = new JObject {
                        [ "factor" ] = 1.087326,
                        [ "offset" ] = 57157
                    },
                    [ "y" ] = new JObject {
                        [ "factor" ] = 1.087326,
                        [ "offset" ] = 59287
                    }
                },
                [ "game" ] = new JObject {
                    [ "id" ]          = game.code,
                    [ "game" ]        = game.code,
                    [ "name" ]        = game.FullName(),
                    [ "version" ]     = gameVersion,
                    [ "generatedAt" ] = DateTime.Now
                }

                // [ "x1" ]          = x1,
                // [ "x2" ]          = x2,
                // [ "y1" ]          = y1,
                // [ "y2" ]          = y2,
                // [ "minZoom" ]     = minZoom,
                // [ "maxZoom" ]     = maxZoom,
                // [ "gameVersion" ] = gameVersion,
                // [ "generatedAt" ] = DateTime.Now
            };
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