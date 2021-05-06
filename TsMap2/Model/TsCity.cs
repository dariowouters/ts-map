using System.Collections.Generic;
using Newtonsoft.Json;

namespace TsMap2.Model {
    public class TsCity {
        public TsCity( string name, string country, ulong token, List< int > xOffsets, List< int > yOffsets ) {
            this.Name     = name;
            this.Country  = country;
            this.Token    = token;
            this.XOffsets = xOffsets;
            this.YOffsets = yOffsets;
        }

        public                string                       Name              { get; set; }
        [ JsonIgnore ] public string                       LocalizationToken { get; set; }
        public                string                       Country           { get; set; }
        [ JsonIgnore ] public ulong                        Token             { get; set; }
        [ JsonIgnore ] public List< int >                  XOffsets          { get; }
        [ JsonIgnore ] public List< int >                  YOffsets          { get; }
        [ JsonIgnore ] public Dictionary< string, string > LocalizedNames    { get; }
    }
}