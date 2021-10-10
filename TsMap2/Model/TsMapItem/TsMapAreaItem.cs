using System.Collections.Generic;
using TsMap2.Helper;
using TsMap2.Scs;
using TsMap2.Scs.FileSystem.Map;

namespace TsMap2.Model.TsMapItem {
    public class TsMapAreaItem : TsMapItem {
        public TsMapAreaItem( ScsSector sector ) : base( sector, sector.LastOffset ) {
            Valid = true;
            TsMapAreaItem825();
        }

        public List< ulong > NodeUids   { get; private set; }
        public uint          ColorIndex { get; private set; }
        public bool          DrawOver   { get; private set; }

        private void TsMapAreaItem825() {
            int fileOffset = Sector.LastOffset + 0x34; // Set position at start of flags

            DrawOver = MemoryHelper.ReadUint8( Sector.Stream, fileOffset ) != 0;
            int dlcGuardCount = Store().Game.IsEts2()
                                    ? ScsConst.Ets2DlcGuardCount
                                    : ScsConst.AtsDlcGuardCount;
            Hidden = MemoryHelper.ReadInt8( Sector.Stream, fileOffset + 0x01 ) > dlcGuardCount;

            NodeUids = new List< ulong >();

            int nodeCount = MemoryHelper.ReadInt32( Sector.Stream, fileOffset += 0x05 ); // 0x05(flags)
            fileOffset += 0x04;                                                          // 0x04(nodeCount)
            for ( var i = 0; i < nodeCount; i++ ) {
                NodeUids.Add( MemoryHelper.ReadUInt64( Sector.Stream, fileOffset ) );
                fileOffset += 0x08;
            }

            ColorIndex =  MemoryHelper.ReadUInt32( Sector.Stream, fileOffset );
            fileOffset += 0x04; // 0x04(colorIndex)
            BlockSize  =  fileOffset - Sector.LastOffset;
        }
    }
}