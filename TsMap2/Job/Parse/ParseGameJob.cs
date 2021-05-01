using System;
using TsMap2.Model;
using TsMap2.ScsHash;

namespace TsMap2.Job.Parse {
    public class ParseGameJob : ThreadJob {
        protected override void Do() {
            Console.WriteLine( "[START] ParseGameJob" );
            // Console.WriteLine( this.Store().Settings.GamePath );

            ScsFile ets2File = this.Store().Rfs.GetFileEntry( TsSiiDef.Ets2LogoScene );
            ScsFile atsFile  = this.Store().Rfs.GetFileEntry( TsSiiDef.AtsLogoScene );

            if ( ets2File != null ) // Log.Msg( "ETS2 detected" );
                this.Store().Game.Code = TsGame.GAME_ETS;
            else if ( atsFile != null ) // Log.Msg( "ATS detected" );
                this.Store().Game.Code = TsGame.GAME_ATS;
            else // Log.Msg( "Unknown game" );
                this.Store().Game.Code = null;
        }

        protected override void OnEnd() {
            Console.WriteLine( "[END] ParseGameJob" );
        }

        public override string JobName() => "ParseGameJob";
    }
}