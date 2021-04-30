using Newtonsoft.Json.Linq;

namespace TsMap2.Factory {
    public interface IFactory {
        string GetFileName();
        string GetSavingPath();

        JObject JsonData();
    }
}