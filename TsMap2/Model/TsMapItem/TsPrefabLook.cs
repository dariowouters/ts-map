using System.Collections.Generic;
using System.Drawing;

namespace TsMap {
    public abstract class TsPrefabLook {
        protected readonly List< PointF > Points;

        protected TsPrefabLook( List< PointF > points ) => this.Points = points;

        protected TsPrefabLook() : this( new List< PointF >() ) { }
        public int   ZIndex { get; set; }
        public Brush Color  { get; set; }

        public void AddPoint( PointF p ) {
            this.Points.Add( p );
        }

        public void AddPoint( float x, float y ) {
            this.AddPoint( new PointF( x, y ) );
        }

        public abstract void Draw( Graphics g );
    }

    public class TsPrefabRoadLook : TsPrefabLook {
        public TsPrefabRoadLook() => this.ZIndex = 1;

        public float Width { private get; set; }

        public override void Draw( Graphics g ) {
            g.DrawLines( new Pen( this.Color, this.Width ), this.Points.ToArray() );
        }
    }

    public class TsPrefabPolyLook : TsPrefabLook {
        public TsPrefabPolyLook( List< PointF > points ) : base( points ) { }

        public override void Draw( Graphics g ) {
            g.FillPolygon( this.Color, this.Points.ToArray() );
        }
    }
}