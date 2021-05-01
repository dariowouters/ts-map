using System;
using TsMap2.Helper;
using TsMap2.Job;
using TsMap2.Job.Parse;

namespace TsMap2 {
    internal class Program {
        private static void Main( string[] args ) {
            Console.WriteLine( "===============================" );
            Console.WriteLine( "> TsMap2 - &copy; 2021 JAGFx" );
            Console.WriteLine( "> v0.0.0.0" );
            Console.WriteLine( "===============================" );
            Console.WriteLine( "" );

            try {
                var store = StoreHelper.Instance;

                var s = new SettingsLoadJob();
                s.RunAndWait();

                var c = new ParseScsFilesJob();
                c.Run();

                Console.WriteLine( $@"Game: {store.Game.FullName()}" );
            } catch ( Exception e ) {
                Console.WriteLine( e );
                throw;
            }
        }
    }
}