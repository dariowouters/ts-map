using System;
using TsMap.FileSystem;

namespace TsMap
{
    public class TsMapSettings
    {
        public string FilePath { get; }
        public TsMapper Mapper { get; }

        public readonly int Version;
        public readonly bool HasUICorrections = false;
        public readonly float Scale;

        public TsMapSettings(TsMapper mapper, string filePath)
        {
            Mapper = mapper;
            FilePath = filePath;

            var file = UberFileSystem.Instance.GetFile(FilePath);
            var stream = file.Entry.Read();

            Version = BitConverter.ToInt32(stream, 0x0);
            Scale = BitConverter.ToSingle(stream, 0x3C);
            HasUICorrections = BitConverter.ToBoolean(stream, 0x44);
        }

    }
}
