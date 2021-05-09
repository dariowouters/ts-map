using TsMap2.Helper;
using TsMap2.Scs;

namespace TsMap2.Model.TsMapItem {
    public class TsMapFerryItem : TsMapItem {
        public TsMapFerryItem( TsSector sector, int startOffset ) : base( sector, startOffset ) {
            this.Valid = true;
            this.TsFeryItem825( startOffset );
        }

        public ulong FerryPortId { get; private set; }
        public bool  Train       { get; private set; }

        public ulong        OverlayToken { get; private set; }
        public TsMapOverlay Overlay      { get; private set; }

        public void TsFeryItem825( int startOffset ) {
            int fileOffset = startOffset + 0x34; // Set position at start of flags
            this.Train = MemoryHelper.ReadUint8( this.Sector.Stream, fileOffset ) != 0;
            this.OverlayToken = this.Train
                                    ? ScsHash.StringToToken( "train_ico" )
                                    : ScsHash.StringToToken( "port_overlay" );
            this.Overlay = Store().Def.LookupOverlay( this.OverlayToken );

            this.FerryPortId = MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset += 0x05 );
            Store().Def.AddFerryPortLocation( this.FerryPortId, this.X, this.Z );
            fileOffset     += 0x08       + 0x1C; // 0x08(ferryPorId) + 0x1C(prefab_uid & node_uid & unloadoffset)
            this.BlockSize =  fileOffset - startOffset;
        }
    }
}