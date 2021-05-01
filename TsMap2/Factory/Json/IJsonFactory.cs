using Newtonsoft.Json.Linq;

namespace TsMap2.Factory.Json {
    public interface IJsonFactory< out T > {
        string GetFileName();
        string GetSavingPath();
        string GetLoadingPath();
        void   Save();
        T      Load();

        JObject RawData();

        T Convert( JObject raw );
    }
}