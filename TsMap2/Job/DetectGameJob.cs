using System;
using TsMap2.Model;
using TsMap2.ScsHash;

namespace TsMap2.Job {
    public class DetectGameJob : ThreadJob {
        private readonly TsMapperContext _mapperContext;

        public DetectGameJob( TsMapperContext mapperContext ) => this._mapperContext = mapperContext;

        protected override void Do() {
            Console.Write( "Game detection... " );

            Settings settings = this._mapperContext.GetSettings();
            ScsFile  ets2File = this._mapperContext.rfs.GetFileEntry( TsSiiDef.Ets2LogoScene );
            ScsFile  atsFile  = this._mapperContext.rfs.GetFileEntry( TsSiiDef.AtsLogoScene );

            if ( ets2File != null ) // Log.Msg( "ETS2 detected" );
                Console.WriteLine( "ETS2 detected" );
            else if ( atsFile != null ) // Log.Msg( "ATS detected" );
                Console.WriteLine( "ATS detected" );
            else // Log.Msg( "Unknown game" );
                Console.WriteLine( "Unknown game" );
        }

        public override void OnEnd() {
            // Console.WriteLine( "AJob end" );
        }

        protected override string JobName() => "AJob";
    }
}