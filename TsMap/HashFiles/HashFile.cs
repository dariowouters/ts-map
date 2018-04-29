using System.Collections.Generic;
using System.IO;
using System.Text;
using Ionic.Zlib;

namespace TsMap.HashFiles
{
    using static Helper;

    internal class ScsHeader
    {
        public uint Magic { get; set; }
        public ushort Version { get; set; }
        public ushort Salt { get; set; }
        public uint HashMethod { get; set; }
        public int EntryCount { get; set; }
        public int StartOffset { get; set; }

        public override string ToString()
        {
            return $"Magic {Magic}\n" +
                   $"Version {Version}\n" +
                   $"Salt {Salt}\n" +
                   $"HashMethod {HashMethod}\n" +
                   $"EntryCount {EntryCount}\n" +
                   $"StartOffset {StartOffset}";
        }
    }

    public class ScsHashEntry
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
        /// 5 = Directory, 7 = Compressed Directory
        /// </summary>
        public uint Flags { get; set; } // TODO: Check Flags
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

        public byte[] Read()
        {
            Hf.Fs.Seek(Offset, SeekOrigin.Begin);
            var buff = new byte[CompressedSize];
            Hf.Fs.Read(buff, 0, CompressedSize);
            return IsCompressed() ? Inflate(buff) : buff;
        }

        public byte[] Inflate(byte[] buff)
        {
            return ZlibStream.UncompressBuffer(buff);
        }

        public bool IsCompressed()
        {
            return Size != CompressedSize;
        }

        public override string ToString()
        {
            return $"Hash {Hash}\n" +
                   $"Offset {Offset}\n" +
                   $"Flags {Flags}\n" +
                   $"Crc {Crc}\n" +
                   $"Size {Size}\n" +
                   $"CompressedSize {CompressedSize}";
        }
    }

    /// <summary>
    /// Used to read CityHash .scs files
    /// </summary>
    public class HashFile
    {
        /// <summary>
        /// SCS#
        /// </summary>
        private const uint Magic = 592659283;
        /// <summary>
        /// CITY
        /// </summary>
        private const uint HashMethod = 1498696003;
        private const ushort SupportedHashVersion = 1;
        private const ushort HeaderBlockSize = 0x14;
        private const ushort EntryBlockSize = 0x20;
        /// <summary>
        /// CityHash64("")
        /// </summary>
        private const ulong RootDirHash = 11160318154034397263;

        private readonly string _path;

        public FileStream Fs { get; }

        private ScsHeader Header { get; }

        private RootFileSystem _rfs;

        public Dictionary<ulong, ScsHashEntry> Entries;

        public static void Uncompress(string path) // TODO: remove (temp function)
        {
            if (File.Exists(path))
            {
                var compressed = File.ReadAllBytes(path);
                File.WriteAllBytes(@"C:\scs_tmp\test2.txt", ZlibStream.UncompressBuffer(compressed));
            }
        }

        public HashFile(string filePath, RootFileSystem rfs)
        {
            _path = filePath;
            _rfs = rfs;

            if (!File.Exists(_path)) return;

            Fs = File.OpenRead(_path);
            Entries = new Dictionary<ulong, ScsHashEntry>();

            Header = new ScsHeader
            {
                Magic = ReadUint(Fs, 0x0),
                Version = ReadUshort(Fs, 0x04),
                Salt = ReadUshort(Fs, 0x06),
                HashMethod = ReadUint(Fs, 0x08),
                EntryCount = ReadInt(Fs, 0x0C),
                StartOffset = ReadInt(Fs, 0x10)
            };

            if (Header.Magic != Magic)
            {
                Log.Msg("Incorrect File Structure");
                return;
            }

            if (Header.HashMethod != HashMethod)
            {
                Log.Msg("Incorrect Hash Method");
                return;
            }

            if (Header.Version != SupportedHashVersion)
            {
                Log.Msg("Unsupported Hash Version");
                return;
            }

            for (var i = 0; i < Header.EntryCount; i++)
            {
                var offset = Header.StartOffset + i * EntryBlockSize;
                var entry = new ScsHashEntry
                {
                    Hash = ReadUlong(Fs, offset),
                    Offset = ReadLong(Fs, offset + 0x08),
                    Flags = ReadUint(Fs, offset + 0x10),
                    Crc = ReadUint(Fs, offset + 0x14),
                    Size = ReadInt(Fs, offset + 0x18),
                    CompressedSize = ReadInt(Fs, offset + 0x1C),
                    Hf = this
                };

                Entries.Add(entry.Hash, entry);
            }

            _rfs.AddDirEntry(GetEntry(RootDirHash));

        }

        public ScsHashEntry GetEntry(ulong hash)
        {
            return Entries.ContainsKey(hash) ? Entries[hash]: null;
        }

        public ScsHashEntry GetEntry(string name)
        {
            return GetEntry(CityHash.CityHash64(Encoding.UTF8.GetBytes(name), (ulong)name.Length));
        }
    }
}
