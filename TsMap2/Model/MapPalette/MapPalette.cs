using System.Drawing;

namespace TsMap2.Model.MapPalette {
    public class MapPalette {
        /// <summary>
        ///     Background of map
        /// </summary>
        public Brush Background;

        /// <summary>
        ///     City names color
        /// </summary>
        public Brush CityName;

        /// <summary>
        ///     Brush for error text
        /// </summary>
        public Brush Error;

        /// <summary>
        ///     Ferry Lines
        /// </summary>
        public Brush FerryLines;

        /// <summary>
        ///     Prefab polygon dark background
        /// </summary>
        public Brush PrefabDark;

        /// <summary>
        ///     Prefab polygon green background (called green in blender, seems to be the same as PrefabLight in in-game map)
        /// </summary>
        public Brush PrefabGreen;

        /// <summary>
        ///     Prefab polygon light background
        /// </summary>
        public Brush PrefabLight;

        /// <summary>
        ///     Prefab roads (prefabs are crosspoints, etc.)
        /// </summary>
        public Brush PrefabRoad;

        /// <summary>
        ///     Color of Road segments
        /// </summary>
        public Brush Road;

        public MapPalette() {
            Background  = new SolidBrush( Color.FromArgb( 48,  48,  48 ) );
            Road        = new SolidBrush( Color.FromArgb( 255, 220, 80 ) );
            PrefabRoad  = new SolidBrush( Color.FromArgb( 255, 220, 80 ) );
            PrefabLight = new SolidBrush( Color.FromArgb( 236, 203, 153 ) );
            PrefabDark  = new SolidBrush( Color.FromArgb( 225, 163, 56 ) );
            PrefabGreen = new SolidBrush( Color.FromArgb( 170, 203, 150 ) ); // TODO: Check if green has a specific z-index
            CityName    = new SolidBrush( Color.FromArgb( 222, 222, 222 ) );
            Error       = new SolidBrush( Color.FromArgb( 48,  48,  48 ) );
            FerryLines  = Brushes.White;
        }
    }
}