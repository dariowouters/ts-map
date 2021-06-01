using TsMap2.Helper;
using TsMap2.Scs.FileSystem.Map;

namespace TsMap2.Model.TsMapItem {
    public class TsMapFerryItem : TsMapItem {
        public TsMapFerryItem( ScsSector sector ) : base( sector ) {
            Valid = true;
            TsFerryItem825();
        }

        public ulong FerryPortId { get; private set; }
        public bool  Train       { get; private set; }

        public ulong        OverlayToken { get; private set; }
        public TsMapOverlay Overlay      { get; private set; }

        private void TsFerryItem825() {
            int fileOffset = Sector.LastOffset + 0x34; // Set position at start of flags
            Train = MemoryHelper.ReadUint8( Sector.Stream, fileOffset ) != 0;
            OverlayToken = Train
                               ? ScsHashHelper.StringToToken( "train_ico" )
                               : ScsHashHelper.StringToToken( "port_overlay" );
            Overlay = Store().Def.LookupOverlay( OverlayToken );

            FerryPortId = MemoryHelper.ReadUInt64( Sector.Stream, fileOffset += 0x05 );
            Store().Def.AddFerryPortLocation( FerryPortId, X, Z );
            fileOffset += 0x08       + 0x1C; // 0x08(ferryPorId) + 0x1C(prefab_uid & node_uid & unloadoffset)
            BlockSize  =  fileOffset - Sector.LastOffset;
        }
    }
}