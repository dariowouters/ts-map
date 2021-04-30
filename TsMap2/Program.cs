using System;
using Newtonsoft.Json.Linq;
using TsMap2.Factory.Json;
using TsMap2.Model;

namespace TsMap2 {
    internal class Program {
        private static void Main( string[] args ) {
            Console.WriteLine( "Hello World!" );
            var fk = new TsSettingsJsonFactory( new Settings { Name = "plop" } );

            fk.Save();
            JObject l = fk.Load();
            Console.WriteLine( l );
        }
    }
}