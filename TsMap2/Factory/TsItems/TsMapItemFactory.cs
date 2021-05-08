using TsMap2.Helper;
using TsMap2.Model.TsMapItem;

namespace TsMap2.Factory.TsItems {
    public abstract class TsMapItemFactory< T > {
        protected readonly TsSector Sector;

        protected TsMapItemFactory( TsSector sector ) => this.Sector = sector;

        public abstract T Retrieve( int startOffset );

        protected static StoreHelper _store() => StoreHelper.Instance;
    }
}