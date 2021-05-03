namespace TsMap2.Model {
    public class Settings {
        public string AtsPath      = "C:\\Plop";
        public string Ets2Path     = "C:\\Plop";
        public string FallbackGame = TsGame.GAME_ETS;
        public string Name;

        public string GetActiveGamePath() {
            if ( this.FallbackGame == TsGame.GAME_ETS )
                return this.Ets2Path;

            if ( this.FallbackGame == TsGame.GAME_ATS )
                return this.AtsPath;

            return null;
        }
    }
}