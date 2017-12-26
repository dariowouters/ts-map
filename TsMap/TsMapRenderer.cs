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
            var startTime = DateTime.Now.Ticks;
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
            
            var roads = _mapper.Roads.Where(item =>
                    item.X >= startX - 1500 && item.X <= endX + 1500 && item.Z >= startY - 1500 &&
                    item.Z <= endY + 1500 && !item.Hidden)
                .ToList();


            foreach (var road in roads)
            {
                var startNode = road.GetStartNode();
                var endNode = road.GetEndNode();

                var sx = startNode.X;
                var sz = startNode.Z;
                var ex = endNode.X;
                var ez = endNode.Z;

                var radius = Math.Sqrt(Math.Pow(sx - ex, 2) + Math.Pow(sz - ez, 2));
                 
                var tanSx = Math.Cos(-(Math.PI * 0.5f - startNode.Rotation)) * radius;
                var tanEx = Math.Cos(-(Math.PI * 0.5f - endNode.Rotation)) * radius;
                var tanSz = Math.Sin(-(Math.PI * 0.5f - startNode.Rotation)) * radius;
                var tanEz = Math.Sin(-(Math.PI * 0.5f - endNode.Rotation)) * radius;

                List<PointF> points = new List<PointF>();

                for (var i = 0; i < 32; i++)
                {
                    var s = i / (float) (32 - 1);
                    var x = (float) TsRoadLook.Hermite(s, sx, ex, tanSx, tanEx);
                    var z = (float) TsRoadLook.Hermite(s, sz, ez, tanSz, tanEz);
                    points.Add(new PointF((x - startX) * scaleX, (z - startY) * scaleY));
                }
                
                var roadWidth = road.RoadLook.GetWidth() * scaleX;

                g.DrawCurve(new Pen(_palette.Road, roadWidth), points.ToArray());
            }

            var prefabs = _mapper.Prefabs.Where(item =>
                    item.X >= startX - 1500 && item.X <= endX + 1500 && item.Z >= startY - 1500 &&
                    item.Z <= endY + 1500 && !item.Hidden)
                .ToList();

            List<TsPrefabLook> drawingQueue = new List<TsPrefabLook>();

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
                        
                        var colorFlag = prefabItem.Prefab.MapPoints[polyPoints.First().Key].PrefabColorFlags;

                        Brush fillColor = _palette.PrefabLight;
                        if ((colorFlag & 0x02) != 0) fillColor = _palette.PrefabLight;
                        else if ((colorFlag & 0x04) != 0) fillColor = _palette.PrefabDark;
                        else if ((colorFlag & 0x08) != 0) fillColor = _palette.PrefabGreen;
                        // else fillColor = _palette.Error; // Unknown

                        var prefabLook = new TsPrefabPolyLook(polyPoints.Values.ToList())
                        {
                            ZIndex = ((colorFlag & 0x01) != 0) ? 3 : 2,
                            Color = fillColor
                        };
                        
                        drawingQueue.Add(prefabLook);
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
                        
                        TsPrefabLook prefabLook = new TsPrefabRoadLook()
                        {
                            Color = _palette.PrefabRoad,
                            Width = 10f * scaleX,
                        };

                        prefabLook.AddPoint((newPointStart.X - startX) * scaleX, (newPointStart.Y - startY) * scaleY);
                        prefabLook.AddPoint((newPointEnd.X - startX) * scaleX, (newPointEnd.Y - startY) * scaleY);

                        drawingQueue.Add(prefabLook);
                    }
                }
            }

            foreach (var prefabLook in drawingQueue.OrderBy(p => p.ZIndex))
            {
                prefabLook.Draw(g);
            }


            var cities = _mapper.Cities.Where(item =>
                    item.X >= startX - 1500 && item.X <= endX + 1500 && item.Z >= startY - 1500 &&
                    item.Z <= endY + 1500 && !item.Hidden)
                .ToList();

            foreach (var city in cities)
            {
                var cityFont = new Font("Arial", 80 * scaleX, FontStyle.Bold);
                g.DrawString(city.CityName, cityFont, _palette.CityName, (city.X - startX) * scaleX, (city.Z - startY) * scaleY);
            }

            var overlays = _mapper.MapOverlays.Where(item =>
                    item.X >= startX - 1500 && item.X <= endX + 1500 && item.Z >= startY - 1500 &&
                    item.Z <= endY + 1500)
                .ToList();

            foreach (var overlayItem in overlays)
            {
                Bitmap b = overlayItem.Overlay.GetBitmap();
                if (b != null) g.DrawImage(b, (overlayItem.X - b.Width - startX) * scaleX, (overlayItem.Z - b.Height - startY) * scaleY, b.Width * 2 * scaleX, b.Height * 2 * scaleY);
            }


            var elapsedTime = DateTime.Now.Ticks - startTime;
            g.DrawString($"DrawTime: {elapsedTime / TimeSpan.TicksPerMillisecond} ms, x: {centerX}, y: {centerY}, scale: {baseScale}", defaultFont, Brushes.WhiteSmoke, 5, 5);

        }
    }
}
