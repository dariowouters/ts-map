using System.Collections.Generic;
using Newtonsoft.Json;

namespace TsMap2.Model.GeoJson {
    public abstract class Geometry {
        [ JsonProperty( "coordinates" ) ] public List< List< float > > Coordinates { get; set; } = new();

        [ JsonProperty( "type" ) ] public abstract string Type { get; }

        public void AddCoordinate( (float, float) coords ) {
            ( float x, float y ) = coords;
            Coordinates.Add( new List< float > { x, y } );
        }
    }

    public class Point : Geometry {
        public override string Type { get; } = "Point";
    }
}