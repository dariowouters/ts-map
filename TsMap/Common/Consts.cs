using System.Collections.Generic;
using TsMap.Helpers.Logger;

namespace TsMap.Common
{
    internal static class Consts
    {
        /// <summary>
        /// List of DLC guards that are in the game, DLC guards can be found in the map editor.
        /// <!--DLC guards seem to be hardcoded in the exe of the game, so no real easy way to make this dynamic-->
        /// </summary>
        public static readonly List<DlcGuard> DefaultAtsDlcGuards = new List<DlcGuard>()
        {
            new DlcGuard("No Guard", 0),
            new DlcGuard("dlc_nevada", 1),
            new DlcGuard("dlc_arizona", 2),
            new DlcGuard("dlc_nm", 3),
            new DlcGuard("dlc_or", 4),
            new DlcGuard("dlc_wa", 5),
            new DlcGuard("dlc_or_and_wa", 6),
            new DlcGuard("dlc_ut", 7),
            new DlcGuard("dlc_nm_and_ut", 8),
            new DlcGuard("dlc_id", 9),
            new DlcGuard("dlc_id_and_or", 10),
            new DlcGuard("dlc_id_and_ut", 11),
            new DlcGuard("dlc_id_and_wa", 12),
            new DlcGuard("dlc_co", 13),
            new DlcGuard("dlc_co_and_nm", 14),
            new DlcGuard("dlc_co_and_ut", 15),
            new DlcGuard("dlc_wy", 16),
            new DlcGuard("dlc_co_and_wy", 17),
            new DlcGuard("dlc_id_and_wy", 18),
            new DlcGuard("dlc_ut_and_wy", 19),
            new DlcGuard("dlc_tx", 20),
            new DlcGuard("dlc_nm_and_tx", 21),
            new DlcGuard("dlc_mt", 22),
            new DlcGuard("dlc_id_and_mt", 23),
            new DlcGuard("dlc_mt_and_wy", 24),
            new DlcGuard("dlc_ok", 25),
            new DlcGuard("dlc_ok_and_co", 26),
            new DlcGuard("dlc_ok_and_nm", 27),
            new DlcGuard("dlc_ok_and_tx", 28),
            new DlcGuard("dlc_ks", 29),
            new DlcGuard("dlc_ks_and_co", 30),
            new DlcGuard("dlc_ks_and_ok", 31),
            new DlcGuard("dlc_ne", 32),
            new DlcGuard("dlc_ne_and_co", 33),
            new DlcGuard("dlc_ne_and_ks", 34),
            new DlcGuard("dlc_ne_and_wy", 34),
        };

        /// <summary>
        /// List of DLC guards that are in the game, DLC guards can be found in the map editor.
        /// <!--DLC guards seem to be hardcoded in the exe of the game, so no real easy way to make this dynamic-->
        /// </summary>
        public static readonly List<DlcGuard> DefaultEts2DlcGuards = new List<DlcGuard>()
        {
            new DlcGuard("No Guard", 0),
            new DlcGuard("dlc_east", 1),
            new DlcGuard("dlc_north", 2),
            new DlcGuard("dlc_fr", 3),
            new DlcGuard("dlc_it", 4),
            new DlcGuard("dlc_fr_and_it", 5),
            new DlcGuard("dlc_balt", 6),
            new DlcGuard("dlc_balt_and_east", 7),
            new DlcGuard("dlc_balt_and_north", 8),
            new DlcGuard("dlc_blke", 9),
            new DlcGuard("dlc_blke_and_east", 10),
            new DlcGuard("dlc_iberia", 11),
            new DlcGuard("dlc_iberia_and_fr", 12),
            new DlcGuard("dlc_russia", 13, false),
            new DlcGuard("dlc_balt_and_russia", 14, false),
            new DlcGuard("dlc_krone", 15),
            new DlcGuard("dlc_blkw", 16),
            new DlcGuard("dlc_blkw_and_east", 17),
            new DlcGuard("dlc_blkw_and_blke", 18),
            new DlcGuard("dlc_feldbinder", 19),
        };

        public const float LaneWidth = 4.5f;

        /// <summary>
        /// Magic mark that should be at the start of the file, 'SCS#' as utf-8 bytes
        /// </summary>
        internal const uint ScsMagic = 592659283;

        internal const LogLevel MinimumLogLevel = LogLevel.Debug;
    }
}
