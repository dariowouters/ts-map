using System.IO;
using Serilog;
using TsMap2.Helper;
using TsMap2.Scs;

namespace TsMap2.Model.TsMapItem {
    public class TsMapMapOverlayItem : TsMapItem {
        public TsMapMapOverlayItem( TsSector sector, int startOffset ) : base( sector, startOffset ) {
            this.Valid = true;
            this.TsMapOverlayItem825( startOffset );
        }

        public string       OverlayName         { get; private set; }
        public TsMapOverlay Overlay             { get; private set; }
        public byte         ZoomLevelVisibility { get; private set; }

        public void TsMapOverlayItem825( int startOffset ) {
            int fileOffset = startOffset + 0x34; // Set position at start of flags
            this.ZoomLevelVisibility = MemoryHelper.ReadUint8( this.Sector.Stream, fileOffset );
            int dlcGuardCount = Store().Game.IsEts2()
                                    ? ScsConst.Ets2DlcGuardCount
                                    : ScsConst.AtsDlcGuardCount;
            this.Hidden = MemoryHelper.ReadInt8( this.Sector.Stream, fileOffset + 0x01 ) > dlcGuardCount || this.ZoomLevelVisibility == 255;

            byte  type                                         = MemoryHelper.ReadUint8( this.Sector.Stream, fileOffset + 0x02 );
            ulong overlayToken                                 = MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset += 0x05 );
            if ( type == 1 && overlayToken == 0 ) overlayToken = ScsHash.StringToToken( "parking_ico" ); // parking
            this.Overlay     = Store().Def.LookupOverlay( overlayToken );
            this.OverlayName = ScsHash.TokenToString( overlayToken );
            if ( this.Overlay == null ) {
                this.Valid = false;
                if ( overlayToken != 0 )
                    Log.Warning( $"Could not find Overlay: '{this.OverlayName}'({ScsHash.StringToToken( this.OverlayName ):X}), in {Path.GetFileName( this.Sector.FilePath )} @ {fileOffset}" );
            }

            fileOffset     += 0x08       + 0x08; // 0x08(overlayId) + 0x08(nodeUid)
            this.BlockSize =  fileOffset - startOffset;
        }
    }
}