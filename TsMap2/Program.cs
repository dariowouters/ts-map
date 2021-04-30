using System;
using TsMap2.Factory.Json;
using TsMap2.Job;
using TsMap2.Model;
using TsMap2.ScsHash;

namespace TsMap2 {
    internal class Program {
        private static void Main( string[] args ) {
            Console.WriteLine( "===============================" );
            Console.WriteLine( "> TsMap2 - &copy; 2021 JAGFx" );
            Console.WriteLine( "> v0.0.0.0" );
            Console.WriteLine( "===============================" );
            Console.WriteLine( "" );

            // Load settings file
            var settingsJsonFactory =
                new TsSettingsJsonFactory< Settings >( new Settings { Name = "plop", GamePath = "/media/equinox/Documents/Projects/ETS/Euro Truck Simulator 2" } );
            settingsJsonFactory.Save();
            Settings settings = settingsJsonFactory.Load();

            // Create the TsMapper
            var rfs    = new RootFileSystem( settings.GamePath );
            var mapper = new TsMapperContext( rfs );
            mapper.SetSettings( settings );

            // Do a work
            Console.WriteLine( "== Game detection ==" );
            var j = new DetectGameJob( mapper );
            j.Run();
        }
    }
}