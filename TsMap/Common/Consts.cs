using TsMap.Helpers.Logger;

namespace TsMap.Common
{
    internal static class Consts
    {
        /// <summary>
        /// Number of DLC guards that are in the game, DLC guards can be found in the map editor.
        /// <!--DLC guards seem to be hardcoded in the exe of the game, so no real easy way to make this dynamic-->
        /// </summary>
        public const int AtsDlcGuardCount = 19;

        /// <summary>
        /// Number of DLC guards that are in the game, DLC guards can be found in the map editor.
        /// <!--DLC guards seem to be hardcoded in the exe of the game, so no real easy way to make this dynamic-->
        /// </summary>
        public const int Ets2DlcGuardCount = 12;
        public const float LaneWidth = 4.5f;

        /// <summary>
        /// Magic mark that should be at the start of the file, 'SCS#' as utf-8 bytes
        /// </summary>
        internal const uint ScsMagic = 592659283;

        internal const LogLevel MinimumLogLevel = LogLevel.Debug;
    }
}
