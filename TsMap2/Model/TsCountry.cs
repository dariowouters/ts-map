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

        public TsCountry( string countryCode, int countryId, string name, ulong token, float x, float y ) {
            this.CountryCode = countryCode;
            this.CountryId   = countryId;
            this.Name        = name;
            this.Token       = token;
            this.X           = x;
            this.Y           = y;
        }

        [ JsonIgnore ] public string                       LocalizationToken { get; }
        public                Dictionary< string, string > LocalizedNames    { get; } = new Dictionary< string, string >();


        public void AddLocalizedName( string locale, string name ) {
            if ( !this.LocalizedNames.ContainsKey( locale ) ) this.LocalizedNames.Add( locale, name );
        }

        public string GetLocalizedName( string locale ) =>
            this.LocalizedNames.ContainsKey( locale )
                ? this.LocalizedNames[ locale ]
                : this.Name;
    }
}