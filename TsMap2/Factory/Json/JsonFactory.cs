using Newtonsoft.Json.Linq;
using TsMap2.Helper;

namespace TsMap2.Factory.Json {
    public abstract class Factory : IFactory {
        public abstract string GetFileName();

        public abstract string GetSavingPath();

        public void Save() {
            JsonHelper.SaveFile( this.GetFileName(), this.GetSavingPath(), this.JsonData() );
        }

        public JObject Load() => JsonHelper.LoadFile( this.GetFileName(), this.GetSavingPath() );

        public abstract JObject JsonData();
    }
}