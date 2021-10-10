using System.Collections.Generic;
using Newtonsoft.Json;

namespace TsMap2.Model.GeoJson {
    public class GeoJson {
        [ JsonProperty( "type" ) ]     public string          Type     { get; }      = "FeatureCollection";
        [ JsonProperty( "features" ) ] public List< Feature > Features { get; set; } = new();
    }
}