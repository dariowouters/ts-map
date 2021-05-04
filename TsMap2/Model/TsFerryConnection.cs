using System.Collections.Generic;
using System.Drawing;
using TsMap2.Model;

namespace TsMap2.Model {
    public class TsFerryPoint {
        public double Rotation;
        public float  X;
        public float  Z;

        public TsFerryPoint( float x, float z ) {
            this.X = x;
            this.Z = z;
        }

        public void SetRotation( double rot ) {
            this.Rotation = rot;
        }
    }
}

public class TsFerryConnection {
    public List< TsFerryPoint > Connections = new List< TsFerryPoint >();
    public ulong                StartPortToken    { get; set; }
    public PointF               StartPortLocation { get; private set; }
    public ulong                EndPortToken      { get; set; }
    public PointF               EndPortLocation   { get; private set; }

    public void AddConnectionPosition( int index, float x, float z ) {
        if ( this.Connections.Count > index ) return;
        this.Connections.Add( new TsFerryPoint( x / 256, z / 256 ) );
    }

    public void AddRotation( int index, double rot ) {
        if ( this.Connections.Count <= index ) return;
        this.Connections[ index ].SetRotation( rot );
    }

    public void SetPortLocation( ulong ferryPortId, float x, float z ) {
        if ( ferryPortId == this.StartPortToken )
            this.StartPortLocation                                        = new PointF( x, z );
        else if ( ferryPortId == this.EndPortToken ) this.EndPortLocation = new PointF( x, z );
    }
}