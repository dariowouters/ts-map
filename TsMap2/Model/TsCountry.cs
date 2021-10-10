using System.Collections.Generic;
using Newtonsoft.Json;

namespace TsMap2.Model {
    public class TsCountry {
        public                string CountryCode;
        public                int    CountryId;
        public                string Name;
        [ JsonIgnore ] public ulong  Token;
        public                float  X;
        public                float  Y;

        public TsCountry( string countryCode, int countryId, string name, ulong token, float x, float y, string localizationToken ) {
            CountryCode       = countryCode;
            CountryId         = countryId;
            Name              = name;
            Token             = token;
            X                 = x;
            Y                 = y;
            LocalizationToken = localizationToken;
        }

        [ JsonIgnore ] public string                       LocalizationToken { get; }
        public                Dictionary< string, string > LocalizedNames    { get; } = new Dictionary< string, string >();

        public void AddLocalizedName( string locale, string name ) {
            if ( !LocalizedNames.ContainsKey( locale ) ) LocalizedNames.Add( locale, name );
        }

        public string GetLocalizedName( string locale ) =>
            LocalizedNames.ContainsKey( locale )
                ? LocalizedNames[ locale ]
                : Name;
    }
}