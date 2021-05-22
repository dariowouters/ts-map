using TsMap2.Scs;

namespace TsMap2.Model.TsMapItem {
    public class TsSector {
        public readonly TsItemType ItemType;
        private         bool       _empty;
        public          ulong      Uid;
        public          float      X;
        public          float      Z;

        public TsSector( TsItemType itemType, string filePath, int version, byte[] stream ) {
            this.ItemType = itemType;
            this.FilePath = filePath;
            this.Version  = version;
            this.Stream   = stream;
        }

        public string FilePath { get; }
        public int    Version  { get; }
        public byte[] Stream   { get; private set; }

        public void ClearFileData() {
            this.Stream = null;
        }
    }
}