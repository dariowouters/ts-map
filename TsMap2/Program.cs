using System;
using Serilog;
using TsMap2.Helper;
using TsMap2.Job;
using TsMap2.Job.Parse;

namespace TsMap2 {
    internal class Program {
        private static void Main( string[] args ) {
            // Console.WriteLine( "===============================" );
            // Console.WriteLine( "> TsMap2 - &copy; 2021 JAGFx" );
            // Console.WriteLine( "> v0.0.0.0" );
            // Console.WriteLine( "===============================" );
            // Console.WriteLine( "" );

            LoggerHelper.Init();

            Log.Information( "Hello, world!" );

            try {
                var s = new SettingsLoadJob();
                s.RunAndWait();

                var c = new ParseScsFilesJob();
                c.Run();
            } catch ( Exception e ) {
                Log.Error( e, "Something went wrong" );
                throw;
            } finally {
                Log.CloseAndFlush();
            }
        }
    }
}