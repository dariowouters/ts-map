using TsMap2.Helper;
using TsMap2.Model.TsMapItem;

namespace TsMap2.Factory.TsItems {
    public abstract class TsMapItemFactory< T > {
        protected readonly TsSector Sector;
        protected readonly int      StartOffset;

        protected TsMapItemFactory( TsSector sector, int startOffset ) {
            this.Sector      = sector;
            this.StartOffset = startOffset;

            int fileOffset = startOffset;
            this.Sector.Uid = MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset += 0x04 );

            this.Sector.X = MemoryHelper.ReadSingle( this.Sector.Stream, fileOffset += 0x08 );
            this.Sector.Z = MemoryHelper.ReadSingle( this.Sector.Stream, fileOffset += 0x08 );
        }

        public abstract T Retrieve();

        protected static StoreHelper _store() => StoreHelper.Instance;
    }
}