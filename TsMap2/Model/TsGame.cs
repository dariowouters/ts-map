namespace TsMap2.Model {
    public class TsGame {
        public const string GAME_ETS = "ets";
        public const string GAME_ATS = "ets";

        public string Code = null;

        public string Version = "";

        public string FullName() {
            if ( this.Code == GAME_ETS )
                return "Euro Truck Simulator 2";

            if ( this.Code == GAME_ATS )
                return "American Truck Simulator";

            return "";
        }
    }
}