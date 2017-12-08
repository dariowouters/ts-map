using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace TsMap
{
    public class TsMapRenderer
    {
        private readonly TsMapper _mapper;
        private readonly MapPalette _palette;

        public TsMapRenderer(TsMapper mapper, MapPalette palette)
        {
            _mapper = mapper;
            _palette = palette;
        }

        private static PointF RotatePoint(float x, float z, float angle, float rotX, float rotZ)
        {
            var s = Math.Sin(angle);
            var c = Math.Cos(angle);
            double newX = x - rotX;
            double newZ = z - rotZ;
            return new PointF((float) ((newX * c) - (newZ * s) + rotX), (float) ((newX * s) + (newZ * c) + rotZ));
        }

        public void Render(Graphics g, Rectangle clip, float baseScale, PointF pos)
        {
            g.FillRectangle(_palette.Background, new Rectangle(0, 0, clip.X + clip.Width, clip.Y + clip.Height));
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.None;

            var defaultFont = new Font("Arial", 10.0f, FontStyle.Bold);

            if (_mapper == null)
            {
                g.DrawString("Map object not initialized", defaultFont, _palette.Error, 5, 5);
                return;
            }

            var centerX = pos.X;
            var centerY = pos.Y;

            float totalX, totalY;

            if (clip.Width > clip.Height)
            {
                totalX = baseScale;
                totalY = baseScale * clip.Height / clip.Width;
            }
            else
            {
                totalY = baseScale;
                totalX = baseScale * clip.Width / clip.Height;
            }

            var startX = clip.X + centerX - totalX;
            var endX = clip.X + centerX + totalX;
            var startY = clip.Y + centerY - totalY;
            var endY = clip.Y + centerY + totalY;

            var scaleX = clip.Width / (endX - startX);
            var scaleY = clip.Height / (endY - startY);

            if (float.IsInfinity(scaleX) || float.IsNaN(scaleX)) scaleX = clip.Width;
            if (float.IsInfinity(scaleY) || float.IsNaN(scaleY)) scaleY = clip.Height;

            var itemsNearby = _mapper.Items.Values.Where(item => item.X >= startX - 1500 && item.X <= endX + 1500 && item.Z >= startY - 1500 && item.Z <= endY + 1500).ToList();

            var roads = itemsNearby.Where(item => item.Type == TsItemType.Road && !item.Hidden);
            
            foreach (var road in roads) // TODO: Smooth out roads, fix connection between road segments
            {
                var startNode = road.GetStartNode();
                var endNode = road.GetEndNode();

                var startPoint = new PointF((startNode.X - startX) * scaleX, (startNode.Z - startY) * scaleY);
                var endPoint = new PointF((endNode.X - startX) * scaleX, (endNode.Z - startY) * scaleY);

                var roadWidth = road.RoadLook.GetWidth() * scaleX;

                g.DrawLine(new Pen(_palette.Road, roadWidth), startPoint, endPoint);
            }

            // g.DrawString($"x: {centerX}, y: {centerY}, scale: {baseScale}", defaultFont, Brushes.WhiteSmoke, 5, 5);

            var prefabs = itemsNearby.Where(item => item.Type == TsItemType.Prefab && !item.Hidden);

            foreach (var prefabItem in prefabs) // TODO: Road Width
            {
                var originNode = _mapper.GetNodeByUid(prefabItem.Nodes[0]);
                var mapPointOrigin = prefabItem.Prefab.PrefabNodes[prefabItem.Origin];

                var rot = (float)(originNode.Rotation - Math.PI - Math.Atan2(mapPointOrigin.RotZ, mapPointOrigin.RotX) + Math.PI / 2);

                var prefabStartX = originNode.X - mapPointOrigin.X;
                var prefabStartZ = originNode.Z - mapPointOrigin.Z;

                List<int> pointsDrawn = new List<int>();

                for (var i = 0; i < prefabItem.Prefab.MapPoints.Count; i++)
                {
                    var mapPoint = prefabItem.Prefab.MapPoints[i];
                    pointsDrawn.Add(i);
                    // TODO: Add Drawing Queue To Correctly Render Items On Top of Each other
                    if (mapPoint.LaneCount == -1) // non-road Prefab
                    {
                        Dictionary<int, PointF> polyPoints = new Dictionary<int, PointF>();
                        var nextPoint = i;
                        do
                        {
                            foreach (var neighbour in prefabItem.Prefab.MapPoints[nextPoint].Neighbours)
                            {
                                if (!polyPoints.ContainsKey(neighbour)) // New Polygon Neighbour
                                {
                                    nextPoint = neighbour;
                                    var newPoint = RotatePoint(prefabStartX + prefabItem.Prefab.MapPoints[nextPoint].X,
                                        prefabStartZ + prefabItem.Prefab.MapPoints[nextPoint].Z, rot, originNode.X, originNode.Z);

                                    polyPoints.Add(nextPoint,
                                        new PointF((newPoint.X - startX) * scaleX,
                                        (newPoint.Y - startY) * scaleY));
                                    break;
                                }
                                nextPoint = -1;
                            }
                        } while (nextPoint != -1);

                        Brush fillColor;
                        var colorFlag = prefabItem.Prefab.MapPoints[polyPoints.First().Key].PrefabColorFlags;

                        if (colorFlag == 0) fillColor = _palette.PrefabLight;
                        else if ((colorFlag & 0x02) != 0) fillColor = _palette.PrefabLight;
                        else if ((colorFlag & 0x04) != 0) fillColor = _palette.PrefabDark;
                        else if ((colorFlag & 0x08) != 0) fillColor = _palette.PrefabGreen;
                        else fillColor = _palette.Error; // Unknown

                        g.FillPolygon(fillColor, polyPoints.Values.ToArray());
                        continue;
                    }

                    foreach (var neighbourPointIndex in mapPoint.Neighbours) // TODO: Fix connection between road segments
                    {
                        if (pointsDrawn.Contains(neighbourPointIndex)) continue;
                        var neighbourPoint = prefabItem.Prefab.MapPoints[neighbourPointIndex];

                        if ((mapPoint.Hidden || neighbourPoint.Hidden) && prefabItem.Prefab.PrefabNodes.Count + 1 <
                            prefabItem.Prefab.MapPoints.Count) continue;

                        var newPointStart = RotatePoint(prefabStartX + mapPoint.X,
                            prefabStartZ + mapPoint.Z, rot, originNode.X, originNode.Z);

                        var newPointEnd = RotatePoint(prefabStartX + neighbourPoint.X,
                            prefabStartZ + neighbourPoint.Z, rot, originNode.X, originNode.Z);
                        
                        g.DrawLine(new Pen(_palette.PrefabRoad, 10f * scaleX),
                            (newPointStart.X - startX) * scaleX,
                            (newPointStart.Y - startY) * scaleY,
                            (newPointEnd.X - startX) * scaleX,
                            (newPointEnd.Y - startY) * scaleY);
                    }
                }
            }

            var cities = itemsNearby.Where(item => item.Type == TsItemType.City && !item.Hidden);

            foreach (var city in cities)
            {
                var cityFont = new Font("Arial", 80 * scaleX, FontStyle.Bold);
                g.DrawString(city.CityName, cityFont, _palette.CityName, (city.X - startX) * scaleX, (city.Z - startY) * scaleY);
            }
        }
    }
}
