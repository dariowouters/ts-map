using TsMap2.Scs;

namespace TsMap2.Model.TsMapItem {
    public class TsSector {
        public readonly TsItemType ItemType;
        private         bool       _empty;
        public          ulong      Uid;
        public          float      X;
        public          float      Z;

        public TsSector( TsItemType itemType, string filePath, int version, byte[] stream ) {
            ItemType = itemType;
            FilePath = filePath;
            Version  = version;
            Stream   = stream;
        }

        public string FilePath { get; }
        public int    Version  { get; }
        public byte[] Stream   { get; private set; }

        public void ClearFileData() {
            Stream = null;
        }
    }
}