using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TsMap.FileSystem;
using TsMap.Helpers.Logger;
using TsMap.Helpers;
using TsMap.TsItem;
using static System.Net.WebRequestMethods;
using static System.Collections.Specialized.BitVector32;

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
