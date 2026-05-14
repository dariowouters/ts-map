using System.Drawing;

namespace TsMap
{
    public class MapPalette
    {
        public MapPalette()
        {
            // defaults

            Background = new SolidBrush(Color.FromArgb(72, 78, 102));
            Road = Brushes.White;
            PrefabRoad = Brushes.White;
            PrefabLight = new SolidBrush(Color.FromArgb(236, 203, 153));
            PrefabDark = new SolidBrush(Color.FromArgb(225, 163, 56));
            PrefabGreen = new SolidBrush(Color.FromArgb(170, 203, 150)); // TODO: Check if green has a specific z-index

            CityName = Brushes.LightCoral;

            FerryLines = new SolidBrush(Color.FromArgb(80, 255, 255, 255));

            Error = Brushes.LightCoral;
        }

        /// <summary>
        /// Background of map
        /// </summary>
        public Brush Background;

        /// <summary>
        /// Color of Road segments
        /// </summary>
        public Brush Road;

        /// <summary>
        /// Ferry Lines
        /// </summary>
        public Brush FerryLines;

        /// <summary>
        /// Prefab roads (prefabs are crosspoints, etc.)
        /// </summary>
        public Brush PrefabRoad;

        /// <summary>
        /// Prefab polygon light background
        /// </summary>
        public Brush PrefabLight;

        /// <summary>
        /// Prefab polygon dark background
        /// </summary>
        public Brush PrefabDark;

        /// <summary>
        /// Prefab polygon green background (called green in blender, seems to be the same as PrefabLight in in-game map)
        /// </summary>
        public Brush PrefabGreen;

        /// <summary>
        /// City names color
        /// </summary>
        public Brush CityName;

        /// <summary>
        /// Brush for error text
        /// </summary>
        public Brush Error;
    }
}