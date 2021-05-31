using System.IO;
using Newtonsoft.Json.Linq;
using TsMap2.Helper;

namespace TsMap2.Factory.Json {
    public abstract class JsonFactory< T > : IJsonFactory< T > {
        protected       StoreHelper Store => StoreHelper.Instance;
        public abstract string      GetFileName();

        public abstract string GetSavingPath();

        public abstract string GetLoadingPath();

        public abstract T Convert( JObject raw );

        public abstract JContainer RawData();

        public void Save() {
            Save( GetSavingPath() );
        }

        public T Load() => Load( GetLoadingPath() );

        public void Save( string savingPath ) {
            JsonHelper.SaveFile( GetFileName(), savingPath, RawData() );
        }

        public T Load( string loadingPath ) {
            JObject raw = JsonHelper.LoadFile( GetFileName(), loadingPath );
            return Convert( raw );
        }

        public bool FileExist( string path ) => File.Exists( path );

        public bool FileExist() => FileExist( Path.Combine( GetLoadingPath(), GetFileName() ) );

        public bool DirExist( string path ) => Directory.Exists( path );

        public bool DirExist() => DirExist( GetLoadingPath() );
    }
}