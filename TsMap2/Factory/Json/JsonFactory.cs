using Newtonsoft.Json.Linq;
using TsMap2.Helper;

namespace TsMap2.Factory.Json {
    public abstract class JsonFactory< T > : IJsonFactory< T > {
        public abstract string GetFileName();

        public abstract string GetSavingPath();

        public abstract T Convert( JObject raw );

        public abstract JObject RawData();

        public void Save() {
            JsonHelper.SaveFile( this.GetFileName(), this.GetSavingPath(), this.RawData() );
        }

        public T Load() {
            JObject raw = JsonHelper.LoadFile( this.GetFileName(), this.GetSavingPath() );
            return this.Convert( raw );
        }
    }
}