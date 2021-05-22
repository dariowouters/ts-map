using System;
using Serilog;
using TsMap2.Helper;
using TsMap2.Job;
using TsMap2.Job.Export;
using TsMap2.Job.Parse.Def;
using TsMap2.Job.Parse.Map;

namespace TsMap2 {
    internal class Program {
        private static void Main( string[] args ) {
            // Console.WriteLine( "===============================" );
            // Console.WriteLine( "> TsMap2 - &copy; 2021 JAGFx" );
            // Console.WriteLine( "> v0.0.0.0" );
            // Console.WriteLine( "===============================" );
            // Console.WriteLine( "" );

            LoggerHelper.Init();

            Log.Information( "HomeDir: {0}", AppPath.HomeDirApp );

            try {
                var s = new SettingsLoadJob();
                s.RunAndWait();

                var c = new ParseScsDefJob();
                c.Run();

                var m = new ParseMapJob();
                m.Run();

                var e = new ExportJob();
                e.Run();
            } catch ( Exception e ) {
                Log.Error( "Unexpected Exception: {0} | Stack: {1}", e.GetBaseException().Message, e.GetBaseException().StackTrace );
                Log.Error( "Unexpected Exception: {0} | Stack: {1}", e.Message,                    e.StackTrace );
            } /*finally {
                Log.CloseAndFlush();
            }*/
        }
    }
}