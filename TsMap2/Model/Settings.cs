using TsMap2.Helper;
using TsMap2.Model.MapPalette;

namespace TsMap2.Model {
    public class Settings {
        public string      AtsPath              = "C:\\Plop";
        public string      Ets2Path             = "C:\\Plop";
        public Export      ExportSettings       = new Export();
        public string      FallbackGame         = TsGame.GAME_ETS;
        public MapColor    MapColor             = new MapColor();
        public string      OutputPath           = AppPath.OutputDir;
        public RenderFlags RenderFlags          = RenderFlags.All;
        public string      SelectedLocalization = "";

        public string GetActiveGamePath() {
            if ( FallbackGame == TsGame.GAME_ETS )
                return Ets2Path;

            if ( FallbackGame == TsGame.GAME_ATS )
                return AtsPath;

            return null;
        }
    }
}