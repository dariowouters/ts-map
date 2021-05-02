using Newtonsoft.Json;

namespace TsMap2.Model {
    public class TsCountry {
        public string CountryCode;

        public                int    CountryId;
        public                string Name;
        [ JsonIgnore ] public ulong  Token;
        public                float  X;
        public                float  Y;
    }
}