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

        public TsMapRenderer(TsMapper mapper)
        {
            _mapper = mapper;
        }

        private static PointF RotatePoint(float x, float z, float angle, float rotX, float rotZ)
        {
            var s = Math.Sin(angle);
            var c = Math.Cos(angle);
            double newX = x - rotX;
            double newZ = z - rotZ;
            return new PointF((float) ((newX * c) - (newZ * s) + rotX), (float) ((newX * s) + (newZ * c) + rotZ));
        }

        private static PointF GetCornerCoords(float x, float z, float width, double angle)
        {
            return new PointF(
                (float) (x + width * Math.Cos(angle)),
                (float) (z + width * Math.Sin(angle))
            );
        }

        public void Render(Graphics g, Rectangle clip, float baseScale, PointF pos, MapPalette palette, RenderFlags renderFlags = RenderFlags.All)
        {
            var startTime = DateTime.Now.Ticks;
            g.FillRectangle(palette.Background, new Rectangle(0, 0, clip.X + clip.Width, clip.Y + clip.Height));
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.None;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var defaultFont = new Font("Arial", 10.0f, FontStyle.Bold);

            if (_mapper == null)
            {
                g.DrawString("Map object not initialized", defaultFont, palette.Error, 5, 5);
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


            if ((renderFlags & RenderFlags.FerryConnections) != RenderFlags.None)
            {
                var ferryConnections = _mapper.FerryConnections.Where(item => !item.Hidden)
                    .ToList();

                foreach (var ferryConnection in ferryConnections)
                {
                    var connections = _mapper.LookupFerryConnection(ferryConnection.FerryPortId);

                    foreach (var conn in connections)
                    {
                        var newPoints = new List<PointF>
                        {
                            new PointF((conn.StartPortLocation.X - startX) * scaleX,
                                (conn.StartPortLocation.Y - startY) * scaleY)
                        };

                        foreach (var connection in conn.connections)
                        {
                            newPoints.Add(
                                new PointF((connection.X - startX) * scaleX, (connection.Y - startY) * scaleY));
                        }
                        newPoints.Add(new PointF((conn.EndPortLocation.X - startX) * scaleX,
                            (conn.EndPortLocation.Y - startY) * scaleY));

                        var pen = new Pen(palette.FerryLines, 50 * scaleX) {DashPattern = new[] {10f, 10f}};
                        g.DrawCurve(pen, newPoints.ToArray());
                    }
                }
            }

            if ((renderFlags & RenderFlags.MapAreas) != RenderFlags.None)
            {
                var mapAreas = _mapper.MapAreas.Where(item =>
                        item.X >= startX - 1500 && item.X <= endX + 1500 && item.Z >= startY - 1500 &&
                        item.Z <= endY + 1500 && !item.Hidden)
                    .ToList();


                foreach (var mapArea in mapAreas.OrderBy(x => x.DrawOver))
                {
                    var points = new List<PointF>();

                    foreach (var mapAreaNode in mapArea.NodeUids)
                    {
                        var node = _mapper.GetNodeByUid(mapAreaNode);
                        if (node == null) continue;
                        points.Add(new PointF((node.X - startX) * scaleX, (node.Z - startY) * scaleY));
                    }

                    Brush fillColor = palette.PrefabLight;
                    if ((mapArea.ColorIndex & 0x01) != 0) fillColor = palette.PrefabLight;
                    else if ((mapArea.ColorIndex & 0x02) != 0) fillColor = palette.PrefabDark;
                    else if ((mapArea.ColorIndex & 0x03) != 0) fillColor = palette.PrefabGreen;

                    g.FillPolygon(fillColor, points.ToArray());
                }
            }

            var prefabs = _mapper.Prefabs.Where(item =>
                    item.X >= startX - 1500 && item.X <= endX + 1500 && item.Z >= startY - 1500 &&
                    item.Z <= endY + 1500 && !item.Hidden)
                .ToList();

            if ((renderFlags & RenderFlags.Prefabs) != RenderFlags.None)
            {
                List<TsPrefabLook> drawingQueue = new List<TsPrefabLook>();

                foreach (var prefabItem in prefabs) // TODO: Road Width
                {
                    var originNode = _mapper.GetNodeByUid(prefabItem.Nodes[0]);
                    if (prefabItem.Prefab.PrefabNodes == null) continue;
                    var mapPointOrigin = prefabItem.Prefab.PrefabNodes[prefabItem.Origin];

                    var rot = (float) (originNode.Rotation - Math.PI -
                                       Math.Atan2(mapPointOrigin.RotZ, mapPointOrigin.RotX) + Math.PI / 2);

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
                                if (prefabItem.Prefab.MapPoints[nextPoint].Neighbours.Count == 0) break;

                                foreach (var neighbour in prefabItem.Prefab.MapPoints[nextPoint].Neighbours)
                                {
                                    if (!polyPoints.ContainsKey(neighbour)) // New Polygon Neighbour
                                    {
                                        nextPoint = neighbour;
                                        var newPoint = RotatePoint(
                                            prefabStartX + prefabItem.Prefab.MapPoints[nextPoint].X,
                                            prefabStartZ + prefabItem.Prefab.MapPoints[nextPoint].Z, rot, originNode.X,
                                            originNode.Z);

                                        polyPoints.Add(nextPoint,
                                            new PointF((newPoint.X - startX) * scaleX,
                                                (newPoint.Y - startY) * scaleY));
                                        break;
                                    }
                                    nextPoint = -1;
                                }
                            } while (nextPoint != -1);

                            if (polyPoints.Count < 2) continue;

                            var colorFlag = prefabItem.Prefab.MapPoints[polyPoints.First().Key].PrefabColorFlags;

                            Brush fillColor = palette.PrefabLight;
                            if ((colorFlag & 0x02) != 0) fillColor = palette.PrefabLight;
                            else if ((colorFlag & 0x04) != 0) fillColor = palette.PrefabDark;
                            else if ((colorFlag & 0x08) != 0) fillColor = palette.PrefabGreen;
                            // else fillColor = _palette.Error; // Unknown

                            var prefabLook = new TsPrefabPolyLook(polyPoints.Values.ToList())
                            {
                                ZIndex = ((colorFlag & 0x01) != 0) ? 3 : 2,
                                Color = fillColor
                            };

                            drawingQueue.Add(prefabLook);
                            continue;
                        }

                        foreach (var neighbourPointIndex in mapPoint.Neighbours
                        ) // TODO: Fix connection between road segments
                        {
                            if (pointsDrawn.Contains(neighbourPointIndex)) continue;
                            var neighbourPoint = prefabItem.Prefab.MapPoints[neighbourPointIndex];

                            if ((mapPoint.Hidden || neighbourPoint.Hidden) && prefabItem.Prefab.PrefabNodes.Count + 1 <
                                prefabItem.Prefab.MapPoints.Count) continue;

                            var roadYaw = Math.Atan2(neighbourPoint.Z - mapPoint.Z, neighbourPoint.X - mapPoint.X);

                            var cornerCoords = new List<PointF>();

                            var coords = GetCornerCoords((prefabStartX + mapPoint.X - startX) * scaleX,
                                (prefabStartZ + mapPoint.Z - startY) * scaleY,
                                (4.5f * mapPoint.LaneCount + mapPoint.LaneOffset) / 2f * scaleX, roadYaw + Math.PI / 2);

                            cornerCoords.Add(RotatePoint(coords.X, coords.Y, rot, (originNode.X - startX) * scaleX,
                                (originNode.Z - startY) * scaleY));

                            coords = GetCornerCoords((prefabStartX + neighbourPoint.X - startX) * scaleX,
                                (prefabStartZ + neighbourPoint.Z - startY) * scaleY,
                                (4.5f * neighbourPoint.LaneCount + neighbourPoint.LaneOffset) / 2f * scaleX,
                                roadYaw + Math.PI / 2);
                            cornerCoords.Add(RotatePoint(coords.X, coords.Y, rot, (originNode.X - startX) * scaleX,
                                (originNode.Z - startY) * scaleY));

                            coords = GetCornerCoords((prefabStartX + neighbourPoint.X - startX) * scaleX,
                                (prefabStartZ + neighbourPoint.Z - startY) * scaleY,
                                (4.5f * neighbourPoint.LaneCount + mapPoint.LaneOffset) / 2f * scaleX,
                                roadYaw - Math.PI / 2);
                            cornerCoords.Add(RotatePoint(coords.X, coords.Y, rot, (originNode.X - startX) * scaleX,
                                (originNode.Z - startY) * scaleY));

                            coords = GetCornerCoords((prefabStartX + mapPoint.X - startX) * scaleX,
                                (prefabStartZ + mapPoint.Z - startY) * scaleY,
                                (4.5f * mapPoint.LaneCount + mapPoint.LaneOffset) / 2f * scaleX, roadYaw - Math.PI / 2);
                            cornerCoords.Add(RotatePoint(coords.X, coords.Y, rot, (originNode.X - startX) * scaleX,
                                (originNode.Z - startY) * scaleY));

                            TsPrefabLook prefabLook = new TsPrefabPolyLook(cornerCoords)
                            {
                                Color = palette.PrefabRoad,
                                ZIndex = 4,
                            };

                            drawingQueue.Add(prefabLook);
                        }

                    }
                }

                foreach (var prefabLook in drawingQueue.OrderBy(p => p.ZIndex))
                {
                    prefabLook.Draw(g);
                }
            }

            if ((renderFlags & RenderFlags.Roads) != RenderFlags.None)
            {
                var roads = _mapper.Roads.Where(item =>
                        item.X >= startX - 1500 && item.X <= endX + 1500 && item.Z >= startY - 1500 &&
                        item.Z <= endY + 1500 && !item.Hidden)
                    .ToList();

                foreach (var road in roads)
                {
                    var startNode = road.GetStartNode();
                    var endNode = road.GetEndNode();

                    if (road.GetPoints() == null)
                    {
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
                            var s = i / (float) (8 - 1);
                            var x = (float) TsRoadLook.Hermite(s, sx, ex, tanSx, tanEx);
                            var z = (float) TsRoadLook.Hermite(s, sz, ez, tanSz, tanEz);
                            newPoints.Add(new PointF(x, z));
                        }
                        road.AddPoints(newPoints);
                    }

                    var points = road.GetPoints();

                    for (var i = 0; i < points.Length; i++)
                    {
                        var point = points[i];
                        points[i] = new PointF((point.X - startX) * scaleX, (point.Y - startY) * scaleY);
                    }

                    var roadWidth = road.RoadLook.GetWidth() * scaleX;

                    g.DrawCurve(new Pen(palette.Road, roadWidth), points.ToArray());
                }
            }

            if ((renderFlags & RenderFlags.CityNames) != RenderFlags.None)
            {
                var cities = _mapper.Cities.Where(item =>
                        item.X >= startX - 1500 && item.X <= endX + 1500 && item.Z >= startY - 1500 &&
                        item.Z <= endY + 1500 && !item.Hidden)
                    .ToList();

                foreach (var city in cities)
                {
                    var cityFont = new Font("Arial", 80 * scaleX, FontStyle.Bold);

                    var name = city.City.Name;

                    if (city.City.NameLocalized != string.Empty)
                    {
                        var localName = _mapper.GetLocalizedName(city.City.NameLocalized);
                        if (localName != null) name = localName;
                    }

                    g.DrawString(name, cityFont, palette.CityName, (city.X - startX) * scaleX,
                        (city.Z - startY) * scaleY);
                }
            }


            if ((renderFlags & RenderFlags.MapOverlays) != RenderFlags.None)
            {
                var overlays = _mapper.MapOverlays.Where(item =>
                        item.X >= startX - 1500 && item.X <= endX + 1500 && item.Z >= startY - 1500 &&
                        item.Z <= endY + 1500 && !item.Hidden)
                    .ToList();

                foreach (var overlayItem in overlays) // TODO: Scaling
                {
                    Bitmap b = overlayItem.Overlay.GetBitmap();
                    if (b != null)
                        g.DrawImage(b, (overlayItem.X - b.Width - startX) * scaleX,
                            (overlayItem.Z - b.Height - startY) * scaleY,
                            b.Width * 2 * scaleX, b.Height * 2 * scaleY);
                }
            }

            if ((renderFlags & RenderFlags.MapOverlays) != RenderFlags.None)
            {
                var companies = _mapper.Companies.Where(item =>
                        item.X >= startX - 1500 && item.X <= endX + 1500 && item.Z >= startY - 1500 &&
                        item.Z <= endY + 1500 && !item.Hidden)
                    .ToList();

                foreach (var companyItem in companies) // TODO: Scaling
                {
                    Bitmap b = companyItem.Overlay?.GetBitmap();
                    if (b != null)
                        g.DrawImage(b, (companyItem.X - startX) * scaleX, (companyItem.Z - startY) * scaleY,
                            b.Width * scaleX, b.Height * scaleY);
                }

                foreach (var prefab in prefabs) // Draw all prefab overlays
                {
                    var originNode = _mapper.GetNodeByUid(prefab.Nodes[0]);
                    if (prefab.Prefab.PrefabNodes == null) continue;
                    var mapPointOrigin = prefab.Prefab.PrefabNodes[prefab.Origin];

                    var rot = (float) (originNode.Rotation - Math.PI -
                                       Math.Atan2(mapPointOrigin.RotZ, mapPointOrigin.RotX) + Math.PI / 2);

                    var prefabStartX = originNode.X - mapPointOrigin.X;
                    var prefabStartZ = originNode.Z - mapPointOrigin.Z;
                    foreach (var spawnPoint in prefab.Prefab.SpawnPoints)
                    {
                        var newPoint = RotatePoint(prefabStartX + spawnPoint.X, prefabStartZ + spawnPoint.Z, rot,
                            originNode.X, originNode.Z);

                        switch (spawnPoint.Type)
                        {
                            case TsSpawnPointType.Fuel:
                            {
                                var overlay = _mapper.LookupOverlay(0x11C686A54F);
                                Bitmap b = overlay?.GetBitmap();

                                if (b != null)
                                    g.DrawImage(b, (newPoint.X - b.Width / 2f - startX) * scaleX,
                                        (newPoint.Y - b.Height / 2f - startY) * scaleY,
                                        b.Width * scaleX, b.Height * scaleY);
                                break;
                            }
                            case TsSpawnPointType.Service:
                            {
                                var overlay = _mapper.LookupOverlay(0x2358E7493388A97);
                                Bitmap b = overlay?.GetBitmap();

                                if (b != null)
                                    g.DrawImage(b, (newPoint.X - b.Width / 2f - startX) * scaleX,
                                        (newPoint.Y - b.Height / 2f - startY) * scaleY,
                                        b.Width * scaleX, b.Height * scaleY);
                                break;
                            }
                            case TsSpawnPointType.WeightStation:
                            {
                                var overlay = _mapper.LookupOverlay(0xD50E1058FBBF179F);
                                Bitmap b = overlay?.GetBitmap();

                                if (b != null)
                                    g.DrawImage(b, (newPoint.X - b.Width / 2f - startX) * scaleX,
                                        (newPoint.Y - b.Height / 2f - startY) * scaleY,
                                        b.Width * scaleX, b.Height * scaleY);
                                break;
                            }
                            case TsSpawnPointType.TruckDealer:
                            {
                                var overlay = _mapper.LookupOverlay(0xEE210C8438914);
                                Bitmap b = overlay?.GetBitmap();

                                if (b != null)
                                    g.DrawImage(b, (newPoint.X - b.Width / 2f - startX) * scaleX,
                                        (newPoint.Y - b.Height / 2f - startY) * scaleY,
                                        b.Width * scaleX, b.Height * scaleY);
                                break;
                            }
                            case TsSpawnPointType.GarageOutdoor:
                            {
                                var overlay = _mapper.LookupOverlay(0x4572831B4D58CC5B);
                                Bitmap b = overlay?.GetBitmap();

                                if (b != null)
                                    g.DrawImage(b, (newPoint.X - b.Width / 2f - startX) * scaleX,
                                        (newPoint.Y - b.Height / 2f - startY) * scaleY,
                                        b.Width * scaleX, b.Height * scaleY);
                                break;
                            }
                            case TsSpawnPointType.Recruitment:
                            {
                                var overlay = _mapper.LookupOverlay(0x1E18DD7A560F3E5A);
                                Bitmap b = overlay?.GetBitmap();

                                if (b != null)
                                    g.DrawImage(b, (newPoint.X - b.Width / 2f - startX) * scaleX,
                                        (newPoint.Y - b.Height / 2f - startY) * scaleY,
                                        b.Width * scaleX, b.Height * scaleY);
                                break;
                            }
                        }
                    }

                    var lastId = -1;
                    foreach (var triggerPoint in prefab.Prefab.TriggerPoints
                    ) // trigger points in prefabs: garage, hotel, ...
                    {
                        var newPoint = RotatePoint(prefabStartX + triggerPoint.X, prefabStartZ + triggerPoint.Z, rot,
                            originNode.X, originNode.Z);

                        if (triggerPoint.TriggerId == lastId) continue;
                        lastId = (int) triggerPoint.TriggerId;

                        if (triggerPoint.TriggerActionUid == 0x18991B7A99E279C) // parking trigger
                        {
                            var overlay = _mapper.LookupOverlay(0x2358E762E112CD4);
                            Bitmap b = overlay?.GetBitmap();

                            if (b != null)
                                g.DrawImage(b, (newPoint.X - b.Width / 2f - startX) * scaleX,
                                    (newPoint.Y - b.Height / 2f - startY) * scaleY,
                                    b.Width * scaleX, b.Height * scaleY);
                        }
                    }
                }

                var triggers = _mapper.Triggers.Where(item =>
                        item.X >= startX - 1500 && item.X <= endX + 1500 && item.Z >= startY - 1500 &&
                        item.Z <= endY + 1500 && !item.Hidden)
                    .ToList();

                foreach (var triggerItem in triggers) // TODO: Scaling
                {
                    Bitmap b = triggerItem.Overlay?.GetBitmap();
                    if (b != null)
                        g.DrawImage(b, (triggerItem.X - startX) * scaleX, (triggerItem.Z - startY) * scaleY,
                            b.Width * scaleX, b.Height * scaleY);
                }

                var ferryItems = _mapper.FerryConnections.Where(item =>
                        item.X >= startX - 1500 && item.X <= endX + 1500 && item.Z >= startY - 1500 &&
                        item.Z <= endY + 1500)
                    .ToList();

                foreach (var ferryItem in ferryItems) // TODO: Scaling
                {
                    Bitmap b = ferryItem.Overlay?.GetBitmap();
                    if (b != null)
                        g.DrawImage(b, (ferryItem.X - startX) * scaleX, (ferryItem.Z - startY) * scaleY,
                            b.Width * scaleX, b.Height * scaleY);
                }
            }
            var elapsedTime = DateTime.Now.Ticks - startTime;
            if ((renderFlags & RenderFlags.TextOverlay) != RenderFlags.None)
            {
                g.DrawString(
                    $"DrawTime: {elapsedTime / TimeSpan.TicksPerMillisecond} ms, x: {centerX}, y: {centerY}, scale: {baseScale}",
                    defaultFont, Brushes.WhiteSmoke, 5, 5);
            }

        }
    }
}
