using System;
using System.Collections.Generic;
using TsMap2.Helper;

namespace TsMap2.Model {
    public class TsRoadLook {
        public readonly List< string > LanesLeft;
        public readonly List< string > LanesRight;

        public float Offset;

        public TsRoadLook( ulong token ) {
            this.LanesLeft  = new List< string >();
            this.LanesRight = new List< string >();
            this.Token      = token;
        }

        public ulong Token { get; }

        public static double Hermite( float s, float x, float z, double tanX, double tanZ ) {
            double h1 = 2 * Math.Pow( s, 3 ) - 3 * Math.Pow( s, 2 )  + 1;
            double h2 = -2 * Math.Pow( s, 3 )                        + 3 * Math.Pow( s, 2 );
            double h3 = Math.Pow( s,      3 ) - 2 * Math.Pow( s, 2 ) + s;
            double h4 = Math.Pow( s, 3 )                             - Math.Pow( s, 2 );
            return h1 * x + h2 * z + h3 * tanX + h4 * tanZ;
        }

        public float GetWidth() => this.Offset + Common.LaneWidth * this.LanesLeft.Count + Common.LaneWidth * this.LanesRight.Count;
    }
}