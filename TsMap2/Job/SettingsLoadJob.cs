using System.IO;
using Newtonsoft.Json.Linq;
using Serilog;
using TsMap2.Factory.Json;
using TsMap2.Helper;
using TsMap2.Model;

namespace TsMap2.Job {
    public class SettingsLoadJob : ThreadJob {
        public SettingsJsonFactory SettingFactory = new SettingsJsonFactory( new Settings() );

        protected override void Do() {
            Log.Debug( "[Job][Setting] Loading" );

            if ( !SettingFactory.FileExist() ) {
                var context = new JObject { [ "path" ] = SettingFactory.GetLoadingPath() };
                throw new JobException( "Unable to find the setting file", JobName(), context );
            }

            Settings settings = SettingFactory.Load();

            if ( settings.GetActiveGamePath() == null || !Directory.Exists( settings.GetActiveGamePath() ) )
                throw new JobException( "Game path was not found or is incorrect", JobName(), settings );

            Store().SetSetting( settings );

            Log.Information( "[Job][Setting] Loaded" );
        }
    }
}