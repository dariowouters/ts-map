using System;
using System.Collections.Generic;
using System.Drawing;

namespace TsMap
{
    public class SettingsManager
    {
        public Settings Settings { get; set; }
        private static readonly Lazy<SettingsManager> _instance = new Lazy<SettingsManager>(() =>
        {
            return new SettingsManager();
        });

        public SettingsManager()
        {
            this.Settings = JsonHelper.LoadSettings();

            // managing default palette
            if (this.Settings.Palette == null || (this.Settings.Palette != null && this.Settings.Palette.Background == null))
                this.Settings.Palette = new MapPaletteSettings(new MapPalette());

            if (this.Settings.TileGenerator == null)
            {
                this.Settings.TileGenerator = new TilesGeneratorSettings()
                {
                    TileSize = 256,
                    MapPadding = 500,
                    ExportFlags = ExportFlags.All,
                    RenderFlags = RenderFlags.All
                };
            }
        }

        public static SettingsManager Current
        {
            get
            {
                return _instance.Value;
            }
        }

        public void SaveSettings()
        {
            JsonHelper.SaveSettings(_instance.Value.Settings);
        }
    }

    public class Settings
    {
        public Settings()
        {
            this.Palette = new MapPaletteSettings();
            this.TileGenerator = new TilesGeneratorSettings();
        }

        public string LastGamePath { get; set; }
        public string LastModPath { get; set; }
        public string LastTileMapPath { get; set; }
        public MapPaletteSettings Palette { get; set; }
        public TilesGeneratorSettings TileGenerator { get; set; }
        public List<Mod> Mods { get; set; }
    }

    public class TilesGeneratorSettings
    {
        public string LastTileMapPath { get; set; }
        public int TileSize { get; set; }
        public int MapPadding { get; set; }
        public ExportFlags ExportFlags { get; set; }
        public RenderFlags RenderFlags { get; set; }
        public bool GenerateTiles { get; set; }
        public int StartZoomLevel { get; set; }
        public int EndZoomLevel { get; set; }
    }

    public class MapPaletteSettings
    {
        /// <summary>
        /// Background of map
        /// </summary>
        public string Background;

        /// <summary>
        /// Color of Road segments
        /// </summary>
        public string Road;

        /// <summary>
        /// Ferry Lines
        /// </summary>
        public string FerryLines;

        /// <summary>
        /// Prefab roads (prefabs are crosspoints, etc.)
        /// </summary>
        public string PrefabRoad;

        /// <summary>
        /// Prefab polygon light background
        /// </summary>
        public string PrefabLight;

        /// <summary>
        /// Prefab polygon dark background
        /// </summary>
        public string PrefabDark;

        /// <summary>
        /// Prefab polygon green background (called green in blender, seems to be the same as PrefabLight in in-game map)
        /// </summary>
        public string PrefabGreen;

        /// <summary>
        /// City names color
        /// </summary>
        public string CityName;

        /// <summary>
        /// Brush for error text
        /// </summary>
        public string Error;

        public MapPaletteSettings()
        {

        }

        public MapPaletteSettings(MapPalette palette)
        {
            this.CityName = "#" + RGBToHex(((SolidBrush)palette.CityName).Color);
            this.Background = "#" + RGBToHex(((SolidBrush)palette.Background).Color);
            this.Error = "#" + RGBToHex(((SolidBrush)palette.Error).Color);
            this.FerryLines = "#" + RGBToHex(((SolidBrush)palette.FerryLines).Color);
            this.PrefabDark = "#" + RGBToHex(((SolidBrush)palette.PrefabDark).Color);
            this.PrefabGreen = "#" + RGBToHex(((SolidBrush)palette.PrefabGreen).Color);
            this.PrefabLight = "#" + RGBToHex(((SolidBrush)palette.PrefabLight).Color);
            this.PrefabRoad = "#" + RGBToHex(((SolidBrush)palette.PrefabRoad).Color);
            this.Road = "#" + RGBToHex(((SolidBrush)palette.Road).Color);
        }

        public MapPalette ToBrushPalette()
        {
            MapPalette palette = new MapPalette();

            palette.CityName = this.ConvertColor(this.CityName);
            palette.Background = this.ConvertColor(this.Background);
            palette.FerryLines = this.ConvertColor(this.FerryLines);
            palette.Error = this.ConvertColor(this.Error);
            palette.PrefabDark = this.ConvertColor(this.PrefabDark);
            palette.PrefabGreen = this.ConvertColor(this.PrefabGreen);
            palette.PrefabLight = this.ConvertColor(this.PrefabLight);
            palette.PrefabRoad = this.ConvertColor(this.PrefabRoad);
            palette.Road = this.ConvertColor(this.Road);

            return palette;
        }

        private string RGBToHex(Color color)
        {
            return color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
        }

        private Brush ConvertColor(string color)
        {
            var converter = new ColorConverter();
            var brush = new SolidBrush(ColorTranslator.FromHtml(color));
            return brush;
        }
    }
}
