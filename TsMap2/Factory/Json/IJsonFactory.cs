using Newtonsoft.Json.Linq;

namespace TsMap2.Factory.Json {
    public interface IFactory {
        string  GetFileName();
        string  GetSavingPath();
        void    Save();
        JObject Load();

        JObject JsonData();
    }
}