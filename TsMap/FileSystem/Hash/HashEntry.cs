using System;
using System.IO;
using System.IO.Compression;
using TsMap.Helpers;
using TsMap.Helpers.Logger;

namespace TsMap.FileSystem.Hash
{
    public class HashEntry : Entry
    {
        /// <summary>
        /// <para>0001 => Directory</para>
        /// <para>0010 => Compressed</para>
        /// </summary>
        public uint Flags { get; set; }
        public uint Crc { get; set; }

        public HashEntry(HashArchiveFile fsFile) : base(fsFile)
        {
        }

        public override byte[] Read()
        {
            var buff = MemoryHelper.ReadBytes(GetArchiveFile().Br, (long)Offset, (int)CompressedSize);
            return IsCompressed() ? Inflate(buff) : buff;
        }

        protected override byte[] Inflate(byte[] buff)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream(buff))
                {
                    var inflatedBytes = new byte[Size];

                    // Read past the two bytes of the zlib header (CMF, FLG)
                    ms.Seek(2, SeekOrigin.Begin);


                    using (var ds = new DeflateStream(ms, CompressionMode.Decompress))
                    {
                        ds.Read(inflatedBytes, 0, (int)Size);

                        return inflatedBytes;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Instance.Error($"Could not inflate hash entry: 0x{Hash:X}, of '{GetArchiveFile().GetPath()}', reason: {e.Message}");
                return new byte[0];
            }
        }

        public override bool IsDirectory()
        {
            return MemoryHelper.IsBitSet(Flags, 0);
        }
        public override bool IsCompressed()
        {
            return MemoryHelper.IsBitSet(Flags, 1);
        }
    }
}
