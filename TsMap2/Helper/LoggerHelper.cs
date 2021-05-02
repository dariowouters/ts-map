using Serilog;

namespace TsMap2.Helper {
    public static class LoggerHelper {
        public static void Init() {
            Log.Logger = new LoggerConfiguration()
                         .MinimumLevel.Debug()
                         .WriteTo.Console()
                         .WriteTo.File( AppPath.LogPath, rollingInterval: RollingInterval.Day )
                         .CreateLogger();
        }
    }
}