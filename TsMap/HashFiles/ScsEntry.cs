using System.IO;
using System.Text;
using Ionic.Zlib;

namespace TsMap.HashFiles
{
    public abstract class ScsEntry
    {
        public abstract ScsRootFile GetRootFile();

        public abstract byte[] Read();

        public abstract byte[] Inflate(byte[] buff);

        public abstract bool IsCompressed();

        public abstract ulong GetHash();

        public abstract bool IsDirectory();

    }

    public class ScsZipEntry : ScsEntry
    {
        /// <summary>
        /// Offset to start of data
        /// </summary>
        public int Offset { get; set; }

        public ushort CompressionMethod { get; set; }
        public int CompressedSize { get; set; }
        public int Size { get; set; }
        public short NameLength { get; set; }
        public string Name { get; set; }

        private ScsZipFile _szf;

        public ScsZipEntry(ScsZipFile szf)
        {
            _szf = szf;
        }

        public override ScsRootFile GetRootFile()
        {
            return _szf;
        }

        public override byte[] Read()
        {
            _szf.Br.BaseStream.Seek(Offset, SeekOrigin.Begin);
            var buff = _szf.Br.ReadBytes(CompressedSize);
            return IsCompressed() ? Inflate(buff) : buff;
        }

        public override byte[] Inflate(byte[] buff)
        {
            return DeflateStream.UncompressBuffer(buff);
        }

        public override bool IsCompressed()
        {
            return CompressedSize != Size;
        }

        public override ulong GetHash()
        {
            return CityHash.CityHash64(Encoding.UTF8.GetBytes(Name), (ulong)Name.Length);
        }

        public override bool IsDirectory()
        {
            return CompressedSize == 0;
        }
    }

    public class ScsHashEntry : ScsEntry
    {
        /// <summary>
        /// File/dir name hashed by CityHash64
        /// </summary>
        public ulong Hash { get; set; }
        /// <summary>
        /// Offset from start of .scs file
        /// </summary>
        public long Offset { get; set; }
        /// <summary>
        /// Files:
        /// 0, 2, 4, 6
        /// <para>Directories:</para>
        /// <para>1, 3 => (some?) dirs in effect.scs</para>
        /// <para>5 => Directory</para>
        /// <para>7 => Compressed Directory</para>
        /// </summary>
        public uint Flags { get; set; }
        public uint Crc { get; set; }
        /// <summary>
        /// Total size when inflated
        /// </summary>
        public int Size { get; set; }
        /// <summary>
        /// Size in scs file
        /// </summary>
        public int CompressedSize { get; set; }

        public HashFile Hf;

        public override ScsRootFile GetRootFile()
        {
            return Hf;
        }

        public override byte[] Read()
        {
            Hf.Br.BaseStream.Seek(Offset, SeekOrigin.Begin);
            var buff = Hf.Br.ReadBytes(CompressedSize);
            return IsCompressed() ? Inflate(buff) : buff;
        }

        public override byte[] Inflate(byte[] buff)
        {
            return ZlibStream.UncompressBuffer(buff);
        }

        public override bool IsCompressed()
        {
            return Size != CompressedSize;
        }

        public override ulong GetHash()
        {
            return Hash;
        }

        public override bool IsDirectory()
        {
            switch (Flags)
            {
                case 1:
                case 3:
                case 5:
                case 7:
                    return true;
                default:
                    return false;
            }
        }
    }
}
