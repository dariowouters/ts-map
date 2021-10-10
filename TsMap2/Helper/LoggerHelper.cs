using Serilog;

namespace TsMap2.Helper {
    public static class LoggerHelper {
        public static void Init( bool debug = false ) {
            Log.Logger = debug
                             ? new LoggerConfiguration()
                               .MinimumLevel.Debug()
                               .WriteTo.Console()
                               .WriteTo.File( AppPath.LogPath, rollingInterval: RollingInterval.Day )
                               .CreateLogger()
                             : new LoggerConfiguration()
                               .MinimumLevel.Information()
                               .WriteTo.Console()
                               .WriteTo.File( AppPath.LogPath, rollingInterval: RollingInterval.Day )
                               .CreateLogger();
        }
    }
}