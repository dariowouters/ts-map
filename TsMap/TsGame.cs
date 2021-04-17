using TsMap.HashFiles;

namespace TsMap {
    public class TsGame {
        private readonly TsMapper _mapper;

        public TsGame( TsMapper mapper ) {
            this._mapper = mapper;

            const string ets2LogoSceneSii = "def/ets2_logo_scene.sii";
            const string atsLogoSceneSii  = "def/ats_logo_scene.sii";
            ScsFile      ets2File         = this._mapper.Rfs.GetFileEntry( ets2LogoSceneSii );
            ScsFile      atsFile          = this._mapper.Rfs.GetFileEntry( atsLogoSceneSii );

            Log.Msg( "Game detection..." );

            if ( ets2File != null ) {
                Log.Msg( "ETS2 detected" );
                this.code = "ets2";
            } else if ( atsFile != null ) {
                Log.Msg( "ATS detected" );
                this.code = "ats";
            } else {
                Log.Msg( "Unknown game" );
                this.code = null;
            }
        }

        public string code { get; }

        public string FullName() {
            if ( this.code == "ets2" )
                return "Euro Truck Simulator 2";
            if ( this.code == "ats" )
                return "American Truck Simulator";
            return "";
        }
    }
}