namespace TsMap2.Model {
    public class TsGame {
        public const string GAME_ETS = "ets";
        public const string GAME_ATS = "ats";

        public TsGame( string code, string version ) {
            Code    = code;
            Version = version;
        }

        public string Code    { get; }
        public string Version { get; }

        public string FullName() {
            if ( IsEts2() )
                return "Euro Truck Simulator 2";

            if ( IsAts() )
                return "American Truck Simulator";

            return "Unknown";
        }

        public bool IsEts2() => Code == GAME_ETS;

        public bool IsAts() => Code == GAME_ATS;
    }
}