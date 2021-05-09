using System.Collections.Generic;
using TsMap2.Helper;
using TsMap2.Scs;

namespace TsMap2.Model.TsMapItem {
    public class TsMapAreaItem : TsItem {
        public TsMapAreaItem( TsSector sector, int startOffset ) : base( sector, startOffset ) {
            this.Valid = true;
            this.TsMapAreaItem825( startOffset );
        }

        public List< ulong > NodeUids   { get; private set; }
        public uint          ColorIndex { get; private set; }
        public bool          DrawOver   { get; private set; }

        public void TsMapAreaItem825( int startOffset ) {
            int fileOffset = startOffset + 0x34; // Set position at start of flags

            this.DrawOver = MemoryHelper.ReadUint8( this.Sector.Stream, fileOffset ) != 0;
            int dlcGuardCount = Store().Game.IsEts2()
                                    ? ScsConst.Ets2DlcGuardCount
                                    : ScsConst.AtsDlcGuardCount;
            this.Hidden = MemoryHelper.ReadInt8( this.Sector.Stream, fileOffset + 0x01 ) > dlcGuardCount;

            this.NodeUids = new List< ulong >();

            int nodeCount = MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset += 0x05 ); // 0x05(flags)
            fileOffset += 0x04;                                                               // 0x04(nodeCount)
            for ( var i = 0; i < nodeCount; i++ ) {
                this.NodeUids.Add( MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset ) );
                fileOffset += 0x08;
            }

            this.ColorIndex =  MemoryHelper.ReadUInt32( this.Sector.Stream, fileOffset );
            fileOffset      += 0x04; // 0x04(colorIndex)
            this.BlockSize  =  fileOffset - startOffset;
        }
    }
}