using System;
using System.Collections.Generic;
using TsMap2.Model;

namespace TsMap2.Helper {
    public static class SettingGeneratorHelper {
        public static Settings GenerateANewSettingFromConsole() {
            var setting = new Settings();

            setting.AtsPath  = ReadString( "American Truck Simulator game path", setting.AtsPath );
            setting.Ets2Path = ReadString( "Euro Truck Simulator 2 game path",   setting.Ets2Path );

            string exportFor = ReadStringFromChoice( "Export for", new List< string >( new[] { "Jagfx", "Default" } ), "Jagfx" );
            setting.ExportSettings.ExportType = exportFor == "Jagfx"
                                                    ? TsExportType.JAGFX
                                                    : TsExportType.DEFAULT;

            setting.ExportSettings.TilePadding = ReadInt( "Tiles map padding", setting.ExportSettings.TilePadding.ToString() );
            setting.ExportSettings.TileSize    = ReadInt( "Tiles map size",    setting.ExportSettings.TileSize.ToString() );
            setting.ExportSettings.TileZoomMin = ReadInt( "Min zoom export",   setting.ExportSettings.TileZoomMin.ToString() );
            setting.ExportSettings.TileZoomMax = ReadInt( "Max zoom export",   setting.ExportSettings.TileZoomMax.ToString() );

            string fallbackGame = ReadStringFromChoice( "Choose your game for export",
                                                        new List< string >( new[] { "American Truck Simulator", "Euro Truck Simulator 2" } ),
                                                        "Euro Truck Simulator 2" );
            setting.FallbackGame = fallbackGame == "Euro Truck Simulator 2"
                                       ? TsGame.GAME_ETS
                                       : TsGame.GAME_ATS;

            setting.OutputPath = ReadString( "Export to", setting.OutputPath );

            // Console.WriteLine( "Render the map city name: " );
            // Console.WriteLine( "Render the map prefabs: " );
            // Console.WriteLine( "Render the map roads: " );
            // Console.WriteLine( "Render the map areas: " );
            // Console.WriteLine( "Render the map overlays: " );
            // Console.WriteLine( "Render the map ferry connections: " );

            return setting;
        }

        private static string ReadString( string text, string? placeholder = null ) {
            Request( text, placeholder );

            string reading = Console.ReadLine()!;
            reading = string.IsNullOrEmpty( reading )
                          ? placeholder ?? string.Empty
                          : reading;

            Console.WriteLine( reading );

            return reading;
        }

        private static int ReadInt( string text, string? placeholder = null ) => Convert.ToInt32( ReadString( text, placeholder ) );

        private static string ReadStringFromChoice( string text, List< string > options, string? placeholder = null ) {
            Request( text, placeholder );

            var reading = "";
            do {
                Console.WriteLine( "Choices: " );
                foreach ( string option in options ) Console.WriteLine( "\t" + $@"> {option}" );

                reading = Console.ReadLine();
                reading = string.IsNullOrEmpty( reading )
                              ? placeholder ?? string.Empty
                              : reading;

                Console.WriteLine( reading );

                if ( !options.Contains( reading ) )
                    Console.WriteLine( "Invalid entry, please type a correct choice" );
            } while ( !options.Contains( reading ) );

            return reading;
        }

        // --

        private static void Request( string text, string? placeholder = null ) {
            Console.WriteLine( "" );
            Console.WriteLine( "---------" );

            var t = $@"{text}";

            if ( !string.IsNullOrEmpty( placeholder ) )
                t += $@" ({placeholder})";

            t += " ? :";

            Console.WriteLine( t );
        }
    }
}