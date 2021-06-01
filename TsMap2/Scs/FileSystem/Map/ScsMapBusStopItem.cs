using System.IO;
using Serilog;
using TsMap2.Model.TsMapItem;

namespace TsMap2.Scs.FileSystem.Map {
    public class ScsMapBusStopItem : TsMapItem {
        public ScsMapBusStopItem( ScsSector sector ) : base( sector ) {
            Valid = false;
            if ( Sector.Version < 836 || Sector.Version >= 847 )
                TsBusStopItem825();
            else if ( Sector.Version >= 836 || Sector.Version < 847 )
                TsBusStopItem836();
            else
                Log.Warning( $"Unknown base file version ({Sector.Version}) for item {Type} in file '{Path.GetFileName( Sector.FilePath )}' @ {Sector.LastOffset}." );
        }

        private void TsBusStopItem825() {
            int fileOffset = Sector.LastOffset + 0x34; // Set position at start of flags

            fileOffset += 0x05       + 0x18; // 0x05(flags) + 0x18(city_name & padding & prefab_uid & node_uid)
            BlockSize  =  fileOffset - Sector.LastOffset;
        }

        private void TsBusStopItem836() {
            int fileOffset = Sector.LastOffset + 0x34; // Set position at start of flags

            fileOffset += 0x05       + 0x1C; // 0x05(flags) + 0x18(city_name & padding & prefab_uid & node_uid & padding2)
            BlockSize  =  fileOffset - Sector.LastOffset;
        }
    }
}