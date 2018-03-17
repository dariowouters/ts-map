using System.Drawing;

namespace TsMap
{
    public class MapPalette
    {
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