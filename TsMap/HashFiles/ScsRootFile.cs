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


    public abstract class ScsRootFile
    {
        protected readonly RootFileSystem _rfs;
        protected readonly string _path;

        protected ScsRootFile(string path, RootFileSystem rfs)
        {
            _path = path;
            _rfs = rfs;
        }

        public abstract ScsEntry GetEntry(string name);

        public abstract List<ScsEntry> GetEntriesValues();

    }

    public class ScsZipFile : ScsRootFile
    {
        public BinaryReader Br { get; }
        private Dictionary<string, ScsZipEntry> Entries;

        public ScsZipFile(string path, RootFileSystem rfs) : base(path, rfs)
        {
            if (!File.Exists(_path)) return;

            Br = new BinaryReader(File.OpenRead(_path));
            Entries = new Dictionary<string, ScsZipEntry>();

            var entryCount = (short) ReadUInt16(Br, -22 + 10, SeekOrigin.End);

            var fileOffset = 0;

            for (var i = 0; i < entryCount; i++)
            {
                var entry = new ScsZipEntry(this)
                {
                    CompressionMethod = ReadUInt16(Br, fileOffset += 8),
                    CompressedSize = ReadInt32(Br, fileOffset += 10),
                    Size = ReadInt32(Br, fileOffset += 4),
                    NameLength = (short) ReadUInt16(Br, fileOffset += 4),
                };

                var extraFieldLength = ReadUInt16(Br, fileOffset += 2);
                Br.BaseStream.Seek(fileOffset += 2, SeekOrigin.Begin);
                entry.Name = Encoding.UTF8.GetString(Br.ReadBytes(entry.NameLength));

                fileOffset += entry.NameLength + extraFieldLength;
                entry.Offset = fileOffset; // Offset to data

                fileOffset += entry.CompressedSize;

                if (entry.CompressedSize != 0) // only files
                {
                    var filePath = entry.Name.Replace('\\', '/');
                    _rfs.AddZipEntry(entry, filePath);
                }

                Entries.Add(entry.Name, entry);
            }

        }

        public override ScsEntry GetEntry(string name)
        {
            return Entries.ContainsKey(name) ? Entries[name] : null;
        }

        public override List<ScsEntry> GetEntriesValues()
        {
            List<ScsEntry> entries = new List<ScsEntry>();
            foreach (var scsZipEntry in Entries.Values)
            {
                entries.Add(scsZipEntry);
            }

            return entries;
        }
    }

    /// <summary>
    /// Used to read CityHash .scs files
    /// </summary>
    public class HashFile : ScsRootFile
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

        public BinaryReader Br { get; }

        private ScsHeader Header { get; }

        public Dictionary<ulong, ScsHashEntry> Entries;

        public HashFile(string filePath, RootFileSystem rfs) : base(filePath, rfs)
        {
            if (!File.Exists(_path)) return;

            Br = new BinaryReader(File.OpenRead(_path));
            Entries = new Dictionary<ulong, ScsHashEntry>();

            Header = new ScsHeader
            {
                Magic = ReadUInt32(Br, 0x0),
                Version = ReadUInt16(Br, 0x04),
                Salt = ReadUInt16(Br, 0x06),
                HashMethod = ReadUInt32(Br, 0x08),
                EntryCount = ReadInt32(Br, 0x0C),
                StartOffset = ReadInt32(Br, 0x10)
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

            Br.BaseStream.Seek(Header.StartOffset, SeekOrigin.Begin);
            var entriesRaw = Br.ReadBytes(Header.EntryCount * EntryBlockSize);

            for (var i = 0; i < Header.EntryCount; i++)
            {
                var offset = i * EntryBlockSize;
                var entry = new ScsHashEntry
                {
                    Hash = MemoryHelper.ReadUInt64(entriesRaw, offset),
                    Offset = MemoryHelper.ReadInt64(entriesRaw, offset + 0x08),
                    Flags = MemoryHelper.ReadUInt32(entriesRaw, offset + 0x10),
                    Crc = MemoryHelper.ReadUInt32(entriesRaw, offset + 0x14),
                    Size = MemoryHelper.ReadInt32(entriesRaw, offset + 0x18),
                    CompressedSize = MemoryHelper.ReadInt32(entriesRaw, offset + 0x1C),
                    Hf = this
                };

                Entries.Add(entry.Hash, entry);
            }

            var rootDir = GetEntry(RootDirHash);

            if (rootDir == null) // Try to add important sub directories directly
            {
                var defEntry = (ScsHashEntry) GetEntry("def");
                if (defEntry != null)
                {
                    var dir = _rfs.GetDirectory("def");

                    dir?.AddHashEntry(defEntry);
                }
                var mapEntry = (ScsHashEntry) GetEntry("map");
                if (mapEntry != null)
                {
                    var dir = _rfs.GetDirectory("map");

                    dir?.AddHashEntry(mapEntry);
                }
                var materialEntry = (ScsHashEntry) GetEntry("material");
                if (materialEntry != null)
                {
                    var dir = _rfs.GetDirectory("material");

                    dir?.AddHashEntry(materialEntry);
                }
                var prefabEntry = (ScsHashEntry) GetEntry("prefab");
                if (prefabEntry != null)
                {
                    var dir = _rfs.GetDirectory("prefab");

                    dir?.AddHashEntry(prefabEntry);
                }
                var prefab2Entry = (ScsHashEntry) GetEntry("prefab2");
                if (prefab2Entry != null)
                {
                    var dir = _rfs.GetDirectory("prefab2");

                    dir?.AddHashEntry(prefab2Entry);
                }
                var modelEntry = (ScsHashEntry) GetEntry("model");
                if (modelEntry != null)
                {
                    var dir = _rfs.GetDirectory("model");

                    dir?.AddHashEntry(modelEntry);
                }
                var model2Entry = (ScsHashEntry) GetEntry("model2");
                if (model2Entry != null)
                {
                    var dir = _rfs.GetDirectory("model2");

                    dir?.AddHashEntry(model2Entry);
                }
                var localeEntry = (ScsHashEntry) GetEntry("locale");
                if (localeEntry != null)
                {
                    var dir = _rfs.GetDirectory("locale");
                    if (dir == null)
                    {
                        _rfs.GetRootDirectory()?.AddDirectoryManually("locale", localeEntry);
                        dir = _rfs.GetDirectory("locale");
                        if (dir == null) Log.Msg("Fuck");
                    }
                    dir?.AddHashEntry(localeEntry);
                }
            }
            else
            {
                _rfs.AddHashEntry(rootDir);
            }
        }

        public override string ToString()
        {
            return _path.Substring(_path.LastIndexOf('\\') + 1);
        }

        private ScsHashEntry GetEntry(ulong hash)
        {
            return Entries.ContainsKey(hash) ? Entries[hash]: null;
        }

        public sealed override ScsEntry GetEntry(string name)
        {
            return GetEntry(CityHash.CityHash64(Encoding.UTF8.GetBytes(name), (ulong)name.Length));
        }

        public override List<ScsEntry> GetEntriesValues()
        {
            List<ScsEntry> entries = new List<ScsEntry>();
            foreach (var scsHashEntry in Entries.Values)
            {
                entries.Add(scsHashEntry);
            }

            return entries;
        }
    }
}
