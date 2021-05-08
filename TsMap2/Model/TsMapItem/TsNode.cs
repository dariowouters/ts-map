using System;
using TsMap2.Helper;

namespace TsMap2.Model.TsMapItem {
    public class TsNode {
        public TsNode( TsSector sector, int fileOffset ) {
            this.Uid = MemoryHelper.ReadUInt64( sector.Stream, fileOffset );
            this.X   = MemoryHelper.ReadInt32( sector.Stream, fileOffset += 0x08 ) / 256f;
            this.Z   = MemoryHelper.ReadInt32( sector.Stream, fileOffset += 0x08 ) / 256f;

            float rX = MemoryHelper.ReadSingle( sector.Stream, fileOffset += 0x04 );
            float rZ = MemoryHelper.ReadSingle( sector.Stream, fileOffset + 0x08 );

            double rot = Math.PI - Math.Atan2( rZ, rX );
            this.Rotation = (float) ( rot % Math.PI * 2 );
        }

        public ulong Uid { get; }

        public float X        { get; }
        public float Z        { get; }
        public float Rotation { get; }
    }
}