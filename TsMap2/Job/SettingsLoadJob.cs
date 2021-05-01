using System;
using TsMap2.Factory.Json;
using TsMap2.Model;

namespace TsMap2.Job {
    public class SettingsLoadJob : ThreadJob {
        public TsSettingsJsonFactory SettingFactory = new TsSettingsJsonFactory( new Settings() );

        protected override void Do() {
            Console.WriteLine( "[START] SettingJob" );
            this.Store().SetSetting( this.SettingFactory.Load() );
        }

        public override string JobName() => "Setting";

        protected override void OnEnd() {
            Console.WriteLine( "[END] SettingJob" );
        }
    }
}