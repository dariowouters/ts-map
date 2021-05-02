namespace TsMap2.Model {
    public class TsGame {
        public const string GAME_ETS = "ets";
        public const string GAME_ATS = "ets";

        public string Code = null;

        public string Version = "";

        public string FullName() {
            if ( this.IsEts2() )
                return "Euro Truck Simulator 2";

            if ( this.IsAts() )
                return "American Truck Simulator";

            return "";
        }

        public bool IsEts2() => this.Code == GAME_ETS;

        public bool IsAts() => this.Code == GAME_ATS;
    }
}