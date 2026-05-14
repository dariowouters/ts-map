using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TsMap.TsItem;
using System.Dynamic;

namespace TsMap
{
    public class RoadManager
    {
        public static void CollectPoints(List<TsRoadItem> roads)
        {
            List<dynamic> points = new List<dynamic>();

            foreach (var road in roads)
            {
                AddPoints(road);

                if (road.HasPoints())
                {
                    foreach (var point in road.GetPoints())
                    {
                        var j = new
                        {
                            x = point.X,
                            y = point.Y
                        };

                        points.Add(j);
                    }
                }
            }

            JsonHelper.SaveRoadPoints(points);
        }

        private static void PersistsRoadPoints()
        {
            
        }

        private static void AddPoints(TsRoadItem road)
        { 
            if (!road.HasPoints())
            {
                var startNode = road.GetStartNode();
                var endNode = road.GetEndNode();

                var newPoints = new List<PointF>();

                var sx = startNode.X;
                var sz = startNode.Z;
                var ex = endNode.X;
                var ez = endNode.Z;

                var radius = Math.Sqrt(Math.Pow(sx - ex, 2) + Math.Pow(sz - ez, 2));

                var tanSx = Math.Cos(-(Math.PI * 0.5f - startNode.Rotation)) * radius;
                var tanEx = Math.Cos(-(Math.PI * 0.5f - endNode.Rotation)) * radius;
                var tanSz = Math.Sin(-(Math.PI * 0.5f - startNode.Rotation)) * radius;
                var tanEz = Math.Sin(-(Math.PI * 0.5f - endNode.Rotation)) * radius;

                for (var i = 0; i < 8; i++)
                {
                    var s = i / (float)(8 - 1);
                    var x = (float)TsRoadLook.Hermite(s, sx, ex, tanSx, tanEx);
                    var z = (float)TsRoadLook.Hermite(s, sz, ez, tanSz, tanEz);
                    newPoints.Add(new PointF(x, z));
                }

                road.AddPoints(newPoints);
            }
        }
    }
}
