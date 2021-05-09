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
            this.Save( this.GetSavingPath() );
        }

        public T Load() => this.Load( this.GetLoadingPath() );

        public void Save( string savingPath ) {
            JsonHelper.SaveFile( this.GetFileName(), savingPath, this.RawData() );
        }

        public T Load( string loadingPath ) {
            JObject raw = JsonHelper.LoadFile( this.GetFileName(), loadingPath );
            return this.Convert( raw );
        }

        public bool FileExist( string path ) => File.Exists( path );

        public bool FileExist() => this.FileExist( Path.Combine( this.GetLoadingPath(), this.GetFileName() ) );

        public bool DirExist( string path ) => Directory.Exists( path );

        public bool DirExist() => this.DirExist( this.GetLoadingPath() );
    }
}