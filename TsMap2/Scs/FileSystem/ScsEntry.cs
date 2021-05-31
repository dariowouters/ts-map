using System.IO;
using System.Text;
using Ionic.Zlib;

namespace TsMap2.Scs.FileSystem {
    public abstract class ScsEntry {
        public abstract ScsRootFile GetRootFile();

        public abstract byte[] Read();

        public abstract byte[] Inflate( byte[] buff );

        public abstract bool IsCompressed();

        public abstract ulong GetHash();

        public abstract bool IsDirectory();
    }

    public class ScsZipEntry : ScsEntry {
        private readonly ScsZipFile _szf;

        public ScsZipEntry( ScsZipFile szf ) => _szf = szf;

        /// <summary>
        ///     Offset to start of data
        /// </summary>
        public int Offset { get; set; }

        public ushort CompressionMethod { get; set; }
        public int    CompressedSize    { get; set; }
        public int    Size              { get; set; }
        public short  NameLength        { get; set; }
        public string Name              { get; set; }

        public override ScsRootFile GetRootFile() => _szf;

        public override byte[] Read() {
            BinaryReader br = _szf.Br;
            br.BaseStream.Seek( Offset, SeekOrigin.Begin );
            byte[] buff = br.ReadBytes( CompressedSize );

            return IsCompressed()
                       ? Inflate( buff )
                       : buff;
        }

        public override byte[] Inflate( byte[] buff ) => DeflateStream.UncompressBuffer( buff );

        public override bool IsCompressed() => CompressedSize != Size;

        public override ulong GetHash() => CityHash.CityHash64( Encoding.UTF8.GetBytes( Name ), (ulong) Name.Length );

        public override bool IsDirectory() => CompressedSize == 0;
    }

    public class ScsHashEntry : ScsEntry {
        public HashFile Hf;

        /// <summary>
        ///     File/dir name hashed by CityHash64
        /// </summary>
        public ulong Hash { get; set; }

        /// <summary>
        ///     Offset from start of .scs file
        /// </summary>
        public long Offset { get; set; }

        /// <summary>
        ///     Files:
        ///     0, 2, 4, 6
        ///     <para>Directories:</para>
        ///     <para>1, 3 => (some?) dirs in effect.scs</para>
        ///     <para>5 => Directory</para>
        ///     <para>7 => Compressed Directory</para>
        /// </summary>
        public uint Flags { get; set; }

        public uint Crc { get; set; }

        /// <summary>
        ///     Total size when inflated
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        ///     Size in scs file
        /// </summary>
        public int CompressedSize { get; set; }

        public override ScsRootFile GetRootFile() => Hf;

        public override byte[] Read() {
            BinaryReader br = Hf.Br;
            br.BaseStream.Seek( Offset, SeekOrigin.Begin );
            byte[] buff = br.ReadBytes( CompressedSize );
            br.Dispose();

            return IsCompressed()
                       ? Inflate( buff )
                       : buff;
        }

        public override byte[] Inflate( byte[] buff ) => ZlibStream.UncompressBuffer( buff );

        public override bool IsCompressed() => Size != CompressedSize;

        public override ulong GetHash() => Hash;

        public override bool IsDirectory() {
            switch ( Flags ) {
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