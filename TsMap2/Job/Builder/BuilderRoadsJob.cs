using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Serilog;
using TsMap2.Model;
using TsMap2.Model.TsMapItem;

namespace TsMap2.Job.Builder {
    public class BuilderRoadsJob : ThreadJob {
        protected override void Do() {
            Log.Debug( "[Job][Builder][Road] Building" );

            foreach ( TsMapRoadItem road in Store().Map.Roads ) {
                TsNode startNode = road.GetStartNode();
                TsNode endNode   = road.GetEndNode();

                var newPoints = new List< PointF >();

                float sx = startNode.X;
                float sz = startNode.Z;
                float ex = endNode.X;
                float ez = endNode.Z;

                double radius = Math.Sqrt( Math.Pow( sx - ex, 2 ) + Math.Pow( sz - ez, 2 ) );

                double tanSx = Math.Cos( -( Math.PI * 0.5f - startNode.Rotation ) ) * radius;
                double tanEx = Math.Cos( -( Math.PI * 0.5f - endNode.Rotation ) )   * radius;
                double tanSz = Math.Sin( -( Math.PI * 0.5f - startNode.Rotation ) ) * radius;
                double tanEz = Math.Sin( -( Math.PI * 0.5f - endNode.Rotation ) )   * radius;

                for ( var i = 0; i < 8; i++ ) {
                    float s = i / (float)( 8 - 1 );
                    var   x = (float)TsRoadLook.Hermite( s, sx, ex, tanSx, tanEx );
                    var   z = (float)TsRoadLook.Hermite( s, sz, ez, tanSz, tanEz );
                    newPoints.Add( new PointF( x, z ) );
                }

                road.AddPoints( newPoints );
            }

            Store().Map.Roads = Store()
                                .Map.Roads
                                .Where( item => item.HasPoints() )
                                .ToList();

            Log.Information( "[Job][Builder][Road] Done" );
        }
    }
}