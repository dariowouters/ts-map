using System.Collections.Generic;
using System.Drawing;

namespace TsMap2.Model {
    public class TsFerryPoint {
        public double Rotation;
        public float  X;
        public float  Z;

        public TsFerryPoint( float x, float z ) {
            X = x;
            Z = z;
        }

        public void SetRotation( double rot ) {
            Rotation = rot;
        }
    }

    public class TsFerryConnection {
        public List< TsFerryPoint > Connections = new List< TsFerryPoint >();
        public ulong                StartPortToken    { get; set; }
        public PointF               StartPortLocation { get; private set; }
        public ulong                EndPortToken      { get; set; }
        public PointF               EndPortLocation   { get; private set; }

        public void AddConnectionPosition( int index, float x, float z ) {
            if ( Connections.Count > index ) return;
            Connections.Add( new TsFerryPoint( x / 256, z / 256 ) );
        }

        public void AddRotation( int index, double rot ) {
            if ( Connections.Count <= index ) return;
            Connections[ index ].SetRotation( rot );
        }

        public void SetPortLocation( ulong ferryPortId, float x, float z ) {
            if ( ferryPortId == StartPortToken )
                StartPortLocation                                   = new PointF( x, z );
            else if ( ferryPortId == EndPortToken ) EndPortLocation = new PointF( x, z );
        }
    }
}