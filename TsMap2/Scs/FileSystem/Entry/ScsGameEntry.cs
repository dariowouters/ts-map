using System;
using TsMap2.Model;

namespace TsMap2.Scs.FileSystem.Entry {
    public class ScsGameEntry : AbstractScsEntry< string > {
        public string Get() {
            VerifyRfs();

            ScsFile ets2File = Store.Rfs.GetFileEntry( ScsPath.Def.Ets2LogoScene );
            ScsFile atsFile  = Store.Rfs.GetFileEntry( ScsPath.Def.AtsLogoScene );
            var     gameCode = "";

            if ( ets2File != null ) // Log.Msg( "ETS2 detected" );
                gameCode = TsGame.GAME_ETS;
            else if ( atsFile != null ) // Log.Msg( "ATS detected" );
                gameCode = TsGame.GAME_ATS;

            return gameCode;
        }

        public override string Generate( byte[] stream ) => throw new NotImplementedException();
    }
}