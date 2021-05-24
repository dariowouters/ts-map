using System.IO;
using TsMap2.Helper;

namespace TsMap2.Factory {
    public class RawFactory {
        private readonly byte[] raw;
        public RawFactory( byte[] raw ) => this.raw = raw;

        public void Save( RawType type, string fileName ) {
            RawHelper.SaveFile( fileName, Path.Combine( AppPath.RawFolder, RawHelper.RawTypeToString( type ) ), this.raw );
        }

        public byte[] Load( string fileName ) => RawHelper.LoadFile( fileName, AppPath.RawFolder );
    }

    public enum RawType {
        COUNTRY,
        CITY,
        FERRY_CONNECTION,
        OVERLAY,
        PREFAB,
        ROAD_LOOK,
        MAP_LOCALIZATION,
        MAP_SECTORS
    }
}