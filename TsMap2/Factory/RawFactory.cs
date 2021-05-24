using TsMap2.Helper;

namespace TsMap2.Factory {
    public class RawFactory {
        private readonly byte[] raw;
        public RawFactory( byte[] raw ) => this.raw = raw;

        public void Save( string fileName ) {
            RawHelper.SaveFile( fileName, AppPath.RawFolder, this.raw );
        }

        public byte[] Load( string fileName ) => RawHelper.LoadFile( fileName, AppPath.RawFolder );
    }

    public enum RawType {
        COUNTRY,
        CITY,
        FERRY_CONNECTION,
        OVERLAY,
        PREFAB,
        ROAD_LOOK
    }
}