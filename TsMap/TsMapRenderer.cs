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
        private const float itemDrawMargin = 1000f;

        private int[] zoomCaps = { 1000, 5000, 18500, 45000 };

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

        private static int GetZoomIndex(Rectangle clip, float scale)
        {
            var smallestSize = (clip.Width > clip.Height) ? clip.Height / scale : clip.Width / scale;
            if (smallestSize < 1000) return 0;
            if (smallestSize < 5000) return 1;
            if (smallestSize < 18500) return 2;
            return 3;
        }

        public void Render(Graphics g, Rectangle clip, float scale, PointF startPoint, MapPalette palette, RenderFlags renderFlags = RenderFlags.All)
        {
            var startTime = DateTime.Now.Ticks;
            g.ResetTransform();
            g.FillRectangle(palette.Background, new Rectangle(0, 0, clip.Width, clip.Height));

            g.ScaleTransform(scale, scale);
            g.TranslateTransform(-startPoint.X, -startPoint.Y);
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.None;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var defaultFont = new Font("Arial", 10.0f, FontStyle.Bold);

            if (_mapper == null)
            {
                g.DrawString("Map object not initialized", defaultFont, palette.Error, 5, 5);
                return;
            }

            var zoomIndex = GetZoomIndex(clip, scale);

            var endPoint = new PointF(startPoint.X + clip.Width / scale, startPoint.Y + clip.Height / scale);

            var ferryStartTime = DateTime.Now.Ticks;

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
                            new PointF(conn.StartPortLocation.X, conn.StartPortLocation.Y)
                        };

                        foreach (var connection in conn.connections)
                        {
                            newPoints.Add(new PointF(connection.X, connection.Y));
                        }
                        newPoints.Add(new PointF(conn.EndPortLocation.X, conn.EndPortLocation.Y));

                        var pen = new Pen(palette.FerryLines, 50) {DashPattern = new[] {10f, 10f}};
                        g.DrawCurve(pen, newPoints.ToArray());
                    }
                }
            }
            var ferryTime = DateTime.Now.Ticks - ferryStartTime;

            var mapAreaStartTime = DateTime.Now.Ticks;
            if ((renderFlags & RenderFlags.MapAreas) != RenderFlags.None)
            {
                var mapAreas = _mapper.MapAreas.Where(item =>
                        item.X >= startPoint.X - itemDrawMargin && item.X <= endPoint.X + itemDrawMargin && item.Z >= startPoint.Y - itemDrawMargin &&
                        item.Z <= endPoint.Y + itemDrawMargin && !item.Hidden)
                    .ToList();


                foreach (var mapArea in mapAreas.OrderBy(x => x.DrawOver))
                {
                    var points = new List<PointF>();

                    foreach (var mapAreaNode in mapArea.NodeUids)
                    {
                        var node = _mapper.GetNodeByUid(mapAreaNode);
                        if (node == null) continue;
                        points.Add(new PointF(node.X, node.Z));
                    }

                    Brush fillColor = palette.PrefabLight;
                    if ((mapArea.ColorIndex & 0x01) != 0) fillColor = palette.PrefabLight;
                    else if ((mapArea.ColorIndex & 0x02) != 0) fillColor = palette.PrefabDark;
                    else if ((mapArea.ColorIndex & 0x03) != 0) fillColor = palette.PrefabGreen;

                    g.FillPolygon(fillColor, points.ToArray());
                }
            }
            var mapAreaTime = DateTime.Now.Ticks - mapAreaStartTime;

            var prefabStartTime = DateTime.Now.Ticks;
            var prefabs = _mapper.Prefabs.Where(item =>
                    item.X >= startPoint.X - itemDrawMargin && item.X <= endPoint.X + itemDrawMargin && item.Z >= startPoint.Y - itemDrawMargin &&
                    item.Z <= endPoint.Y + itemDrawMargin && !item.Hidden)
                .ToList();

            if ((renderFlags & RenderFlags.Prefabs) != RenderFlags.None)
            {
                List<TsPrefabLook> drawingQueue = new List<TsPrefabLook>();

                foreach (var prefabItem in prefabs)
                {
                    var originNode = _mapper.GetNodeByUid(prefabItem.Nodes[0]);
                    if (prefabItem.Prefab.PrefabNodes == null) continue;

                    if (!prefabItem.HasLooks())
                    {
                        var mapPointOrigin = prefabItem.Prefab.PrefabNodes[prefabItem.Origin];

                        var rot = (float)(originNode.Rotation - Math.PI -
                                           Math.Atan2(mapPointOrigin.RotZ, mapPointOrigin.RotX) + Math.PI / 2);

                        var prefabstartX = originNode.X - mapPointOrigin.X;
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
                                                prefabstartX + prefabItem.Prefab.MapPoints[nextPoint].X,
                                                prefabStartZ + prefabItem.Prefab.MapPoints[nextPoint].Z, rot, originNode.X,
                                                originNode.Z);

                                            polyPoints.Add(nextPoint, new PointF(newPoint.X, newPoint.Y));
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

                                prefabItem.AddLook(prefabLook);
                                continue;
                            }

                            var mapPointLaneCount = mapPoint.LaneCount;

                            if (mapPointLaneCount == -2 && i < prefabItem.Prefab.PrefabNodes.Count)
                            {
                                if (mapPoint.ControlNodeIndex != -1) mapPointLaneCount = prefabItem.Prefab.PrefabNodes[mapPoint.ControlNodeIndex].LaneCount;
                            }

                            foreach (var neighbourPointIndex in mapPoint.Neighbours) // TODO: Fix connection between road segments
                            {
                                if (pointsDrawn.Contains(neighbourPointIndex)) continue;
                                var neighbourPoint = prefabItem.Prefab.MapPoints[neighbourPointIndex];

                                if ((mapPoint.Hidden || neighbourPoint.Hidden) && prefabItem.Prefab.PrefabNodes.Count + 1 <
                                    prefabItem.Prefab.MapPoints.Count) continue;

                                var roadYaw = Math.Atan2(neighbourPoint.Z - mapPoint.Z, neighbourPoint.X - mapPoint.X);

                                var neighbourLaneCount = neighbourPoint.LaneCount;

                                if (neighbourLaneCount == -2 && neighbourPointIndex < prefabItem.Prefab.PrefabNodes.Count)
                                {
                                    if (neighbourPoint.ControlNodeIndex != -1) neighbourLaneCount = prefabItem.Prefab.PrefabNodes[neighbourPoint.ControlNodeIndex].LaneCount;
                                }

                                if (mapPointLaneCount == -2 && neighbourLaneCount != -2) mapPointLaneCount = neighbourLaneCount;
                                else if (neighbourLaneCount == -2 && mapPointLaneCount != -2) neighbourLaneCount = mapPointLaneCount;
                                else if (mapPointLaneCount == -2 && neighbourLaneCount == -2)
                                {
                                    Console.WriteLine($"Could not find lane count for ({i}, {neighbourPointIndex}), defaulting to 1 for {prefabItem.Prefab.FilePath}");
                                    mapPointLaneCount = neighbourLaneCount = 1;
                                }

                                var cornerCoords = new List<PointF>();

                                var coords = GetCornerCoords(prefabstartX + mapPoint.X, prefabStartZ + mapPoint.Z,
                                    (4.5f * mapPointLaneCount + mapPoint.LaneOffset) / 2f, roadYaw + Math.PI / 2);

                                cornerCoords.Add(RotatePoint(coords.X, coords.Y, rot, originNode.X, originNode.Z));

                                coords = GetCornerCoords(prefabstartX + neighbourPoint.X, prefabStartZ + neighbourPoint.Z,
                                    (4.5f * neighbourLaneCount + neighbourPoint.LaneOffset) / 2f,
                                    roadYaw + Math.PI / 2);
                                cornerCoords.Add(RotatePoint(coords.X, coords.Y, rot, originNode.X, originNode.Z));

                                coords = GetCornerCoords(prefabstartX + neighbourPoint.X, prefabStartZ + neighbourPoint.Z,
                                    (4.5f * neighbourLaneCount + mapPoint.LaneOffset) / 2f,
                                    roadYaw - Math.PI / 2);
                                cornerCoords.Add(RotatePoint(coords.X, coords.Y, rot, originNode.X, originNode.Z));

                                coords = GetCornerCoords(prefabstartX + mapPoint.X, prefabStartZ + mapPoint.Z,
                                    (4.5f * mapPointLaneCount + mapPoint.LaneOffset) / 2f, roadYaw - Math.PI / 2);
                                cornerCoords.Add(RotatePoint(coords.X, coords.Y, rot, originNode.X, originNode.Z));

                                TsPrefabLook prefabLook = new TsPrefabPolyLook(cornerCoords)
                                {
                                    Color = palette.PrefabRoad,
                                    ZIndex = 4,
                                };

                                prefabItem.AddLook(prefabLook);
                            }
                        }
                    }

                    prefabItem.GetLooks().ForEach(x => drawingQueue.Add(x));
                }

                foreach (var prefabLook in drawingQueue.OrderBy(p => p.ZIndex))
                {
                    prefabLook.Draw(g);
                }
            }
            var prefabTime = DateTime.Now.Ticks - prefabStartTime;

            var roadStartTime = DateTime.Now.Ticks;
            if ((renderFlags & RenderFlags.Roads) != RenderFlags.None)
            {
                var roads = _mapper.Roads.Where(item =>
                        item.X >= startPoint.X - itemDrawMargin && item.X <= endPoint.X + itemDrawMargin && item.Z >= startPoint.Y - itemDrawMargin &&
                        item.Z <= endPoint.Y + itemDrawMargin && !item.Hidden)
                    .ToList();

                foreach (var road in roads)
                {
                    var startNode = road.GetStartNode();
                    var endNode = road.GetEndNode();

                    if (!road.HasPoints())
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

                    var roadWidth = road.RoadLook.GetWidth();

                    g.DrawCurve(new Pen(palette.Road, roadWidth), road.GetPoints()?.ToArray());
                }
            }
            var roadTime = DateTime.Now.Ticks - roadStartTime;

            var mapOverlayStartTime = DateTime.Now.Ticks;
            if ((renderFlags & RenderFlags.MapOverlays) != RenderFlags.None)
            {
                var overlays = _mapper.MapOverlays.Where(item =>
                        item.X >= startPoint.X - itemDrawMargin && item.X <= endPoint.X + itemDrawMargin && item.Z >= startPoint.Y - itemDrawMargin &&
                        item.Z <= endPoint.Y + itemDrawMargin && !item.Hidden)
                    .ToList();

                foreach (var overlayItem in overlays) // TODO: Scaling
                {
                    Bitmap b = overlayItem.Overlay.GetBitmap();
                    if (b != null)
                        g.DrawImage(b, overlayItem.X - b.Width, overlayItem.Z - b.Height, b.Width * 2, b.Height * 2);
                }
            }
            var mapOverlayTime = DateTime.Now.Ticks - mapOverlayStartTime;

            var mapOverlay2StartTime = DateTime.Now.Ticks;
            if ((renderFlags & RenderFlags.MapOverlays) != RenderFlags.None)
            {
                var companies = _mapper.Companies.Where(item =>
                        item.X >= startPoint.X - itemDrawMargin && item.X <= endPoint.X + itemDrawMargin && item.Z >= startPoint.Y - itemDrawMargin &&
                        item.Z <= endPoint.Y + itemDrawMargin && !item.Hidden)
                    .ToList();

                foreach (var companyItem in companies) // TODO: Scaling
                {
                    Bitmap b = companyItem.Overlay?.GetBitmap();
                    if (b != null)
                        g.DrawImage(b, companyItem.X, companyItem.Z, b.Width, b.Height);
                }

                foreach (var prefab in prefabs) // Draw all prefab overlays
                {
                    var originNode = _mapper.GetNodeByUid(prefab.Nodes[0]);
                    if (prefab.Prefab.PrefabNodes == null) continue;
                    var mapPointOrigin = prefab.Prefab.PrefabNodes[prefab.Origin];

                    var rot = (float) (originNode.Rotation - Math.PI -
                                       Math.Atan2(mapPointOrigin.RotZ, mapPointOrigin.RotX) + Math.PI / 2);

                    var prefabstartX = originNode.X - mapPointOrigin.X;
                    var prefabStartZ = originNode.Z - mapPointOrigin.Z;
                    foreach (var spawnPoint in prefab.Prefab.SpawnPoints)
                    {
                        var newPoint = RotatePoint(prefabstartX + spawnPoint.X, prefabStartZ + spawnPoint.Z, rot,
                            originNode.X, originNode.Z);

                        Bitmap b = null;

                        switch (spawnPoint.Type)
                        {
                            case TsSpawnPointType.Fuel:
                            {
                                var overlay = _mapper.LookupOverlay(0x11C686A54F);
                                b = overlay?.GetBitmap();
                                break;
                            }
                            case TsSpawnPointType.Service:
                            {
                                var overlay = _mapper.LookupOverlay(0x2358E7493388A97);
                                b = overlay?.GetBitmap();
                                break;
                            }
                            case TsSpawnPointType.WeightStation:
                            {
                                var overlay = _mapper.LookupOverlay(0xD50E1058FBBF179F);
                                b = overlay?.GetBitmap();
                                break;
                            }
                            case TsSpawnPointType.TruckDealer:
                            {
                                var overlay = _mapper.LookupOverlay(0xEE210C8438914);
                                b = overlay?.GetBitmap();
                                break;
                            }
                            case TsSpawnPointType.GarageOutdoor:
                            {
                                var overlay = _mapper.LookupOverlay(0x4572831B4D58CC5B);
                                b = overlay?.GetBitmap();
                                break;
                            }
                            case TsSpawnPointType.Recruitment:
                            {
                                var overlay = _mapper.LookupOverlay(0x1E18DD7A560F3E5A);
                                b = overlay?.GetBitmap();
                                break;
                            }
                        }
                        if (b != null)
                            g.DrawImage(b, newPoint.X - b.Width / 2f, newPoint.Y - b.Height / 2f, b.Width, b.Height);

                    }

                    var lastId = -1;
                    foreach (var triggerPoint in prefab.Prefab.TriggerPoints) // trigger points in prefabs: garage, hotel, ...
                    {
                        var newPoint = RotatePoint(prefabstartX + triggerPoint.X, prefabStartZ + triggerPoint.Z, rot,
                            originNode.X, originNode.Z);

                        if (triggerPoint.TriggerId == lastId) continue;
                        lastId = (int) triggerPoint.TriggerId;

                        if (triggerPoint.TriggerActionUid == 0x18991B7A99E279C) // parking trigger
                        {
                            var overlay = _mapper.LookupOverlay(0x2358E762E112CD4);
                            Bitmap b = overlay?.GetBitmap();

                            if (b != null)
                                g.DrawImage(b, newPoint.X - b.Width / 2f, newPoint.Y - b.Height / 2f, b.Width, b.Height);
                        }
                    }
                }

                var triggers = _mapper.Triggers.Where(item =>
                        item.X >= startPoint.X - itemDrawMargin && item.X <= endPoint.X + itemDrawMargin && item.Z >= startPoint.Y - itemDrawMargin &&
                        item.Z <= endPoint.Y + itemDrawMargin && !item.Hidden)
                    .ToList();

                foreach (var triggerItem in triggers) // TODO: Scaling
                {
                    Bitmap b = triggerItem.Overlay?.GetBitmap();
                    if (b != null)
                        g.DrawImage(b, triggerItem.X, triggerItem.Z, b.Width, b.Height);
                }

                var ferryItems = _mapper.FerryConnections.Where(item =>
                        item.X >= startPoint.X - itemDrawMargin && item.X <= endPoint.X + itemDrawMargin && item.Z >= startPoint.Y - itemDrawMargin &&
                        item.Z <= endPoint.Y + itemDrawMargin)
                    .ToList();

                foreach (var ferryItem in ferryItems) // TODO: Scaling
                {
                    Bitmap b = ferryItem.Overlay?.GetBitmap();
                    if (b != null)
                        g.DrawImage(b, ferryItem.X, ferryItem.Z, b.Width, b.Height);
                }
            }
            var mapOverlay2Time = DateTime.Now.Ticks - mapOverlay2StartTime;

            var cityStartTime = DateTime.Now.Ticks;
            if ((renderFlags & RenderFlags.CityNames) != RenderFlags.None) // TODO: Fix position and scaling
            {
                var cities = _mapper.Cities.Where(item => !item.Hidden).ToList();

                var cityFont = new Font("Arial", 100 + zoomCaps[zoomIndex] / 100, FontStyle.Bold);

                foreach (var city in cities)
                {
                    var name = city.City.Name;

                    if (city.City.NameLocalized != string.Empty)
                    {
                        var localName = _mapper.GetLocalizedName(city.City.NameLocalized);
                        if (localName != null) name = localName;
                    }

                    var node = _mapper.GetNodeByUid(city.NodeUid);
                    var coords = (node == null) ? new PointF(city.X, city.Z) : new PointF(node.X, node.Z);
                    if (city.City.XOffsets.Count > zoomIndex && city.City.YOffsets.Count > zoomIndex)
                    {
                        coords.X += city.City.XOffsets[zoomIndex] / (scale * zoomCaps[zoomIndex]);
                        coords.Y += city.City.YOffsets[zoomIndex] / (scale * zoomCaps[zoomIndex]);
                    }

                    var textSize = g.MeasureString(name, cityFont);

                    g.DrawString(name, cityFont, new SolidBrush(Color.FromArgb(210, 0, 0, 0)), coords.X + 2, coords.Y + 2);
                    g.DrawString(name, cityFont, palette.CityName, coords.X, coords.Y);
                }
            }
            var cityTime = DateTime.Now.Ticks - cityStartTime;

            g.ResetTransform();
            var elapsedTime = DateTime.Now.Ticks - startTime;
            if ((renderFlags & RenderFlags.TextOverlay) != RenderFlags.None)
            {
                g.DrawString(
                    $"DrawTime: {elapsedTime / TimeSpan.TicksPerMillisecond} ms, x: {startPoint.X}, y: {startPoint.Y}, scale: {scale}",
                    defaultFont, Brushes.WhiteSmoke, 5, 5);

                //g.FillRectangle(new SolidBrush(Color.FromArgb(100, 0, 0, 0)), 5, 20, 150, 150);
                //g.DrawString($"Road: {roadTime / TimeSpan.TicksPerMillisecond}ms", defaultFont, Brushes.White, 10, 40);
                //g.DrawString($"Prefab: {prefabTime / TimeSpan.TicksPerMillisecond}ms", defaultFont, Brushes.White, 10, 55);
                //g.DrawString($"Ferry: {ferryTime / TimeSpan.TicksPerMillisecond}ms", defaultFont, Brushes.White, 10, 70);
                //g.DrawString($"MapOverlay: {mapOverlayTime / TimeSpan.TicksPerMillisecond}ms", defaultFont, Brushes.White, 10, 85);
                //g.DrawString($"MapOverlay2: {mapOverlay2Time / TimeSpan.TicksPerMillisecond}ms", defaultFont, Brushes.White, 10, 100);
                //g.DrawString($"MapArea: {mapAreaTime / TimeSpan.TicksPerMillisecond}ms", defaultFont, Brushes.White, 10, 115);
                //g.DrawString($"City: {cityTime / TimeSpan.TicksPerMillisecond}ms", defaultFont, Brushes.White, 10, 130);
            }

        }
    }
}
