using System;
using TsMap2.Helper;

namespace TsMap2.Scs.FileSystem.Map {
    public class ScsSector {
        // public readonly ulong Uid;
        // public          float X;
        // public          float Z;
        // private         bool  _empty;
        public int LastOffset = 0x14;

        public ScsSector( string filePath, byte[] stream ) {
            FilePath = filePath;
            Stream   = stream;

            // int fileOffset = LastOffset;

            // Uid = MemoryHelper.ReadUInt64( Stream, fileOffset += 0x04 );
            //
            // X = MemoryHelper.ReadSingle( Stream, fileOffset += 0x08 );
            // Z = MemoryHelper.ReadSingle( Stream, fileOffset += 0x08 );
        }

        public string      FilePath  { get; }
        public ScsItemType ItemType  => (ScsItemType)MemoryHelper.ReadUInt32( Stream, LastOffset );
        public uint        ItemCount => BitConverter.ToUInt32( Stream, 0x10 );
        public int         Version   => BitConverter.ToInt32( Stream, 0x0 );
        public byte[]      Stream    { get; private set; }

        public void ClearFileData() {
            Stream = null;
        }
    }
}