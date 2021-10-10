using System.Collections.Generic;
using Newtonsoft.Json;

namespace TsMap2.Model.GeoJson {
    public class Feature {
        [ JsonProperty( "geometry" ) ]   public Geometry                     Geometry   { get; set; }
        [ JsonProperty( "properties" ) ] public Dictionary< string, string > Properties { get; set; } = new();
        [ JsonProperty( "type" ) ]       public string                       Type       { get; }      = "Feature";

        public Feature() { }

        public Feature( TsCity city ) {
            var geometry = new Point();
            geometry.AddCoordinate( ( city.X, -city.Y ) );
            Geometry = geometry;

            Properties.Add( "name", city.Name );
        }
    }
}