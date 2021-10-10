using System.Drawing;

namespace TsMap2.Model.MapPalette {
    public class MapColor {
        /// <summary>
        ///     Background of map
        /// </summary>
        public string Background;

        /// <summary>
        ///     City names color
        /// </summary>
        public string CityName;

        /// <summary>
        ///     Brush for error text
        /// </summary>
        public string Error;

        /// <summary>
        ///     Ferry Lines
        /// </summary>
        public string FerryLines;

        /// <summary>
        ///     Prefab polygon dark background
        /// </summary>
        public string PrefabDark;

        /// <summary>
        ///     Prefab polygon green background (called green in blender, seems to be the same as PrefabLight in in-game map)
        /// </summary>
        public string PrefabGreen;

        /// <summary>
        ///     Prefab polygon light background
        /// </summary>
        public string PrefabLight;

        /// <summary>
        ///     Prefab roads (prefabs are crosspoints, etc.)
        /// </summary>
        public string PrefabRoad;

        /// <summary>
        ///     Color of Road segments
        /// </summary>
        public string Road;

        public MapColor() : this( new MapPalette() ) { }

        public MapColor( MapPalette palette ) {
            CityName    = "#" + RGBToHex( ( (SolidBrush) palette.CityName ).Color );
            Background  = "#" + RGBToHex( ( (SolidBrush) palette.Background ).Color );
            Error       = "#" + RGBToHex( ( (SolidBrush) palette.Error ).Color );
            FerryLines  = "#" + RGBToHex( ( (SolidBrush) palette.FerryLines ).Color );
            PrefabDark  = "#" + RGBToHex( ( (SolidBrush) palette.PrefabDark ).Color );
            PrefabGreen = "#" + RGBToHex( ( (SolidBrush) palette.PrefabGreen ).Color );
            PrefabLight = "#" + RGBToHex( ( (SolidBrush) palette.PrefabLight ).Color );
            PrefabRoad  = "#" + RGBToHex( ( (SolidBrush) palette.PrefabRoad ).Color );
            Road        = "#" + RGBToHex( ( (SolidBrush) palette.Road ).Color );
        }

        public MapPalette ToBrushPalette() =>
            new MapPalette {
                CityName    = ConvertColor( CityName ),
                Background  = ConvertColor( Background ),
                FerryLines  = ConvertColor( FerryLines ),
                Error       = ConvertColor( Error ),
                PrefabDark  = ConvertColor( PrefabDark ),
                PrefabGreen = ConvertColor( PrefabGreen ),
                PrefabLight = ConvertColor( PrefabLight ),
                PrefabRoad  = ConvertColor( PrefabRoad ),
                Road        = ConvertColor( Road )
            };

        private string RGBToHex( Color color ) => color.R.ToString( "X2" ) + color.G.ToString( "X2" ) + color.B.ToString( "X2" );

        private Brush ConvertColor( string color ) {
            // var converter = new ColorConverter();
            var brush = new SolidBrush( ColorTranslator.FromHtml( color ) );
            return brush;
        }
    }
}