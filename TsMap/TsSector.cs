using System;
using System.IO;

namespace TsMap
{
    public class TsSector
    {
        public string FilePath { get; }
        public TsMapper Mapper { get; }

        private bool _empty;

        public byte[] Stream { get; private set; }

        public TsSector(TsMapper mapper, string filePath)
        {
            Mapper = mapper;
            FilePath = filePath;
            if (!File.Exists(FilePath))
            {
                _empty = true;
                return;
            }

            Stream = File.ReadAllBytes(FilePath);
        }

        public void Parse()
        {
            var itemCount = BitConverter.ToUInt32(Stream, 0x10);
            if (itemCount == 0) _empty = true;
            if (_empty) return;

            var lastOffset = 0x14;

            for (var i = 0; i < itemCount; i++)
            {
                TsItem item = new TsItem(this, lastOffset);
                if (item.Valid)
                {
                    Mapper.Items.Add(item.Uid, item);
                }
                lastOffset += item.BlockSize;
            }

            var nodeCount = BitConverter.ToInt32(Stream, lastOffset);
            for (var i = 0; i < nodeCount; i++)
            {
                TsNode node = new TsNode(this, lastOffset += 0x04);
                if (!Mapper.Nodes.ContainsKey(node.Uid))
                    Mapper.Nodes.Add(node.Uid, node);
                lastOffset += 0x34;
            }
        }

        public void ClearFileData()
        {
            Stream = null;
        }
    }
}
