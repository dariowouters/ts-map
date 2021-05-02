using Newtonsoft.Json;

namespace TsMap2.Model {
    public class TsCountry {
        public                string CountryCode;
        public                int    CountryId;
        public                string Name;
        [ JsonIgnore ] public ulong  Token;
        public                float  X;
        public                float  Y;

        public TsCountry( string countryCode, int countryId, string name, ulong token, float x, float y ) {
            this.CountryCode = countryCode;
            this.CountryId   = countryId;
            this.Name        = name;
            this.Token       = token;
            this.X           = x;
            this.Y           = y;
        }
    }
}