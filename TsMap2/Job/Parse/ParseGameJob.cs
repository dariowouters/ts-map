using Serilog;
using TsMap2.Model;
using TsMap2.Scs.FileSystem.Entry;

namespace TsMap2.Job.Parse {
    public class ParseGameJob : ThreadJob {
        protected override void Do() {
            Log.Debug( "[Job][Game] Loading" );
            // Console.WriteLine( this.Store().Settings.GamePath );

            var gameEntry    = new ScsGameEntry();
            var versionEntry = new ScsVersionEntry();

            Store().Game = new TsGame( gameEntry.Get(), versionEntry.Get() );

            Log.Information( "[Job][Game] Loaded. Game: {0} | Version: {1}", Store().Game.FullName(), Store().Game.Version );
        }
    }
}