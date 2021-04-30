using System;
using TsMap2.Factory.Json;
using TsMap2.Model;
using TsMap2.ScsHash;
using TsMap2.Work;

namespace TsMap2 {
    internal class Program {
        private static void Main( string[] args ) {
            Console.WriteLine( "Hello World!" );

            // Load settings file
            var settingsJsonFactory =
                new TsSettingsJsonFactory< Settings >( new Settings { Name = "plop", GamePath = "S:\\Games\\Steam\\steamapps\\common\\Euro Truck Simulator 2" } );
            settingsJsonFactory.Save();
            Settings settings = settingsJsonFactory.Load();
            Console.WriteLine( settings );

            // Create the TsMapper
            var rfs    = new RootFileSystem( settings.GamePath );
            var mapper = new TsMapperContext( rfs );
            mapper.SetSettings( settings );

            // Do a work
            var w = new AWork( mapper );
            w.Run();
        }
    }
}